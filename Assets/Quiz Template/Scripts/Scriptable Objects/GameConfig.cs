using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Game Config", menuName = "Quiz Objects/Game Config", order = 51)]
public class GameConfig : ScriptableObject
{
    public Sprite cashSprite, expSprite;

    [Space]
    public int[] harndessHeartCount = { 3, 2, 1, 1 };

    [Space]
    public LocalizedString[] notifyLocales;
}
