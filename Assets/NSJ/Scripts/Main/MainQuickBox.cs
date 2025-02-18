using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainQuickBox : MainBox
{
    private TMP_InputField _quickNickNameInput => GetUI<TMP_InputField>("QuickNickNameInput");
    private GameObject _quickColorBox => GetUI("QuickColorBox");
    private void Awake()
    {
        Bind();
        Init();
    }


    private void Start()
    {
        SubscribesEvent();
    }

    private void OnEnable()
    {
        ClearQuickBox();
    }
    /// <summary>
    /// 빠른 시작
    /// </summary>
    private void StartRandomMatch()
    {
        string nickName = _quickNickNameInput.text;
        if (nickName != string.Empty) // 닉네임 변동 사항 있을 시에 닉네임 변경
        {
            nickName.ChangeNickName();
        }

        LoadingBox.StartLoading();
        PhotonNetwork.JoinRandomRoom();
    }
    /// <summary>
    /// 빠른 시작 매칭 실패 시 새로운 방 자동 생성
    /// </summary>
    private void CreateRandomRoom(short returnCode, string message)
    {
        string roomCode = Util.GetRandomRoomCode(6); // 랜덤 방코드 획득
        int maxPlayer = 10;
        bool isVisible = true;

        // 방 옵션 세팅
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayer;
        options.IsVisible = isVisible;
        options.SetPrivacy(true); // 빠른시작 방이니까 프라이버시 모드

        PhotonNetwork.CreateRoom(roomCode, options);
    }

    /// <summary>
    /// 빠른 시작 초기화
    /// </summary>
    private void ClearQuickBox()
    {
        _quickNickNameInput.text = string.Empty;
        _quickColorBox.SetActive(false);
    }

    private void Init()
    {

    }

    private void SubscribesEvent()
    {
        LobbyScene.Instance.OnJoinRandomFailedEvent += CreateRandomRoom;
        GetUI<Button>("QuickColorButton").onClick.AddListener(() => { _quickColorBox.SetActive(!_quickColorBox.activeSelf); });
        GetUI<Button>("QuickColorButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("QuickStartButton").onClick.AddListener(StartRandomMatch);
        GetUI<Button>("QuickStartButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("QuickBackButton").onClick.AddListener(() => Panel.ChangeBox(MainPanel.Box.Main));
        GetUI<Button>("QuickBackButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonOff));
    }
}
