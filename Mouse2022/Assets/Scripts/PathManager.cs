using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathManager : MonoBehaviour
{
    [SerializeField] AnimationCurve curve;
    [SerializeField] bool running = false;
    [SerializeField] Transform plane;
    [SerializeField] GameObject anchorPrefab;
    [SerializeField] GameObject controlPointPrefab;
    [SerializeField] List<Transform> bezierPoints = new List<Transform>();

    // Pontos de controle
    //p0 Ponto inicial
    //p1 Primeiro ponto de controle
    //p2 Segundo ponto de controle
    //p3 Ponto final

    // Quantidade de segmentos da curva (quanto maior, mais detalhada)
    public int curveResolution = 20;
    [SerializeField] protected float duration = 5f;
    public Transform mouse;

    private Camera mainCamera;
    private GameObject selectedObject;
    private bool isDragging = false;

    [SerializeField, Range(0f, 1f)] private float raceProgress = 0f;
    private int targetAnchor;
    private float lookAheadDistance = .01f;

    private List<float> arcLengthTable;  // For arc length parameterization
    private float totalLength = 0f;      // Total length of the curve

    bool editMode = false;
    bool spawnMode = true;
    bool playMode = false;

    void Start()
    {
        // Ensure the main camera is assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        targetAnchor = 3;
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0) && !playMode) // 0 is for left mouse button
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // Create a ray from the camera to the mouse position
                RaycastHit hit;

                // Perform the raycast, and check if it hits a collider on the plane
                if (Physics.Raycast(ray, out hit))
                {
                    // Check if the object hit is the plane
                    if (spawnMode && hit.transform == plane)
                    {
                        Transform anchor;
                        // Spawn the prefab at the hit point on the plane
                        //anchor = Instantiate(anchorPrefab, hit.point, Quaternion.identity).transform;
                        anchor = Instantiate(anchorPrefab, transform).transform;
                        anchor.position = hit.point;
                        if (bezierPoints.Count > 0)
                        {
                            CreateControlPoints(bezierPoints[bezierPoints.Count - 1], anchor);
                        }
                        bezierPoints.Add(anchor);
                    }
                    else
                    {
                        if (editMode && hit.collider.gameObject.CompareTag("Draggable"))
                        {
                            selectedObject = hit.collider.gameObject;
                            isDragging = true;
                        }
                    }
                }
            }

            if (editMode)
            {
                DragAndDrop();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) || playMode)
        {
            if (bezierPoints.Count >= 4)
            {
                ComputeArcLength(bezierPoints[targetAnchor - 3], bezierPoints[targetAnchor - 2], bezierPoints[targetAnchor - 1], bezierPoints[targetAnchor]);
                running = true;
                if (!playMode) { playMode = true; }
            }
        }

        if (!isDragging && playMode)
            RunOnPath();
    }

    void OnDrawGizmos()
    {
        if(bezierPoints.Count >= 4)
        {
            for(int i = 0; i < bezierPoints.Count - 1; i += 3)
            {
                CubicBezierCurve(bezierPoints[i], bezierPoints[i+1], bezierPoints[i+2], bezierPoints[i+3]);
            }
        }
    }

    // Compute the total arc length of a Bézier curve segment
    void ComputeArcLength(Transform p0, Transform p1, Transform p2, Transform p3)
    {
        arcLengthTable = new List<float> { 0f }; // Reset table
        totalLength = 0f;
        Vector3 previousPoint = p0.position;

        for (int i = 1; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector3 currentPoint = GetPointOnBezierCurve(p0.position, p1.position, p2.position, p3.position, t);

            float segmentLength = Vector3.Distance(previousPoint, currentPoint);
            totalLength += segmentLength;
            arcLengthTable.Add(totalLength); // Store cumulative arc length

            previousPoint = currentPoint;
        }
    }

    // Find the parameter t corresponding to a given arc length (s)
    float GetTForArcLength(float s)
    {
        float targetLength = s * totalLength;
        for (int i = 1; i < arcLengthTable.Count; i++)
        {
            if (arcLengthTable[i] >= targetLength)
            {
                float t1 = (i - 1) / (float)curveResolution;
                float t2 = i / (float)curveResolution;

                // Linear interpolation to find t
                float length1 = arcLengthTable[i - 1];
                float length2 = arcLengthTable[i];

                float t = Mathf.Lerp(t1, t2, (targetLength - length1) / (length2 - length1));
                return t;
            }
        }
        return 1f;
    }

    Vector3 GetPointOnBezierCurve( Vector3 p0, Vector3 p1,  Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float t2 = t * t;
        float u2 = u * u;
        float u3 = u2 * u;
        float t3 = t2 * t;

        Vector3 result =
            (u3) * p0 +
            (3f * u2 * t) * p1 +
            (3f * u * t2) * p2 +
            (t3) * p3;

        return result;
    }

    void CubicBezierCurve(Transform p0, Transform p1, Transform p2, Transform p3)
    {
        Vector3 previousPoint = p0.position;

        for (int i = 1; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector3 currentPoint = GetPointOnBezierCurve(p0.position, p1.position, p2.position, p3.position, t);

            //float arcLengthProgress = t;
            //float easing = curve.Evaluate(arcLengthProgress);
            //t = GetTForArcLength(easing);

            //Vector3 currentposition = GetPointOnBezierCurve(p0.position, p1.position, p2.position, p3.position, t);


            // Desenha uma linha entre os pontos calculados da curva
            //Gizmos.DrawLine(previousPoint, currentPoint);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(currentPoint, .25f);
            
            //Gizmos.color = Color.black;
            //Gizmos.DrawSphere(currentposition, .25f);
            previousPoint = currentPoint;
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(p0.position, p1.position);
        Gizmos.DrawLine(p3.position, p2.position);
    }

    void CreateControlPoints(Transform p0, Transform p3)
    {
        Transform p1, p2;
        // Instancia o prefab dos pontos de controle na cena
        p1 = Instantiate(controlPointPrefab, p0.transform).transform;
        p2 = Instantiate(controlPointPrefab, p3.transform).transform;

        p1.position = Vector3.Lerp(p0.position, p3.position, 0.3f);
        p2.position = Vector3.Lerp(p0.position, p3.position, 0.7f);

        bezierPoints.Add(p1);
        bezierPoints.Add(p2);
    }

    void DragAndDrop()
    {
        if (isDragging && Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast to the plane to get the dragging position
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the ray hit the plane
                if (hit.transform == plane)
                {
                    // Move the selected object to the position on the plane where the ray hits
                    selectedObject.transform.position = hit.point;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            selectedObject = null;
        }
    }

    void RunOnPath()
    {
        if (running)
        {
            if (raceProgress < 1f)
            {               
                float arcLengthProgress = raceProgress;
                float easing = curve.Evaluate(arcLengthProgress);
                float t = GetTForArcLength(easing);

                Vector3 currentposition = GetPointOnBezierCurve(bezierPoints[targetAnchor - 3].position, bezierPoints[targetAnchor - 2].position, bezierPoints[targetAnchor - 1].position, bezierPoints[targetAnchor].position, t);
                mouse.position = currentposition;

                Vector3 targetposition = GetPointOnBezierCurve(bezierPoints[targetAnchor - 3].position, bezierPoints[targetAnchor - 2].position, bezierPoints[targetAnchor - 1].position, bezierPoints[targetAnchor].position, Mathf.Clamp01(t + lookAheadDistance));
                mouse.LookAt(targetposition);

                raceProgress += Time.deltaTime / duration;               
            }
            else
            {
                if (bezierPoints.Count - 1 >= targetAnchor + 3)
                {
                    targetAnchor += 3;
                }
                else
                {
                    playMode = running = false;
                    targetAnchor = 3;
                }
                raceProgress = 0f;
            }
        }
        

    }    
    
    public void EditButton()
    {
        editMode = true;
        playMode = spawnMode = false;
    }

    public void SpawnButton()
    {
        spawnMode = true;
        playMode = editMode = false;
    }

    public void PlayButton()
    {
        playMode = true;
        editMode = spawnMode = false;
        if (bezierPoints.Count >= 4)
        {
            ComputeArcLength(bezierPoints[targetAnchor - 3], bezierPoints[targetAnchor - 2], bezierPoints[targetAnchor - 1], bezierPoints[targetAnchor]);
            running = true;
        }
    }

    public void ResetButton()
    {
        bezierPoints.Clear();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnMode = true;
        playMode = editMode = false;
    }

    public void EasingCurve(int ease)
    {
        switch (ease)
        {
            case 0: SetCurveTypeLinear();
                break;
            case 1: SetCurveTypeEaseIn();
                break;
            case 2: SetCurveTypeEaseOut();
                break;
        }
    }

    void SetCurveTypeLinear()
    {
        for (int i = 0; i < curve.length; i++)
        {
            Keyframe key = curve[i];

            key.inTangent = 1f;
            key.outTangent = 1f;

            curve.MoveKey(i, key);
        }
    }

    void SetCurveTypeEaseIn()
    {
        for (int i = 0; i < curve.length; i++)
        {
            Keyframe key = curve[i];

            key.inTangent = 2f;  // Steeper tangent at the start (more curve)

            curve.MoveKey(i, key);
        }
    }

    void SetCurveTypeEaseOut()
    {
        for (int i = 0; i < curve.length; i++)
        {
            Keyframe key = curve[i];

            key.outTangent = 0f;  // Flattening out at the end

            curve.MoveKey(i, key);
        }
    }
}
