using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Game Config", menuName = "Quiz Objects/Game Config", order = 51)]
public class GameConfig : ScriptableObject
{
    public Sprite cashSprite;

    [Space]
    public int mediumPrice;
    public int hardPrice;

    [Space]
    public LocalizedString[] notifyLocales;
}
