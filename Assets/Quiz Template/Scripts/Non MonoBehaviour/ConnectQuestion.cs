using System.Linq;
using UnityEngine;

public class ConnectQuestion : IQuestion
{
    private readonly string question;
    private readonly Sprite image;

    private readonly string[] firstPart = new string[4];
    private readonly string[] secondPart = new string[4];

    private readonly string[] allPairs = new string[8];

    public string QuestionText { get => question; }
    public Sprite Image { get => image; }

    public string[] FirstPart { get => firstPart; }
    public string[] SecondPart { get => secondPart; }

    public string[] AllAnswers { get => allPairs; }

    public ConnectQuestion(string questionString, Sprite sprite, string[] answers)
    {
        if (answers.Length != 8) return;

        question = questionString;
        image = sprite;
        firstPart = answers.Take(4).ToArray();
        secondPart = answers.TakeLast(4).ToArray();
        allPairs = answers;
    }
}
