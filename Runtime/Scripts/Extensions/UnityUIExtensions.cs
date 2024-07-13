using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Toolkit
{


public static class UnityUIExtensions
{
    public static float GetTransitionDuration(this VisualElement element, string property)
    {
        var durations = element.resolvedStyle.transitionDuration.ToArray();
        var propertyIndex = element.resolvedStyle.transitionProperty.ToList().IndexOf(property);
        if (propertyIndex < durations.Length && propertyIndex >= 0)
        {
            return durations[propertyIndex].value;
        }

        return 0;
    }

    public static void AddTransition(this VisualElement element, string property, float duration, float delay, EasingMode easing = EasingMode.EaseInOut)
    {
        if (element.style.transitionProperty == null || element.style.transitionProperty.value == null)
        {
            element.style.transitionProperty = new StyleList<StylePropertyName>(new List<StylePropertyName>());
            element.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue>());
            element.style.transitionDelay = new StyleList<TimeValue>(new List<TimeValue>());
            element.style.transitionTimingFunction = new StyleList<EasingFunction>(new List<EasingFunction>());
        }

        if (element.style.transitionProperty.value.Contains(property))
        {
            var index = element.style.transitionProperty.value.IndexOf(property);
            element.style.transitionDuration.value[index] = new TimeValue(duration);
            element.style.transitionDelay.value[index] = new TimeValue(delay);
            element.style.transitionTimingFunction.value[index] = new EasingFunction(easing);
            return;
        }

        element.style.transitionProperty.value.Add(property);
        element.style.transitionDuration.value.Add(new TimeValue(duration));
        element.style.transitionDelay.value.Add(new TimeValue(delay));
        element.style.transitionTimingFunction.value.Add(new EasingFunction(easing));
        return;
    }

    public static async Task FadeOut(this VisualElement element, float duration = 0.5f)
    {
        element.AddTransition("opacity", duration, 0);
        await Awaitable.NextFrameAsync();
        element.style.opacity = 0;
        await Awaitable.WaitForSecondsAsync(duration);
        element.style.display = DisplayStyle.None;
        element.pickingMode = PickingMode.Ignore;
    }

    public static async Task FadeIn(this VisualElement element, float duration = 0.5f)
    {
        element.style.display = DisplayStyle.Flex;
        element.AddTransition("opacity", duration, 0);
        await Awaitable.NextFrameAsync();
        element.style.opacity = 1;
        await Awaitable.WaitForSecondsAsync(duration);
        element.pickingMode = PickingMode.Position;
    }

    public static async void SetTranslation(this VisualElement element, float x, float y, float duration = 0.5f)
    {
        element.AddTransition("translate", duration, 0);
        await Awaitable.NextFrameAsync();
        element.style.translate = new StyleTranslate(new Translate(x, y));
        await Awaitable.WaitForSecondsAsync(duration);
    }

    public static async void SetTranslationY(this VisualElement element, float y, float duration = 0.5f)
    {
        element.AddTransition("translate", duration, 0);
        await Awaitable.NextFrameAsync();
        element.style.translate = new StyleTranslate(new Translate(element.style.translate.value.x.value, y));
        await Awaitable.WaitForSecondsAsync(duration);
    }

    public static async void SetTranslationX(this VisualElement element, float x, float duration = 0.5f)
    {
        element.AddTransition("translate", duration, 0);
        await Awaitable.NextFrameAsync();
        element.style.translate = new StyleTranslate(new Translate(x, element.style.translate.value.y.value));
        await Awaitable.WaitForSecondsAsync(duration);
    }

    public static Vector2 GetTranslation(this VisualElement element)
    {
        return new Vector2(element.style.translate.value.x.value, element.style.translate.value.y.value);
    }

    public static float GetTranslationX(this VisualElement element)
    {
        return element.style.translate.value.x.value;
    }

    public static float GetTranslationY(this VisualElement element)
    {
        return element.style.translate.value.y.value;
    }

    public static float AddTranslationX(this VisualElement element, float x)
    {
        var current = element.GetTranslationX();
        element.SetTranslationX(current + x);
        return current + x;
    }

    public static float AddTranslationY(this VisualElement element, float y)
    {
        var current = element.GetTranslationY();
        element.SetTranslationY(current + y);
        return current + y;
    }

    public static VisualElement CloneTreeEl(this VisualTreeAsset uxml, VisualElement parent)
    {
        uxml.CloneTree(parent, out var firstIndex, out _);
        return parent[firstIndex];
    }
}

}