using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float time = 1;

    void Update()
    {
        time -= Time.deltaTime;
        if (time <= 0)
        {
            Destroy(gameObject);
        }
    }

}
