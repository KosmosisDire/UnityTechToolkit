using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using SimToolkit;

[ExecuteAlways]
public class TransformGizmo : MonoBehaviour
{
    [Header("Control")]
    [SerializeField] private Transform target;
    [SerializeField] private Axis positionAxes = Axis.X | Axis.Y | Axis.Z | Axis.XY | Axis.XZ | Axis.YZ;
    [SerializeField] private Axis rotationAxes = Axis.X | Axis.Y | Axis.Z;
    [SerializeField] private bool useTangetRotation = true;

    [Tooltip("How many degrees per pixel dragged the gizmo rotates when using tangent rotation.")]
    [Range(0.01f, 1)]
    [SerializeField] private float rotationSensitivity = 0.3f;
    [SerializeField] private TransformSpace space = TransformSpace.World;

    [Header("Position Shapes")]
    [SerializeField] private LineRenderer xLine;
    [SerializeField] private LineRenderer yLine;
    [SerializeField] private LineRenderer zLine;
    [SerializeField] private MeshRenderer xHead;
    [SerializeField] private MeshRenderer yHead;
    [SerializeField] private MeshRenderer zHead;
    [SerializeField] private MeshRenderer xPlane;
    [SerializeField] private MeshRenderer yPlane;
    [SerializeField] private MeshRenderer zPlane;
    [SerializeField] private LineRenderer xPlaneOutline;
    [SerializeField] private LineRenderer yPlaneOutline;
    [SerializeField] private LineRenderer zPlaneOutline;

    [Header("Rotation Shapes")]
    [SerializeField] private LineRenderer xRotation;
    [SerializeField] private LineRenderer yRotation;
    [SerializeField] private LineRenderer zRotation;
    [SerializeField] private float rotationRadius = 1;
    [SerializeField] private int rotationSegments = 64;
    [SerializeField] private float rotationThickness = 3;

    public List<Component> allShapes => new List<Component>(){xLine, yLine, zLine, xHead, yHead, zHead, xPlane, yPlane, zPlane, xPlaneOutline, yPlaneOutline, zPlaneOutline, xRotation, yRotation, zRotation };

    [Header("Colors")]
    [SerializeField] private Color xColor;
    [SerializeField] private Color yColor;
    [SerializeField] private Color zColor;
    [SerializeField] private float planeAlpha = 0.4f;
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private float hoverBlend = 0.6f;

    [SerializeField] private Color blendColor = new(0,0,0,0);

    public event Action<Vector3, Vector3> OnMove;
    public event Action<Quaternion, Quaternion> OnRotate;
    public event Action<Axis, ControlType> OnGrab;
    public event Action<Axis, ControlType> OnRelease;

    private LineRenderer grabbedShape;
    private ControlType? grabbedType;
    private Axis? grabbedAxis;

    private bool anyIntersect;
    private Axis? grabbedTranslation;
    private Axis? grabbedPlane;
    private Axis? grabbedRotation;
    private Vector3 startingPosition;
    private Vector3 startingProjectedMousePosition;
    private Vector3 targetStartingPosition;
    private Quaternion rotation;
    private Quaternion startingRotation;
    private Vector3 startingAxis;


    // Public properties
    public bool IsGrabbed => grabbedTranslation.HasValue || grabbedPlane.HasValue || grabbedRotation.HasValue;
    public bool IsHovered => anyIntersect;
    public ControlType GrabType => grabbedType.GetValueOrDefault();
    public Axis GrabbedAxis => grabbedAxis.GetValueOrDefault();
    public Vector3 Position => transform.position;
    public Quaternion Rotation => rotation;
    public TransformSpace Space { get => space; set => space = value;}
    public Axis RotationAxes { get => rotationAxes; set => rotationAxes = value; }
    public Axis PositionAxes { get => positionAxes; set => positionAxes = value; }
    public Color BlendColor 
    { 
        get => blendColor; 
        set 
        { 
            blendColor = value; 
            RefreshColors();
        } 
    }

    public void SetPosition(Vector3 position, bool invoke = true)
    {
        if (position == transform.position) return;
        var lastPosition = transform.position;
        transform.position = position;
        if (invoke) OnMove?.Invoke(lastPosition, transform.position);
    }

    public void SetRotation(Quaternion rotation, bool invoke = true)
    {
        if (rotation == this.rotation) return;
        var lastRotation = this.rotation;
        this.rotation = rotation;
        if (invoke) OnRotate?.Invoke(lastRotation, rotation);
    }
    
    public enum TransformSpace
    {
        Local,
        World,
    }

    [Flags]
    public enum ControlType
    {
        Linear = 1,
        Planar = 2,
        Rotation = 4,
    }

    [Flags]
    public enum Axis
    {
        X = 1,
        Y = 2,
        Z = 4,
        XY = 8,
        XZ = 16,
        YZ = 32,
    }

    private void OnEnable()
    {
        // generate rotation circles
        var xRotationPoints = new List<Vector3>();
        var yRotationPoints = new List<Vector3>();
        var zRotationPoints = new List<Vector3>();

        for (int i = 0; i < rotationSegments; i++)
        {
            var angle = i * 360f / rotationSegments;
            var x = Mathf.Cos(angle * Mathf.Deg2Rad) * rotationRadius;
            var y = Mathf.Sin(angle * Mathf.Deg2Rad) * rotationRadius;
            var point = new Vector3(x, y, 0);
            xRotationPoints.Add(point);
            yRotationPoints.Add(point);
            zRotationPoints.Add(point);
        }

        // rotate the points to the correct axis
        for (int i = 0; i < rotationSegments; i++)
        {
            xRotationPoints[i] = Quaternion.Euler(0, 0, 90) * xRotationPoints[i];
            yRotationPoints[i] = Quaternion.Euler(0, 0, 0) * yRotationPoints[i];
            zRotationPoints[i] = Quaternion.Euler(0, 0, 90) * zRotationPoints[i];
        }

        xRotation.GetComponent<ScreenspaceLineRenderer>().widthMultiplier = rotationThickness;
        yRotation.GetComponent<ScreenspaceLineRenderer>().widthMultiplier = rotationThickness;
        zRotation.GetComponent<ScreenspaceLineRenderer>().widthMultiplier = rotationThickness;

        xRotation.positionCount = rotationSegments;
        yRotation.positionCount = rotationSegments;
        zRotation.positionCount = rotationSegments;

        xRotation.SetPositions(xRotationPoints.ToArray());
        yRotation.SetPositions(yRotationPoints.ToArray());
        zRotation.SetPositions(zRotationPoints.ToArray());

        xRotation.loop = true;
        yRotation.loop = true;
        zRotation.loop = true;

        xRotation.transform.localScale = Vector3.one;
        yRotation.transform.localScale = Vector3.one;
        zRotation.transform.localScale = Vector3.one;

        rotation = transform.rotation;


        xLine.name = "Line";
        yLine.name = "Line";
        zLine.name = "Line";

        xPlane.name = "Plane";
        yPlane.name = "Plane";
        zPlane.name = "Plane";

        xRotation.name = "Rotation";
        yRotation.name = "Rotation";
        zRotation.name = "Rotation";

        RefreshColors();
    }

    private void ColorAxis(Axis axis, Color? color = null)
    {
        var line = axis == Axis.X ? xLine : axis == Axis.Y ? yLine : zLine;
        var head = axis == Axis.X ? xHead : axis == Axis.Y ? yHead : zHead;
        var axisColor = axis == Axis.X ? xColor : axis == Axis.Y ? yColor : zColor;
        axisColor = Color.Lerp(axisColor, blendColor.WithAlpha(1), blendColor.a);
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.black, 0), new GradientColorKey(axisColor, 1) }, new[] { new GradientAlphaKey(0.5f, 0), new GradientAlphaKey(1, 1) });

        if (color.HasValue)
        {
            var endColor = Color.Lerp(axisColor, color.Value, hoverBlend);
            gradient.SetKeys(new[] { new GradientColorKey(Color.black, 0), new GradientColorKey(endColor, 1) }, new[] { new GradientAlphaKey(0.5f, 0), new GradientAlphaKey(1, 1) });
            line.colorGradient = gradient;

            if (Application.isPlaying)
                head.material.color = endColor;

            return;
        }


        line.colorGradient = gradient;

        if (Application.isPlaying)
            head.material.color = axisColor;
    }

    private void ColorPlane(Axis axis, Color? color = null)
    {
        var plane = axis == Axis.XY ? zPlane : axis == Axis.XZ ? yPlane : xPlane;
        var planeOutline = axis == Axis.XY ? zPlaneOutline : axis == Axis.XZ ? yPlaneOutline : xPlaneOutline;
        var axisColor = axis == Axis.XY ? zColor : axis == Axis.XZ ? yColor : xColor;
        axisColor = Color.Lerp(axisColor, blendColor.WithAlpha(1), blendColor.a);

        if (color.HasValue)
        {
            if (Application.isPlaying)
                plane.material.color = Color.Lerp(axisColor, color.Value, hoverBlend).WithAlpha(planeAlpha);

            planeOutline.startColor = planeOutline.endColor = Color.Lerp(axisColor, color.Value, hoverBlend);
            return;
        }
        
        if (Application.isPlaying)
            plane.material.color = axisColor.WithAlpha(planeAlpha);

        planeOutline.startColor = planeOutline.endColor = axisColor;
    }

    private void ColorRotation(Axis axis, Color? color = null)
    {
        var rotationObj = axis == Axis.X ? xRotation : axis == Axis.Y ? yRotation : zRotation;
        if (rotationObj == null) return;
        var axisColor = axis == Axis.X ? xColor : axis == Axis.Y ? yColor : zColor;
        axisColor = Color.Lerp(axisColor, blendColor.WithAlpha(1), blendColor.a).WithAlpha(0.3f);

        if (color.HasValue)
        {
            rotationObj.endColor = rotationObj.startColor = Color.Lerp(axisColor, color.Value, hoverBlend);
            return;
        }
        
        rotationObj.endColor = rotationObj.startColor = axisColor;
    }

    private Vector3? CheckAxisIntersection(Axis axis)
    {
        var line = axis == Axis.X ? xLine : axis == Axis.Y ? yLine : zLine;
        var head = axis == Axis.X ? xHead : axis == Axis.Y ? yHead : zHead;
        var mouse = Input.mousePosition;
        var ray = Camera.main.ScreenPointToRay(mouse);

        var headBounds = head.GetWorldBounds();

        var lineDist = line.ScreenDistance(mouse, Camera.main);

        if (lineDist < rotationThickness || headBounds.IntersectRay(ray))
        {
            Math3d.ClosestPointsOnTwoLines(out var rayPoint, out var linePoint, ray.origin, ray.direction, transform.position, line.transform.forward);
            
            return linePoint;
        }

        return null;
    }

    private Vector3? CheckPlaneIntersection(Axis axis)
    {
        var plane = axis == Axis.XY ? zPlane : axis == Axis.XZ ? yPlane : xPlane;
        var mouse = Input.mousePosition;
        var ray = Camera.main.ScreenPointToRay(mouse);

        Math3d.LinePlaneIntersection(out var intersection, ray.origin, ray.direction, plane.transform.forward, plane.transform.position);

        // check position in local space
        var localIntersection = plane.worldToLocalMatrix.MultiplyPoint(intersection);
        var localSize = plane.localBounds.size;
        var localCenter = plane.localBounds.center;

        // use the two largest dimensions as the size
        float[] localDimentions = new float[3]{localSize.x, localSize.y, localSize.z};
        float[] localCenters = new float[3]{localCenter.x, localCenter.y, localCenter.z};
        float[] localIntersections = new float[3]{localIntersection.x, localIntersection.y, localIntersection.z};

        Array.Sort(localDimentions);
        Array.Sort(localCenters);
        Array.Sort(localIntersections);
        
        var size2D = new Vector2(localDimentions[1], localDimentions[2]);
        var center2D = new Vector2(localCenters[1], localCenters[2]);
        var intersect2D = new Vector2(localIntersections[1], localIntersections[2]);

        // check if the point is within the bounds of the plane
        var distX = MathF.Abs(intersect2D.x - center2D.x);
        var distY = MathF.Abs(intersect2D.y - center2D.y);
        var contains = distX < size2D.x / 2 && distY < size2D.y / 2;

        if (contains)
        {
            return intersection;
        }

        return null;
    }

    private Vector3? CheckRotationIntersection(Axis axis)
    {
        var rotationObj = axis == Axis.X ? xRotation : axis == Axis.Y ? yRotation : zRotation;
        if (rotationObj == null) return null;
        var mouse = Input.mousePosition;
        var ray = Camera.main.ScreenPointToRay(mouse);

        var bounds = rotationObj.GetWorldBounds();
        if (bounds.IntersectRay(ray))
        {
            // check if the mouse is actually intersecting the outline of the circle
            var center = transform.position;
            var closestPointOnCircle = GetMouseProjectionOnRotation(rotationObj.transform.forward);

            Debug.DrawLine(center, closestPointOnCircle, Color.red);

            // project onto screen to compare distances
            var screenClosest = Camera.main.WorldToScreenPoint(closestPointOnCircle);
            var screenMouse = Input.mousePosition;
            var distance = Vector2.Distance(screenClosest, screenMouse);

            var rotationThicknessPx = Mathf.Min(Screen.width,Screen.height)/100f * rotationThickness;
            var intersectThickness = rotationThicknessPx < 10 ? 10 : rotationThicknessPx;
            return (distance < intersectThickness) ? closestPointOnCircle : (Vector3?)null;
        }

        return null;
    }

    private Vector3 GetMouseProjectionOnAxis(Axis axis)
    {
        var axisVector = axis == Axis.X ? transform.right : axis == Axis.Y ? transform.up : transform.forward;
        var mouse = Input.mousePosition;
        var mouseRay = Camera.main.ScreenPointToRay(mouse);
        var axisRay = new Ray(transform.position, axisVector);
        Math3d.ClosestPointsOnTwoLines(out Vector3 mousePoint, out Vector3 axisPoint, mouseRay.origin, mouseRay.direction, axisRay.origin, axisRay.direction);
        return axisPoint;
    }

    private Vector3 GetMouseProjectionOnPlane(Axis axis)
    {
        var normal = axis == Axis.XY ? transform.forward : axis == Axis.XZ ? transform.up : transform.right;
        var mouse = Input.mousePosition;
        var mouseRay = Camera.main.ScreenPointToRay(mouse);
        Math3d.LinePlaneIntersection(out Vector3 intersection, mouseRay.origin, mouseRay.direction, normal, transform.position);
        return intersection;
    }

    private Vector3 GetMouseProjectionOnRotationTangentAxis(Axis axis, Vector2? mousePosition = null)
    {
        var rotationObj = axis == Axis.X ? xRotation : axis == Axis.Y ? yRotation : zRotation;
        if (rotationObj == null) return Vector3.zero;
        var mouse = mousePosition ?? Input.mousePosition;
        var center = transform.position;
        var ray = Camera.main.ScreenPointToRay(mouse);
        var closestPointOnCircle = startingProjectedMousePosition;

        // get the vector tangent to the circle at that point
        var tangent = Vector3.Cross((closestPointOnCircle - center).normalized, startingAxis).normalized;

        // project the mouse onto that tangent
        Math3d.ClosestPointsOnTwoLines(out Vector3 mousePoint, out Vector3 tangentPoint, ray.origin, ray.direction, closestPointOnCircle, tangent);

        // draw tangent line using gizmo
        Debug.DrawLine(closestPointOnCircle, tangentPoint, Color.green);

        return tangentPoint;
    }

    private Vector3 GetMouseProjectionOnRotation(Vector3 axis, Vector2? mousePosition = null)
    {
        // get the closest point on the rotation circle
        var mouse = mousePosition ?? Input.mousePosition;
        var ray = Camera.main.ScreenPointToRay(mouse);
        var center = transform.position;
        Math3d.LinePlaneIntersection(out Vector3 intersection, ray.origin, ray.direction, axis, center);
        var closestPointOnCircle = center + rotationRadius * transform.lossyScale.x * (intersection - center).normalized;

        return closestPointOnCircle;
    }

    private void Update()
    {
        xRotation.enabled = rotationAxes.HasFlag(Axis.X);
        yRotation.enabled = rotationAxes.HasFlag(Axis.Y);
        zRotation.enabled = rotationAxes.HasFlag(Axis.Z);
        xLine.enabled = positionAxes.HasFlag(Axis.X);
        yLine.enabled = positionAxes.HasFlag(Axis.Y);
        zLine.enabled = positionAxes.HasFlag(Axis.Z);
        xHead.enabled = xLine.enabled;
        yHead.enabled = yLine.enabled;
        zHead.enabled = zLine.enabled;
        xPlane.enabled = positionAxes.HasFlag(Axis.YZ);
        yPlane.enabled = positionAxes.HasFlag(Axis.XZ);
        zPlane.enabled = positionAxes.HasFlag(Axis.XY);
        xPlaneOutline.enabled = xPlane.enabled;
        yPlaneOutline.enabled = yPlane.enabled;
        zPlaneOutline.enabled = zPlane.enabled;

        if (!Application.isPlaying) return;
        
        allShapes.Sort((a, b) => Vector3.Distance(a?.transform.position ?? Vector3.zero, Camera.main.transform.position).CompareTo(Vector3.Distance(b?.transform.position ?? Vector3.zero, Camera.main.transform.position)));

        if (!grabbedTranslation.HasValue && !grabbedPlane.HasValue && !grabbedRotation.HasValue)
        {
            CheckForGrab();
        }
        else
        {
            WhileGrabbing();
        }

        // invert plane scale to keep it facing the camera
        var mainCamera = Application.isPlaying ? Camera.main : Camera.current;
        if (mainCamera != null)
        {
            var gizmoToCam = mainCamera.transform.position - transform.position;
            var xRight = Vector3.Dot(gizmoToCam, xPlaneOutline.transform.right) < 0;
            var xUp = Vector3.Dot(gizmoToCam, xPlaneOutline.transform.up) < 0;
            var xForward = Vector3.Dot(gizmoToCam, xPlaneOutline.transform.forward) < 0;

            var yRight = Vector3.Dot(gizmoToCam, yPlane.transform.right) < 0;
            var yUp = Vector3.Dot(gizmoToCam, yPlane.transform.up) < 0;
            var yForward = Vector3.Dot(gizmoToCam, yPlane.transform.forward) < 0;

            var zRight = Vector3.Dot(gizmoToCam, zPlane.transform.right) < 0;
            var zUp = Vector3.Dot(gizmoToCam, zPlane.transform.up) < 0;
            var zForward = Vector3.Dot(gizmoToCam, zPlane.transform.forward) < 0;

            var xScaleX = MathF.Abs(xPlaneOutline.transform.localScale.x);
            var xScaleY = MathF.Abs(xPlaneOutline.transform.localScale.y);
            var xScaleZ = MathF.Abs(xPlaneOutline.transform.localScale.z);
            var yScaleX = MathF.Abs(yPlaneOutline.transform.localScale.x);
            var yScaleY = MathF.Abs(yPlaneOutline.transform.localScale.y);
            var yScaleZ = MathF.Abs(yPlaneOutline.transform.localScale.z);
            var zScaleX = MathF.Abs(zPlaneOutline.transform.localScale.x);
            var zScaleY = MathF.Abs(zPlaneOutline.transform.localScale.y);
            var zScaleZ = MathF.Abs(zPlaneOutline.transform.localScale.z);

            xPlaneOutline.transform.localScale = new Vector3(xRight ? -xScaleX : xScaleX, xUp ? -xScaleY : xScaleY, xForward ? -xScaleZ : xScaleZ);
            yPlaneOutline.transform.localScale = new Vector3(yRight ? -yScaleX : yScaleX, yUp ? -yScaleY : yScaleY, yForward ? -yScaleZ : yScaleZ);
            zPlaneOutline.transform.localScale = new Vector3(zRight ? -zScaleX : zScaleX, zUp ? -zScaleY : zScaleY, zForward ? -zScaleZ : zScaleZ);
        }

        if (space == TransformSpace.Local)
        {
            transform.rotation = rotation;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private bool GetIntersection(out LineRenderer shape, out ControlType type, out Axis axis)
    {
        bool anyIntersected = false;
        float intersectDistance = float.MaxValue;
        shape = null;
        type = ControlType.Linear;
        axis = Axis.X;

        foreach (var item in allShapes)
        {
            if (item == null) continue;
            if (item.name == "Line" && item is LineRenderer line)
            {
                var intersectedAxis = line == xLine ? Axis.X : line == yLine ? Axis.Y : line == zLine ? Axis.Z : (Axis?)null;
                if (intersectedAxis == null) continue;
                if (!positionAxes.HasFlag(intersectedAxis)) continue;
                var intersect = CheckAxisIntersection(intersectedAxis.Value);
                var distance = intersect != null ? Vector3.Distance(Camera.main.transform.position, intersect.Value) : float.MaxValue;
                if (distance < intersectDistance) 
                {
                    anyIntersected = true;
                    intersectDistance = distance;

                    axis = intersectedAxis.Value;
                    shape = line.GetComponent<LineRenderer>();
                    type = ControlType.Linear;
                }
            }
            else if (item.name == "Plane" && item is MeshRenderer plane)
            {
                var intersectedAxis = plane == xPlane ? Axis.XY : plane == yPlane ? Axis.XZ : plane == zPlane ? Axis.YZ : (Axis?)null;
                if (intersectedAxis == null) continue;
                if (!positionAxes.HasFlag(intersectedAxis)) continue;
                var intersect = CheckPlaneIntersection(intersectedAxis.Value);

                var distance = intersect != null ? Vector3.Distance(Camera.main.transform.position, intersect.Value) : float.MaxValue;
                if (distance < intersectDistance) 
                {
                    anyIntersected = true;
                    intersectDistance = distance;

                    axis = intersectedAxis.Value;
                    shape = plane.GetComponent<LineRenderer>();
                    type = ControlType.Planar;
                }
            }
            else if (item.name == "Rotation" && item is LineRenderer rotation)
            {
                var intersectedAxis = rotation == xRotation ? Axis.X : rotation == yRotation ? Axis.Y : rotation == zRotation ? Axis.Z : (Axis?)null;
                if (intersectedAxis == null) continue;
                if (!rotationAxes.HasFlag(intersectedAxis)) continue;
                var intersect = CheckRotationIntersection(intersectedAxis.Value);

                var distance = intersect != null ? Vector3.Distance(Camera.main.transform.position, intersect.Value) : float.MaxValue;
                if (distance < intersectDistance) 
                {
                    anyIntersected = true;
                    intersectDistance = distance;

                    axis = intersectedAxis.Value;
                    shape = rotation;
                    type = ControlType.Rotation;
                }
            }
        }

        RefreshColors();

        if (anyIntersected)
        {
            switch (type)
            {
                case ControlType.Linear:
                    ColorAxis(axis, hoverColor);
                    break;
                case ControlType.Planar:
                    ColorPlane(axis, hoverColor);
                    break;
                case ControlType.Rotation:
                    ColorRotation(axis, hoverColor);
                    break;
            }
        }

        return anyIntersected;
    }

    private void CheckForGrab()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        anyIntersect = GetIntersection(out var intersectedShape, out var intersectedType, out var intersectedAxis);

        if (anyIntersect)
        {
            if (Input.GetMouseButtonDown(0))
            {
                switch (intersectedType)
                {
                    case ControlType.Linear:
                        grabbedTranslation = intersectedAxis;
                        break;
                    case ControlType.Planar:
                        grabbedPlane = intersectedAxis;
                        break;
                    case ControlType.Rotation:
                        grabbedRotation = intersectedAxis;
                        break;
                }

                grabbedShape = intersectedShape;
                grabbedType = intersectedType;
                grabbedAxis = intersectedAxis;
                startingAxis = grabbedShape.transform.forward;
                startingPosition = transform.position;
                startingRotation = rotation;
                startingProjectedMousePosition = grabbedPlane.HasValue ? GetMouseProjectionOnPlane(grabbedPlane.Value) : grabbedTranslation.HasValue ? GetMouseProjectionOnAxis(grabbedTranslation.Value) : GetMouseProjectionOnRotation(startingAxis);
                if(target != null) targetStartingPosition = target.position;


                OnGrab?.Invoke(intersectedAxis, intersectedType);
            }
        }
    }

    private void WhileGrabbing()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (grabbedRotation.HasValue) ColorRotation(grabbedRotation.Value);
            if (grabbedTranslation.HasValue) ColorAxis(grabbedTranslation.Value);
            if (grabbedPlane.HasValue) ColorPlane(grabbedPlane.Value);
            grabbedRotation = null;
            grabbedTranslation = null;
            grabbedPlane = null;

            OnRelease?.Invoke(grabbedAxis.Value, grabbedType.Value);
            grabbedShape = null;
            grabbedTranslation = null;
            grabbedType = null;
        }

        if (grabbedRotation != null || grabbedTranslation != null || grabbedPlane != null)
        {
            OnDrag();
        }
    }

    private void OnDrag()
    {
        var rotationAxis = grabbedRotation.GetValueOrDefault();
        var axis = grabbedTranslation.GetValueOrDefault();
        var plane = grabbedPlane.GetValueOrDefault();

        if (grabbedRotation.HasValue) ColorRotation(grabbedRotation.Value, hoverColor);
        if (grabbedTranslation.HasValue) ColorAxis(grabbedTranslation.Value, hoverColor);
        if (grabbedPlane.HasValue) ColorPlane(grabbedPlane.Value, hoverColor);
        
        var mouseProjection = Vector3.zero;
        if (grabbedRotation != null) mouseProjection = useTangetRotation ? GetMouseProjectionOnRotationTangentAxis(rotationAxis) : GetMouseProjectionOnRotation(startingAxis);
        if (grabbedTranslation != null) mouseProjection = GetMouseProjectionOnAxis(axis);
        if (grabbedPlane != null) mouseProjection = GetMouseProjectionOnPlane(plane);

        if (grabbedRotation == null)
        {
            var delta = mouseProjection - startingProjectedMousePosition;
            var newPosition = startingPosition + delta;
            if (newPosition != transform.position)
            {
                var lastPosition = transform.position;
                transform.position = newPosition;
                if (target != null) target.position = targetStartingPosition + delta;
                OnMove?.Invoke(lastPosition, transform.position);
            }
        }
        else
        {
            if (useTangetRotation)
            {
                var lastRotation = rotation;

                var screenStarting = Camera.main.WorldToScreenPoint(startingProjectedMousePosition);
                var screenCurrent = Camera.main.WorldToScreenPoint(mouseProjection);

                var delta = Vector3.Distance(screenCurrent, screenStarting);
                var centerToMouse = mouseProjection - transform.position;
                var centerToStart = startingProjectedMousePosition - transform.position;
                var sign = Vector3.SignedAngle(centerToMouse, centerToStart, startingAxis) > 0 ? 1 : -1;
                var newRotation = Quaternion.AngleAxis(-sign * delta * rotationSensitivity, startingAxis);
                rotation = newRotation * startingRotation;
                if (target != null) target.rotation = rotation;
                OnRotate?.Invoke(lastRotation, rotation);
            }
            else
            {
                var center = transform.position;
                var lastRotation = rotation;
                var startingVector = startingProjectedMousePosition - center;
                var newVector = mouseProjection - center;
                var newRotation = Quaternion.FromToRotation(startingVector, newVector);

                // add the new rotation to the starting rotation
                rotation = newRotation * startingRotation;
                if (target != null) target.rotation = rotation;
                OnRotate?.Invoke(lastRotation, rotation);
            }
        }
    }

    public void RefreshColors()
    {
        ColorAxis(Axis.X);
        ColorAxis(Axis.Y);
        ColorAxis(Axis.Z);
        ColorPlane(Axis.XY);
        ColorPlane(Axis.XZ);
        ColorPlane(Axis.YZ);
        ColorRotation(Axis.X);
        ColorRotation(Axis.Y);
        ColorRotation(Axis.Z);
    }

    public static TransformGizmo Create(Transform target = null)
    {
        // instantiate from resources
        var gizmo = Instantiate(Resources.Load<TransformGizmo>("TransformGizmo"));
        if (target != null)
        {
            gizmo.SetPosition(target.position);
            gizmo.SetRotation(target.rotation);
        }
        gizmo.target = target;
        return gizmo;
    }
}
