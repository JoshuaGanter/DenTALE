﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public delegate void ClickGameObject(GameObject gameObject);
    public static event ClickGameObject OnGameObjectClicked;

    public GameObject InspectLight;
    public GameObject Background;

    private CameraMove inspectController;
    private GyroOrientation gyroController;

    private Quaternion savedCameraRotation;
    private Vector3 savedCameraPosition;
    private GameObject _target;
    private bool _inInspectionMode;

    void OnGameStateChange(GameState newGameState)
    {
        if (newGameState == GameState.Adventure || newGameState == GameState.Inventory)
        {
            gyroController.enabled = true;
            inspectController.enabled = false;

            if (_inInspectionMode)
            {
                gameObject.transform.position = savedCameraPosition;
                //gameObject.transform.rotation = savedCameraRotation;
                _inInspectionMode = false;
                InspectLight.SetActive(false);
                Background.SetActive(false);
            }
        }
        else if (newGameState == GameState.Inspect)
        {
            _target = GameManager.Instance.TargetObject;
            //inspectController.target = _target.transform;

            savedCameraPosition = gameObject.transform.position;
            //savedCameraRotation = gameObject.transform.rotation;
            gameObject.transform.position -= new Vector3(0, 2000, 0);
            InspectLight.SetActive(true);
            Background.SetActive(true);
            _inInspectionMode = true;
            gyroController.enabled = false;
            //inspectController.enabled = true;
        }
    }

    private void OnTargetChange(GameObject target)
    {
        _target = target;
    }

    void Start()
    {
        inspectController = gameObject.GetComponent<CameraMove>();
        gyroController = gameObject.GetComponent<GyroOrientation>();
        savedCameraRotation = gameObject.transform.rotation;
        savedCameraPosition = gameObject.transform.position;

        GameManager.Instance.OnGameStateChange += OnGameStateChange;
        GameManager.Instance.OnTargetChanged += OnTargetChange;
    }
}
