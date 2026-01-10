using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using YG;
using YG.Utils.LB;
using static QuizCardExtentions;

public class LeaderboardController : AbstractController
{
	[SerializeField] private VisualTreeAsset leaderboardCardTemplate;
	[SerializeField] private LocalizedString youLocale;

	private const int MAX_PLAYER_LENGTH = 100;
    private readonly Color defaultColor = new Color(0.8392157f, 0.945098f, 0.972549f, 1f);
	private readonly Color[] firstColors = new Color[] { ConvertHexToColor("FFD700"), ConvertHexToColor("#95a2a3"), ConvertHexToColor("#CD7F32") };

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
			YG2.GetLeaderboard("Stars", 3, MAX_PLAYER_LENGTH);
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

		LBPlayerData[] playerData = data.players.Take(MAX_PLAYER_LENGTH).ToArray();
		for (int i = 0; i < playerData.Length; i++)
		{
            LeaderboardUIData lbData = new LeaderboardUIData
            {
                position = playerData[i].rank,
                name = playerData[i].name,
                starsCount = playerData[i].score
            };

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

			cached.positionText.text = data.position switch
			{
				1 => "🥇",
				2 => "🥈",
				3 => "🥉",
				_ => $"{data.position}"
			};

			if (data.position < 4) 
			{
				cached.positionBackground.style.unityBackgroundImageTintColor = Color.clear;
				cached.positionText.AddToClassList("position-text--big");
			}
			else
			{
				cached.positionBackground.style.unityBackgroundImageTintColor = defaultColor;
				cached.positionText.RemoveFromClassList("position-text--big");
			}

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

		userLeaderboardCard.Q<VisualElement>("CardBackground").AddToClassList("card-background--transparent");
		userLeaderboardCard.userData = cached;
	}

	private void UpdateUserData(LBCurrentPlayerData data)
	{
		CachedRefs refs = userLeaderboardCard.userData as CachedRefs;

		refs.positionBackground.style.unityBackgroundImageTintColor = data.rank switch
		{
			1 => firstColors[0],
			2 => firstColors[1],
			3 => firstColors[2],
			_ => defaultColor
		};

		refs.positionText.text = $"{data.rank}";
		refs.nameText.text = YG2.player.name != InfoYG.ANONYMOUS && YG2.player.auth ? YG2.player.name : youLocale.GetLocalizedString();
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