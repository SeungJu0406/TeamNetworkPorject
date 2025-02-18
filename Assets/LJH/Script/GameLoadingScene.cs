using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameLoadingScene : MonoBehaviourPun
{
    // �κ���� �־���� �κ� �ִ� ������ ������ �������� ���� 
    // ���� �����ϸ� ���� ������� 
    // ��ǥ�� ��ȯ
    private Transform[] _spawnPoints;
    private static Transform[] s_spawnPoints { get { return Instance._spawnPoints; } set { Instance._spawnPoints = value; } }

    private GameObject player;
    public static GameObject MyPlayer { get { return Instance.player; } }

    private PlayerController _myController;
    public static PlayerController MyPlayerController { get { return Instance._myController; } }
    private Color color;

    private bool isOnGame = false;
    public static bool IsOnGame { get { return Instance.isOnGame; } set { Instance.isOnGame = value; } }

    public event UnityAction OnStartGameEvent;

    public static bool IsTest
    {
        get
        {
            return NSJ_Test.TestGame.Instance == null ? false : true;
        }
    }

    public static GameLoadingScene Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject); // ���� �ν��Ͻ��� ������ ���� ������ ��ü�� ����
        }
    }
    private void Start()
    {

    }

    public void GameStart()
    {
        StartCoroutine(GameStartDelaying());
    }

    IEnumerator GameStartDelaying()
    {
        // ���� ���� �� �÷��̾� ������Ʈ ����
        photonView.RPC(nameof(DestroyMyPlayer), RpcTarget.All);
        yield return 2f.GetDelay();
        // ���Ӿ����� ����ȯ
        SceneChanger.LoadLevel(1);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        yield return 2f.GetDelay();

        RandomSpawner(); // ���� ���� �� ��ȯ 
        yield return null;

        PlayerDataContainer.Instance.RandomSetjob(); // ���� ���� ���� 

        photonView.RPC(nameof(RpcSyncPlayerData), RpcTarget.AllBuffered);
        // ���� ���� �Ǻ� ����
        photonView.RPC(nameof(StartJudgeGameOver), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void StartJudgeGameOver()
    {
        // �ε� ����
        LoadingBox.StopLoading();

        StartCoroutine(GameOverRoutine());
    }
    /// <summary>
    /// ���� ���� ���� �Ǻ� �ڷ�ƾ
    /// </summary>
    IEnumerator GameOverRoutine()
    {
        if (IsTest)
            yield break;

        // �κ� �������� �Ǻ� ����
        while (LobbyScene.Instance != null)
        {
            yield return null;
        }
        // ���� ���� �� �ణ�� ���� Ÿ��
        yield return 3f.GetDelay();

        while (true)
        {
            // ų�� ���� ���� ����� �ڷ�ƾ ����
            if (GameOverKill())
                yield break;

            // �̼ǽ¸��� ���� ���� ����� �ڷ�ƾ ����
            if (GameOverMission())
                yield break;
            yield return null;
        }
    }


    // ���� ���� ���� 
    // �����¸� : ���� ��� ���
    // �����¸� : ���� ��� ��� , �̼� ������ �޼�
    // => ��ǥ ���� , �÷��̾� ����ø��� ���� Ȯ��    �̼��Ҷ����� ������ Ȯ�� 
    // Ȯ���ؼ� ���� ������ ���� �����ϱ�
    // ���� ���� ������ �Ǹ� ��� ���â �����ְ� �ٽ� ���� �κ��
    // ���� üũ�� ����(PhotonNetwork.MasterClient)�� �ؾ��ϳ�? �ƴ� �� �̵��� rpc�� ������� 
    private int GooseNotDead = 0; // ������ ���� , ����
    private int DuckNotDead = 0;
    public bool GameOverKill() // ��ǥ���� �� ���νø��� ȣ�� 
    {

        PlayerDataContainer.Instance.SetPlayerTypeCounts();
        GooseNotDead = PlayerDataContainer.Instance.GooseCount;
        DuckNotDead = PlayerDataContainer.Instance.DuckCount;

        if (GooseNotDead <= DuckNotDead)// ������ ���� �������� ������ ���� �¸� , ��ǥ���� ���̱�ϱ�   or ���� ������ ������
        {
            // �����¸��� ���� ��� ǥ�� �� �κ�� �̵�
            GameUI.ShowGameOver(true, PlayerType.Duck);
            isOnGame = false;
            return true;
        }
        else if (DuckNotDead == 0)  // ������ �� ������  ���� �¸� 
        {
            // �����¸��� ���� ��� ǥ�� �� �κ�� �̵�
            GameUI.ShowGameOver(true, PlayerType.Goose);
            isOnGame = false;
            return true;
        }

        return false;
    }
    public bool GameOverMission() // �̼ǿϷ�ø��� ȣ�� 
    {

        if (GameManager.Instance._missionScoreSlider.value == 1f)
        {
            // �̼ǿϷ�¸��� ���� ��� ǥ�� �� �κ�� �̵� 
            // ���� �¸�
            GameUI.ShowGameOver(true, PlayerType.Goose);
            isOnGame = false;
            return true;
        }

        if (GameManager.Instance.IsDuckWin == true)
        {
            GameUI.ShowGameOver(true, PlayerType.Duck);
            isOnGame = false;
            return true;
        }
        return false;

    }


    /// <summary>
    /// �κ�(��)�� ���ư���
    /// </summary>
    public static void BackLobby()
    {
        LoadingBox.StartLoading();

        SceneChanger.LoadLevel(0);
        PlayerDataContainer.Instance.ClearPlayerData();
        s_spawnPoints = null;
        PhotonNetwork.CurrentRoom.IsOpen = true;
    }

    public static void MovePlayerRandomPoint()
    {
        MyPlayer.transform.position = Instance.GetRandomSpawnPoint();
    }

    private void RandomSpawner()
    {
        photonView.RPC("RpcRandomSpawner", RpcTarget.All);
    }

    private void spawnPlayer(Vector3 Pos)
    {
        player = PhotonNetwork.Instantiate("LJH_Player", Pos, Quaternion.identity);
        _myController = player.GetComponent<PlayerController>();
        color = PlayerDataContainer.Instance.GetPlayerData(PhotonNetwork.LocalPlayer.GetPlayerNumber()).PlayerColor;
        GameObject panel = PhotonNetwork.Instantiate("NamePanel", Pos, Quaternion.identity);
        panel.GetComponent<UiFollowingPlayer>().setTarget(player);

    }

    [PunRPC]
    private void RpcRandomSpawner()
    {
        spawnPlayer(GetRandomSpawnPoint());
    }

    [PunRPC]
    private void RpcSyncPlayerData()
    {
        player.GetComponent<PlayerController>().SettingColor(color.r, color.g, color.b);  // �ϴ� ���� �� ������ �ȵ� 
        player.GetComponent<PlayerController>().SetJobs();
        StartCoroutine(SetPlayerTypeCountRoutine());
    }
    IEnumerator SetPlayerTypeCountRoutine()
    {
        // ��Ʈ��ũ ��Ȳ�̱� ������ ���� �����̸� �� �Ŀ� ��ġ ����� �ؾ��� �� ����
        yield return 0.5f.GetDelay();
        PlayerDataContainer.Instance.SetPlayerTypeCounts();
    }

    [PunRPC]
    private void DestroyMyPlayer()
    {
        LoadingBox.StartLoading();

        OnStartGameEvent?.Invoke();
    }

    /// <summary>
    /// ���� ������ ����Ʈ ȹ��
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomSpawnPoint()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Respawn");
        _spawnPoints = new Transform[obj.transform.childCount];
        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            _spawnPoints[i] = obj.transform.GetChild(i);
        }

        int x = Random.Range(0, _spawnPoints.Length);

        return _spawnPoints[x].position;
    }
}
