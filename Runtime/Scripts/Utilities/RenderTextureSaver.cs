using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureSaver : MonoBehaviour
{
    public RenderTexture renderTexture;
    public string savePath = "Assets/SavedRenderTexture.png";
    public TextureFormat textureFormat = TextureFormat.RGBA32;

    [ContextMenu("Save Render Texture")]
    public void SaveRenderTexture()
    {
        Debug.Log(renderTexture.width + "x" + renderTexture.height);
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, textureFormat, false);
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(savePath, bytes);
    }
}
