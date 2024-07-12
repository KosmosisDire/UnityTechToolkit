using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    public Vector3 towardsAxis = Vector3.forward;

    // Update is called once per frame
    void Update()
    {
        var camera = Application.isPlaying ? Camera.main : Camera.current;
        if (camera != null)
        {
            var rotation = Quaternion.LookRotation(camera.transform.position - transform.position, camera.transform.TransformDirection(towardsAxis));
            transform.rotation = rotation * Quaternion.Euler(0, 90, 0);
        }
    }
}
