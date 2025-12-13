using System;
using System.Collections;
using System.Threading.Tasks;
using R3;
using UnityEngine;

public class Delayer
{
    protected MonoBehaviour target;

    public Delayer(MonoBehaviour target)
    {
        this.target = target;
    }

    public virtual Coroutine SetTimeout(Action callback, float seconds)
    {
        if (target.isActiveAndEnabled == false)
        {
            return null;
        }

        return target.StartCoroutine(GetTimedDelayEnumerator(callback, seconds));
    }

    public virtual async Task SetTimeoutAsync(float seconds)
    {
        if (target.isActiveAndEnabled == false)
        {
            return;
        }

        var onCompleteSubject = new ReplaySubject<Unit>(1);

        target.StartCoroutine(GetTimedDelayEnumerator(() => { onCompleteSubject.OnNext(Unit.Default); }, seconds));

        await onCompleteSubject.FirstAsync();
    }

    public virtual Coroutine SetInterval(Action callback, float seconds)
    {
        if (target.isActiveAndEnabled == false)
        {
            return null;
        }

        return target.StartCoroutine(GetIntervalEnumerator(callback, seconds));
    }

    public virtual Coroutine DelayForOneFrame(Action callback)
    {
        if (target.isActiveAndEnabled == false)
        {
            return null;
        }

        return target.StartCoroutine(GetFrameDelayEnumerator(callback));
    }

    public virtual Coroutine IntervalForOneFrame(Action callback)
    {
        if (target.isActiveAndEnabled == false)
        {
            return null;
        }

        return target.StartCoroutine(GetFrameIntervalEnumerator(callback));
    }

    protected IEnumerator GetIntervalEnumerator(Action callback, float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            callback();
        }
    }

    protected IEnumerator GetFrameIntervalEnumerator(Action callback)
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            callback();
        }
    }

    protected IEnumerator GetTimedDelayEnumerator(Action callback, float delay)
    {
        yield return new WaitForSeconds(delay);
        callback();
    }

    protected IEnumerator GetFrameDelayEnumerator(Action callback)
    {
        yield return new WaitForEndOfFrame();
        callback();
    }
}