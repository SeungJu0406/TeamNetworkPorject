using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMainBox :MainBox
{

    // MainBox
    private TMP_Text _mainNameText => GetUI<TMP_Text>("MainNameText");
    private TMP_Text _mainNickNameText => GetUI<TMP_Text>("MainNickNameText");

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
        ClearMainBox();
    }

    /// <summary>
    /// �α׾ƿ�
    /// </summary>
    private void LogOut()
    {
        LoadingBox.StartLoading();
        BackendManager.Auth.SignOut(); // �α׾ƿ�
        PhotonNetwork.Disconnect(); // ���� ���� ����
    }

    /// <summary>
    /// ���� ȭ�� �ʱ�ȭ
    /// </summary>
    private void ClearMainBox()
    {
        if (BackendManager.Instance == null)
            return;
        if (BackendManager.User == null)
            return;
        _mainNameText.SetText($"{BackendManager.User.FirstName} {BackendManager.User.SecondName} ���� �α����߽��ϴ�".GetText());
        _mainNickNameText.SetText($"[{BackendManager.User.NickName}]".GetText());
    }

    private void Init()
    {
        
    }

    private void SubscribesEvent()
    {
        GetUI<Button>("MainLogOutButton").onClick.AddListener(LogOut);
        GetUI<Button>("MainLogOutButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonOff));

        GetUI<Button>("MainQuickMatchButton").onClick.AddListener(() => Panel.ChangeBox(MainPanel.Box.Quick));
        GetUI<Button>("MainQuickMatchButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("MainJoinButton").onClick.AddListener(() => Panel.ChangeBox(MainPanel.Box.Join));
        GetUI<Button>("MainJoinButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
    }
}
