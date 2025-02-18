using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomPanel : BaseUI
{

    #region private 필드
    private const string SHOWTEXT = "보이기";
    private const string HIDETEXT = "숨기기";
    enum Box { Room, Size }
    private GameObject[] _boxs = new GameObject[(int)Box.Size];

    private TMP_InputField _roomCodeText => GetUI<TMP_InputField>("RoomCodeText");
    private TMP_Text _roomTitleText => GetUI<TMP_Text>("RoomTitleText");
    private TMP_Text _roomCodeActiveText => GetUI<TMP_Text>("RoomCodeActiveText");
    private GameObject _roomStartButton => GetUI("RoomStartButton");
    private TMP_Text _roomPlayerCountText => GetUI<TMP_Text>("RoomPlayerCountText");
    private TMP_Text _roomReadyButtonText => GetUI<TMP_Text>("RoomReadyButtonText");
    #endregion

    private void Awake()
    {
        Bind();
        Init();
    }

    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            UpdateChangeRoom();
        }

        //foreach (Player player in PhotonNetwork.PlayerList)
        //{
        //    if (PhotonNetwork.LocalPlayer.NickName.Trim() == player.NickName.Trim())
        //    {
        //        LeftRoom();
        //        return;
        //    }
        //}

        SubscribesEvent();
    }

    private void OnEnable()
    {
        ChangeBox(Box.Room);
        PlayerNumbering.OnPlayerNumberingChanged += UpdateChangeRoom;
    }

    private void OnDisable()
    {
        PlayerNumbering.OnPlayerNumberingChanged -= UpdateChangeRoom;
       
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    private void GameStart()
    {
       GameLoadingScene.Instance.GameStart();
    }

    /// <summary>
    /// 게임 준비 버튼
    /// </summary>
    private void GameReady()
    {
        if (PhotonNetwork.LocalPlayer.GetReady() == false)
        {
            OnReady();
        }
        else
        {
            OffReady();
        }
    }
    #region Ready
    /// <summary>
    /// 레디 하기
    /// </summary>
    private void OnReady()
    {
        PhotonNetwork.LocalPlayer.SetReady(true);
        _roomReadyButtonText.SetText("준비 완료".GetText());
    }
    /// <summary>
    /// 레디 안하기
    /// </summary>
    private void OffReady()
    {
        PhotonNetwork.LocalPlayer.SetReady(false);
        _roomReadyButtonText.SetText("준비".GetText());
    }
    #endregion

    /// <summary>
    /// 플레이어 변화에 따른 룸 업데이트
    /// </summary>
    private void UpdateChangeRoom()
    {
        UpdatePlayerCount();
        SetStartAndReadyButton();
    }
    /// <summary>
    /// 플레이어 카운트 업데이트
    /// </summary>
    private void UpdatePlayerCount()
    {
        if (_roomPlayerCountText == null)
        {
            Debug.LogError(_roomPlayerCountText);
        }
        else if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogError(PhotonNetwork.CurrentRoom);
        }
        _roomPlayerCountText.SetText($"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}".GetText());
    }

    /// <summary>
    /// 시작버튼, 레디버튼 활성화/ 비활성화
    /// </summary>
    private void SetStartAndReadyButton()
    {
        // 마스터클라이언트는 시작버튼 활성화 레디버튼 비활성화
        if (PhotonNetwork.IsMasterClient)
        {
            GetUI("RoomStartButtonBox").SetActive(true);
            GetUI("RoomReadyButtonBox").SetActive(false);
        }
        else
        {
            GetUI("RoomStartButtonBox").SetActive(false);
            GetUI("RoomReadyButtonBox").SetActive(true);
        }
    }



    /// <summary>
    ///  플레이어 프로퍼티 변경에 따른 업데이트
    /// </summary>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    private void UpdatePlayerProperty(Player arg0, ExitGames.Client.Photon.Hashtable arg1)
    {
        CheckAllReady();
    }

    /// <summary>
    /// 모두 레디 했는지 체크
    /// </summary>
    private void CheckAllReady()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        GetUI("RoomStartButton").SetActive(false);
        // 플레이어 수가 최대 플레이어 수보다 적을때 시작 불가
        if (PhotonNetwork.PlayerList.Length < PhotonNetwork.CurrentRoom.MaxPlayers)
            return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient == true)
                continue;
            if (player.GetReady() == false)
            {
                GetUI("RoomStartButton").SetActive(false);
                return;
            }

        }
        GetUI("RoomStartButton").SetActive(true);


    }
    /// <summary>
    /// 방 코드 복사
    /// </summary>
    private void CopyRoomCode()
    {
        _roomCodeText.text.CopyText();
    }
    /// <summary>
    /// 방코드 숨기기/ 보이기
    /// </summary>
    private void ToggleActiveRoomCode()
    {
        if (_roomCodeText.contentType == TMP_InputField.ContentType.Password)
        {
            // 보이기
            _roomCodeText.contentType = TMP_InputField.ContentType.Standard;
            _roomCodeActiveText.SetText(HIDETEXT);
        }
        else
        {
            // 숨기기
            _roomCodeText.contentType = TMP_InputField.ContentType.Password;
            _roomCodeActiveText.SetText(SHOWTEXT);
        }
        StartCoroutine(ToggleActiveRoomCodeRoutine());
    }

    IEnumerator ToggleActiveRoomCodeRoutine()
    {
        string temp = _roomCodeText.text;
        _roomCodeText.text = string.Empty;
        yield return null;
        _roomCodeText.text = temp;
    }

    /// <summary>
    /// 마스터 클라이언트 바뀐것에 대한 콜백
    /// </summary>
    /// <param name="arg0"></param>
    private void UpdateMasterClientSwitch(Player arg0)
    {
        SetStartAndReadyButton();
        // TODO : 방장 바뀌었을 때 기능 추가
    }

    /// <summary>
    /// 방 떠나기
    /// </summary>
    private void LeftRoom()
    {
        LoadingBox.StartLoading();
        PhotonNetwork.LeaveRoom();
    }

    #region 패널 조작

    /// <summary>
    /// UI 박스 변경
    /// </summary>
    private void ChangeBox(Box box)
    {
        LoadingBox.StopLoading();

        for (int i = 0; i < _boxs.Length; i++)
        {
            if (_boxs[i] == null)
                return;

            if (i == (int)box) // 바꾸고자 하는 박스만 활성화
            {
                _boxs[i].SetActive(true);
                ClearBox(box); // 초기화 작업도 진행
            }
            else
            {
                _boxs[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// UI 박스 초기화 작업
    /// </summary>
    /// <param name="box"></param>
    private void ClearBox(Box box)
    {
        switch (box)
        {
            case Box.Room:
                ClearRoomBox();
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 룸 초기화
    /// </summary>
    private void ClearRoomBox()
    {
        if (PhotonNetwork.InRoom == false) return;

        if (PhotonNetwork.CurrentRoom.GetPrivacy() == true) // 방이 프라이버시 모드인 경우
        {
            PhotonNetwork.LocalPlayer.NickName = $"새 {PhotonNetwork.LocalPlayer.ActorNumber}"; // 닉네임을 새 N 으로 변경
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = BackendManager.User.NickName; // 닉네임을 저장된 유저닉네임으로 변경
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient == true)
            {
                // 누구의 방 텍스트 설정
                _roomTitleText.SetText($"{player.NickName}의 방".GetText());
                break;
            }
        }


        // 방 코드 설정
        _roomCodeText.text = $"{PhotonNetwork.CurrentRoom.Name}";
        _roomCodeText.contentType = TMP_InputField.ContentType.Standard;
        _roomCodeActiveText.text = HIDETEXT;

        // 플레이어 초기 레디 설정
        OffReady();
    }


    #endregion

    // 초기 설정
    private void Init()
    {
        _boxs[(int)Box.Room] = GetUI("RoomBox");
    }

    // 이벤트 구독
    private void SubscribesEvent()
    {
        LobbyScene.Instance.OnMasterClientSwitchedEvent += UpdateMasterClientSwitch;
        LobbyScene.Instance.OnPlayerPropertiesUpdateEvent += UpdatePlayerProperty;

        GetUI<Button>("RoomLeftButton").onClick.AddListener(LeftRoom);
        GetUI<Button>("RoomLeftButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonOff));

        GetUI<Button>("RoomCodeActiveButton").onClick.AddListener(ToggleActiveRoomCode);
        GetUI<Button>("RoomCodeActiveButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("RoomCopyButton").onClick.AddListener(CopyRoomCode);
        GetUI<Button>("RoomCopyButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("RoomStartButton").onClick.AddListener(GameStart);
        GetUI<Button>("RoomStartButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("RoomReadyButton").onClick.AddListener(GameReady);
        GetUI<Button>("RoomReadyButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("SettingButton").onClick.AddListener(() => OptionPanel.SetActiveOption(true));
        GetUI<Button>("SettingButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

    }




}
