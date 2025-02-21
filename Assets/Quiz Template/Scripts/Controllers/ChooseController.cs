using UnityEngine;

public class ChooseController : MonoBehaviour
{
    public static GameManager gameManager;

    public GameObject quizCardPrefab;
    public Transform cardsContainer;

    public void OnGameStart(QuizCard[] quizCards)
    {
        foreach (QuizCard content in quizCards)
        {
            QuizCardSetter card = Instantiate(quizCardPrefab, cardsContainer).GetComponent<QuizCardSetter>();
            card.cardContent = content;
            card.SetContent(this);
        }
    }

    public void OnQuizChoosed(int index)
    {
        gameManager.ChangeActiveWindow(transform, GameManager.GameState.ChoosingLevel, index);
    }
}
