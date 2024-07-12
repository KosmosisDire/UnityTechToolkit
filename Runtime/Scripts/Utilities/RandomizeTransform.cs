using UnityEngine;

public class RandomizeTransform : MonoBehaviour
{
    public bool randomizePosition = true;
    public bool randomizeRotation = true;
    public bool randomizeScale = true;

    public Vector3 positionRange = new Vector3(0, 0, 0);
    public Vector3 rotationRange = new Vector3(0f, 360f, 0f);
    public Vector3 scaleMin = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 scaleMax = new Vector3(0.4f, 0.4f, 0.4f);

    // Start is called before the first frame update
    void Start()
    {
        if (randomizePosition)
        {
            transform.position += new Vector3(Random.Range(-positionRange.x, positionRange.x), Random.Range(-positionRange.y, positionRange.y), Random.Range(-positionRange.z, positionRange.z));
        }
        if (randomizeRotation)
        {
            transform.rotation = Quaternion.Euler(Random.Range(-rotationRange.x, rotationRange.x), Random.Range(-rotationRange.y, rotationRange.y), Random.Range(-rotationRange.z, rotationRange.z));
        }
        if (randomizeScale)
        {
            transform.localScale = new Vector3(Random.Range(scaleMin.x, scaleMax.x), Random.Range(scaleMin.y, scaleMax.y), Random.Range(scaleMin.z, scaleMax.z));
        }
    }

}
