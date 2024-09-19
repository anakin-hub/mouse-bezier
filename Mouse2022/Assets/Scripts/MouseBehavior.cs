using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseBehavior : MonoBehaviour
{
    [SerializeField] public AnimationCurve curve;
    public Transform targetPos;
    [SerializeField] Transform targetPosCopy;
    Vector3 currentTarget;
    [SerializeField] float totalMovementTime = 10000;
    [SerializeField] float currentMovementTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        targetPosCopy = new GameObject().transform;
        targetPosCopy.position = new Vector3(targetPos.position.x, this.transform.position.y, targetPos.position.z);
        this.transform.LookAt(targetPosCopy);
    }


    // Update is called once per frame
    void Update()
    {
        if (currentMovementTime < totalMovementTime)
        {
            float normalizedProgress = currentMovementTime / totalMovementTime; // 0-1
            float easing = curve.Evaluate(normalizedProgress);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosCopy.position, easing);
            currentMovementTime++;
        }
    }
}
