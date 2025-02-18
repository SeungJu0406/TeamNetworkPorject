using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;
using Photon.Pun.UtilityScripts;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerController : MonoBehaviourPun
{
    [SerializeField] VoiceManager voiceManager;

    // 플레이어 타입 거위(시민) 오리(임포스터) 
    [SerializeField] public PlayerType playerType;
    [HideInInspector] public int PlayerNumber = -1;
    [SerializeField] float moveSpeed;  // 이동속도 
    [SerializeField] float Detectradius;  // 탐색 범위
    [SerializeField] LayerMask layerMask;
    [SerializeField] int KillCoolDown = 10;
    [SerializeField] public int _remainCoolDown = 0;
    public int RemainCoolDown { get { return _remainCoolDown; } set { _remainCoolDown = value; OnChangeRemainCoolDownEvent?.Invoke( _remainCoolDown); } }
    public event UnityAction<int> OnChangeRemainCoolDownEvent;


    [SerializeField] GameObject IdleBody;
    [SerializeField] GameObject Ghost;
    [SerializeField] GameObject Corpse;



    [SerializeField] SpriteRenderer body;
    [SerializeField] SpriteRenderer GhostRender;
    [SerializeField] SpriteRenderer CorpRender;

    [SerializeField] Animator eyeAnim;
    [SerializeField] Animator bodyAnim;
    [SerializeField] Animator feetAnim;
    
   
    private Vector3 privPos;
    private Vector3 privDir;
    [SerializeField] float threshold = 0.001f;
    private Color randomColor;
    bool isOnMove = false;
    public bool isGhost = false;
    private PlayerVentUsable _playerVentUsable;

    Coroutine coroutine;
    private void Awake()
    {
        _playerVentUsable = GetComponent<PlayerVentUsable>();
    }
    private void Start()
    {
        name = $"{photonView.Owner.NickName}_Player";

        // 랜덤으로 역할 지정하는 기능이 필요 (대기실 입장에는 필요없고 게임 입장시 필요)
        if (photonView.IsMine == false)  // 소유권자 구분
            return;



        // 랜덤 색 설정 , 추후에 색 지정 기능을 넣으면 랜덤 대신 지정 숫자를 보내면 될듯
        if (LobbyScene.Instance != null)
        {
            // 로비씬에서는 랜덤색
            randomColor = new Color(Random.value, Random.value, Random.value, 1f);
        }
        else
        {
            // 게임씬에서는 저장되있던 색
            randomColor = PlayerDataContainer.Instance.GetPlayerData(PhotonNetwork.LocalPlayer.GetPlayerNumber()).PlayerColor;
        }
        SettingColor(randomColor.r, randomColor.g, randomColor.b);

        StartCoroutine(SetPlayerDataRoutine());

        RemainCoolDown = 15;
        StartCoroutine(CoolDown());
    }
    /// <summary>
    /// 플레이어 데이터 세팅
    /// </summary>
    IEnumerator SetPlayerDataRoutine()
    {
        while (true)
        {
            int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
            if (playerNumber != -1)
            {
                photonView.RPC(nameof(RPCSetPlayerNumber), RpcTarget.All, playerNumber);
                PlayerDataContainer.Instance.SetPlayerData(playerNumber, PhotonNetwork.LocalPlayer.NickName, playerType, randomColor.r, randomColor.g, randomColor.b, false);
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 플레이어컨트롤러에 플레이어 넘버 RPC캐싱
    /// </summary>
    [PunRPC]
    private void RPCSetPlayerNumber(int playerNumber)
    {
        PlayerNumber = playerNumber;
    }

    private void Update()
    {
        if (photonView.IsMine == false)  // 소유권자 구분
            return;

        MoveCheck();

        if (_playerVentUsable.InVent == true)
            return;

        if (LobbyScene.Instance == null) // 로비씬이 아닐때 (게임중일때)
        {
            if (GameLoadingScene.IsOnGame == false) // 게임진행값이 false 가 아니라면 중지
                return;
        }
        if (VoteScene.Instance != null) // 투표씬에서 움직임 금지
            return;

        Move();       
        FindNearObject();
       
    }

    /// <summary>
    /// 어빌리티 팝업창 오픈 기능
    /// </summary>
   

    public void SettingColor(float r, float g , float b) 
    {
        photonView.RPC("RpcSetColors", RpcTarget.AllBuffered, r,g,b);
    }
    // r 신고 , e 상호작용 , space 살인 
    // 주변 오브젝트 탐색(미니게임 , 사보타지 , 시체 , 긴급회의 , 다른 플레이어) 탐색된 오브젝트에 따라 다른 행동이 가능하게
    // 신고가 되면 시체도 사라져야 함 
    private Collider2D privNearCol = null;
    private Color privColor;
    private void FindNearObject()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, Detectradius, layerMask);
        Collider2D nearCol = null;
        float minDistance = 1000f;
        foreach (Collider2D col in colliders)
        {

            if (col.transform.position != this.transform.position) // 자신 아니고 
            {

                if (col != null)
                {
                    float distance = Vector2.Distance(this.transform.position, col.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;  // 가장 가까운 물체 찾기
                        if (playerType == PlayerType.Goose)
                        {
                            if (col.gameObject.layer == 10)
                            {
                                nearCol = null;
                            }
                            else
                                nearCol = col;
                        }
                        else
                            nearCol = col;
                    }
                }



            }
        }

        if (isGhost == false)
        {
            ObjectHighLight(nearCol);// 선택된 오브젝트 강조 및 선택
            PlayerAction(nearCol);// 선택 오브젝트에 대한 행동 
        }

    }

    private void ObjectHighLight(Collider2D nearCol)
    {
        if (nearCol != privNearCol)
        {

            if (privNearCol != null)
            {
                SpriteRenderer prevRenderer = privNearCol.GetComponent<SpriteRenderer>();
                if (prevRenderer != null)
                    // prevRenderer.color = privColor;    
                    //if (playerType == PlayerType.Goose)
                    //{
                    //    prevRenderer.enabled = true;
                    //}
                    //else
                    prevRenderer.enabled = false;
            }


            if (nearCol != null)
            {
                SpriteRenderer renderer = nearCol.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    // privColor = renderer.color; // 현재 색상 저장
                    // renderer.color = highLightColor; // 하이라이트 색상
                    //if (playerType == PlayerType.Goose)
                    //{
                    //    renderer.enabled = false;
                    //}
                    //else
                    renderer.enabled = true;

                }
            }

            // 현재 가장 가까운 물체를 이전 물체로 저장
            privNearCol = nearCol;
        }
    }

    private void PlayerAction(Collider2D nearCol)
    {
        if (nearCol != null)
        {
            if (nearCol.gameObject.layer == 13) // 나중에 시체용 레이어 더해서 사용하기 
            {

                if (Input.GetKeyDown(KeyCode.R))
                {
                    if (GameManager.IsStartVote == true)
                        return;
                    //Debug.Log("신고함!");

                    // 투표 열리는 기능 추가 해야함 
                    StartCoroutine(StartVoteRoutine());

                    nearCol.gameObject.GetComponent<ReportingObject>().Reporting(); //신고시 시체 삭제, 씬 재진입이면 필요없을지도
                    int corpseID = nearCol.gameObject.GetComponent<PhotonView>().ViewID;

                    //GameFlowManager.Instance.ReportingOn();
                    // 신고 On, UI 띄우기 및 씬 전환
                    photonView.RPC(nameof(RPCReport),RpcTarget.All,corpseID);

                }
            }
            else if (nearCol.gameObject.layer == gameObject.layer)
            {
                coroutine = StartCoroutine(Kill(nearCol));
            }
            else if (nearCol.gameObject.layer == 8)  // 미션 
            {
                if (Input.GetKeyDown(KeyCode.E))
                {

                    nearCol.gameObject.GetComponent<ActiveMission>().GetMission(randomColor,playerType);
                }
            }
            else if (nearCol.gameObject.layer == 10) // 사보타지(임포스터만 가능 ) , 미션 함수 가져올 때 인수로 본인 컬러 넘겨줘야함 
            {
                if (playerType == PlayerType.Duck)
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {

                        nearCol.gameObject.GetComponent<ActiveMission>().GetMission(randomColor, playerType);
                    }
                }
            }
        }
        else
        {
            //Debug.Log("탐색중");
        }
    }

    private bool canKill = true;
    IEnumerator Kill(Collider2D col)
    {
        // 로비씬에서는 킬 금지
        if (LobbyScene.Instance != null)
            yield break;

        if (col.gameObject != this.gameObject)
        {
            if (playerType == PlayerType.Duck)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (canKill)
                    {
                        //Debug.Log("죽임!"); // 적 플레이어 처치
                        SoundManager.SFXPlay(SoundManager.Data.Kill);

                        col.gameObject.GetComponent<PlayerController>().Die();

                        // 쿨타임 시작
                        canKill = false;
                        RemainCoolDown = KillCoolDown;
                        StartCoroutine(CoolDown());
                    }
                    else
                    {
                        Debug.Log($"쿨타임 {RemainCoolDown:F1}초 남음");
                    }
                }
            }
        }
    }
    private IEnumerator CoolDown()
    {
        while (RemainCoolDown > 0)
        {
            RemainCoolDown--;
            yield return 1f.GetDelay();
        }
        RemainCoolDown = 0;
        canKill = true; // 쿨타임 종료
        Debug.Log("킬 가능!");
    }

    /// <summary>
    /// 투표씬 네트워크 중복 체크 금지 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator StartVoteRoutine()
    {
        GameManager.Instance.SetIsStartVote(true);
        while (true)
        {
            if (VoteScene.Instance != null)
            {
                GameManager.Instance.SetIsStartVote(false);
                yield break;
            }
            yield return null;
        }
    }

    [PunRPC]
    private void RPCReport(int corpseID)
    {
        StartCoroutine(ReportRoutine(corpseID));
    }

    IEnumerator ReportRoutine(int corpseID)
    {
        Color reporterColor = PlayerDataContainer.Instance.GetPlayerData(PlayerNumber).PlayerColor;
        PhotonView corpseView  = PhotonView.Find(corpseID);
        ReportingObject reportingObject = corpseView.GetComponent<ReportingObject>();

        GameUI.ShowReport(reporterColor, reportingObject.CorpseColor);
        yield return GameUI.Report.Duration.GetDelay();
        if (PhotonNetwork.IsMasterClient == true)
        {
            SceneChanger.LoadScene("VoteScene", LoadSceneMode.Additive);
        }
    }

    public void Die()
    {
        StartCoroutine(switchGhost());
    }
    IEnumerator switchGhost()
    {
        bool isGame = VoteScene.Instance == null ? true : false;

        photonView.RPC(nameof(RPCDie), RpcTarget.All, isGame);
        photonView.RPC("RpcChildActive", RpcTarget.All, "GooseIdel", false, isGame);
        photonView.RPC("RpcChildActive", RpcTarget.All, "Goosecorpse", true, isGame);
        yield return new WaitForSeconds(1f);
        photonView.RPC("RpcChildActive", RpcTarget.All, "GoosePolter", true, isGame);
        gameObject.layer = 9;    // ghost 레이어로 바꾸기 
        PlayerDataContainer.Instance.UpdatePlayerGhostList(PlayerNumber);      
    }


    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 moveDir = new Vector2(x, y).normalized;

        if (moveDir == Vector2.zero)
            return;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime);


        if (x < 0) // 왼쪽으로 이동 시
        {
            privDir = new Vector3(0.7f, 0.7f, 0.7f);
            transform.localScale = privDir;

        }
        else if (x > 0) // 오른쪽으로 이동 시
        {
            privDir = new Vector3(-0.7f, 0.7f, 0.7f);
            transform.localScale = privDir;

        }
        else  // 이전 방향을 유지하게 
        {
            transform.localScale = privDir;
        }
    }


    private void MoveCheck()
    {

        float distance = Vector3.Distance(transform.position, privPos);

        if (distance > threshold)
        {
            if (!isOnMove)
            {
                isOnMove = true;
                eyeAnim.SetBool("Running", true);
                bodyAnim.SetBool("Running", true);
                feetAnim.SetBool("Running", true);
                
                // 발소리 사운드 시작
                if(_playStepSound == null)
                   _playStepSound= StartCoroutine(PlayStepSound());

            }
        }
        else
        {
            if (isOnMove) // 상태가 변경된 경우에만 애니메이션 업데이트
            {
                isOnMove = false;
                eyeAnim.SetBool("Running", false);
                bodyAnim.SetBool("Running", false);
                feetAnim.SetBool("Running", false);
                
                // 발소리 사운드 종료
                if(_playStepSound != null)
                {
                    StopCoroutine(_playStepSound);
                    _playStepSound = null;
                }
            }
        }
        privPos = transform.position;
    }

    Coroutine _playStepSound;
    /// <summary>
    /// 발소리 루프 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayStepSound()
    {
        while (true) 
        {
            SoundManager.SFXPlay(SoundManager.Data.GooseStep);
            //yield return SoundManager.Data.GooseStep.length .GetDelay();
            yield return 0.748f.GetDelay();
        }
    }
    [PunRPC]
    private void RPCDie(bool isGame)
    {
        if (photonView.IsMine)
        {
            SoundManager.SFXPlay(isGame == true ? SoundManager.Data.Dead : null);
            voiceManager.MeDead();
        }
    }

    [PunRPC]
    private void RpcChildActive(string name, bool isActive, bool isGame)
    {

        if (name == "GooseIdel")
        {
            IdleBody.SetActive(isActive);
            gameObject.layer = 9;
        }
        else if (name == "Goosecorpse")
        {
            Corpse.SetActive(isGame? true : false);
            Corpse.transform.SetParent(null);
        }
        else if (name == "GoosePolter")
        {
            isGhost = true;
            Debug.Log($"액터넘버 = {PhotonNetwork.LocalPlayer.ActorNumber}");
            Ghost.SetActive(isActive);
            
        }
    }

    [PunRPC]

    private void RpcSetColors(float r, float g, float b)
    {
        Color color = new Color(r, g, b);
        body.color = color;
        GhostRender.color = color;
        CorpRender.color = color;
    }


    public void SetJobs()
    {
        Debug.Log("직업변경");
        photonView.RPC("RpcSetJobs", RpcTarget.All, PhotonNetwork.LocalPlayer.GetPlayerNumber());
       
    }
    [PunRPC]
    private void RpcSetJobs(int playerNumber) 
    {
        playerType = PlayerDataContainer.Instance.GetPlayerJob(playerNumber);

        
        // 내 플레이어만 해당 UI 실행
        if (photonView.IsMine == true)
        {
            GameUI.ShowPlayer(playerType);
        }
    }
}
