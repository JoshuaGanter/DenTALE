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
        if (newGameState == GameState.LookAround)
        {
            inspectController.enabled = false;
            gyroController.enabled = true;

            Debug.Log("Saved Rotation to Set= " + savedCameraRotation);
            Debug.Log("Saved Position to Set= " + savedCameraPosition);
            gameObject.transform.position = savedCameraPosition;
            gameObject.transform.rotation = savedCameraRotation;
            Debug.Log("Set Position=" + gameObject.transform.position);
            Debug.Log("Set Rotation=" + gameObject.transform.rotation);
        }
        else if (newGameState == GameState.InspectObject)
        {
            savedCameraRotation = gameObject.transform.rotation;
            savedCameraPosition = gameObject.transform.position;
            Debug.Log("Saved Rotation= " + savedCameraRotation);
            Debug.Log("Saved Position= " + savedCameraPosition);

            inspectController.target = GameManager.Instance.Target.transform;
            inspectController.enabled = true;
            gyroController.enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        inspectController = gameObject.GetComponent<CameraMove>();
        gyroController = gameObject.GetComponent<GyroOrientation>();

        GameManager.Instance.OnGameStateChange += OnGameStateChange;
    }

    // Update is called once per frame
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
