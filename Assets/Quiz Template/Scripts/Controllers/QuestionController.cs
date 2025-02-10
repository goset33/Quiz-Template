using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionController : MonoBehaviour
{
    public static GameManager gameManager;

    private int rightIndex;
    private List<int> choosedSequence = new();

    private List<IQuestion> cards = new();

    public int currentQuestion;
    public int rightAnswers;

    [Space]
    public GameObject inMenuWindow;
    public GameObject showRightButton, nextButton;
    public TextMeshProUGUI questionText, counterText;
    public Image image;
    public RectTransform answersParent;
    public GameObject answerButtonPrefab;

    // Инициализация контроллера. Обязательно вызвать ПЕРЕД началом каждого ответа на вопросы
    // Принимает на вход контейнер с вопросами, на которые потребуется отвечать пользователю
    public void Init(CardsContainer container)
    {
        List<IQuestion> allPool = new(container.QuestionCards);
        if (gameManager.shouldShuffle)
        {
            // allPool.OrderBy(_ => Random.value).Reverse(); 

            int n = allPool.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                IQuestion temp = allPool[i];
                allPool[i] = allPool[j];
                allPool[j] = temp;
            }
        }
        cards = container.shouldCutCardPool ? new List<IQuestion>(allPool.Take(container.numberOfQuestions)) : allPool;

        currentQuestion = 1;
        rightAnswers = 0;
        LoadNextQuestion(cards[currentQuestion - 1]);
    }

    // Функция используется для занесения данных из входной переменной card в интерфейс
    private void LoadNextQuestion(IQuestion card)
    {
        questionText.text = card.QuestionText;
        image.sprite = card.Image;
        counterText.text = $"{currentQuestion}/{cards.Count}";

        if (card is MainTypeQuestion)
        {
            List<string> wrongs = new(card.OtherAnswers.OrderBy(_ => Random.value).Take(gameManager.chosenLevelIndex + 1)); // Рандомные неправильные ответы, отрезанные по сложности уровня
            List<string> allAnswers = new(wrongs.Append(card.FirstAnswer).OrderBy(_ => Random.value)); // Рандомные варианты ответов
            for (int i = 0; i < allAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                int index = i;
                button.GetComponent<Button>().onClick.AddListener(() => AnswerButtonPressed(index));
                button.GetComponentInChildren<TextMeshProUGUI>().text = allAnswers[i];

                if (allAnswers[i] == card.FirstAnswer)
                {
                    rightIndex = i;
                    print("Right index was set: " + i);
                }
            }
        }
        else if (card is CounterQuestion)
        {
            choosedSequence = null;
            List<string> answers = new(card.OtherAnswers.Take(gameManager.chosenLevelIndex + 2)); // TODO: Нужно ли обрезать?
            List<string> randomizedAnswers = new(answers.OrderBy(_ => Random.value));
            for (int i = 0; i < answers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                int index = i;
                button.GetComponent<Button>().onClick.AddListener(() => AnswerButtonPressed(index)); // TODO: Использовать другой метод?
                button.GetComponentInChildren<TextMeshProUGUI>().text = answers[i];

                choosedSequence.Add(card.OtherAnswers.IndexOf(answers[i]));
            }
        }
    }

    private void ClearScreen()
    {
        nextButton.SetActive(false);
        showRightButton.SetActive(false);
        for (int i = 0; i < answersParent.childCount; i++)
        {
            Destroy(answersParent.GetChild(i).gameObject);
        }
    }

    // Нажатие кнопки ответа. На вход подается номер кнопки начиная с 0
    public void AnswerButtonPressed(int buttonIndex)
    {
        if (nextButton.activeSelf) return;

        nextButton.SetActive(true);
        Image pressedImage = answersParent.GetChild(buttonIndex).GetComponent<Image>();
        if (buttonIndex == rightIndex)
        {
            rightAnswers++;
            GameManager.ChangeCash(1);

            pressedImage.sprite = gameManager.questionConfig.rightAnswerSprite;
            pressedImage.color = gameManager.questionConfig.rightButtonColor;
            print("Right!");
        }
        else
        {
            showRightButton.SetActive(true);
            pressedImage.sprite = gameManager.questionConfig.wrongAnswerSprite;
            pressedImage.color = gameManager.questionConfig.wrongButtonColor;

            print("Incorrect!");
        }
    }

    public void NextButtonPressed()
    {
        ClearScreen();
        if (currentQuestion != cards.Count) // Если вопрос был не последний
        {
            currentQuestion++;
            LoadNextQuestion(cards[currentQuestion - 1]);
        }
        else
        {
            gameManager.NextStep(rightAnswers, transform);
        }
    }

    public void ShowRightAnswer()
    {
        if (GameManager.ChangeCash(-1))
        {
            Image rightButton = answersParent.GetChild(rightIndex).GetComponent<Image>();
            rightButton.sprite = gameManager.questionConfig.rightAnswerSprite;
            rightButton.color = gameManager.questionConfig.rightButtonColor;
        }
    }

    public void MenuButtonPressed()
    {
        inMenuWindow.SetActive(!inMenuWindow.activeSelf);
    }

    public void BackInMenu()
    {
        ClearScreen();
        inMenuWindow.SetActive(false);
        gameManager.BackInMenu(transform);
    }
}
