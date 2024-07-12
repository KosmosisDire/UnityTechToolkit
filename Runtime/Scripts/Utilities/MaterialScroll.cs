using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScroll : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    public Vector2 direction = Vector2.up;
    public Material material;

    private Vector2 offset;

    // Update is called once per frame
    void Update()
    {
        offset += scrollSpeed * Time.deltaTime * direction;
        material.mainTextureOffset = offset;
    }
}
