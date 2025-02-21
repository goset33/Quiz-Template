using System.Linq;
using UnityEngine;

public class MainTypeQuestion : IQuestion
{
    private readonly string question;
    private readonly Sprite image;
    private readonly int type;

    private readonly string rightAnswer;
    private readonly string[] wrongAnswers = new string[3];
    private readonly string[] allAnswers = new string[4];


    public string QuestionText { get => question; }
    public Sprite Image { get => image; }
    public int Type { get => type; }

    public string RightAnswer { get => rightAnswer; }
    public string[] WrongAnswers { get => wrongAnswers; }
    public string[] AllAnswers { get => allAnswers; }

    public MainTypeQuestion(string questionString, int questType, Sprite sprite, string[] answers)
    {
        if (answers.Length != 4) return;

        question = questionString;
        image = sprite;
        type = questType;
        rightAnswer = answers[0];
        wrongAnswers = answers.Skip(1).ToArray();
        allAnswers = answers;
    }

    public MainTypeQuestion(MainTypeQuestion instance)
    {
        question = instance.QuestionText;
        image = instance.Image;
        type = instance.Type;
        rightAnswer = instance.RightAnswer;
        wrongAnswers = instance.WrongAnswers;
        allAnswers = instance.AllAnswers;
    }
}
