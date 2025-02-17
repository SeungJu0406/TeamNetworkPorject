using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour 
{
    [SerializeField] private Button _settingButton;

    public enum Box { Main, Quick, Join, Create, Size }

    [System.Serializable]
    struct BoxStruct
    {
        public MainBox Main;
        public MainBox Quick; 
        public MainBox Join;
        public MainBox Create;
    }
    [SerializeField] BoxStruct _boxStruct;

    private MainBox[] _boxs = new MainBox[(int)Box.Size];


    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        SubscribesEvent();
    }

    private void OnEnable()
    { 
        if (LobbyScene.Instance != null &&LobbyScene.IsJoinRoomCancel == true) // 로딩 캔슬 초기화 및 UI 변경 금지
        {
            LobbyScene.IsJoinRoomCancel = false;
            return;
        }

        ChangeBox(Box.Main);
    }

    private void OnDisable()
    {
        if (LobbyScene.IsJoinRoomCancel == true) // 로딩 캔슬 시
        {
            CancelJoinRoom();
        }
    }

    /// <summary>
    /// 방 입장 캔슬
    /// </summary>
    private void CancelJoinRoom()
    {
        LoadingBox.StartLoading();
        PhotonNetwork.LeaveRoom();
    }


    /// <summary>
    /// UI 박스 변경
    /// </summary>
    public void ChangeBox(Box box)
    {
        LoadingBox.StopLoading();

        for (int i = 0; i < _boxs.Length; i++)
        {
            if (_boxs[i] == null)
                return;

            if (i == (int)box) // 바꾸고자 하는 박스만 활성화
            {
                _boxs[i].gameObject.SetActive(true);
                //ClearBox(box); // 초기화 작업도 진행
            }
            else
            {
                _boxs[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 초기 설정
    /// </summary>
    private void Init()
    {
        InitBoxs(_boxStruct.Main, Box.Main );
        InitBoxs(_boxStruct.Quick, Box.Quick );
        InitBoxs(_boxStruct.Join, Box.Join );
        InitBoxs(_boxStruct.Create, Box.Create );
    }

    private void InitBoxs(MainBox box, Box index)
    {
        _boxs[(int)index] = box;
        box.Panel = this;
    }


    /// <summary>
    /// 이벤트 구독
    /// </summary>
    private void SubscribesEvent()
    {
        _settingButton.onClick.AddListener(() => OptionPanel.SetActiveOption(true));
        _settingButton.onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
    }
}
