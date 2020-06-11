using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public delegate void ClickGameObject(GameObject gameObject);
    public static event ClickGameObject OnGameObjectClicked;

    private CameraMove inspectController;
    private GyroOrientation gyroController;

    void OnGameStateChange(GameState newGameState)
    {
        if (newGameState == GameState.LookAround)
        {
            inspectController.enabled = false;
            gyroController.enabled = true;
        }
        else if (newGameState == GameState.InspectObject)
        {
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
