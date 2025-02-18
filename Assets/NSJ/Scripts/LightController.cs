using NSJ_Test;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] bool _lightTest;
    [SerializeField] GameObject _light;
    private void Start()
    {
        if (_lightTest == false)
        {
            if (TestGame.Instance != null)
            {
                _light.SetActive(false);
            }
            else
            {
                _light.SetActive(true);
            }
        }
        StartCoroutine(CheckPlayerDie());
    }

    IEnumerator CheckPlayerDie()
    {
        int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();

        while (true)
        {
            if (PlayerDataContainer.Instance.GetPlayerData(playerNumber).IsGhost == false)
            {
                _light.SetActive(true);
            }
            else if(PlayerDataContainer.Instance.GetPlayerData(playerNumber).IsGhost == true)
            {
                _light.SetActive(false);
            }
            yield return 0.1f.GetDelay();
        }
    }
}
