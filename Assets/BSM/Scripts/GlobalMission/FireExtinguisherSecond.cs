using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisherSecond : MonoBehaviour
{

    [SerializeField] private GameObject _powder;
    [SerializeField] private GameObject _powderPoint;

    [SerializeField] private GameObject _fire1;
    [SerializeField] private GameObject _fire2;
    [SerializeField] private GameObject _fire3;


    private (float, float) _fire1PosX = (-200f, -60f);
    private (float, float) _fire1PosY = (-50f, 100f);

    private (float, float) _fire2PosX = (240f, 395f);
    private (float, float) _fire2PosY = (109f, 293f);

    private (float, float) _fire3PosX = (300f, 390f);
    private (float, float) _fire3PosY = (-80f, 15f);
     
    private Animator _powderAnim; 
    private RectTransform _rect;
    private RectTransform _fireExtinguisher;
    private Coroutine _burnCo;

    private float _elapsedTime;
    private int _burnHash;

    private bool isPowder;
    public bool IsPowder
    {
        get
        {
            return isPowder;
        }
        set
        {
            isPowder = value; 
            _powder.SetActive(isPowder); 
        }
    }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _powderAnim = _powder.GetComponent<Animator>();
        _rect = _powder.GetComponent<RectTransform>();
        _fireExtinguisher = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        _elapsedTime = 0;
        _fireExtinguisher.anchoredPosition = new Vector2(-319f, -147f);

        _fire1.SetActive(true);
        _fire2.SetActive(true);
        _fire3.SetActive(true);
    }

    private void Start()
    {
        _burnHash = Animator.StringToHash("Burn");
    }

    private void Update()
    {
        FollowPowderPoint(); 

    }

    private void FollowPowderPoint()
    {
        _powder.transform.position = _powderPoint.transform.position;
    }
      
    public void FireCheck()
    {
        (float, float) _rectPos = (_rect.anchoredPosition.x, _rect.anchoredPosition.y);
 
        if(_rectPos.Item1 > _fire1PosX.Item1 && _rectPos.Item1 < _fire1PosX.Item2
            && _rectPos.Item2 >_fire1PosY.Item1 && _rectPos.Item2 < _fire1PosY.Item2)
        {
            _elapsedTime += Time.deltaTime;

            if(_elapsedTime > 2f)
            {
                _burnCo = StartCoroutine(BurnCoroutine(_fire1));
            }
        }
        else if (_rectPos.Item1 > _fire2PosX.Item1 && _rectPos.Item1 < _fire2PosX.Item2 
            && _rectPos.Item2 > _fire2PosY.Item1 && _rectPos.Item2 < _fire2PosY.Item2)
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > 2f)
            {
                _burnCo = StartCoroutine(BurnCoroutine(_fire2));  
            }
        }
        else if(_rectPos.Item1 > _fire3PosX.Item1 && _rectPos.Item1 < _fire3PosX.Item2
            && _rectPos.Item2 > _fire3PosY.Item1 && _rectPos.Item2 < _fire3PosY.Item2)
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > 2f)
            {
                _burnCo = StartCoroutine(BurnCoroutine(_fire3));
            }
        } 
        else
        {
            _elapsedTime = 0;
        }
    }

    private IEnumerator BurnCoroutine(GameObject go)
    {
        Animator ani = go.GetComponent<Animator>();
        ani.Play(_burnHash); 
        yield return Util.GetDelay(0.5f);
        go.SetActive(false);

    }

}
