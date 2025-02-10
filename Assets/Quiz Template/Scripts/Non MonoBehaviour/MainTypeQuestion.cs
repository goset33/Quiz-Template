using System.Collections.Generic;
using UnityEngine;

public class MainTypeQuestion : IQuestion
{
    private readonly string question;
    private readonly Sprite image;

    private readonly string rightAnswer;
    private readonly List<string> wrongAnswers = new(3);


    public string QuestionText { get => question; }
    public Sprite Image { get => image; }

    public string FirstAnswer { get => rightAnswer; }
    public List<string> OtherAnswers { get => wrongAnswers; }

    public MainTypeQuestion(string questionString, Sprite sprite, string answer1, List<string> answers)
    {
        if (answers.Count != 3) return;

        question = questionString;
        image = sprite;
        rightAnswer = answer1;
        wrongAnswers = answers;
    }
}
