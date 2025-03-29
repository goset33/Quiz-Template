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
    public LocalizedString title, text, button1, button2;

    public PopupSettings(PopupSize _size, LocalizedString _title, LocalizedString _text, LocalizedString _button1, LocalizedString _button2)
    {
        size = _size;
        title = _title;
        text = _text;
        button1 = _button1;
        button2 = _button2;
    }

    public PopupSettings(PopupSize _size, LocalizedString[] strings)
    {
        if (strings == null || strings.Length != 4) return; 

        size = _size;
        title = strings[0];
        text = strings[1];
        button1 = strings[2];
        button2 = strings[3];
    }
}
