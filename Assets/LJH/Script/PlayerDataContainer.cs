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
        // �迭  �ʱ�ȭ
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
    /// �÷��̾� ������ ����
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
    /// �÷��̾� �����͸� �ٽ� �ʱ� ��������
    /// </summary>
    public void ClearPlayerData()
    {
        photonView.RPC(nameof(RPCClearPlayerData),RpcTarget.All);
    }


    /// <summary>
    /// ���� �÷��̾� ������ ����
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
    /// ���� �÷��̾� ����
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
    /// �÷��̾� ������ ��� �� ����
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
    /// �÷��̾� ������ ��������
    /// </summary>
    /// <param name="playerNumber"></param>
    /// <returns></returns>
    public PlayerData GetPlayerData(int playerNumber)
    {
        return playerDataArray[playerNumber];
    }
    /// <summary>
    /// �÷��̾� ���� ��������
    /// </summary>
    /// <param name="playerNumber"></param>
    /// <returns></returns>
    public PlayerType GetPlayerJob(int playerNumber)
    {
        return playerDataArray[playerNumber].Type;
    }
    /// <summary>
    /// ������ �÷��̾�� ���� ���� ����
    /// </summary>
    public void RandomSetjob()
    {
        photonView.RPC(nameof(RpcRandomSetjob), RpcTarget.MasterClient);

    }

    /// <summary>
    /// �÷��̾� ��� ó�� ����
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
        //        // None�� �ƴ� �ε����� �߰�
        //        count++;
        //    }
        //}

        // 5��� ���� �Ѹ�, 5�� ��� �ʰ����� ex) 8�� 2��
        count = (PhotonNetwork.CurrentRoom.MaxPlayers % 7) != 0 ? PhotonNetwork.CurrentRoom.MaxPlayers / 7 + 1 : PhotonNetwork.CurrentRoom.MaxPlayers / 7;
        for (int i = 0; i < count; i++)
        {
            int x;
            // ���� �ε��� ���� ����
            while (true)
            {
                x = Random.Range(0, PhotonNetwork.CurrentRoom.MaxPlayers);

                // ���� ������ ����� �������ٸ� ������ �ٲ�
                // �������ٸ� �ٽ� ����
                if (playerDataArray[x].Type == PlayerType.Goose)
                    break;
            }
            // ������ �ε����� ��� �÷��̾�� ����ȭ
            photonView.RPC(nameof(RpcSetDuckPlayer), RpcTarget.All, x);
        }

        // ���� ���� UI ǥ��
        photonView.RPC(nameof(RpcShowGameStartUI), RpcTarget.All);
    }
    /// <summary>
    /// ���� �÷��̾� ����ȭ
    /// </summary>
    [PunRPC]
    private void RpcSetDuckPlayer(int index)
    {
        // �ش� �ε����� ��ΰ� ���� �÷��̾��� ����
        playerDataArray[index].Type = PlayerType.Duck;
    }
    /// <summary>
    /// ���� ���� UI ����
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
                // �÷��̾� ���ɻ��� ����
                playerData.IsGhost = false;
                playerData.Type = PlayerType.Goose;
            }
        }
    }
}
 