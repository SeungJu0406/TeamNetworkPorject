using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;

[System.Serializable]
public class VoteSceneData : MonoBehaviourPun, IPunObservable
{
    private int _voteCount; // ��ǥ�� ��� ��
    public int VoteCount { get { return _voteCount; } set {
            _voteCount = value;     

            if (_voteCount >= _playerCount)
            {
                OnAllVotePlayerEvent?.Invoke();
            }
        } 
    }
    public event UnityAction OnAllVotePlayerEvent;

    [SerializeField] private int _skipCount; // ��ŵ�� ��� ��
    public int SkipCount { get { return _skipCount; } set { _skipCount = value; } }


    [SerializeField] private float _reportTimeCount; // �Ű��ڸ� ���� �� �ִ� �ð�
    public float ReportTimeCount { get { return _reportTimeCount; } set { _reportTimeCount = value; } }


    [SerializeField] public float _voteTimeCount; // ��ǥ ���� �ð�
    public float VoteTimeCount { get { return _voteTimeCount; } set { _voteTimeCount = value; } }

    private float _playerCount;

    private void Start()
    {
        foreach(PlayerData playerData in PlayerDataContainer.Instance.playerDataArray)
        {
            if (playerData.IsNone == true)
                continue;

            if(playerData.IsGhost == false)
            {
                _playerCount++;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(SkipCount);
            stream.SendNext(ReportTimeCount);
            stream.SendNext(VoteTimeCount);
        }
        else if (stream.IsReading)
        {
            SkipCount = (int)stream.ReceiveNext();
            ReportTimeCount = (float)stream.ReceiveNext();
            VoteTimeCount = (float)stream.ReceiveNext();
        }
    }
}
