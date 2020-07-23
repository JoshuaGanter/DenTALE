using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraCycler : MonoBehaviour
{

    public Camera cam;
    public GameObject[] positions;
    int index;

    public float delay = 3f;

    void Start() {
        StartCoroutine(CyclerRoutine());
    }

    IEnumerator CyclerRoutine() {
        while (true) {
            index++;
            if (index > positions.Length - 1) { index = 0; }
            var c = positions[index];
            cam.transform.position = c.transform.position;
            cam.transform.rotation = c.transform.rotation;
            yield return new WaitForSeconds(delay);
        }
    }


}
