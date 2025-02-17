using UnityEngine;

public class CounterQuestion : IQuestion
{
    private readonly string question;
    private readonly Sprite image;

    private readonly string[] answers = new string[4];


    public string QuestionText { get => question; }
    public Sprite Image { get => image; }

    public string[] AllAnswers { get => answers; }

    public CounterQuestion(string questionString, Sprite sprite, string[] allAnswers)
    {
        if (allAnswers.Length != 4) return;

        question = questionString;
        image = sprite;
        answers = allAnswers;
    }
}
