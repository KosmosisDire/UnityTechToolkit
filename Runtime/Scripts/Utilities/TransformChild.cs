using UnityEngine;

public class TransformChild : MonoBehaviour
{
    public Transform parentTransform;
    public bool absolutePosition = false;

    Vector3 relativePosition;
    Quaternion relativeRotation;
    Vector3 relativeScale;

    public bool position = true;
    public bool rotation = true;
    public bool scale = true;

    public Vector3 positionOffset;

    private void Start() 
    {
        Set(parentTransform);
    }

    public void Set(Transform parent)
    {
        parentTransform = parent;
        if (parent == null) return;
        relativePosition = transform.position - parentTransform.position;
        relativeRotation = Quaternion.Inverse(parentTransform.rotation) * transform.rotation;
        relativeScale = new Vector3(transform.localScale.x / parentTransform.localScale.x, transform.localScale.y / parentTransform.localScale.y, transform.localScale.z / parentTransform.localScale.z);
    }

    void LateUpdate()
    {
        if (parentTransform == null) return;

        if(!absolutePosition)
        {
            if(position) transform.position = parentTransform.position + relativePosition + positionOffset;
            if(rotation) transform.rotation = parentTransform.rotation * relativeRotation;
            if(scale) transform.localScale = new Vector3(relativeScale.x * parentTransform.localScale.x, relativeScale.y * parentTransform.localScale.y, relativeScale.z * parentTransform.localScale.z);
        }
        else
        {
            if(position) transform.position = parentTransform.position + positionOffset;
            if(rotation) transform.rotation = parentTransform.rotation;
            if(scale) transform.localScale = parentTransform.localScale;
        }
    }
}