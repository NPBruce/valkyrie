using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    // How fast to move the screen when arrows used
    static int keyScrollRate = 30;
    // How much to zoom in/out with wheel
    static int mouseWheelScrollRate = 600;
    // Max zoom in
    static int maxZoom = -100;
    // Max zoom out
    static int minZoom = -2500;

    public int minPanX = -5000;
    public int minPanY = -5000;
    public int maxPanX = 5000;
    public int maxPanY = 5000;

    public Vector3 mouseDownCamPosition;
    public Vector2 mouseDownMousePosition;

    void Awake()
    {
        mouseDownCamPosition = gameObject.transform.position;
        mouseDownMousePosition = Vector2.zero;
    }

    // FixedUpdate is not tied to frame rate
    void FixedUpdate () {
        // Check if the scroll wheel has moved
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            // Translate the camera up/down
            gameObject.transform.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * mouseWheelScrollRate), Space.World);

            // Limit how high/low the camera can go
            if (gameObject.transform.position.z > maxZoom)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, maxZoom);
            if (gameObject.transform.position.z < minZoom)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, minZoom);
        }

        // Check for arrow keys and move camera around
        if (Input.GetKey(KeyCode.UpArrow))
        {
            gameObject.transform.Translate(new Vector3(0, keyScrollRate, 0), Space.World);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            gameObject.transform.Translate(new Vector3(0, -keyScrollRate, 0), Space.World);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            gameObject.transform.Translate(new Vector3(-keyScrollRate, 0, 0), Space.World);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            gameObject.transform.Translate(new Vector3(keyScrollRate, 0, 0), Space.World);
        }

        // Mouse edge of screen scrolling and/or click-drag should go here

        // Limit camera movement
        Vector3 pos = gameObject.transform.position;
        if (pos.x < minPanX) pos.x = minPanX;
        if (pos.y < minPanY) pos.y = minPanY;
        if (pos.x > maxPanX) pos.x = maxPanX;
        if (pos.y > maxPanY) pos.y = maxPanY;
        gameObject.transform.position = pos;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownCamPosition = gameObject.transform.position;
            mouseDownMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            // Scaling?? (probably need to do a ray trace like GetMouseTile (but remove the find object - not static)
            gameObject.transform.position = new Vector3(mouseDownCamPosition.x + mouseDownMousePosition.x - Input.mousePosition.x,
                mouseDownCamPosition.y + mouseDownMousePosition.y - Input.mousePosition.y, mouseDownCamPosition.z);
        }
    }

    public static Vector2 GetMouseTile()
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();

        Ray ray = cc.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Plane basePlane = new Plane(Vector3.forward, Vector3.zero);
        float rayDistance = 0;
        basePlane.Raycast(ray, out rayDistance);

        Vector3 clickPoint = ray.GetPoint(rayDistance) / 105f;

        return new Vector2(Mathf.Round(clickPoint.x), Mathf.Round(clickPoint.y));
    }

    public static void SetCameraMin(Vector2 min)
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        cc.minPanX = Mathf.RoundToInt(min.x * 105);
        cc.minPanY = Mathf.RoundToInt(min.y * 105);
    }

    public static void SetCameraMax(Vector2 max)
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        cc.maxPanX = Mathf.RoundToInt(max.x * 105);
        cc.maxPanY = Mathf.RoundToInt(max.y * 105);
    }

    public static void SetCamera(Vector2 pos)
    {
        Camera cam = GameObject.FindObjectOfType<Camera>();
        cam.transform.position = new Vector3(pos.x * 105, pos.y * 105, -800);
    }
}
