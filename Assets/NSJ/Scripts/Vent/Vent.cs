using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Vent;

public class Vent : MonoBehaviourPun
{
    public enum ActorType { Enter, Change}

    public Vent[] MoveableVents;

    public event UnityAction<Vent> OnChangeVentEvent;

    [SerializeField] private Transform _canvas;
    [SerializeField] private GameObject _dirArrowPrefab;

    private List<GameObject> _arrowList = new List<GameObject>();
    private Animator animator;
    private int animatorHash = Animator.StringToHash("Vent");


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    /// <summary>
    /// 벤트 입장
    /// </summary>
    public void Enter(ActorType actorType)
    {
        // 갈수있는 벤트 수 만큼 생성
        foreach (Vent vent in MoveableVents)
        {
            GameObject arrow = Instantiate(_dirArrowPrefab, transform.position, transform.rotation);

            // 화살표 다른 벤트 바라보기
            //Vector2 newPos = vent.transform.position - arrow.transform.position;
            //float rotZ = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;


            // 화살표 다른 벤트 바라보기
            Vector2 newPos = vent.transform.position - arrow.transform.position;

            // 기준 벡터 (1, 0)
            Vector2 referenceVector = new Vector2(1, 0);

            // 내적 계산
            float dotProduct = Vector2.Dot(newPos.normalized, referenceVector);
            float rotZ = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

            // y좌표가 음수일 때 각도를 조정
            if (newPos.y < 0)
            {
                rotZ = 360 - rotZ;
            }

            arrow.transform.rotation = Quaternion.Euler(0, 0, rotZ);
            arrow.transform.SetParent(_canvas);
            arrow.transform.position += arrow.transform.right * 2 ;

            // 버튼 이벤트 등록
            Button arrowButton = arrow.GetComponentInChildren<Button>();
            arrowButton.onClick.AddListener(() => ChangeVent(vent));

            _arrowList.Add(arrow);
           
        }

        //RPC
        if (actorType == ActorType.Enter)
        {
            photonView.RPC(nameof(RPCVent), RpcTarget.AllViaServer);
        }
    }

    /// <summary>
    /// 벤트 퇴장
    /// </summary>
    public void Exit(ActorType actorType)
    {
        foreach (GameObject arrow in _arrowList)
        {
            Destroy(arrow.gameObject);
        }
        _arrowList.Clear();

        //RPC
        if (actorType == ActorType.Enter)
        {
            photonView.RPC(nameof(RPCVent), RpcTarget.AllViaServer);
        }
    }

    /// <summary>
    /// 벤트 변경 이벤트 호출
    /// </summary>
    /// <param name="arrow"></param>
    private void ChangeVent(Vent vent)
    {
        //Vent nextVent= _arrowList[arrow];
        OnChangeVentEvent?.Invoke(vent);
    }

    [PunRPC]
    public void RPCVent()
    {
        animator.Play(animatorHash);
    }
}
