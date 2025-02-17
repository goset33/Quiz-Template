using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionController : MonoBehaviour
{
    public static GameManager gameManager;

    public int currentQuestion;
    public int rightAnswers;

    private int rightIndex;

    private List<GameObject> choosedSequence = new(); // Хранит кнопки в последовательности нажатия
    private GameObject[] rightSequence = new GameObject[0]; // Хранит кнопки в правильной последовательности

    private bool isAnswerShowed = false;
    private List<IQuestion> cards = new();

    [Space]
    public GameObject inMenuWindow;
    public GameObject showRightButton, nextButton;
    public TextMeshProUGUI questionText, counterText;
    public Image image;
    public RectTransform answersParent;
    public GameObject answerButtonPrefab;

    private void OnEnable()
    {
        Init(gameManager.quizzes[gameManager.chosenQuizIndex].testContainer);
    }

    // Инициализация контроллера. Обязательно вызвать ПЕРЕД началом каждого ответа на вопросы
    // Принимает на вход контейнер с вопросами, на которые потребуется отвечать пользователю
    private void Init(CardsContainer container)
    {
        List<IQuestion> allPool = new(container.QuestionCards);
        if (gameManager.shouldShuffle)
        {
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
                button.GetComponent<Button>().onClick.AddListener(() => MainTypeAnswer(index));
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allAnswers[i];

                if (allAnswers[i] == card.FirstAnswer)
                {
                    rightIndex = i;
                    print("Right index was set: " + i);
                }
            }
        }
        else if (card is CounterQuestion)
        {
            choosedSequence.Clear();
            rightSequence = new GameObject[gameManager.chosenLevelIndex + 2];

            List<string> answers = new(card.OtherAnswers.Take(gameManager.chosenLevelIndex + 2));
            List<string> randomizedAnswers = new(answers.OrderBy(_ => Random.value));
            for (int i = 0; i < randomizedAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = randomizedAnswers[i];
                rightSequence[answers.IndexOf(randomizedAnswers[i])] = button;
                button.GetComponent<Button>().onClick.AddListener(() => CountAnswer(button));
            }
        }
    }

    // Обработка нажатия кнопки при вопросе типов 1, 2
    private void MainTypeAnswer(int index)
    {
        if (nextButton.activeSelf) return;

        nextButton.SetActive(true);
        Image pressedButtonImage = answersParent.GetChild(index).GetComponent<Image>();
        if (index == rightIndex)
        {
            rightAnswers++;
            GameManager.ChangeCash(1);

            pressedButtonImage.sprite = gameManager.questionConfig.rightAnswerSprite;
            pressedButtonImage.color = gameManager.questionConfig.rightButtonColor;
            print("Right!");
        }
        else
        {
            showRightButton.SetActive(true);
            pressedButtonImage.sprite = gameManager.questionConfig.wrongAnswerSprite;
            pressedButtonImage.color = gameManager.questionConfig.wrongButtonColor;
            print("Incorrect!");
        }
    }

    // Обработка нажатия кнопки при вопросе типа 3
    private void CountAnswer(GameObject pressedButton)
    {
        if (nextButton.activeSelf) return;

        UpdateButtonIndexes(pressedButton);
        if (choosedSequence.Count == gameManager.chosenLevelIndex + 2)
        {
            int rightCounter = 0;
            for (int i = 0; i < choosedSequence.Count; i++)
            {
                if (choosedSequence[i] == rightSequence[i])
                {
                    rightCounter++;
                }
            }

            nextButton.SetActive(true);
            if (rightCounter == gameManager.chosenLevelIndex + 2)
            {
                rightAnswers++;
                GameManager.ChangeCash(1);
                for (int i = 0; i < answersParent.childCount; i++)
                {
                    Image image = answersParent.GetChild(i).GetComponent<Image>();
                    image.sprite = gameManager.questionConfig.rightAnswerSprite;
                    image.color = gameManager.questionConfig.rightButtonColor;
                }
            }
            else
            {
                showRightButton.SetActive(true);
                for (int i = 0; i < answersParent.childCount; i++)
                {
                    Image image = answersParent.GetChild(i).GetComponent<Image>();
                    image.sprite = gameManager.questionConfig.wrongAnswerSprite;
                    image.color = gameManager.questionConfig.wrongButtonColor;
                }
            }
        }
    }

    // Метод обновляет индексы на кнопках для типа 3
    private void UpdateButtonIndexes(GameObject changingButton)
    {
        if (changingButton != null)
        {
            GameObject changedText = changingButton.transform.GetChild(1).gameObject;
            changedText.SetActive(!changedText.activeSelf);
            if (changedText.activeSelf)
            {
                choosedSequence.Add(changingButton);
            }
            else
            {
                choosedSequence.Remove(changingButton);
            }
        }

        for (int i = 0; i < choosedSequence.Count; i++)
        {
            choosedSequence[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
        }
    }

    // Чистит экран и обнуляет все что нужно обнулить. Вызывать после каждого вопроса
    private void ClearScreen()
    {
        isAnswerShowed = false;
        nextButton.SetActive(false);
        showRightButton.SetActive(false);
        for (int i = 0; i < answersParent.childCount; i++)
        {
            Destroy(answersParent.GetChild(i).gameObject);
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
            gameManager.ChangeActiveWindow(transform, GameManager.GameState.GettingResults, rightAnswers);
        }
    }

    public void ShowRightAnswer()
    {
        if (GameManager.HaveEnoughCash(-1) && !isAnswerShowed)
        {
            isAnswerShowed = true;
            GameManager.ChangeCash(-1);
            if (cards[currentQuestion - 1] is MainTypeQuestion)
            {
                Image rightButton = answersParent.GetChild(rightIndex).GetComponent<Image>();
                rightButton.sprite = gameManager.questionConfig.rightAnswerSprite;
                rightButton.color = gameManager.questionConfig.rightButtonColor;
            }
            else if (cards[currentQuestion - 1] is CounterQuestion)
            {
                choosedSequence = rightSequence.ToList();

                for (int i = 0; i < choosedSequence.Count; i++)
                {
                    Image button = answersParent.GetChild(i).GetComponent<Image>();
                    button.sprite = gameManager.questionConfig.rightAnswerSprite;
                    button.color = gameManager.questionConfig.rightButtonColor;
                }
                UpdateButtonIndexes(null);
            }
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
        gameManager.ChangeActiveWindow(transform, GameManager.GameState.ChoosingQuiz, null);
    }
}
