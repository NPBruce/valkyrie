using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    Vector3 mouseLast = Vector3.zero;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            gameObject.transform.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * 50));
        }

        if(Input.GetKey(KeyCode.UpArrow))
        {
            gameObject.transform.Translate(new Vector3(0, 3, 0));
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            gameObject.transform.Translate(new Vector3(0, -3, 0));
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            gameObject.transform.Translate(new Vector3(-3, 0, 0));
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            gameObject.transform.Translate(new Vector3(3, 0, 0));
        }

        mouseLast = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            gameObject.transform.Translate((mouseLast - Input.mousePosition) * 100);
        }
    }
}
