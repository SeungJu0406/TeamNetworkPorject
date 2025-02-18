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
    /// ��Ʈ ����
    /// </summary>
    public void Enter(ActorType actorType)
    {
        // �����ִ� ��Ʈ �� ��ŭ ����
        foreach (Vent vent in MoveableVents)
        {
            GameObject arrow = Instantiate(_dirArrowPrefab, transform.position, transform.rotation);

            // ȭ��ǥ �ٸ� ��Ʈ �ٶ󺸱�
            //Vector2 newPos = vent.transform.position - arrow.transform.position;
            //float rotZ = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;


            // ȭ��ǥ �ٸ� ��Ʈ �ٶ󺸱�
            Vector2 newPos = vent.transform.position - arrow.transform.position;

            // ���� ���� (1, 0)
            Vector2 referenceVector = new Vector2(1, 0);

            // ���� ���
            float dotProduct = Vector2.Dot(newPos.normalized, referenceVector);
            float rotZ = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

            // y��ǥ�� ������ �� ������ ����
            if (newPos.y < 0)
            {
                rotZ = 360 - rotZ;
            }

            arrow.transform.rotation = Quaternion.Euler(0, 0, rotZ);
            arrow.transform.SetParent(_canvas);
            arrow.transform.position += arrow.transform.right * 2 ;

            // ��ư �̺�Ʈ ���
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
    /// ��Ʈ ����
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
    /// ��Ʈ ���� �̺�Ʈ ȣ��
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
