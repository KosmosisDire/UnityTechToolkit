using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Toolkit
{

    [DefaultExecutionOrder(-1000)]
    public class NotificationManager : MonoBehaviour
    {
        public VisualTreeAsset notificationUxml;
        public string textContainerName = "text";
        public float spacing = 5;
        public float animationLength = 0.4f;


        public static VisualElement notificationPositioner;
        public static VisualElement notificationContainer;
        public static List<VisualElement> notifications = new List<VisualElement>();
        public static List<VisualElement> notificationsToRemove = new List<VisualElement>();
        public static NotificationManager instance;

        public void Start()
        {
            if (!instance)
            {
                instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple Notification Managers detected. Destroying this one.");
                Destroy(this);
            }

            if (!notificationUxml) Debug.LogError("Notification UXML is not set.");

            notificationPositioner = new VisualElement();
            notificationPositioner.name = "notification-positioner";
            notificationPositioner.style.position = Position.Absolute;
            notificationPositioner.style.flexDirection = FlexDirection.Column;
            notificationPositioner.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            notificationPositioner.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            notificationPositioner.style.justifyContent = Justify.FlexStart;
            notificationPositioner.style.alignItems = Align.FlexEnd;
            notificationPositioner.pickingMode = PickingMode.Ignore;

            UIManager.rootElement.Add(notificationPositioner);
        }

        public void UpdatePositions()
        {
            var height = spacing;
            for (int i = 0; i < notifications.Count; i++)
            {
                var notification = notifications[i];
                notification.SetTranslation(-5, height, animationLength);
                height += spacing;

                if (!notificationsToRemove.Contains(notification))
                    height += notification.layout.height;
            }
        }

        public async void Notify(string text, float duration, Action<VisualElement> callback = null)
        {
            if (string.IsNullOrEmpty(text)) return;

            Debug.Log(text);

            notificationPositioner.BringToFront();
            var notification = notificationUxml.CloneTreeEl(notificationPositioner);
            notification.SendToBack();
            notification.Q<Label>(textContainerName).text = text;
            notifications.Insert(0, notification);
            notification.style.position = Position.Absolute;
            notification.style.translate = new StyleTranslate(new Translate(0, new Length(-100, LengthUnit.Percent)));

            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();
            callback?.Invoke(notification);
            notification.FadeIn(animationLength);
            UpdatePositions();

            using var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            // dismiss notification on click
            notification.RegisterCallback<ClickEvent>((evt) =>
            {
                tokenSource.Cancel();
            });

            try
            {
                await Awaitable.WaitForSecondsAsync(duration + animationLength, token);
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }

            notificationsToRemove.Add(notification);
            notification.FadeOut(animationLength * 0.7f);
            UpdatePositions();

            await Awaitable.WaitForSecondsAsync(animationLength * 0.7f);
            

            notifications.Remove(notification);
            notificationsToRemove.Remove(notification);
            notification.RemoveFromHierarchy();
        }

        public void Notify(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            Notify(text, 5f);
        }

        public static void Notice(string text, float duration, Action<VisualElement> callback = null)
        {
            if (string.IsNullOrEmpty(text)) return;
            instance.Notify(text, duration, callback);
        }

        public static void Notice(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            instance.Notify(text);
        }
        
    }
}
