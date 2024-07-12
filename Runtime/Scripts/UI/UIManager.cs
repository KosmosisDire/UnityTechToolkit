using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-10000)]
[RequireComponent(typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    public static UIDocument mainUIDocument;
    public static VisualElement rootElement;
    public static UIManager instance;

    // Start is called before the first frame update
    void Start()
    {
        if (!mainUIDocument)
        {
            mainUIDocument = GetComponent<UIDocument>();
        }

        if (!instance)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple UI Managers detected. Destroying this one.");
            Destroy(this);
        }

        
        rootElement = mainUIDocument.rootVisualElement.Q("root");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
