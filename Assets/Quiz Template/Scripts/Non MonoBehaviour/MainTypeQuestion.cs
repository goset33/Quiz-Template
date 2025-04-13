using System.Linq;
using UnityEngine;

public class MainTypeQuestion : IQuestion
{
    private readonly string question;
    private readonly Sprite image;

    private readonly string rightAnswer;
    private readonly string[] wrongAnswers = new string[3];
    private readonly string[] allAnswers = new string[4];


    public string QuestionText { get => question; }
    public Sprite Image { get => image; }

    public string RightAnswer { get => rightAnswer; }
    public string[] WrongAnswers { get => wrongAnswers; }
    public string[] AllAnswers { get => allAnswers; }

    public MainTypeQuestion(string questionString, Sprite sprite, string[] answers)
    {
        if (answers.Length != 4) return;

        question = questionString;
        image = sprite;
        rightAnswer = answers[0];
        wrongAnswers = answers.Skip(1).ToArray();
        allAnswers = answers;
    }

    public MainTypeQuestion(MainTypeQuestion instance) : this(instance.QuestionText, instance.Image, instance.AllAnswers) { }
}
