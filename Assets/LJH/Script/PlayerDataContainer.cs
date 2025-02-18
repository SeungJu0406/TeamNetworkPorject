using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerDataContainer : MonoBehaviourPun
{
    [SerializeField] public int GooseCount = 0;
    [SerializeField] public int DuckCount = 0;
    [SerializeField] public PlayerData[] playerDataArray;

    private int MaxPlayers = 15;
    public static PlayerDataContainer Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        // 배열  초기화
        playerDataArray = new PlayerData[MaxPlayers];
        for (int i = 0; i < MaxPlayers; i++)
        {
            playerDataArray[i] = new PlayerData("None", PlayerType.Goose, Color.white, true);
        }       
    }

    private void OnEnable()
    {
        SubscribesEventServerCallbacks();
    }

    private void OnDisable()
    {
        UnSubscribesEventServerCallbacks();
    }

    /// <summary>
    /// 플레이어 데이터 세팅
    /// </summary>
    public void SetPlayerData(int playerNumber, string playerName, PlayerType type, float Rcolor, float Gcolor, float Bcolor, bool isGhost)
    {

        StartCoroutine(SetPlayerDataRoutine(playerNumber, playerName, type, Rcolor, Gcolor, Bcolor, isGhost));

    }
    IEnumerator SetPlayerDataRoutine(int playerNumber, string playerName, PlayerType type, float Rcolor, float Gcolor, float Bcolor, bool isGhost)
    {
        yield return 0.1f.GetDelay();

        photonView.RPC("RpcSetPlayerData", RpcTarget.AllBuffered, playerNumber, playerName, type, Rcolor, Gcolor, Bcolor, isGhost);
    }

    /// <summary>
    /// 플레이어 데이터를 다시 초기 세팅으로
    /// </summary>
    public void ClearPlayerData()
    {
        photonView.RPC(nameof(RPCClearPlayerData),RpcTarget.All);
    }


    /// <summary>
    /// 입장 플레이어 데이터 세팅
    /// </summary>
    private void SetEnterPlayerData(Player newPlayer)
    {
        StartCoroutine(SetEnterPlayerDataRoutine(newPlayer));
    }

    IEnumerator SetEnterPlayerDataRoutine(Player newPlayer)
    {
        yield return 0.1f.GetDelay();

        int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
        PlayerData data = GetPlayerData(playerNumber);
        photonView.RPC(nameof(RpcSetEnterPlayerData), RpcTarget.AllBuffered,
           newPlayer,
           playerNumber,
           data.PlayerName,
           data.Type,
           data.PlayerColor.r,
           data.PlayerColor.g,
           data.PlayerColor.b,
           data.IsGhost);
    }

    /// <summary>
    /// 퇴장 플레이어 세팅
    /// </summary>
    private void SetExitPlayerData(Player exitPlayer)
    {
        StartCoroutine(SetExitPlayerDataRoutine(exitPlayer));
    }

    IEnumerator SetExitPlayerDataRoutine(Player exitPlayer)
    {
        yield return 0.1f.GetDelay();
        int playerNumber = exitPlayer.GetPlayerNumber();
        PlayerData data = GetPlayerData(playerNumber);
        photonView.RPC(nameof(RpcSetExitPlayerData), RpcTarget.AllBuffered, playerNumber, "None", PlayerType.Goose, Color.white.r, Color.white.g, Color.white.b, true);
    }
    /// <summary>
    /// 플레이어 직업별 사람 수 측정
    /// </summary>
    public void SetPlayerTypeCounts()
    {
        GooseCount = 0;
        DuckCount = 0;
        for (int i = 0; i <playerDataArray.Length; i++)
        {
            if (playerDataArray[i].IsNone == false && playerDataArray[i].IsGhost == false)
            {
                if (playerDataArray[i].Type == PlayerType.Goose)
                {
                    GooseCount++;
                }
                else
                {
                    DuckCount++;
                }

            }

        }

    }
    /// <summary>
    /// 플레이어 데이터 가져오기
    /// </summary>
    /// <param name="playerNumber"></param>
    /// <returns></returns>
    public PlayerData GetPlayerData(int playerNumber)
    {
        return playerDataArray[playerNumber];
    }
    /// <summary>
    /// 플레이어 직업 가져오기
    /// </summary>
    /// <param name="playerNumber"></param>
    /// <returns></returns>
    public PlayerType GetPlayerJob(int playerNumber)
    {
        return playerDataArray[playerNumber].Type;
    }
    /// <summary>
    /// 게임의 플레이어들 직업 랜덤 선정
    /// </summary>
    public void RandomSetjob()
    {
        photonView.RPC(nameof(RpcRandomSetjob), RpcTarget.MasterClient);

    }

    /// <summary>
    /// 플레이어 사망 처리 변경
    /// </summary>
    public void UpdatePlayerGhostList(int playerNumber)
    {
        photonView.RPC("RpcUpdatePlayerGhostList", RpcTarget.All, playerNumber);
    }

    [PunRPC]
    private void RpcUpdatePlayerGhostList(int playerNumber)
    {
        playerDataArray[playerNumber].IsGhost = true;
    }



    [PunRPC]
    private void RpcSetPlayerData(int playerNumber, string playerName, PlayerType type, float Rcolor, float Gcolor, float Bcolor, bool isGhost)
    {
        if (PhotonNetwork.LocalPlayer.GetPlayerNumber() == -1)
            return;

        int index = playerNumber;
        Color color = new Color(Rcolor, Gcolor, Bcolor, 255f);


        if (playerDataArray[index] == null)
        {
            playerDataArray[index] = new PlayerData(playerName, type, color, isGhost);
        }
        else
        {
            playerDataArray[index].IsNone = false;
            playerDataArray[index].PlayerName = playerName;
            //playerDataArray[ix].Type = type;
            playerDataArray[index].PlayerColor = color;
            playerDataArray[index].IsGhost = isGhost;
        }
    }

    [PunRPC]
    private void RpcSetEnterPlayerData(Player enterPlayer, int playerNumber, string playerName, PlayerType type, float Rcolor, float Gcolor, float Bcolor, bool isGhost)
    {
        if (enterPlayer != PhotonNetwork.LocalPlayer)
            return;

        int index = playerNumber;

        if (index == -1)
            return;

        Color color = new Color(Rcolor, Gcolor, Bcolor, 255f);

        if (playerDataArray[index] == null)
        {
            playerDataArray[index] = new PlayerData(playerName, type, color, isGhost);
        }
        else
        {
            playerDataArray[index].IsNone = false;
            playerDataArray[index].PlayerName = playerName;
            //playerDataArray[ix].Type = type;
            playerDataArray[index].PlayerColor = color;
            playerDataArray[index].IsGhost = isGhost;
        }
    }

    [PunRPC]
    private void RpcSetExitPlayerData(int playerNumber, string playerName, PlayerType type, float Rcolor, float Gcolor, float Bcolor, bool isGhost)
    {
        int index = playerNumber;

        if (index == -1)
            return;

        Color color = new Color(Rcolor, Gcolor, Bcolor, 255f);

        if (playerDataArray[index] == null)
        {
            playerDataArray[index] = new PlayerData(playerName, type, color, isGhost);
        }
        else
        {
            playerDataArray[index].IsNone = true;
            playerDataArray[index].PlayerName = playerName;
            playerDataArray[index].Type = type;
            playerDataArray[index].PlayerColor = color;
            playerDataArray[index].IsGhost = isGhost;
        }
    }

    [PunRPC]
    private void RpcRandomSetjob()
    {
        int count = 0;
        //for (int i = 0; i < playerDataArray.Length; i++)
        //{
        //    if (playerDataArray[i].IsNone == false)
        //    {
        //        // None이 아닌 인덱스만 추가
        //        count++;
        //    }
        //}

        // 5명당 오리 한명씩, 5의 배수 초과부터 ex) 8명 2명
        count = (PhotonNetwork.CurrentRoom.MaxPlayers % 7) != 0 ? PhotonNetwork.CurrentRoom.MaxPlayers / 7 + 1 : PhotonNetwork.CurrentRoom.MaxPlayers / 7;
        for (int i = 0; i < count; i++)
        {
            int x;
            // 랜덤 인덱스 오리 선정
            while (true)
            {
                x = Random.Range(0, PhotonNetwork.CurrentRoom.MaxPlayers);

                // 만약 선정된 대상이 거위였다면 오리로 바꿈
                // 오리였다면 다시 뽑음
                if (playerDataArray[x].Type == PlayerType.Goose)
                    break;
            }
            // 정해진 인덱스를 모든 플레이어와 동기화
            photonView.RPC(nameof(RpcSetDuckPlayer), RpcTarget.All, x);
        }

        // 직업 선정 UI 표시
        photonView.RPC(nameof(RpcShowGameStartUI), RpcTarget.All);
    }
    /// <summary>
    /// 오리 플레이어 동기화
    /// </summary>
    [PunRPC]
    private void RpcSetDuckPlayer(int index)
    {
        // 해당 인덱스를 모두가 오리 플레이어라고 저장
        playerDataArray[index].Type = PlayerType.Duck;
    }
    /// <summary>
    /// 게임 시작 UI 실행
    /// </summary>
    [PunRPC]
    private void RpcShowGameStartUI()
    {
        int index = PhotonNetwork.LocalPlayer.GetPlayerNumber();
        PlayerType type = playerDataArray[index].Type;
        Color color = playerDataArray[index].PlayerColor;
        GameUI.ShowGameStart(type, color);
        StartCoroutine(GameStartDelayRoutine());
    }

    IEnumerator GameStartDelayRoutine()
    {
        yield return GameUI.GameStart.Duration.GetDelay();
        GameLoadingScene.IsOnGame = true;
    }

 
    private void SubscribesEventServerCallbacks()
    {
        ServerCallback.Instance.OnPlayerEnteredRoomEvent += SetEnterPlayerData;
        ServerCallback.Instance.OnPlayerLeftRoomEvent += SetExitPlayerData;
    }
    private void UnSubscribesEventServerCallbacks()
    {
        ServerCallback.Instance.OnPlayerEnteredRoomEvent -= SetEnterPlayerData;
        ServerCallback.Instance.OnPlayerLeftRoomEvent -= SetExitPlayerData;
    }
    [PunRPC]
    private void RPCClearPlayerData()
    {
        foreach (PlayerData playerData in playerDataArray)
        {
            if (playerData.IsNone == false)
            {
                // 플레이어 유령상태 해제
                playerData.IsGhost = false;
                playerData.Type = PlayerType.Goose;
            }
        }
    }
}
 