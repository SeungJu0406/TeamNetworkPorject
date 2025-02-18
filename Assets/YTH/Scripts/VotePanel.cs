using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VotePanel : MonoBehaviourPunCallbacks
{
    public const string RoomName = "playerpaneltest";
    PlayerDataContainer _playerDataContainer => PlayerDataContainer.Instance;

    [SerializeField] VoteManager _voteManager;

    [SerializeField] VoteSceneData _voteData;

    [SerializeField] VoteScenePlayerData[] _playerData;

    [SerializeField] GameObject[] _panelList; // PlayerActorNumber 인덱스로 미리 생성해둔 패널들을 리스트에 담아 연결

    #region UI
    [Header("UI")]

    [SerializeField] GameObject[] _SkipAnonymImage; // 스킵 수 만큼 익명 이미지 생성

    [SerializeField] VotePlayerPanel[] _panelAnonymImage; // 2차원 배열 이용하여 구현 계획

    [SerializeField] Button[] _voteButtons; // 투표하기 위한 버튼들

    [SerializeField] GameObject[] _deadSignImage; // 죽은 상태 표시 이미지

    [SerializeField] GameObject[] _voteSignImage; // 죽은 상태 표시 이미지

    [SerializeField] TMP_Text[] _nickNameText; // 각 플레이어 닉네임 텍스트

    [SerializeField] Image[] _playerColor;

    [SerializeField] TMP_Text _stateText;

    [SerializeField] public Button _skipButton;

    [SerializeField] Slider _reportTimeCountSlider; // 신고자만 말할 수 있는 시간 카운트

    [SerializeField] Slider _voteTimeCountSlider; // 투표 가능 시간 카운트
    #endregion

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _reportTimeCountSlider.maxValue = _voteData.ReportTimeCount;
        _voteTimeCountSlider.maxValue = _voteData.VoteTimeCount;

        _reportTimeCountSlider.value = _voteData.ReportTimeCount;
        _voteTimeCountSlider.value = _voteData.VoteTimeCount;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            SpawnPlayerPanel();
        }

        _voteData.OnAllVotePlayerEvent += CheckAllPlayerVote;
    }

    public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        options.IsVisible = false; // 비공개 방

        PhotonNetwork.JoinOrCreateRoom(RoomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayerPanel();
    }

    private void Update()
    {
        Debug.Log(PhotonNetwork.NetworkClientState);
        CountTime();
    }

    // 각 플레이어 패널을 세팅하는 함수

    private void SpawnPlayerPanel()
    {
        for (int i = 0; i < _panelList.Length; i++)
        {
            PlayerData playerData = PlayerDataContainer.Instance.GetPlayerData(i);
            if (playerData.IsNone == true)
                continue;

            int index = i;         
            _panelList[index].SetActive(true);
            _panelList[index].GetComponent<VoteScenePlayerData>().VoteButton.onClick.AddListener(() => { VoteManager.Vote(index); });

            _nickNameText[index].SetText(playerData.PlayerName);
            _voteSignImage[index].SetActive(false);
            _playerColor[index].color = playerData.PlayerColor;


            // 플레이어 사망일때
            if (playerData.IsGhost)
            {
                _deadSignImage[index].SetActive(true);
                _playerData[index].DidVote = true;
            }
            else
            {
                _deadSignImage[index].SetActive(false);
                _playerData[index].DidVote = false;
            }

            // 플레이어가 오리면 같은 팀 오리끼리는 빨간색으로 보임
            if (PlayerDataContainer.Instance.GetPlayerJob(PhotonNetwork.LocalPlayer.GetPlayerNumber()) == PlayerType.Duck)
            {
                if (playerData.Type == PlayerType.Goose)
                {
                    _nickNameText[index].color = Color.white;
                }
                else if (playerData.Type == PlayerType.Duck)
                {
                    _nickNameText[index].color = Color.red;
                }
            }
            else if (PlayerDataContainer.Instance.GetPlayerJob(PhotonNetwork.LocalPlayer.GetPlayerNumber()) == PlayerType.Goose)
            {
                _nickNameText[index].color = Color.white;
            }
        }
    }

    //투표 종료 후 스킵 수 만큼 익명 이미지 생성
    private void SpawnSkipAnonymImage()
    {
        for (int i = 0; i < _voteData.SkipCount; i++)
        {
            _SkipAnonymImage[i].SetActive(true);
        }
        //photonView.RPC("SpawnSkipAnonymImageRPC", RpcTarget.All, _voteData.SkipCount);
    }

    [PunRPC]
    public void SpawnSkipAnonymImageRPC(int index)
    {

    }

    //투표 종료 후 득표 수 만큼 플레이어 패널에 익명 이미지 생성
    private void SpawnAnonymImage()
    {

        for (int i = 0; i < 12; i++)
        {
            // 해당 패널 투표 수
            int voteCount = VoteManager.VoteCounts[i];

            VotePlayerPanel playerPanel = _panelAnonymImage[i];

            for (int j = 0; j < voteCount; j++)
            {
                playerPanel.PanelAnonymImages[j].SetActive(true);
            }
        }
        //photonView.RPC("SpawnAnonymImageRPC", RpcTarget.All, VoteManager.VoteCounts);
    }

    [PunRPC]
    public void SpawnAnonymImageRPC(int index)
    {

    }

    // 시간 측정 함수
    private void CountTime()
    {
        foreach (Button button in _voteButtons)
        {
            button.interactable = false;
            _skipButton.interactable = false;
        }


        _voteData.ReportTimeCount -= (float)Time.deltaTime; // Time.deltaTime 수정 필요 시 수정
        _reportTimeCountSlider.value = _voteData.ReportTimeCount;
        if (_voteData.ReportTimeCount <= 0) // 리포트 타임 종료 시 투표, 스킵 버튼 활성화
        {
            _stateText.text = "VOTE!";
            foreach (Button button in _voteButtons)
            {
                button.interactable = true;
                _skipButton.interactable = true;
            }
            _reportTimeCountSlider.gameObject.SetActive(false); // 추후 수정할 것
            _voteData.VoteTimeCount -= (float)Time.deltaTime;
            _voteTimeCountSlider.value = _voteData._voteTimeCount;
            if (_voteData.VoteTimeCount <= 0) // 투표 시간 종료 시 투표, 스킵 버튼 비활성화
            {
                DisableButton();
                SpawnAnonymImage();
                SpawnSkipAnonymImage();
                _voteManager.GetVoteResult();
            }
        }
    }

    // 투표 버튼 비활성화 함수
    public void DisableButton()
    {
        foreach (var button in _voteButtons)
        {
            button.enabled = false;
            Debug.Log("투표버튼 비활성화");
        }
        _skipButton.enabled = false;
    }

    IEnumerator SpawnPlayerPanelRoutine()
    {
        yield return 2f.GetDelay();
        SpawnPlayerPanel();
    }

    private void CheckAllPlayerVote()
    {
        _voteData.VoteTimeCount = 3f;
    }
}
