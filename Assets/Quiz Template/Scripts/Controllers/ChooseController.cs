using UnityEngine;
using YG;

public class ChooseController : MonoBehaviour
{
    public static GameManager gameManager;

    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform cardsParent;
    public LevelHandler levelHandler;

    private void OnEnable()
    {
        levelHandler.UpdateLevelUI();
    }

    private void OnGameStart()
    {
        foreach (QuizCard content in gameManager.quizzes)
        {
            QuizCardSetter card = Instantiate(quizCardPrefab, cardsParent).GetComponent<QuizCardSetter>();
            card.cardContent = content;
            card.SetContent(this);
        }
    }

    public void RedrawOrder()
    {
        if (cardsParent.childCount == 0)
        {
            OnGameStart();
        }

        for (int i = 0; i < gameManager.quizzes.Length; i++)
        {
            Transform curr = cardsParent.GetChild(i);
            for (int j = 0; j <  gameManager.quizzes.Length; j++)
            {
                if (YandexGame.savesData.realQuizzesSequence[j] == i)
                {
                    curr.SetSiblingIndex(j);
                    break;
                }
            }
        }
    }

    public void UpdateCardPos(QuizCard card)
    {
        gameManager.UpdateQuizCardPosition(card);
        RedrawOrder();
    }
}
