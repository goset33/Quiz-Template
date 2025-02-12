using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CounterQuestion : IQuestion
{
    private readonly string question;
    private readonly Sprite image;

    private readonly List<string> answers = new(4);


    public string QuestionText { get => question; }
    public Sprite Image { get => image; }

    public string FirstAnswer { get => answers[0]; }
    public List<string> OtherAnswers { get => answers; }

    public CounterQuestion(string questionString, Sprite sprite, string answer1, List<string> otherAnswers)
    {
        if (otherAnswers.Count != 3) return;

        question = questionString;
        image = sprite;
        answers = new() { answer1, otherAnswers[0], otherAnswers[1], otherAnswers[2] };
    }
}
