using UnityEngine;
using UnityEngine.Localization;
using YG;
using YG.Utils.LB;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] private LBPlayerDataYG currentPlayer;

    [SerializeField] private LocalizedString youLocale;

    public void OnLeaderboardInitialized(LBData data)
    {
        if (YG2.player.name == "unauthorized")
        {
            YG2.OpenAuthDialog();
            BackInMenu();
            return;
        }

        LBCurrentPlayerData playerData = data.currentPlayer;

        if (YG2.player.name == "unauthorized" || YG2.player.name == "anonymous")
        {
            currentPlayer.data.name = youLocale.GetLocalizedString();
        }
        else
        {
            currentPlayer.data.name = YG2.player.name;
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
