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
        foreach (QuizCard content in YG2.saves.favoriteCards)
        {
            QuizCardSetter card = Instantiate(quizCardPrefab, cardsParent).GetComponent<QuizCardSetter>();
            card.cardContent = content;
            card.SetContent(this, true);
        }
        foreach (QuizCard content in YG2.saves.otherCards)
        {
            if (content == null) continue;

            QuizCardSetter card = Instantiate(quizCardPrefab, cardsParent).GetComponent<QuizCardSetter>();
            card.cardContent = content;
            card.SetContent(this, false);
        }
    }

    public void RedrawOrder()
    {
        for (int i = 0; i < cardsParent.childCount; i++)
        {
            Destroy(cardsParent.GetChild(i).gameObject);
        }
        OnGameStart();
    }

    public void UpdateCardPos(QuizCard card)
    {
        gameManager.SetAsFavorite(card);
        RedrawOrder();
    }
}
