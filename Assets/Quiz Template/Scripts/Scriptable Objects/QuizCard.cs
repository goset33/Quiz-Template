using UnityEngine;


[CreateAssetMenu(fileName = "New Quiz Card", menuName = "Quiz Objects/Quiz Card", order = 51)]
public class QuizCard : ScriptableObject
{
    public new string name;
    public Sprite image;
    public CardsContainer testContainer;
}
