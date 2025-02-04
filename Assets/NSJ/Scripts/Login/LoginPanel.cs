using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BaseUI
{
    [SerializeField] GameObject _loadingBox;

    #region private 필드
    private Color _defaultInputColor => _loginEmailInput.placeholder.color; // InputField의 placeholder컬러의 기본값 지정
    enum Box { Login, Find, FindSend, SignUp, SendSuccess, Error, ConfirmSend, Size }
    private GameObject[] _boxs = new GameObject[(int)Box.Size];
    // 로그인 박스
    private TMP_InputField _loginEmailInput => GetUI<TMP_InputField>("LoginEmailInput");
    private TMP_InputField _loginPasswordInput => GetUI<TMP_InputField>("LoginPasswordInput");
    private GameObject _loginButton => GetUI("LoginButton");

    // 회원 가입
    private TMP_InputField _signUpEmailInput => GetUI<TMP_InputField>("SignUpEmailInput");
    private TMP_InputField _signUpNickNameInput => GetUI<TMP_InputField>("SignUpNickNameInput");
    private TMP_InputField _signUp1stNameInput => GetUI<TMP_InputField>("SignUp1stNameInput");
    private TMP_InputField _signUp2ndNameInput => GetUI<TMP_InputField>("SignUp2ndNameInput");
    private TMP_InputField _signUpPasswordInput => GetUI<TMP_InputField>("SignUpPasswordInput");
    private TMP_InputField _signUpConfirmInput => GetUI<TMP_InputField>("SignUpConfirmInput");
    private GameObject _signUpButton => GetUI("SignUpButton");

    // 비밀번호 찾기 
    private TMP_InputField _findEmailInput => GetUI<TMP_InputField>("FindEmailInput");
    private GameObject _findButton => GetUI("FindButton");
    #endregion

    private void Awake()
    {
        Bind(); // 바인딩
        Init(); // 초기 설정

    }

    private void Start()
    {
        SubscribeEvent(); // 이벤트 구독
    }

    private void OnEnable()
    {
        if (LobbyScene.Instance != null && LobbyScene.IsLoginCancel == true) // 로딩 캔슬값 초기화, 동시에 UI 변경 금지
        {
            LobbyScene.IsLoginCancel = false;
            return;
        }
        ChangeBox(Box.Login);
    }

    private void OnDisable()
    {
        if (LobbyScene.IsLoginCancel == true) // 로딩 캔슬시
        {
            CancelLogin();
        }
    }

    /// <summary>
    ///  로그인 캔슬
    /// </summary>
    private void CancelLogin()
    {
        LoadingBox.StartLoading();
        BackendManager.Auth.SignOut(); // 로그아웃
        PhotonNetwork.Disconnect(); // 서버 연결 끊기
    }

    #region 로그인
    /// <summary>
    /// 로그인 버튼 활성화
    /// </summary>
    private void ActivateLoginButton(string value)
    {
        _loginButton.SetActive(false);
        // 모든 인풋필드에 작성하지 않으면 로그인버튼 비활성화
        if (_loginEmailInput.text == string.Empty)
            return;
        if (_loginPasswordInput.text == string.Empty)
            return;
        _loginButton.SetActive(true);
    }
    /// <summary>
    /// 로그인
    /// </summary>
    private void Login()
    {
        string email = _loginEmailInput.text;
        string password = _loginPasswordInput.text;

        //로딩화면 On
        LoadingBox.StartLoading();
        // 로그인 시도
        BackendManager.Auth.SignInWithEmailAndPasswordAsync(email, password).
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    ChangeBox(Box.Error);
                    return;
                }
                else if (task.IsFaulted)
                {
                    // 에러 팝업 출력
                    ChangeBox(Box.Error);
                    return;
                }
                else
                {
                    // 이메일 인증 확인
                    bool isEmailVerified = BackendManager.Auth.CurrentUser.IsEmailVerified;

                    if (isEmailVerified)
                    {
                        // 이메일 인증 되었을 시 유저 정보 얻어온 후 -> 서버 연결
                        GetUserData();
                    }
                    else
                    {
                        // 이메일 인증 안되었을 시 인증 요청
                        ChangeBox(Box.SendSuccess);
                        SendEmailVerify();
                    }
                }
            });
    }

    /// <summary>
    /// 유저 데이터 얻기
    /// </summary>
    private void GetUserData()
    {
        DatabaseReference userRef = BackendManager.Auth.CurrentUser.UserId.GetUserDataRef();

        // 유저 데이터 획득 시도
        userRef.GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    ChangeBox(Box.Error);
                    return;
                }

                else if (task.IsFaulted)
                {
                    ChangeBox(Box.Error);
                    return;
                }
                else
                {
                    DataSnapshot snapshot = task.Result;
                    string json = snapshot.GetRawJsonValue();
                    // 백앤드 매니저 UserData에 받아온 유저 데이터 캐싱
                    BackendManager.User = JsonUtility.FromJson<UserDate>(json);

                    //서버 연결
                    ConnectedServer();
                }
            });
    }

    /// <summary>
    /// 서버에 연결
    /// </summary>
    private void ConnectedServer()
    {
        // 포톤 네트워크 플레이어 닉네임에 유저 닉네임 지정
        PhotonNetwork.LocalPlayer.NickName = BackendManager.User.NickName;
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion

    #region 회원 가입
    private void ActivateSignUpButton(string value)
    {
        _signUpButton.SetActive(false);
        // 모든 InputField 를 작성해야만 로그인 버튼 활성화
        if (_signUpEmailInput.text == string.Empty)
            return;
        if (_signUpPasswordInput.text == string.Empty)
            return;
        if (_signUpConfirmInput.text == string.Empty)
            return;
        if (_signUpNickNameInput.text == string.Empty)
            return;
        if (_signUp1stNameInput.text == string.Empty)
            return;
        if (_signUp2ndNameInput.text == string.Empty)
            return;
        _signUpButton.SetActive(true);
    }

    /// <summary>
    /// 회원 가입
    /// </summary>
    private void SignUp()
    {
        string email = _signUpEmailInput.text;
        string password = _signUpPasswordInput.text;
        string confirm = _signUpConfirmInput.text;
        if (password != confirm) // 비밀번호와 비밀번호 확인이 서로 다를경우
        {
            SetErrorInput(_signUpPasswordInput);
            SetErrorInput(_signUpConfirmInput);
        }
        //로딩화면 On
        LoadingBox.StartLoading();
        BackendManager.Auth.CreateUserWithEmailAndPasswordAsync(email, password).   
            ContinueWithOnMainThread(task =>
            {

                if (task.IsCanceled)
                {
                    ChangeBox(Box.Error);
                    return;
                }
                else if (task.IsFaulted)
                {
                    // 에러팝업 
                    ChangeBox(Box.Error);
                    return;
                }
                else
                {
                    // 회원 가입 완료
                    // 유저 정보 데이터베이스 저장
                    SetUserInfo();
                    // 박스 변경
                    ChangeBox(Box.SendSuccess);
                    // 이메일 인증 필요
                    SendEmailVerify();
                }
            });
    }
    /// <summary>
    /// 유저정보 데이터베이스 저장
    /// </summary>
    private void SetUserInfo()
    {
        string nickName = _signUpNickNameInput.text;
        string firstName = _signUp1stNameInput.text;
        string secondName = _signUp2ndNameInput.text;

        // UID 데이터베이스 위치 가져오기
        DatabaseReference userRef = BackendManager.Auth.CurrentUser.UserId.GetUserDataRef();
        // 새로운 유저 데이터 인스턴스
        UserDate userData = new UserDate();
        userData.NickName = nickName;
        userData.FirstName = firstName;
        userData.SecondName = secondName;
        // 유저 데이터 Json화
        string json = JsonUtility.ToJson(userData);
        // 데이터 베이스에 저장
        userRef.SetRawJsonValueAsync(json);
    }
    #endregion

    #region 이메일 인증
    /// <summary>
    /// 이메일 인증 보내기
    /// </summary>
    private void SendEmailVerify()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;

        // 로딩화면 On
        user.SendEmailVerificationAsync().
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    ChangeBox(Box.Error);
                    return;
                }
                else if (task.IsFaulted)
                {
                    // 에러 창 
                    ChangeBox(Box.Error);
                    return;
                }
            });
    }

    /// <summary>
    /// 이메일 인증 확인
    /// </summary>
    private void CheckEmailVerify()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;
        // 유저정보 새로 고침
        user.ReloadAsync().
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                    return;
                else if (task.IsFaulted)
                    return;
                else
                {
                    bool success = user.IsEmailVerified; // 이메일 인증 성공함?
                    if (success)
                    {
                        // 서버 연결
                        LoadingBox.StartLoading();
                        GetUserData();
                    }
                    else
                    {
                        ChangeBox(Box.ConfirmSend);
                        ChangeBox(Box.Error);
                    }
                }
            });
    }
    #endregion

    #region 비밀번호 찾기
    /// <summary>
    /// 복구 버튼 활성화
    /// </summary>
    private void ActivateFindButton(string value)
    {
        _findButton.SetActive(false);
        if (_findEmailInput.text == string.Empty)
            return;
        _findButton.SetActive(true);
    }

    /// <summary>
    /// 비밀번호 찾기
    /// </summary>
    private void FindPassword()
    {
        string email = _findEmailInput.text; // 이메일 캐싱

        // 로딩화면 On
        LoadingBox.StartLoading();
        BackendManager.Auth.SendPasswordResetEmailAsync(email).
            ContinueWithOnMainThread(task =>
            {

                if (task.IsCanceled)
                {
                    ChangeBox(Box.Error);
                    return;
                }
                else if (task.IsFaulted)
                {
                    ChangeBox(Box.Error);
                    SetErrorInput(_findEmailInput);
                }
                else
                {
                    ChangeBox(Box.FindSend);
                }
            });
    }

    #endregion

    #region  로그아웃
    /// <summary>
    /// 로그아웃
    /// </summary>
    private void LogOut()
    {
        BackendManager.Auth.SignOut();
        ChangeBox(Box.Login);
    }
    #endregion

    #region 패널 조작
    /// <summary>
    ///  UI 박스 변경
    /// </summary>
    private void ChangeBox(Box box)
    {
        // UI박스 바뀔 때 로딩창도 사라짐
        LoadingBox.StopLoading();

        if (box == Box.Error) // 에러창은 팝업방식으로
        {
            _boxs[(int)box].SetActive(true);
            ClearBox(box);
            return;
        }

        // TODO : 
        for (int i = 0; i < _boxs.Length; i++)
        {
            if (i == (int)box) // 선택한 박스 빼고 초기화
            {
                _boxs[i].SetActive(true);
                ClearBox(box); //  선택한 해당박스는 초기화 
            }
            else
            {
                _boxs[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// 로그인 패널 박스 클리어 선택
    /// </summary>
    private void ClearBox(Box box)
    {
        switch (box)
        {
            case Box.Login:
                ClearLoginBox();
                break;
            case Box.SignUp:
                ClearSignUpBox();
                break;
            case Box.Find:
                ClearFindBox();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 로그인 박스 초기화
    /// </summary>
    private void ClearLoginBox()
    {
        _loginEmailInput.text = string.Empty;
        _loginPasswordInput.text = string.Empty;
        _loginButton.SetActive(false);
    }

    /// <summary>
    /// 회원가입 박스 초기화
    /// </summary>
    private void ClearSignUpBox()
    {
        _signUpEmailInput.text = string.Empty;
        _signUpNickNameInput.text = string.Empty;
        _signUpPasswordInput.text = string.Empty;
        _signUpPasswordInput.placeholder.color = _defaultInputColor;
        _signUpConfirmInput.text = string.Empty;
        _signUpConfirmInput.placeholder.color = _defaultInputColor;
        _signUp1stNameInput.text = string.Empty;
        _signUp2ndNameInput.text = string.Empty;
        _signUpButton.SetActive(false);
    }

    /// <summary>
    /// 비밀번호 찾기 박스 초기화
    /// </summary>
    private void ClearFindBox()
    {
        _findEmailInput.text = string.Empty;
        _findEmailInput.placeholder.color = _defaultInputColor;
        _findButton.SetActive(false);
    }

    #endregion

    #region 초기 설정
    /// <summary>
    /// 초기 설정
    /// </summary>
    private void Init()
    {
        #region Box 배열 설정
        _boxs[(int)Box.Login] = GetUI("LoginBox");
        _boxs[(int)Box.Find] = GetUI("FindBox");
        _boxs[(int)Box.FindSend] = GetUI("FindSendBox");
        _boxs[(int)Box.SignUp] = GetUI("SignUpBox");
        _boxs[(int)Box.SendSuccess] = GetUI("SendSuccessBox");
        _boxs[(int)Box.Error] = GetUI("ErrorBox");
        _boxs[(int)Box.ConfirmSend] = GetUI("ConfirmSendBox");
        #endregion
    }
    /// <summary>
    /// 이벤트 구독
    /// </summary>
    private void SubscribeEvent()
    {
        #region LoginBox
        GetUI<Button>("LoginFindButton").onClick.AddListener(() => ChangeBox(Box.Find)); // 비밀번호 찾기 버튼
        GetUI<Button>("LoginFindButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("LoginSignUpButton").onClick.AddListener(() => ChangeBox(Box.SignUp)); // 회원가입 버튼
        GetUI<Button>("LoginSignUpButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("LoginButton").onClick.AddListener(Login);
        GetUI<Button>("LoginButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        _loginEmailInput.onValueChanged.AddListener(ActivateLoginButton);
        _loginPasswordInput.onValueChanged.AddListener(ActivateLoginButton);
        #endregion

        #region SignUpBox
        GetUI<Button>("SignUpButton").onClick.AddListener(SignUp);
        GetUI<Button>("SignUpButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("SignUpBackButton").onClick.AddListener(() => ChangeBox(Box.Login));
        GetUI<Button>("SignUpBackButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonOff));

        _signUpEmailInput.onValueChanged.AddListener(ActivateSignUpButton);
        _signUpPasswordInput.onValueChanged.AddListener(ActivateSignUpButton);
        _signUpConfirmInput.onValueChanged.AddListener(ActivateSignUpButton);
        _signUpNickNameInput.onValueChanged.AddListener(ActivateSignUpButton);
        _signUp1stNameInput.onValueChanged.AddListener(ActivateSignUpButton);
        _signUp2ndNameInput.onValueChanged.AddListener(ActivateSignUpButton);
        #endregion

        #region FindBox

        GetUI<Button>("FindBackButton").onClick.AddListener(() => ChangeBox(Box.Login));
        GetUI<Button>("FindBackButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonOff));

        GetUI<Button>("FindButton").onClick.AddListener(FindPassword);
        GetUI<Button>("FindButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
        _findEmailInput.onValueChanged.AddListener(ActivateFindButton);

        #endregion

        #region FindSendBox

        GetUI<Button>("FindSendCheckButton").onClick.AddListener(() => ChangeBox(Box.Login));
        GetUI<Button>("FindSendCheckButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        #endregion

        #region SendSuccessBox
        GetUI<Button>("SendSuccessCheckButton").onClick.AddListener(CheckEmailVerify);
        GetUI<Button>("SendSuccessCheckButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
        #endregion

        #region ConfirmSendBox
        GetUI<Button>("ConfirmSendRetryButton").onClick.AddListener(SendEmailVerify);
        GetUI<Button>("ConfirmSendRetryButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("ConfirmSendOKButton").onClick.AddListener(CheckEmailVerify);
        GetUI<Button>("ConfirmSendOKButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("ConfirmSendLogOutButton").onClick.AddListener(LogOut);
        GetUI<Button>("ConfirmSendLogOutButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
        #endregion

        #region ErrorBox
        GetUI<Button>("ErrorBackButton").onClick.AddListener(() => { GetUI("ErrorBox").SetActive(false); });
        GetUI<Button>("ErrorBackButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
        #endregion

        GetUI<Button>("QuitButton").onClick.AddListener(() =>  // 종료 버튼
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
             Application.Quit();
#endif
        });
        GetUI<Button>("QuitButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));
    }
    #endregion


    /// <summary>
    /// InputField 에러 상태 전환
    /// </summary>
    private void SetErrorInput(TMP_InputField input)
    {
        input.text = string.Empty;
        input.placeholder.color = Util.GetColor(Color.red, _defaultInputColor.a);
    }

}

