using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionBox : BaseUI
{
    enum Box { Sound, GameOption, Quit, Delete,Size}

    [System.Serializable]
    struct BoxStruct
    {
        public GameObject Sound;
        public GameObject GameOption;
        public GameObject Quit;
        public GameObject Delete;
    }
    [SerializeField] private BoxStruct _boxStruct;
    private GameObject _sound { get { return _boxStruct.Sound; } }
    private GameObject _gameOption {  get { return _boxStruct.GameOption; } }
    private GameObject _quit { get { return _boxStruct.Quit; } }
    private GameObject _delete { get { return _boxStruct.Delete; } }

    private GameObject[] _boxs = new GameObject[(int)Box.Size];

    private void Awake()
    {
        Bind();
        Init();

    }
    private void Start()
    {
        SubscribeEvents();
    }

    private void OnEnable()
    {
        ChangeBox(Box.Sound);
    }

    /// <summary>
    /// UI 박스 변경
    /// </summary>
    private void ChangeBox(Box box)
    {
        for (int i = 0; i < _boxs.Length; i++) 
        {
            if(i == (int)box)
            {
                _boxs[i].SetActive(true);
            }
            else
            {
                _boxs[i].SetActive(false);
            }
        }
    }

    private void Init()
    {
        #region 박스 배열 설정

        _boxs[(int)Box.Sound] = _sound;
        _boxs[(int)Box.GameOption] = _gameOption;
        _boxs[(int)Box.Quit] = _quit;
        _boxs[(int)Box.Delete] = _delete;
        #endregion        
    }
    private void SubscribeEvents()
    {
        GetUI<Button>("CancelButton").onClick.AddListener(() => OptionPanel.SetActiveOption(false)); // X 버튼 누르면 옵션창 꺼짐
        GetUI<Button>("CancelButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonOff));

        GetUI<Button>("SoundButton").onClick.AddListener(()=>ChangeBox(Box.Sound));
        GetUI<Button>("SoundButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("GameOptionButton").onClick.AddListener(()=>ChangeBox(Box.GameOption));
        GetUI<Button>("GameOptionButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("QuitButton").onClick.AddListener(()=>ChangeBox(Box.Quit));
        GetUI<Button>("QuitButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("DeleteButton").onClick.AddListener(()=>ChangeBox(Box.Delete));
        GetUI<Button>("DeleteButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
    }
}
