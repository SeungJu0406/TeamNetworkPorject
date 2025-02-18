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
    /// ���� ����
    /// </summary>
    private void StartRandomMatch()
    {
        string nickName = _quickNickNameInput.text;
        if (nickName != string.Empty) // �г��� ���� ���� ���� �ÿ� �г��� ����
        {
            nickName.ChangeNickName();
        }

        LoadingBox.StartLoading();
        PhotonNetwork.JoinRandomRoom();
    }
    /// <summary>
    /// ���� ���� ��Ī ���� �� ���ο� �� �ڵ� ����
    /// </summary>
    private void CreateRandomRoom(short returnCode, string message)
    {
        string roomCode = Util.GetRandomRoomCode(6); // ���� ���ڵ� ȹ��
        int maxPlayer = 10;
        bool isVisible = true;

        // �� �ɼ� ����
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayer;
        options.IsVisible = isVisible;
        options.SetPrivacy(true); // �������� ���̴ϱ� �����̹��� ���

        PhotonNetwork.CreateRoom(roomCode, options);
    }

    /// <summary>
    /// ���� ���� �ʱ�ȭ
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
