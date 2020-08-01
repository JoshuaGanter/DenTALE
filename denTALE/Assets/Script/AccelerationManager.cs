using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnShakeStart();
public delegate void OnShakeEnd();

public class AccelerationManager : MonoBehaviour
{
    public float shakeThreshold;
    public float shakeDegradation;
    public static event OnShakeStart ShakeStarted;
    public static event OnShakeEnd ShakeEnded;

    private float degradingShakeThreshold;
    private float currentShakeLevel;
    private float maxShakeLevel;
    private bool isShaking = false;

    void Start()
    {
        degradingShakeThreshold = shakeThreshold * 0.25f;
        maxShakeLevel = shakeThreshold * 1.5f;
    }

    void Update()
    {
        currentShakeLevel += Input.acceleration.sqrMagnitude - shakeDegradation;
        if (currentShakeLevel < 0)
        {   
            currentShakeLevel = 0;
        }
        else if (currentShakeLevel > maxShakeLevel)
        {
            currentShakeLevel = maxShakeLevel;
        }
        
        if (!isShaking && currentShakeLevel > shakeThreshold)
        {
            isShaking = true;
            if (ShakeStarted != null)
            {
                ShakeStarted();
            }
        }

        if (isShaking && currentShakeLevel < degradingShakeThreshold)
        {
            isShaking = false;
            if (ShakeEnded != null)
            {
                ShakeEnded();
            }
        }
    }

    void OnGUI()
    {
        if (isShaking)
        {
            GUI.Label(new Rect(160, 10, 150, 100), "SHAKING!!");
        }
        GUI.Label(new Rect(320, 10, 150, 100), currentShakeLevel.ToString());
    }
}
