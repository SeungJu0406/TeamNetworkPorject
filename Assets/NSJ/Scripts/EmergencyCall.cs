using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EmergencyCall : BaseUIPun
{
    private GameObject _emergencyCall => GetUI("EmergencyCall");
    private EmergencyCallButton _button => GetUI<EmergencyCallButton>("Button");
    private GameObject _buttonPush => GetUI("ButtonPush");
    [SerializeField] private Animator _animator;

    int _openPopUpHash = Animator.StringToHash("OpenPopup");
    int _closePopUpHash = Animator.StringToHash("ClosePopup");

    private void Awake()
    {
        Bind();

    }

    private void Start()
    {
        SubscribeEvents();
    }

    private void Update()
    {
        if (VoteScene.Instance != null)
        {
            _emergencyCall.SetActive(false);
            return;
        }
    }

    /// <summary>
    /// 버튼 클릭시 누른 이미지 나오기
    /// </summary>
    private void ClickDownButton()
    {
        _buttonPush.SetActive(true);
    } 

    /// <summary>
    /// 버튼 클릭 해제 시 누른 이미지 비표시 및 긴급회의
    /// </summary>
    private void ClickUpButton()
    {
        _buttonPush.SetActive(false);
        // 마우스 포인트가 버튼 위에 있을 시에 긴급소집

        if (GameManager.Instance.GlobalMissionState == true)
            return;
        if (GameManager.IsStartVote == true)
            return;

        if (_button.OnButton)
        {
            StartCoroutine(StartVoteRoutine());
            int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();   
            photonView.RPC(nameof(RPCEmergencyCall),RpcTarget.All, playerNumber);
        }
    }

    IEnumerator StartVoteRoutine()
    {
        GameManager.Instance.SetIsStartVote(true);
        while (true)
        {
            if (VoteScene.Instance != null) 
            {
                GameManager.Instance.SetIsStartVote(false);
                yield break;
            }
            yield return null;
        }
    }

    [PunRPC]
    private void RPCEmergencyCall(int playerNumber)
    {
        StartCoroutine(EmergencyCallRoutine(playerNumber));
    }

    IEnumerator EmergencyCallRoutine(int playerNumber)
    {
        Color reporterColor = PlayerDataContainer.Instance.GetPlayerData(playerNumber).PlayerColor;
        GameUI.ShowEmergency(reporterColor);
        DeleteCorpse();
        yield return GameUI.Emergency.Duration.GetDelay();
        if (PhotonNetwork.IsMasterClient == true)
        {
            SceneChanger.LoadScene("VoteScene", LoadSceneMode.Additive);
        }
    }

    /// <summary>
    /// 시체들 삭제
    /// </summary>
    private void DeleteCorpse()
    {
        GameObject[] Corpse = GameObject.FindGameObjectsWithTag("Dead");
        for (int i = 0; i < Corpse.Length; i++)
        {
            PhotonView targetView = Corpse[i].GetComponent<PhotonView>();
            gameObject.SetActive(false);
        }

    }
    private void Close()
    {
        StartCoroutine(CloseRoutine());    
    }

    IEnumerator CloseRoutine()
    {
       _animator.Play(_closePopUpHash);
        yield return 0.2f.GetDelay();
        _emergencyCall.SetActive(false);
    }

    private void SubscribeEvents()
    {
        GetUI<Button>("CloseButton").onClick.AddListener(Close);
        _button.OnClickDown += ClickDownButton;
        _button.OnClickUp += ClickUpButton;
    }
}
