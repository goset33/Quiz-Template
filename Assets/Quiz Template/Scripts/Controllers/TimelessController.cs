using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class TimelessController : AbstractController
{
	private Label notificationText;
	private VisualElement popupElementBackground, popupElement;

	private Label popupHeader, popupText;
	private VisualElement insertedElement;
	private Button firstNotPreferred, secondPreferred;

    private Action<int> callback;

	public override void Init()
	{
		base.Init();

		notificationText = root.Q<Label>("Notification");
        popupElementBackground = root.Q<VisualElement>("PopupBG");
        popupElement = root.Q<VisualElement>("Popup");

        popupHeader = popupElement.Q<Label>("Header");
		popupText = popupElement.Q<Label>("PopupText");
		insertedElement = popupElement.Q<VisualElement>("InsertedElement");
		firstNotPreferred = popupElement.Q<Button>("FirstNotPreferred");
		secondPreferred = popupElement.Q<Button>("SecondPreferred");

		popupElementBackground.AddToClassList("hided");
		notificationText.AddToClassList("hided");

        firstNotPreferred.clicked += () => ButtonPressed(0);
		secondPreferred.clicked += () => ButtonPressed(1);
	}

	/// <summary>
    /// Создает окно в соответствии с настройками
	/// </summary>
    /// <param name="settings">Настройки окна в виде специального класса</param>
	public void CreatePopup(PopupSettings settings, Action<int> callback)
	{

		// Для плавного появления: при создании добавлять класс который будет менять opacity до 1, при том анимация увеличения окна с Ease-In

		ClearPopup();

		this.callback = callback;
        SoundManager.Instance.ChangeMusicState();

		popupElement.AddToClassList(PopupSettings.sizeClasses[settings.size]);

		popupHeader.text = settings.title.GetLocalizedString();
		popupText.text = settings.text.GetLocalizedString();
		firstNotPreferred.text = settings.button1.GetLocalizedString();
		secondPreferred.text = settings.button2.GetLocalizedString();

		if (settings.objectElement != null && settings.objectController != null && (int) settings.size >= 1500)
		{
			VisualElement element = settings.objectElement.CloneTree();

			insertedElement.RemoveFromClassList("inserted-element--hide");
            insertedElement.Add(element);

			settings.objectController.Init(5f, this, element);
            this.callback += _ => settings.objectController.Dispose();
		}

		popupElementBackground.RemoveFromClassList("hided");
    }

	public void ButtonPressed(int index)
	{
		ClearPopup();
        SoundManager.Instance.ChangeMusicState();

		callback?.Invoke(index);
		callback = null;
	}

	private void ClearPopup()
	{
		popupElementBackground.AddToClassList("hided");

        insertedElement.AddToClassList("inserted-element--hide");
        if (insertedElement.childCount > 0)
		{
            insertedElement.Clear();
		}

		HashSet<string> classes = popupElement.GetClasses().ToHashSet();
        foreach (var _class in classes)
		{
			popupElement.RemoveFromClassList(_class);
		}
	}

	/// <summary>
    /// Создает всплывающий текст-уведомление
	/// </summary>
    /// <param name="localizedString">Локализованная строка текста, который будет показан</param>
	public void CreateNotification(LocalizedString localizedString)
	{
		notificationText.text = localizedString.GetLocalizedString();
		notificationText.RemoveFromClassList("hided");

		notificationText.AddToClassList("notification--transition");
		notificationText.schedule.Execute(() => {
			notificationText.AddToClassList("hided");
			notificationText.RemoveFromClassList("notification--transition");
		}).StartingIn(2001L);
	}
}

public class PopupSettings
{
	public enum PopupSize
	{
		Small = 500,
		Medium = 1000,
		Big = 1500,
		Large = 1800
	}

	public static Dictionary<PopupSize, string> sizeClasses = new() 
	{ { PopupSize.Small, "popup-small" }, { PopupSize.Medium, "popup-medium" }, { PopupSize.Big, "popup-big" }, { PopupSize.Large, "popup-large" } };

	public PopupSize size;
	public VisualTreeAsset objectElement;
	public AbstractObjectController objectController;
	public LocalizedString title, text, button1, button2;

	public PopupSettings(PopupSize _size, VisualTreeAsset element, AbstractObjectController objectController, LocalizedString[] strings)
	{
		if (strings == null || strings.Length != 4) return;

		size = _size;
		objectElement = element;
		this.objectController = objectController;
        title = strings[0];
		text = strings[1];
		button1 = strings[2];
		button2 = strings[3];
	}

	public PopupSettings(PopupSize _size, LocalizedString[] strings) : this(_size, null, null, strings) { }
}