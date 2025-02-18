using GameUIs;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class VoteManager : MonoBehaviourPunCallbacks
{
    public static VoteManager Instance;

    private int[] _voteCounts = new int[12]; // 각 플레이어의(ActorNumber와 연결된 인덱스 번호)의 득표수를 배열로 저장
    public static int[] VoteCounts { get { return Instance._voteCounts; } }

    public static PlayerDataContainer PlayerDataContainer => PlayerDataContainer.Instance;
    public static PhotonView PhotonView { get { return Instance.photonView; } }

    [SerializeField] VoteSceneData _voteData;

    [SerializeField] VotePanel _votePanel;
    public static VotePanel VotePanel { get { return Instance._votePanel; } }

    [SerializeField] VoteScenePlayerData[] _playerData;

    [SerializeField] GameObject[] _voteSignImage; // 투표한 플레이어 표시 이미지 

    [SerializeField] Button[] _voteButtons;

    private void Awake()
    {
        InitSingleTon();
        _votePanel.gameObject.SetActive(true);
    }

    private void Start()
    {
        MoveRandomPoint();
    }

    public static void Vote(int index) // 플레이어 패널을 눌러 투표
    {
        Debug.LogWarning($"{index} 투표");

         if (PlayerDataContainer.GetPlayerData(PhotonNetwork.LocalPlayer.GetPlayerNumber()).IsGhost == true)
        return;

        PhotonView.RPC("VotePlayerRPC", RpcTarget.All, index, PhotonNetwork.LocalPlayer.GetPlayerNumber());
        VotePanel.DisableButton();

        // 투표한 본인 패널에 이미지 띄우기      
    }

    [PunRPC]
    public void VotePlayerRPC(int index, int votePlayer)
    {
        _voteCounts[index]++;
        _voteData.VoteCount++;
        _voteSignImage[votePlayer].SetActive(true);
        SoundManager.SFXPlay(SoundManager.Data.Vote);
        Debug.Log($"{index}번 플레이어 득표수 {_voteCounts[index]} ");
    }

    public void OnClickSkip()  // 스킵 버튼 누를 시
    {
        if (PlayerDataContainer.GetPlayerData(PhotonNetwork.LocalPlayer.GetPlayerNumber()).IsGhost == true)
            return;

        photonView.RPC("OnClickSkipRPC", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.GetPlayerNumber());
        _votePanel.DisableButton();
    }

    [PunRPC]
    public void OnClickSkipRPC(int votePlayer)
    {
        _voteData.SkipCount++;
        _voteData.VoteCount++;
        _voteSignImage[votePlayer].SetActive(true);
        SoundManager.SFXPlay(SoundManager.Data.Vote);
        Debug.Log($" 스킵 수 : {_voteData.SkipCount}");
    }

    // 투표 종료 후 집계 기능
    public void GetVoteResult()
    {
        // 최다 득표자 찾는 기능
        bool isKick = false;
        int top = -1;
        int top2 = -1;
        int playerIndex = -1;

        for (int i = 0; i < 12; i++)
        {
            if (_voteCounts[i] > top)
            {
                top = _voteCounts[i];
                playerIndex = i;

                isKick = true;
            }
            else if (_voteCounts[i] == top)
            {
                top2 = _voteCounts[i];
                Debug.Log("동점표로 없던 일~");
                isKick = false;
                continue;
            }
        }
        Debug.Log($"{_voteData.SkipCount}표 기권!");
        Debug.Log($"{playerIndex}번 플레이어 당선 {top}표 : 추방됩니다");


        PlayerData playerData = PlayerDataContainer.Instance.GetPlayerData(playerIndex);
        if (isKick == true)
        {
            StartCoroutine(ShowVoteResultRoutine(playerIndex ,playerData.PlayerColor, playerData.PlayerName, playerData.Type));
        }
        else
        {
            //TODO: 동점 시 아무도 안쫓겨나는 컷 씬
            StartCoroutine(ShowVoteSkipRoutine());
        }
    }

    /// <summary>
    /// 추방 컷씬 이후 다시 투표씬 닫기
    /// </summary>
    IEnumerator ShowVoteResultRoutine(int playerIndex ,Color playerColor, string name, PlayerType type)
    {
        yield return 3f.GetDelay();
        GameUI.ShowVoteKick(playerColor, name, type);

        yield return (GameUI.VoteResult._duration - 1f).GetDelay();

        if (playerIndex == PhotonNetwork.LocalPlayer.GetPlayerNumber())
        {
            PlayerController myController = GameLoadingScene.MyPlayer.GetComponent<PlayerController>();
            // 사망
            myController.Die();
        }

        yield return 1f.GetDelay();

        if (PhotonNetwork.IsMasterClient == true)
        {
            SceneChanger.UnLoadScene("VoteScene");
        }
    }
    IEnumerator ShowVoteSkipRoutine()
    {
        yield return 3f.GetDelay();
        GameUI.ShowVoteSkip();
        yield return 7f.GetDelay();

        if (PhotonNetwork.IsMasterClient == true)
        {
            SceneChanger.UnLoadScene("VoteScene");
        }
    }

    /// <summary>
    /// 플레이어 랜덤 포인트로 이동
    /// </summary>
    private void MoveRandomPoint()
    {
        GameLoadingScene.MovePlayerRandomPoint();
    }

    private void InitSingleTon()
    {
        if(Instance  == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
