using System;
using UnityEngine.UIElements;

public class MenuController : AbstractController
{
    public static event Action GameStarted, SettingsOpened, LeaderboardOpened;

    public override void Init()
    {
        base.Init();
        root.Query<Button>().ForEach(button =>
        {
            switch (button.name)
            {
                case "PlayButton":
                    button.clicked += PlayButtonPressed;
                    break;
                case "SettingsButton":
                    button.clicked += SettingsButtonPressed;
                    break;
                case "LeaderboardsButton":
                    button.clicked += LeaderboardButtonPressed;
                    break;
            }
        });
    }

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
