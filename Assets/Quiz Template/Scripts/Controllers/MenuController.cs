using System;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static GameManager gameManager;

    public static event Action GameStarted;

    public void PlayButtonPressed()
    {
        GameStarted?.Invoke();
    }

    public void OnlineButtonPressed()
    {

    }

    public void AchivementsButtonPressed()
    {

    }

    public void SettingsButtonPressed()
    {

    }
}
