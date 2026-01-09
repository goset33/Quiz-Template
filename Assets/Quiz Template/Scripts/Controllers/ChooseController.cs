using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using YG;

public class ChooseController : AbstractController
{
    public static event Action<QuizCard> QuizChoosed;

    [SerializeField] private VisualTreeAsset quizCardTemplate;

    private ListView quizListView;
    private List<QuizUIData> quizItems = new List<QuizUIData>();

    public override void Init()
    {
        base.Init();
        quizListView = root.Q<ListView>();

        root.Q<Label>("CurrencyAmount").SetBinding("text", new DataBinding
        {
            dataSource = YG2.saves,
            dataSourcePath = PropertyPath.FromName(nameof(SavesYG.cash)),
            bindingMode = BindingMode.ToTarget
        });

        SetupListView();
    }

    private void SetupListView()
    {
        quizListView.makeItem = () =>
        {
            var card = quizCardTemplate.CloneTree();

            var cached = new CachedRefs
            {
                background = card.Q<VisualElement>("Background"),
                expBar = card.Q<ProgressBar>("ExpBar"),
                levelText = card.Q<Label>("LevelText"),
            };

            quizListView.selectionType = SelectionType.Single;
            quizListView.selectionChanged += (selectedItems) =>
            {
                foreach (var item in selectedItems)
                {
                    if (item is QuizUIData quizData)
                    {
                        print($"Selected: {quizData.quizCard.name}");
                        QuizChooseButtonPressed(quizData.quizCard);
                    }
                }
            };

            card.userData = cached;
            return card;
        };

        quizListView.bindItem = (element, index) =>
        {
            QuizUIData quizData = quizItems[index];

            if (element.userData is not CachedRefs cached) return;

            cached.background.style.backgroundImage = new StyleBackground(quizData.image);
            cached.expBar.highValue = quizData.maxExp;
            cached.expBar.value = quizData.exp;
            cached.levelText.text = $"{quizData.level}";
        };

        quizListView.itemsSource = quizItems;
        quizListView.fixedItemHeight = 512;
        quizListView.Rebuild();
    }

    private void RefreshAllQuizData()
    {
        quizItems.Clear();

        foreach (QuizCard quizCard in GameManager.Instance.quizzes)
        {
            QuizCardSaveData saveData = YG2.saves.quizCards.GetSaveDataByQuizCard(quizCard);

            quizItems.Add(new QuizUIData
            {
                quizCard = quizCard,
                image = quizCard.image,
                level = saveData.level,
                exp = saveData.exp,
                maxExp = saveData.maxExp
            });
        }

        quizListView?.RefreshItems();
    }

    private void QuizChooseButtonPressed(QuizCard quizCard)
    {
        QuizChoosed?.Invoke(quizCard);
    }

    public override void ChangeVisibilityState(bool newState)
    {
        base.ChangeVisibilityState(newState);

        if (newState)
        {
             quizListView?.ClearSelection();

            root.RemoveFromClassList("hidden-container");
            RefreshAllQuizData();
        }
        else
        {
            root.AddToClassList("hidden-container");
        }
    }

    class CachedRefs
    {
        public VisualElement background;
        public ProgressBar expBar;
        public Label levelText;
    }
}

public class QuizUIData
{
    public QuizCard quizCard;

    public Sprite image;
    public int level;
    public int exp, maxExp;
}
