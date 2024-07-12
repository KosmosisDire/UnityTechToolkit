using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderValueMirror : MonoBehaviour
{
    public Slider slider;
    public UnityEvent<float> onValueChanged;

    void Start()
    {
        UpdateValue();
    }

    async void UpdateValue()
    {
        while (Application.isPlaying)
        {
            var listenerCount = onValueChanged.GetPersistentEventCount();
            for (int i = 0; i < listenerCount; i++)
            {
                onValueChanged.GetPersistentTarget(i).GetType().GetMethod(onValueChanged.GetPersistentMethodName(i)).Invoke(onValueChanged.GetPersistentTarget(i), new object[] { slider.value });
            }
            await Awaitable.WaitForSecondsAsync(0.2f);
        }
    }
}
