using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public delegate void ClickGameObject(GameObject gameObject);
    public static event ClickGameObject OnGameObjectClicked;

    public GameObject InspectLight;

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
            //inspectController.enabled = false;

            if (_inInspectionMode)
            {
                gameObject.transform.position = savedCameraPosition;
                //gameObject.transform.rotation = savedCameraRotation;
                _inInspectionMode = false;
                InspectLight.SetActive(false);
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

    void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameState.Inspect && Input.GetMouseButtonDown(0) && OnGameObjectClicked != null)
        {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
            {
                OnGameObjectClicked(hit.transform.gameObject);
			}
		}

        if (GameManager.Instance.CurrentGameState == GameState.Inspect && Input.touchCount == 1)
        {
            _target.transform.Rotate(/*Input.touches[0].deltaPosition.y/2*/ 0, 0, -Input.touches[0].deltaPosition.x/2);
        }
    }
}
