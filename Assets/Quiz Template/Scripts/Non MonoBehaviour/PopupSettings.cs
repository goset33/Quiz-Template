using UnityEngine;
using UnityEngine.Localization;

public class PopupSettings
{
    public enum PopupSize 
    { 
        Small = 500,
        Medium = 1000,
        Big = 1500,
        Large = 1800
    }

    public PopupSize size;
    public GameObject objectPrefab;
    public LocalizedString title, text, button1, button2;

    public PopupSettings(PopupSize _size, GameObject prefab, LocalizedString[] strings)
    {
        if (strings == null || strings.Length != 4) return;

        size = _size;
        objectPrefab = prefab;
        title = strings[0];
        text = strings[1];
        button1 = strings[2];
        button2 = strings[3];
    }

    public PopupSettings(PopupSize _size, GameObject prefab, LocalizedString _title, LocalizedString _text, LocalizedString _button1, LocalizedString _button2) : this(_size, prefab, new LocalizedString[4] { _title, _text, _button1, _button2 }) { }

    public PopupSettings(PopupSize _size, LocalizedString[] strings) : this(_size, null, strings) { }
}
