using System.Collections.Generic;
using System.Threading.Tasks;
using SimToolkit;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[System.Serializable]
public class ToolbarItem<T> 
{
    [HideInInspector] public Button buttonEl;
    public VectorImage icon;
    public string text;
    [Space(30)]
    public UnityEvent<T> onClick;

    public ToolbarItem(VectorImage icon, string text, UnityAction<T> onClick)
    {
        this.icon = icon;
        this.text = text;
        this.onClick = new UnityEvent<T>();
        
        if (Application.isPlaying)
        {
            this.onClick.AddListener(onClick);
        }
        else
        {
            try
            {
                #if UNITY_EDITOR
                UnityEventTools.AddPersistentListener(this.onClick, onClick);
                #endif
            }
            catch
            {
                Debug.LogWarning("Could not add persistent listener to toolbar item (make sure not to add a lambda function)");
            }
        }
    }

    public ToolbarItem()
    {
    }

    public void Create(T data)
    {
        buttonEl = new Button();
        buttonEl.AddToClassList("toolbar-item");
        if (icon)
        {
            var image = new Background();
            image.vectorImage = icon;
            buttonEl.iconImage = image;
            if (string.IsNullOrEmpty(text)) buttonEl.AddToClassList("icon-only");
            else
            {
                buttonEl.AddToClassList("button-text-icon");
            }
        }

        if (!string.IsNullOrEmpty(text))
        {
            buttonEl.text = text;
        }

        var item = this;
        buttonEl.RegisterCallback<ClickEvent>((e) => item.onClick.Invoke(data));
    }

    public ToolbarItem<T> Clone()
    {
        return new ToolbarItem<T>()
        {
            buttonEl = buttonEl,
            icon = icon,
            onClick = onClick
        };
    }
}

public class Toolbar<T>
{
    public VisualElement container;
    private List<ToolbarItem<T>> items = new List<ToolbarItem<T>>();
    private List<T> data = new List<T>();

    public void AddItem(ToolbarItem<T> item, T data)
    {
        items.Add(item);
        this.data.Add(data);
    }

    public void Create(VisualElement parent)
    {
        container = new VisualElement();
        container.AddToClassList("toolbar");
        parent.Add(container);

        for (int i = 0; i < items.Count; i++)
        {
            items[i].Create(data[i]);
            container.Add(items[i].buttonEl);
        }
    }

    public WorldUIElement Follow(Transform transform, Vector2 offset = default)
    {
        container.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), 0));
        var toolbarPosition = UIManager.instance.gameObject.AddComponent<WorldUIElement>();
        toolbarPosition.uiDoc = UIManager.mainUIDocument;
        toolbarPosition.element = container;
        toolbarPosition.target = transform;
        toolbarPosition.screenOffset = offset;
        return toolbarPosition;
    }

    public async Task Hide(float duration = 0.5f)
    {
        await container.FadeOut(duration);
    }
    
    public async Task Show(float duration = 0.5f)
    {
        await container.FadeIn(duration);
    }

    public async void Delete()
    {
        container.RemoveFromHierarchy();
    }
}
