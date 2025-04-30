using UnityEngine;

[CreateAssetMenu(fileName = "New Quiz Card", menuName = "Quiz Objects/Quiz Card", order = 51)]
public class QuizCard : ScriptableObject
{
    // Количество вопросов, которое должно быть в каждой сложности (Легкая, средняя, жесткая).
    public int[] questionsAmount = new int[3];

    public string[] names = new string[1]; // Первое указанное имя будет использоваться в промпте
    public Sprite image;


    public QuestionContainer testContainer;

    public string GetName()
    {
        return names[0];
    }
}
