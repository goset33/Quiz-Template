using UnityEngine;

[CreateAssetMenu(fileName = "New Quiz Card", menuName = "Quiz Objects/Quiz Card", order = 51)]
public class QuizCard : ScriptableObject
{
    // Количество вопросов, которое должно быть в каждой сложности (Легкая, средняя, жесткая, босс)
    // Если 0, то все что есть
    public int[] questionsAmount = new int[4];

    public string[] names = new string[1]; // Первое указанное имя будет использоваться в промпте
    public Sprite image;

    public const int MIN_EXP = 30; // Начальное значение для 1-го уровня

    public QuestionContainer testContainer;

    public string GetName()
    {
        return names[0];
    }
}
