using System;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static event Action GameStarted, SettingsOpened, LeaderboardOpened;

    public void PlayButtonPressed()
    {
        GameStarted?.Invoke();
    }

    public void SettingsButtonPressed()
    {
        SettingsOpened?.Invoke();
    }

    public void AchivementsButtonPressed()
    {

    }

    public void LeaderboardButtonPressed()
    {
        LeaderboardOpened?.Invoke();
    }
}
