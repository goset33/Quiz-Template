using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using YG;

public class ChooseController : AbstractController
{
    public static event Action<QuizCard> QuizChoosed;

    [SerializeField] private VisualTreeAsset quizCardTemplate;

    private ListView quizListView;
    private List<QuizUIData> quizItems = new List<QuizUIData>();

    //[SerializeField] private Transform cardsParent;
    //[SerializeField] private TextMeshProUGUI cashCounter;

    public override void Init()
    {
        base.Init();

        quizItems = new(GameManager.Instance.quizzes.Count);
        for (int i = 0;  i < quizItems.Count; i++)
        {
            QuizCard quizCard = GameManager.Instance.quizzes[i];
            QuizCardSaveData saveData = YG2.saves.quizCards.GetSaveDataByQuizCard(quizCard);

            quizItems[i].image = quizCard.image;
            quizItems[i].level = saveData.level;
            quizItems[i].exp = saveData.exp;
            quizItems[i].maxExp = saveData.maxExp;
        }

        quizListView = root.Q<ListView>();
        quizListView.makeItem = () =>
        {
            return quizCardTemplate.CloneTree();
        };

        quizListView.bindItem = (element, index) =>
        {
            QuizUIData quizData = quizItems[index];
            QuizCard quizCard = GameManager.Instance.quizzes.GetQuizCardByQuizUIData(quizData);

            var startButton = element.Q<Button>();
            if (startButton != null)
            {
                startButton.clickable.clicked += () => QuizChooseButtonPressed(quizCard);
            }
        };

        quizListView.itemsSource = quizItems;
        quizListView.fixedItemHeight = 200; // Высота одной карточки
        quizListView.Rebuild();
    }

    private void QuizChooseButtonPressed(QuizCard quizCard)
    {
        QuizChoosed?.Invoke(quizCard);
    }

    public void BackInMenu()
    {
        GameManager.Instance.OpenWindow<MenuController>();
    }
}

public class QuizUIData
{
    public Sprite image;
    public int level;
    public int exp, maxExp;
}
