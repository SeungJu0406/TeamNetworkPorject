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

    #region private �ʵ�
    private const string SHOWTEXT = "���̱�";
    private const string HIDETEXT = "�����";
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
    /// ���� ����
    /// </summary>
    private void GameStart()
    {
       GameLoadingScene.Instance.GameStart();
    }

    /// <summary>
    /// ���� �غ� ��ư
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
    /// ���� �ϱ�
    /// </summary>
    private void OnReady()
    {
        PhotonNetwork.LocalPlayer.SetReady(true);
        _roomReadyButtonText.SetText("�غ� �Ϸ�".GetText());
    }
    /// <summary>
    /// ���� ���ϱ�
    /// </summary>
    private void OffReady()
    {
        PhotonNetwork.LocalPlayer.SetReady(false);
        _roomReadyButtonText.SetText("�غ�".GetText());
    }
    #endregion

    /// <summary>
    /// �÷��̾� ��ȭ�� ���� �� ������Ʈ
    /// </summary>
    private void UpdateChangeRoom()
    {
        UpdatePlayerCount();
        SetStartAndReadyButton();
    }
    /// <summary>
    /// �÷��̾� ī��Ʈ ������Ʈ
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
    /// ���۹�ư, �����ư Ȱ��ȭ/ ��Ȱ��ȭ
    /// </summary>
    private void SetStartAndReadyButton()
    {
        // ������Ŭ���̾�Ʈ�� ���۹�ư Ȱ��ȭ �����ư ��Ȱ��ȭ
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
    ///  �÷��̾� ������Ƽ ���濡 ���� ������Ʈ
    /// </summary>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    private void UpdatePlayerProperty(Player arg0, ExitGames.Client.Photon.Hashtable arg1)
    {
        CheckAllReady();
    }

    /// <summary>
    /// ��� ���� �ߴ��� üũ
    /// </summary>
    private void CheckAllReady()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        GetUI("RoomStartButton").SetActive(false);
        // �÷��̾� ���� �ִ� �÷��̾� ������ ������ ���� �Ұ�
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
    /// �� �ڵ� ����
    /// </summary>
    private void CopyRoomCode()
    {
        _roomCodeText.text.CopyText();
    }
    /// <summary>
    /// ���ڵ� �����/ ���̱�
    /// </summary>
    private void ToggleActiveRoomCode()
    {
        if (_roomCodeText.contentType == TMP_InputField.ContentType.Password)
        {
            // ���̱�
            _roomCodeText.contentType = TMP_InputField.ContentType.Standard;
            _roomCodeActiveText.SetText(HIDETEXT);
        }
        else
        {
            // �����
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
    /// ������ Ŭ���̾�Ʈ �ٲ�Ϳ� ���� �ݹ�
    /// </summary>
    /// <param name="arg0"></param>
    private void UpdateMasterClientSwitch(Player arg0)
    {
        SetStartAndReadyButton();
        // TODO : ���� �ٲ���� �� ��� �߰�
    }

    /// <summary>
    /// �� ������
    /// </summary>
    private void LeftRoom()
    {
        LoadingBox.StartLoading();
        PhotonNetwork.LeaveRoom();
    }

    #region �г� ����

    /// <summary>
    /// UI �ڽ� ����
    /// </summary>
    private void ChangeBox(Box box)
    {
        LoadingBox.StopLoading();

        for (int i = 0; i < _boxs.Length; i++)
        {
            if (_boxs[i] == null)
                return;

            if (i == (int)box) // �ٲٰ��� �ϴ� �ڽ��� Ȱ��ȭ
            {
                _boxs[i].SetActive(true);
                ClearBox(box); // �ʱ�ȭ �۾��� ����
            }
            else
            {
                _boxs[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// UI �ڽ� �ʱ�ȭ �۾�
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
    /// �� �ʱ�ȭ
    /// </summary>
    private void ClearRoomBox()
    {
        if (PhotonNetwork.InRoom == false) return;

        if (PhotonNetwork.CurrentRoom.GetPrivacy() == true) // ���� �����̹��� ����� ���
        {
            PhotonNetwork.LocalPlayer.NickName = $"�� {PhotonNetwork.LocalPlayer.ActorNumber}"; // �г����� �� N ���� ����
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = BackendManager.User.NickName; // �г����� ����� �����г������� ����
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient == true)
            {
                // ������ �� �ؽ�Ʈ ����
                _roomTitleText.SetText($"{player.NickName}�� ��".GetText());
                break;
            }
        }


        // �� �ڵ� ����
        _roomCodeText.text = $"{PhotonNetwork.CurrentRoom.Name}";
        _roomCodeText.contentType = TMP_InputField.ContentType.Standard;
        _roomCodeActiveText.text = HIDETEXT;

        // �÷��̾� �ʱ� ���� ����
        OffReady();
    }


    #endregion

    // �ʱ� ����
    private void Init()
    {
        _boxs[(int)Box.Room] = GetUI("RoomBox");
    }

    // �̺�Ʈ ����
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
