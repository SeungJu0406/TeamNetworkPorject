using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameLoadingScene : MonoBehaviourPun
{
    // 로비씬에 넣어놓고 로비에 있는 정보를 가지고 게임으로 들어가기 
    // 씬에 입장하면 랜덤 스폰기능 
    // 투표씬 전환
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
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject); // 기존 인스턴스가 있으면 새로 생성된 객체를 제거
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
        // 게임 시작 전 플레이어 오브젝트 비우기
        photonView.RPC(nameof(DestroyMyPlayer), RpcTarget.All);
        yield return 2f.GetDelay();
        // 게임씬으로 씬전환
        SceneChanger.LoadLevel(1);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        yield return 2f.GetDelay();

        RandomSpawner(); // 스폰 지정 및 소환 
        yield return null;

        PlayerDataContainer.Instance.RandomSetjob(); // 랜덤 직업 설정 

        photonView.RPC(nameof(RpcSyncPlayerData), RpcTarget.AllBuffered);
        // 게임 승패 판별 시작
        photonView.RPC(nameof(StartJudgeGameOver), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void StartJudgeGameOver()
    {
        // 로딩 종료
        LoadingBox.StopLoading();

        StartCoroutine(GameOverRoutine());
    }
    /// <summary>
    /// 게임 오버 조건 판별 코루틴
    /// </summary>
    IEnumerator GameOverRoutine()
    {
        if (IsTest)
            yield break;

        // 로비 씬에서는 판별 금지
        while (LobbyScene.Instance != null)
        {
            yield return null;
        }
        // 게임 시작 전 약간의 여유 타임
        yield return 3f.GetDelay();

        while (true)
        {
            // 킬로 인한 게임 종료시 코루틴 끊기
            if (GameOverKill())
                yield break;

            // 미션승리로 인한 게임 종료시 코루틴 끊기
            if (GameOverMission())
                yield break;
            yield return null;
        }
    }


    // 게임 종료 조건 
    // 오리승리 : 거위 모두 사망
    // 거위승리 : 오리 모두 사망 , 미션 게이지 달성
    // => 투표 종료 , 플레이어 사망시마다 조건 확인    미션할때마다 게이지 확인 
    // 확인해서 조건 맞으면 게임 종료하기
    // 게임 종료 조건이 되면 모두 결과창 보여주고 다시 같이 로비로
    // 조건 체크를 방장(PhotonNetwork.MasterClient)만 해야하나? 아님 씬 이동이 rpc니 상관없나 
    private int GooseNotDead = 0; // 생존한 오리 , 거위
    private int DuckNotDead = 0;
    public bool GameOverKill() // 투표종료 및 살인시마다 호출 
    {

        PlayerDataContainer.Instance.SetPlayerTypeCounts();
        GooseNotDead = PlayerDataContainer.Instance.GooseCount;
        DuckNotDead = PlayerDataContainer.Instance.DuckCount;

        if (GooseNotDead <= DuckNotDead)// 거위의 수가 오리보다 적으면 오리 승리 , 투표가도 못이기니까   or 남은 거위가 없으면
        {
            // 오리승리로 게임 결과 표시 후 로비로 이동
            GameUI.ShowGameOver(true, PlayerType.Duck);
            isOnGame = false;
            return true;
        }
        else if (DuckNotDead == 0)  // 오리가 다 죽으면  거위 승리 
        {
            // 거위승리로 게임 결과 표시 후 로비로 이동
            GameUI.ShowGameOver(true, PlayerType.Goose);
            isOnGame = false;
            return true;
        }

        return false;
    }
    public bool GameOverMission() // 미션완료시마다 호출 
    {

        if (GameManager.Instance._missionScoreSlider.value == 1f)
        {
            // 미션완료승리로 게임 결과 표시 후 로비로 이동 
            // 거위 승리
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
    /// 로비(씬)로 돌아가기
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
        player.GetComponent<PlayerController>().SettingColor(color.r, color.g, color.b);  // 일단 보류 색 보존이 안됨 
        player.GetComponent<PlayerController>().SetJobs();
        StartCoroutine(SetPlayerTypeCountRoutine());
    }
    IEnumerator SetPlayerTypeCountRoutine()
    {
        // 네트워크 상황이기 때문에 조금 딜레이를 준 후에 수치 계산을 해야할 것 같음
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
    /// 랜덤 리스폰 포인트 획득
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
