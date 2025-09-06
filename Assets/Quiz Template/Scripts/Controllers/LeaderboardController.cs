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
        LBCurrentPlayerData playerData = data.currentPlayer;

        currentPlayer.data.name = YG2.saves.nickname;

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
