using UnityEngine;
using UnityEngine.Localization;
using YG;
using YG.Utils.LB;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] private LBPlayerDataYG currentPlayer;

    [SerializeField] private LocalizedString youLocale;

    /// <summary>
    /// Метод вызывается при инициализации лидерборда. Устанавливает значения в нижнее поле данных о текущем игроке
    /// </summary>
    /// <param name="data">Данные лидерборда</param>
    public void OnLeaderboardInitialized(LBData data)
    {
        LBCurrentPlayerData playerData = data.currentPlayer;

        if (YG2.player.name == "unauthorized")
        {
            YG2.OpenAuthDialog();
            BackInMenu();
            return;
        }

        currentPlayer.data.rank = playerData.rank.ToString();
        currentPlayer.data.score = playerData.score.ToString();
        currentPlayer.data.currentPlayer = true;
        currentPlayer.UpdateEntries();
    }

    public void BackInMenu()
    {
        GameManager.Instance.ReturnToMenu(transform);
    }
}
