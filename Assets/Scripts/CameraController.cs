using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    Vector3 mouseLast = Vector3.zero;

    // Use this for initialization
    void Start () {
	
	}
	
	// FixedUpdate is not tied to frame rate
	void FixedUpdate () {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            gameObject.transform.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * 600));
        }

        if(Input.GetKey(KeyCode.UpArrow))
        {
            gameObject.transform.Translate(new Vector3(0, 30, 0));
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            gameObject.transform.Translate(new Vector3(0, -30, 0));
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            gameObject.transform.Translate(new Vector3(-30, 0, 0));
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            gameObject.transform.Translate(new Vector3(30, 0, 0));
        }

        mouseLast = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            gameObject.transform.Translate((mouseLast - Input.mousePosition) * 100);
        }
    }
}
