using R3;
using UnityEngine.UIElements;
using YG;

public class SettingsController : AbstractController
{
    public override void Init()
    {
        base.Init();

        root.Query<TextField>().ForEach(field =>
        {
            field.isReadOnly = true;
            field.selectAllOnFocus = false;
            field.selectAllOnMouseUp = false;
            field.doubleClickSelectsWord = false;
            field.tripleClickSelectsLine = false;
        });

        var musicSlider = root.Q<SliderInt>("MusicSlider");
        var vfxSlider = root.Q<SliderInt>("VFXSlider");

        musicSlider.RegisterValueChangedCallback(evt => YG2.saves.musicVolume.Value = evt.newValue);
        vfxSlider.RegisterValueChangedCallback(evt => YG2.saves.vfxVolume.Value = evt.newValue);

        YG2.saves.musicVolume.AsObservable().Subscribe(value => musicSlider.value = value).AddTo(GameManager.disposables);
        YG2.saves.vfxVolume.AsObservable().Subscribe(value => vfxSlider.value = value).AddTo(GameManager.disposables);
    }
}
