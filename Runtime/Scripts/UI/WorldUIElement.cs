using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldUIElement : MonoBehaviour
{
    public VisualElement element;
    public Transform target;
    public Vector3 worldOffset;
    public Vector2 screenOffset;
    public UIDocument uiDoc;

    // Update is called once per frame
    void Update()
    {
        if (element == null) return;
        if (target == null) return;

        // if the target is behind the camera then hide element
        if (Vector3.Dot(target.position - Camera.main.transform.position, Camera.main.transform.forward) < 0)
        {
            element.style.display = DisplayStyle.None;
            return;
        }
        else
        {
            element.style.display = DisplayStyle.Flex;
        }

        var matchX = uiDoc.panelSettings.match;
        var matchY = 1 - uiDoc.panelSettings.match;
        var perfectRatioX = uiDoc.panelSettings.referenceResolution.x / (float)Screen.width;
        var perfectRatioY = uiDoc.panelSettings.referenceResolution.y / (float)Screen.height;

        var targetResX = Mathf.Lerp(uiDoc.panelSettings.referenceResolution.x, Screen.width * perfectRatioY, matchX);
        var targetResY = Mathf.Lerp(uiDoc.panelSettings.referenceResolution.y, Screen.height * perfectRatioX, matchY);

        var ratioX = targetResX / Screen.width;
        var ratioY = targetResY / Screen.height;

        var screenOffsetScaled = new Vector3(screenOffset.x / ratioX, screenOffset.y / ratioY, 0f);
        var screenPos = Camera.main.WorldToScreenPoint(target.position + worldOffset) + screenOffsetScaled;

        element.style.left = screenPos.x * ratioX;
        element.style.top = (Screen.height - screenPos.y) * ratioY;
        element.style.position = Position.Absolute;
    }

}
