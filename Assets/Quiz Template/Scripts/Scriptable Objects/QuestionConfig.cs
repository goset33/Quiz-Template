using UnityEngine;
using static QuizCardExtentions;

[CreateAssetMenu(fileName = "New Question Config", menuName = "Quiz Objects/Question Config", order = 51)]
public class QuestionConfig : ScriptableObject
{
    public Color defaultAnswerFrom = ConvertHexToColor("#ffffff");
    public Color defaultAnswerTo = ConvertHexToColor("#ffffff");

    [Space]
    public Color wrongAnswerFrom = ConvertHexToColor("#FF6B6B");
    public Color wrongAnswerTo = ConvertHexToColor("#FF4A4A");

    [Space]
    public Color rightAnswerFrom = ConvertHexToColor("#28C76F");
    public Color rightAnswerTo = ConvertHexToColor("#1FA85A");
}
