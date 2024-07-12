using UnityEngine;
using UnityEngine.UIElements;

public static class Icons
{
    public static VectorImage GetVectorIcon(string name)
    {
        return Resources.Load<VectorImage>($"Icons/{name}");
    }
}