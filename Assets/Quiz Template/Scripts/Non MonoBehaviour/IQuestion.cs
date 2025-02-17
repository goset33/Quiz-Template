using UnityEngine;

public interface IQuestion
{
    public string QuestionText { get; }
    public Sprite Image { get; }
    public string[] AllAnswers { get; }
}
