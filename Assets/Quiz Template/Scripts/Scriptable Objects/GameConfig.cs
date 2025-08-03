using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Game Config", menuName = "Quiz Objects/Game Config", order = 51)]
public class GameConfig : ScriptableObject
{
    public Sprite cashSprite, expSprite;

    [Space]
    public int[] harndessHeartCount = { 3, 2, 1, 1 }; // Количество стартовых сердец для каждого из уровней сложности
    public float questionTimer = 180f, questionExtraTime = 50f; // В секундах
    public int[][] questionsHardness = new int[][]
    {
        new[] { 4, 3, 2, 1 }, // FTUE 
        new[] { 6, 3, 1, 0 }, // Новичок
        new[] { 2, 5, 2, 1 }, // Умняша 
        new[] { 0, 3, 5, 2 }, // Академик 
        new[] { 0, 0, 0, 10 } // Гуру
    };

    [Space]
    public LocalizedString[] notifyLocales;
}
