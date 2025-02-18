using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainCreateRoomBox : MainBox
{
    [SerializeField] int _minPlayer = 5;
    [SerializeField] int _maxPlayer = 12;

    private TMP_InputField _createNickNameInput => GetUI<TMP_InputField>("CreateNickNameInput");
    private TMP_Text _createPlayerCountText => GetUI<TMP_Text>("CreatePlayerCountText");
    private Slider _createPlayerCountSlider => GetUI<Slider>("CreatePlayerCountSlider");
    private Slider _createRoomOpenSlider => GetUI<Slider>("CreateRoomOpenSlider");
    private TMP_Text _createRoomOpenText => GetUI<TMP_Text>("CreateRoomOpenText");
    private GameObject _createPrivacyCheck => GetUI("CreatePrivacyCheck");
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
        ClearCreateRoomBox();
    }
    /// <summary>
    /// 방 생성
    /// </summary>
    private void CreateRoom()
    {
        string nickName = _createNickNameInput.text;
        if (nickName != string.Empty) // 닉네임 변경 시
        {
            nickName.ChangeNickName();
        }

        string roomCode = Util.GetRandomRoomCode(6); // 랜덤 방코드 획득
        int maxPlayer = (int)_createPlayerCountSlider.value; // 최대 인원 획득
        bool isVisible = (int)_createRoomOpenSlider.value == 0 ? true : false; // 공개 여부 획득

        // 방 옵션 세팅
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayer;
        options.IsVisible = isVisible;
        options.SetPrivacy(_createPrivacyCheck.activeSelf);

        LoadingBox.StartLoading();
        PhotonNetwork.CreateRoom(roomCode, options);
    }

    /// <summary>
    /// 플레이어 카운트 업데이트
    /// </summary>
    private void UpdatePlayerCount(float value)
    {
        _createPlayerCountText.SetText(value.GetText());
    }

    /// <summary>
    /// 공개방 비공개방 업데이트
    /// </summary>
    private void UpdateIsVisible(float value)
    {
        if (value == 1f) // 슬라이더 값 1은 비공개, 0은 공개
        {
            _createRoomOpenText.SetText("비공개".GetText());
        }
        else
        {
            _createRoomOpenText.SetText("공개".GetText());
        }
    }

    /// <summary>
    /// 프라이버시 모드
    /// </summary>
    private void UpdatePrivacyMode()
    {
        if (_createPrivacyCheck.activeSelf == false)
        {
            // 프라이버시 모드 활성화
            _createPrivacyCheck.SetActive(true);
        }
        else
        {
            // 비활성화
            _createPrivacyCheck.SetActive(false);
        }
    }
    /// <summary>
    /// 방 생성 화면 초기화
    /// </summary>
    private void ClearCreateRoomBox()
    {
        _createNickNameInput.text = string.Empty;
        _createPlayerCountSlider.value = (int)((_createPlayerCountSlider.maxValue + _createPlayerCountSlider.minValue) / 2); // 절반 수치만큼
        _createRoomOpenSlider.value = 1f; // 기본 비공개 방
        UpdateIsVisible(1f);
        _createPrivacyCheck.SetActive(false);
    }
    private void Init()
    {
        // TODO : 추후 게임매니저 같은곳에서 최대최소 인원 연동해서 가져와야할 필요가 있음
#if UNITY_EDITOR
        _createPlayerCountSlider.minValue = 1;
#else
        _createPlayerCountSlider.minValue = _minPlayer;
#endif
        _createPlayerCountSlider.maxValue = _maxPlayer;

    }
    private void SubscribesEvent()
    {
        _createPlayerCountSlider.onValueChanged.AddListener(UpdatePlayerCount);
        _createRoomOpenSlider.onValueChanged.AddListener(UpdateIsVisible);
        GetUI<Button>("CreateBackButton").onClick.AddListener(() => Panel.ChangeBox(MainPanel.Box.Join));
        GetUI<Button>("CreateBackButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonOff));

        GetUI<Button>("CreateRoomButton").onClick.AddListener(CreateRoom);
        GetUI<Button>("CreateRoomButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("CreatePrivacyButton").onClick.AddListener(UpdatePrivacyMode);
        GetUI<Button>("CreatePrivacyButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
    }
}
