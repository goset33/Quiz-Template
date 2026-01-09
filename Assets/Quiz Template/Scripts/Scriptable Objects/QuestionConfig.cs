using SixLabors.ImageSharp.ColorSpaces;
using UnityEngine;

[CreateAssetMenu(fileName = "New Question Config", menuName = "Quiz Objects/Question Config", order = 51)]
public class QuestionConfig : ScriptableObject
{
    public Color defaultAnswerFrom = new Color(219, 219, 219);
    public Color defaultAnswerTo = new Color(255, 255, 255);

    [Space]
    public Color wrongAnswerFrom = new Color(253, 75, 73);
    public Color wrongAnswerTo = new Color(255, 115, 114);

    [Space]
    public Color rightAnswerFrom = new Color(30, 164, 87);
    public Color rightAnswerTo = new Color(42, 203, 114);
}
