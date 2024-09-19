using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    [SerializeField] Transform firstPoint;
    [SerializeField] Transform secondPoint;

    public void CreateControlPoint(GameObject prefab, bool first)//first = p1 and !first = p2
    { 
        if (first)
        {
            firstPoint = Instantiate(prefab, transform).transform;
            firstPoint.name = "Control First Point";
        }
        else
        {
            secondPoint = Instantiate(prefab, transform).transform;
            secondPoint.name = "Control Second Point";
        }
    }

    public Vector3 GetFirstPoint() { return firstPoint.position; }
    public Vector3 GetSecondPoint() {  return secondPoint.position; }
    public Vector3 GetPosition() { return transform.position; }
}
