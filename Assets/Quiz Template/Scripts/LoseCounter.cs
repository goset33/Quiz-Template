using System.Threading;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using System.Threading.Tasks;

public class LoseCounter : AbstractObjectController
{
    private RadialProgressBar progressBar;
    private float timer = 5f;
    private TimelessController controller;
    private CancellationTokenSource cts;

    public override void Init(float t, TimelessController controller, VisualElement element)
    {
        timer = t;
        this.controller = controller;
        cts = new CancellationTokenSource();

        progressBar = element.Q<RadialProgressBar>();
        progressBar.maxValue = t;
        progressBar.value = t;

        _ = RunTimerAsync(cts.Token);
    }

    private async Task RunTimerAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (timer > 0 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();

                timer -= Time.deltaTime;
                progressBar.value = timer;
            }

            if (timer <= 0 && !cancellationToken.IsCancellationRequested)
            {
                progressBar.value = 0f;
                controller.ButtonPressed(0);
            }
        }
        catch (OperationCanceledException)
        {
            // Нормальная отмена
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка в таймере: {ex.Message}");
        }
        finally
        {
            Dispose();
        }
    }

    public override void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}