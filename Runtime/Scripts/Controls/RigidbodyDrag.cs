using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RigidbodyDrag : MonoBehaviour
{
    private Camera cam;
    private Rigidbody grabbedRigidbody;
    private Rigidbody targetObject;
    private ConfigurableJoint joint;

    public float spring = 50.0f;
    public float damper = 5.0f;

    private Vector3 grabbedOffset;
    private Vector3 grabbedPosition;
    private Vector3 targetPosition;


    void Start()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        targetObject = new GameObject("Target").AddComponent<Rigidbody>();
        targetObject.isKinematic = true;
        targetObject.useGravity = false;

    }

    public Vector3 CameraPlane => cam.transform.forward;

    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (grabbedRigidbody)
            {
                grabbedRigidbody = null;
                Destroy(joint);
            }

            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.rigidbody)
                {
                    grabbedRigidbody = hit.transform.GetComponentInParent<Rigidbody>();
                    grabbedOffset = hit.point - grabbedRigidbody.transform.position;
                    grabbedPosition = hit.point;
                    joint = grabbedRigidbody.gameObject.AddComponent<ConfigurableJoint>();
                    joint.connectedBody = targetObject;
                    joint.configuredInWorldSpace = true;
                    joint.xMotion = ConfigurableJointMotion.Limited;
                    joint.yMotion = ConfigurableJointMotion.Limited;
                    joint.zMotion = ConfigurableJointMotion.Limited;
                    joint.linearLimitSpring = new SoftJointLimitSpring { spring = spring, damper = damper };
                    joint.linearLimit = new SoftJointLimit { limit = 0.01f };
                    joint.anchor = grabbedRigidbody.transform.InverseTransformPoint(hit.point);
                    joint.connectedAnchor = Vector3.zero;
                    joint.autoConfigureConnectedAnchor = false;
                }
            }
        }

        var r = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (cam.transform.position - grabbedPosition).magnitude));
        targetPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (cam.transform.position - grabbedPosition).magnitude));
        
        if (Physics.Raycast(r, out var h))
        {
            // min distance point to camera plane
            if (Vector3.Distance(h.point, cam.transform.position) < Vector3.Distance(targetPosition, cam.transform.position))
            {
                targetPosition = h.point;
            }
        }

        if (grabbedRigidbody)
        {
            targetObject.position = targetPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            grabbedRigidbody = null;
            Destroy(joint);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.02f);
    }
}