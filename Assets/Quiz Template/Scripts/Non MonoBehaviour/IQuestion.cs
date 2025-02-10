using System.Collections.Generic;
using UnityEngine;

public interface IQuestion
{
    public string QuestionText { get; }
    public Sprite Image { get; }

    public string FirstAnswer { get; }
    public List<string> OtherAnswers { get; }
}
