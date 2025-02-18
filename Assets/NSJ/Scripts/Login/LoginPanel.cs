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

    #region private �ʵ�
    private Color _defaultInputColor => _loginEmailInput.placeholder.color; // InputField�� placeholder�÷��� �⺻�� ����
    enum Box { Login, Find, FindSend, SignUp, SendSuccess, Error, ConfirmSend, Size }
    private GameObject[] _boxs = new GameObject[(int)Box.Size];
    // �α��� �ڽ�
    private TMP_InputField _loginEmailInput => GetUI<TMP_InputField>("LoginEmailInput");
    private TMP_InputField _loginPasswordInput => GetUI<TMP_InputField>("LoginPasswordInput");
    private GameObject _loginButton => GetUI("LoginButton");

    // ȸ�� ����
    private TMP_InputField _signUpEmailInput => GetUI<TMP_InputField>("SignUpEmailInput");
    private TMP_InputField _signUpNickNameInput => GetUI<TMP_InputField>("SignUpNickNameInput");
    private TMP_InputField _signUp1stNameInput => GetUI<TMP_InputField>("SignUp1stNameInput");
    private TMP_InputField _signUp2ndNameInput => GetUI<TMP_InputField>("SignUp2ndNameInput");
    private TMP_InputField _signUpPasswordInput => GetUI<TMP_InputField>("SignUpPasswordInput");
    private TMP_InputField _signUpConfirmInput => GetUI<TMP_InputField>("SignUpConfirmInput");
    private GameObject _signUpButton => GetUI("SignUpButton");

    // ��й�ȣ ã�� 
    private TMP_InputField _findEmailInput => GetUI<TMP_InputField>("FindEmailInput");
    private GameObject _findButton => GetUI("FindButton");
    #endregion

    private void Awake()
    {
        Bind(); // ���ε�
        Init(); // �ʱ� ����

    }

    private void Start()
    {
        SubscribeEvent(); // �̺�Ʈ ����
    }

    private void OnEnable()
    {
        if (LobbyScene.Instance != null && LobbyScene.IsLoginCancel == true) // �ε� ĵ���� �ʱ�ȭ, ���ÿ� UI ���� ����
        {
            LobbyScene.IsLoginCancel = false;
            return;
        }
        ChangeBox(Box.Login);
    }

    private void OnDisable()
    {
        if (LobbyScene.IsLoginCancel == true) // �ε� ĵ����
        {
            CancelLogin();
        }
    }

    /// <summary>
    ///  �α��� ĵ��
    /// </summary>
    private void CancelLogin()
    {
        LoadingBox.StartLoading();
        BackendManager.Auth.SignOut(); // �α׾ƿ�
        PhotonNetwork.Disconnect(); // ���� ���� ����
    }

    #region �α���
    /// <summary>
    /// �α��� ��ư Ȱ��ȭ
    /// </summary>
    private void ActivateLoginButton(string value)
    {
        _loginButton.SetActive(false);
        // ��� ��ǲ�ʵ忡 �ۼ����� ������ �α��ι�ư ��Ȱ��ȭ
        if (_loginEmailInput.text == string.Empty)
            return;
        if (_loginPasswordInput.text == string.Empty)
            return;
        _loginButton.SetActive(true);
    }
    /// <summary>
    /// �α���
    /// </summary>
    private void Login()
    {
        string email = _loginEmailInput.text;
        string password = _loginPasswordInput.text;

        //�ε�ȭ�� On
        LoadingBox.StartLoading();
        // �α��� �õ�
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
                    // ���� �˾� ���
                    ChangeBox(Box.Error);
                    return;
                }
                else
                {
                    // �̸��� ���� Ȯ��
                    bool isEmailVerified = BackendManager.Auth.CurrentUser.IsEmailVerified;

                    if (isEmailVerified)
                    {
                        // �̸��� ���� �Ǿ��� �� ���� ���� ���� �� -> ���� ����
                        GetUserData();
                    }
                    else
                    {
                        // �̸��� ���� �ȵǾ��� �� ���� ��û
                        ChangeBox(Box.SendSuccess);
                        SendEmailVerify();
                    }
                }
            });
    }

    /// <summary>
    /// ���� ������ ���
    /// </summary>
    private void GetUserData()
    {
        DatabaseReference userRef = BackendManager.Auth.CurrentUser.UserId.GetUserDataRef();

        // ���� ������ ȹ�� �õ�
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
                    // ��ص� �Ŵ��� UserData�� �޾ƿ� ���� ������ ĳ��
                    BackendManager.User = JsonUtility.FromJson<UserDate>(json);

                    //���� ����
                    ConnectedServer();
                }
            });
    }

    /// <summary>
    /// ������ ����
    /// </summary>
    private void ConnectedServer()
    {
        // ���� ��Ʈ��ũ �÷��̾� �г��ӿ� ���� �г��� ����
        PhotonNetwork.LocalPlayer.NickName = BackendManager.User.NickName;
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion

    #region ȸ�� ����
    private void ActivateSignUpButton(string value)
    {
        _signUpButton.SetActive(false);
        // ��� InputField �� �ۼ��ؾ߸� �α��� ��ư Ȱ��ȭ
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
    /// ȸ�� ����
    /// </summary>
    private void SignUp()
    {
        string email = _signUpEmailInput.text;
        string password = _signUpPasswordInput.text;
        string confirm = _signUpConfirmInput.text;
        if (password != confirm) // ��й�ȣ�� ��й�ȣ Ȯ���� ���� �ٸ����
        {
            SetErrorInput(_signUpPasswordInput);
            SetErrorInput(_signUpConfirmInput);
        }
        //�ε�ȭ�� On
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
                    // �����˾� 
                    ChangeBox(Box.Error);
                    return;
                }
                else
                {
                    // ȸ�� ���� �Ϸ�
                    // ���� ���� �����ͺ��̽� ����
                    SetUserInfo();
                    // �ڽ� ����
                    ChangeBox(Box.SendSuccess);
                    // �̸��� ���� �ʿ�
                    SendEmailVerify();
                }
            });
    }
    /// <summary>
    /// �������� �����ͺ��̽� ����
    /// </summary>
    private void SetUserInfo()
    {
        string nickName = _signUpNickNameInput.text;
        string firstName = _signUp1stNameInput.text;
        string secondName = _signUp2ndNameInput.text;

        // UID �����ͺ��̽� ��ġ ��������
        DatabaseReference userRef = BackendManager.Auth.CurrentUser.UserId.GetUserDataRef();
        // ���ο� ���� ������ �ν��Ͻ�
        UserDate userData = new UserDate();
        userData.NickName = nickName;
        userData.FirstName = firstName;
        userData.SecondName = secondName;
        // ���� ������ Jsonȭ
        string json = JsonUtility.ToJson(userData);
        // ������ ���̽��� ����
        userRef.SetRawJsonValueAsync(json);
    }
    #endregion

    #region �̸��� ����
    /// <summary>
    /// �̸��� ���� ������
    /// </summary>
    private void SendEmailVerify()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;

        // �ε�ȭ�� On
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
                    // ���� â 
                    ChangeBox(Box.Error);
                    return;
                }
            });
    }

    /// <summary>
    /// �̸��� ���� Ȯ��
    /// </summary>
    private void CheckEmailVerify()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;
        // �������� ���� ��ħ
        user.ReloadAsync().
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                    return;
                else if (task.IsFaulted)
                    return;
                else
                {
                    bool success = user.IsEmailVerified; // �̸��� ���� ������?
                    if (success)
                    {
                        // ���� ����
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

    #region ��й�ȣ ã��
    /// <summary>
    /// ���� ��ư Ȱ��ȭ
    /// </summary>
    private void ActivateFindButton(string value)
    {
        _findButton.SetActive(false);
        if (_findEmailInput.text == string.Empty)
            return;
        _findButton.SetActive(true);
    }

    /// <summary>
    /// ��й�ȣ ã��
    /// </summary>
    private void FindPassword()
    {
        string email = _findEmailInput.text; // �̸��� ĳ��

        // �ε�ȭ�� On
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

    #region  �α׾ƿ�
    /// <summary>
    /// �α׾ƿ�
    /// </summary>
    private void LogOut()
    {
        BackendManager.Auth.SignOut();
        ChangeBox(Box.Login);
    }
    #endregion

    #region �г� ����
    /// <summary>
    ///  UI �ڽ� ����
    /// </summary>
    private void ChangeBox(Box box)
    {
        // UI�ڽ� �ٲ� �� �ε�â�� �����
        LoadingBox.StopLoading();

        if (box == Box.Error) // ����â�� �˾��������
        {
            _boxs[(int)box].SetActive(true);
            ClearBox(box);
            return;
        }

        // TODO : 
        for (int i = 0; i < _boxs.Length; i++)
        {
            if (i == (int)box) // ������ �ڽ� ���� �ʱ�ȭ
            {
                _boxs[i].SetActive(true);
                ClearBox(box); //  ������ �ش�ڽ��� �ʱ�ȭ 
            }
            else
            {
                _boxs[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// �α��� �г� �ڽ� Ŭ���� ����
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
    /// �α��� �ڽ� �ʱ�ȭ
    /// </summary>
    private void ClearLoginBox()
    {
        _loginEmailInput.text = string.Empty;
        _loginPasswordInput.text = string.Empty;
        _loginButton.SetActive(false);
    }

    /// <summary>
    /// ȸ������ �ڽ� �ʱ�ȭ
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
    /// ��й�ȣ ã�� �ڽ� �ʱ�ȭ
    /// </summary>
    private void ClearFindBox()
    {
        _findEmailInput.text = string.Empty;
        _findEmailInput.placeholder.color = _defaultInputColor;
        _findButton.SetActive(false);
    }

    #endregion

    #region �ʱ� ����
    /// <summary>
    /// �ʱ� ����
    /// </summary>
    private void Init()
    {
        #region Box �迭 ����
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
    /// �̺�Ʈ ����
    /// </summary>
    private void SubscribeEvent()
    {
        #region LoginBox
        GetUI<Button>("LoginFindButton").onClick.AddListener(() => ChangeBox(Box.Find)); // ��й�ȣ ã�� ��ư
        GetUI<Button>("LoginFindButton").onClick.AddListener(() => SoundManager.SFXPlay(SoundManager.Data.ButtonClick));

        GetUI<Button>("LoginSignUpButton").onClick.AddListener(() => ChangeBox(Box.SignUp)); // ȸ������ ��ư
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

        GetUI<Button>("QuitButton").onClick.AddListener(() =>  // ���� ��ư
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
    /// InputField ���� ���� ��ȯ
    /// </summary>
    private void SetErrorInput(TMP_InputField input)
    {
        input.text = string.Empty;
        input.placeholder.color = Util.GetColor(Color.red, _defaultInputColor.a);
    }

}

