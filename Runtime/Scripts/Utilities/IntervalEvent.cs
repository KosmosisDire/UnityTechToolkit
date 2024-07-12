using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class IntervalEvent : MonoBehaviour
{
    public UnityEvent onInterval;
    public float interval = 1f;
    public bool startOnAwake = true;
    public CancellationTokenSource cts;

    private void Start()
    {
        if (startOnAwake)
        {
            StartInterval();
        }
    }

    public void StartInterval()
    {
        cts = new CancellationTokenSource();
        Interval();
    }

    public void StopInterval()
    {
        cts.Cancel();
    }

    private async void Interval()
    {
        while (!cts.Token.IsCancellationRequested)
        {
            onInterval.Invoke();
            await Awaitable.WaitForSecondsAsync(interval);
        }
    }

}
