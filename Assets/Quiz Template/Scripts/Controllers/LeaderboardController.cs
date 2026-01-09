using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using YG;
using YG.Utils.LB;

public class LeaderboardController : AbstractController
{
    [SerializeField] private VisualTreeAsset leaderboardCardTemplate;

    [SerializeField] private LocalizedString youLocale;

    private readonly Color defaultColor = new Color(0.8392157f, 0.945098f, 0.972549f, 1f);

    private ListView leaderboardListView;
    private List<LeaderboardUIData> leaderboardItems = new List<LeaderboardUIData>();
    private VisualElement userLeaderboardCard;

    public override void Init()
    {
        base.Init();
        YG2.onGetLeaderboard += UpdateLeaderboard;

        leaderboardListView = root.Q<ListView>();
        SetupListView();

        userLeaderboardCard = leaderboardCardTemplate.CloneTree();
        SetupUserData();
    }

    public override void ChangeVisibilityState(bool newState)
    {
        base.ChangeVisibilityState(newState);

        if (newState)
        {
            YG2.GetLeaderboard("Stars", 10, 10);
        }
    }

    private void OnDisable()
    {
        YG2.onGetLeaderboard -= UpdateLeaderboard;
    }

    private void UpdateLeaderboard(LBData data)
    {
        if (!data.technoName.Equals("Stars")) return;

        if (YG2.player.name == "unauthorized" || !YG2.player.auth)
        {
            YG2.OpenAuthDialog();
            BackInMenu();
            return;
        }

        leaderboardItems.Clear();

        LBPlayerData[] playerData = data.players;
        for (int i = 0; i < playerData.Length; i++)
        {
            LeaderboardUIData lbData = new LeaderboardUIData();
            lbData.position = playerData[i].rank;
            lbData.name = playerData[i].name;
            lbData.starsCount = playerData[i].score;

            leaderboardItems.Add(lbData);
        }

        leaderboardListView?.RefreshItems();
        UpdateUserData(data.currentPlayer);
    }

    private void SetupListView()
    {
        leaderboardListView.makeItem = () =>
        {
            var element = leaderboardCardTemplate.CloneTree();

            var cached = new CachedRefs
            {
                positionBackground = element.Q<VisualElement>("PositionBackground"),
                positionText = element.Q<Label>("PositionText"),
                nameText = element.Q<Label>("NameText"),
                starsCountText = element.Q<Label>("StarCounterText"),
            };

            element.userData = cached;
            return element;
        };

        leaderboardListView.bindItem = (element, index) =>
        {
            if (element.userData is not CachedRefs cached) return;
            LeaderboardUIData data = leaderboardItems[index];

            cached.positionBackground.style.unityBackgroundImageTintColor = data.position switch
            {
                1 => Color.yellow,
                2 => Color.gray,
                3 => Color.brown,
                _ => defaultColor
            };

            cached.positionText.text = $"{data.position}";
            cached.nameText.text = data.name;
            cached.starsCountText.text = $"{data.starsCount}";
        };

        leaderboardListView.itemsSource = leaderboardItems;
        //leaderboardListView.fixedItemHeight = 220;
        leaderboardListView.Rebuild();
    }

    private void SetupUserData()
    { 
        root.Q<GradientElement>().Add(userLeaderboardCard);

        var cached = new CachedRefs
        {
            positionBackground = userLeaderboardCard.Q<VisualElement>("PositionBackground"),
            positionText = userLeaderboardCard.Q<Label>("PositionText"),
            nameText = userLeaderboardCard.Q<Label>("NameText"),
            starsCountText = userLeaderboardCard.Q<Label>("StarCounterText"),
        };

        var style = userLeaderboardCard.Q<VisualElement>("CardBackground").style;
        style.backgroundColor = new Color();
        style.marginBottom = 0;
        style.flexGrow = 1;

        userLeaderboardCard.userData = cached;
    }

    private void UpdateUserData(LBCurrentPlayerData data)
    {
        CachedRefs refs = userLeaderboardCard.userData as CachedRefs;

        refs.positionBackground.style.unityBackgroundImageTintColor = data.rank switch
        {
            1 => Color.yellow,
            2 => Color.gray,
            3 => Color.brown,
            _ => defaultColor
        };

        refs.positionText.text = $"{data.rank}";
        refs.nameText.text = YG2.player.name != "anonymous" && YG2.player.auth ? YG2.player.name : youLocale.GetLocalizedString();
        refs.starsCountText.text = $"{data.score}";
    }

    class CachedRefs
    {
        public VisualElement positionBackground;
        public Label positionText;
        public Label nameText;
        public Label starsCountText;
    }
}

public class LeaderboardUIData
{
    public int position;
    public string name;
    public int starsCount;
}