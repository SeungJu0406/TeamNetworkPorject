using Firebase.Database;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static partial class Util
{

    private static StringBuilder _sb = new StringBuilder();

    private static Dictionary<float, WaitForSeconds> _delayDic = new Dictionary<float, WaitForSeconds>();
    /// <summary>
    /// TMP�� ���� StringBuilder ��ȯ �Լ�
    /// </summary>
    public static StringBuilder GetText<T>(this T value)
    {
        _sb.Clear();
        _sb.Append(value);
        return _sb;
    }

    /// <summary>
    /// ���� ����ȭ
    /// </summary>
    public static T SendAndReceiveStruct<T>(this PhotonStream stream, T value) where T : struct
    {
        if (stream.IsWriting)
        {
            stream.SendNext((T)value);
        }
        else if (stream.IsReading)
        {
            value = (T)stream.ReceiveNext();
        }
        return value;
    }

    /// <summary>
    /// ������Ʈ ����ȭ
    /// </summary>
    public static T SendAndReceiveClass<T>(this PhotonStream stream, T value) where T : Component
    {
        if (stream.IsWriting)
        {
            if (value == null) // null �� ��� ���� �� -1 ����
            {
                stream.SendNext(-1);
            }
            else
            {
                PhotonView photonView = value.GetComponent<PhotonView>();
                stream.SendNext(photonView.ViewID);
            }
        }
        else if (stream.IsReading)
        {
            int id = (int)stream.ReceiveNext();
            if (id <= -1) // -1 �� ��� null
            {
                value = null;
            }
            else
            {
                PhotonView target = PhotonView.Find(id);
                value = target.GetComponent<T>();
            }
        }
        return value;
    }

    /// <summary>
    /// ���� ������Ʈ ����ȭ(�����ε�)
    /// </summary>
    public static GameObject SendAndReceiveClass(this PhotonStream stream, GameObject gameObject)
    {
        if (stream.IsWriting)
        {
            if (gameObject == null) // null �� ��� ���� �� -1 ����
            {
                stream.SendNext(-1);
            }
            else
            {
                PhotonView photonView = gameObject.GetComponent<PhotonView>();
                stream.SendNext(photonView.ViewID);
            }
        }
        else if (stream.IsReading)
        {
            int id = (int)stream.ReceiveNext();
            if (id <= -1) // -1 �� ��� null
            {
                gameObject = null;
            }
            else
            {
                PhotonView target = PhotonView.Find(id);
                gameObject = target.GetComponent<Transform>().gameObject;
            }
        }
        return gameObject;
    }

    /// <summary>
    /// �ڷ�ƾ ������ ��������
    /// </summary>
    public static WaitForSeconds GetDelay(this float time)
    {
        if (_delayDic.ContainsKey(time) == false)
        {
            _delayDic.Add(time, new WaitForSeconds(time));
        }
        return _delayDic[time];
    }

    /// <summary>
    /// ���� �ð� ��������
    /// </summary>
    public static float GetLack(this PhotonMessageInfo info)
    {
        return Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
    }

    /// <summary>
    /// �÷� ��������
    /// a: 0~1
    /// </summary>
    public static Color GetColor(Color color, float a)
    {
        Color newColor = new Color();
        newColor = color;
        newColor.a = a;
        return newColor;
    }

    /// <summary>
    /// UID ���۷��� ��ġ ��������
    /// </summary>
    public static DatabaseReference GetUserDataRef(this string userId)
    {
        DatabaseReference root = BackendManager.DataBase.RootReference;
        DatabaseReference userRef = root.Child("UserData").Child(userId);

        return userRef;
    }

    /// <summary>
    /// ���� Ŭ������ ����
    /// </summary>
    /// <param name="text"></param>
    public static void CopyText(this string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }

    /// <summary>
    /// �г��� ����
    /// </summary>
    /// <param name="nickName"></param>
    public static void ChangeNickName(this string nickName)
    {
        BackendManager.User.NickName = nickName; // ���� ������ �г��� ����
        // �г��� ������ ���̽��� �Ϻ� ���� ����
        BackendManager.SettingDic.Clear();
        BackendManager.SettingDic.Add(UserDate.NICKNAME, nickName);
        BackendManager.Auth.CurrentUser.UserId.GetUserDataRef().UpdateChildrenAsync(BackendManager.SettingDic);
    }

    /// <summary>
    /// ���� �� �ڵ� ���
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string GetRandomRoomCode(int length)
    {
        _sb.Clear();
        for (int i = 0; i < length; i++)
        {
            int numberOrAlphabet = UnityEngine.Random.Range(0, 2);
            if (numberOrAlphabet == 0) // ������
            {
                int numberASKII = UnityEngine.Random.Range(48, 58); // �ƽ�Ű�ڵ� 48~57������(0~9)
                _sb.Append((char)numberASKII);
            }
            else // ������
            {
                int alphabetASKII = UnityEngine.Random.Range(65, 91); // �ƽ�Ű�ڵ� 65~91������ (A~Z)
                _sb.Append((char)alphabetASKII);
            }
        }
        return _sb.ToString();
    }
}
