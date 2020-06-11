using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;

    public float targetDistance = 10.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 15.0f;
    public float zoomFactor = 0.05f;

    public float xSpeed = 20.0f;
    public float ySpeed = 8.0f;
    
    private float x = 0.0f;
    private float y = 0.0f;
    private Quaternion rotation;
    private Vector3 negDistance;
    private Vector3 position;

    private float lastTouchDistance;
    
    int xsign =1;
    
    [AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
    
    void Start () 
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    
        rotation = Quaternion.Euler(y, x, 0);
        negDistance = new Vector3(0.0f, 0.0f, -targetDistance);
        position = rotation * negDistance + target.position;
        transform.rotation = rotation;
        transform.position = position;
    }

    void OnEnable ()
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    
        rotation = Quaternion.Euler(y, x, 0);
        negDistance = new Vector3(0.0f, 0.0f, -targetDistance);
        position = rotation * negDistance + target.position;
        transform.rotation = rotation;
        transform.position = position;
    }
    
    void LateUpdate () 
    {
        if(Input.touchCount == 1)
        {
            Vector3 forward = transform.TransformDirection(Vector3.up);
            Vector3 forward2 = target.transform.TransformDirection(Vector3.up);
            if (Vector3.Dot(forward,forward2) < 0)
                xsign = -1;
            else
                xsign =1;
        
            foreach (Touch touch in Input.touches) 
            {
                if (touch.phase == TouchPhase.Moved) 
                {
                    x += xsign * touch.deltaPosition.x * xSpeed *0.02f;
                    y -= touch.deltaPosition.y * ySpeed *0.02f;
                
                    rotation = Quaternion.Euler(y, x, 0);
                    negDistance.Set(0.0f, 0.0f, -targetDistance);
                    position = rotation * negDistance + target.position;
                    transform.rotation = rotation;
                    transform.position = position;
                }
            }
        }
        else if (Input.touchCount == 2)
        {
            Vector2 touch0Pos, touch1Pos;
            Touch touch0, touch1;
            float distance;
            touch0 = Input.GetTouch(0);
            touch1 = Input.GetTouch(1);
            touch0Pos = touch0.position;
            touch1Pos = touch1.position;
            distance = Vector2.Distance(touch0Pos, touch1Pos);

            if(touch0.phase == TouchPhase.Began && touch1.phase == TouchPhase.Began)
            {
                lastTouchDistance = distance;
            }
            else if (touch0.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Moved)
            {
                var delta = distance - lastTouchDistance;
                if(delta > 0)
                {
                    if(targetDistance > minDistance)
                    {
                        setZoom(delta);
                    } 
                    else if (targetDistance <= minDistance)
                    {
                        targetDistance = minDistance;
                    }
                }
                else if(delta < 0)
                {
                    if(targetDistance < maxDistance)
                    {
                        setZoom(delta);
                    }
                    else if (targetDistance >= maxDistance)
                    {
                        targetDistance = maxDistance;
                    }
                }
            }
        }
    }

    void setZoom(float delta)
    {
        targetDistance -= delta * zoomFactor;
        negDistance.Set(0.0f, 0.0f, -targetDistance);
        position = rotation * negDistance + target.position;
        transform.position = position;
    }
}
