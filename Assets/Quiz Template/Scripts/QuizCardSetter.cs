using System;
using UnityEngine;
using UnityEngine.UI;

public class QuizCardSetter : MonoBehaviour
{
    [HideInInspector] public QuizCard cardContent;

    public ChooseController controller;

    public void SetContent(ChooseController chooseController)
    {
        controller = chooseController;
        GetComponent<Image>().sprite = cardContent.image;
    }

    public void QuizChoosed()
    {
        if (controller != null)
        {
            controller.OnQuizChoosed(transform.GetSiblingIndex());
        }
        else
        {
            throw new NullReferenceException("The content of card are not set!");
        }
    }
}
