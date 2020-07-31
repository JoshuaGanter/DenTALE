using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public delegate void ClickGameObject(GameObject gameObject);
    public static event ClickGameObject OnGameObjectClicked;

    private CameraMove inspectController;
    private GyroOrientation gyroController;

    private Quaternion savedCameraRotation;
    private Vector3 savedCameraPosition;

    void OnGameStateChange(GameState newGameState)
    {
        if (newGameState == GameState.Adventure)
        {
            inspectController.enabled = false;
            gyroController.enabled = true;

            gameObject.transform.position = savedCameraPosition;
            gameObject.transform.rotation = savedCameraRotation;
        }
        else if (newGameState == GameState.Inspect)
        {
            savedCameraRotation = gameObject.transform.rotation;
            savedCameraPosition = gameObject.transform.position;

            
            inspectController.enabled = true;
            gyroController.enabled = false;
        }
    }

    void Start()
    {
        inspectController = gameObject.GetComponent<CameraMove>();
        gyroController = gameObject.GetComponent<GyroOrientation>();

        GameManager.Instance.OnGameStateChange += OnGameStateChange;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && OnGameObjectClicked != null)
        {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
            {
                OnGameObjectClicked(hit.transform.gameObject);
			}
		}
    }
}
