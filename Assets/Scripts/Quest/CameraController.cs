using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    // How fast to move the screen when arrows used
    static float keyScrollRate = 0.3f;
    // How much to zoom in/out with wheel
    static int mouseWheelScrollRate = 6;
    // Max zoom in
    static int maxZoom = -1;
    // Max zoom out
    static int minZoom = -25;

    public int minPanX = -50;
    public int minPanY = -50;
    public int maxPanX = 50;
    public int maxPanY = 50;

    // Units to move per second
    public static float autoPanSpeed = 12;

    public bool targetSet = false;
    public Vector3 camTarget;

    public Vector3 mouseDownCamPosition;
    public Vector2 mouseDownMousePosition;

    public Game game;

    void Awake()
    {
        mouseDownCamPosition = gameObject.transform.position;
        mouseDownMousePosition = Vector2.zero;
        game = Game.Get();
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
            mouseDownMousePosition = GetMouseBoardPlane(this);
        }
        if (Input.GetMouseButton(0))
        {
            Vector2 bPos = GetMouseBoardPlane(this);
            gameObject.transform.Translate(new Vector3(mouseDownMousePosition.x - bPos.x,
                mouseDownMousePosition.y - bPos.y, 0), Space.World);
        }
        Vector3 pos = gameObject.transform.position;
        if (pos.x < minPanX) pos.x = minPanX;
        if (pos.y < minPanY) pos.y = minPanY;
        if (pos.x > maxPanX) pos.x = maxPanX;
        if (pos.y > maxPanY) pos.y = maxPanY;
        gameObject.transform.position = pos;

        if (targetSet)
        {
            float camJumpDist = Vector3.Distance(gameObject.transform.position, camTarget);
            if (camJumpDist > 0.1f)
            {
                // How many units to move this frame
                float moveDist = Time.deltaTime * autoPanSpeed;
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, camTarget, moveDist);
            }
            else
            {
                targetSet = false;
            }
        }
    }

    public Vector2 GetMouseTile()
    {
        Vector2 bPos = GetMouseBoardPlane(this);

        return new Vector2(Mathf.Round(bPos.x), Mathf.Round(bPos.y));
    }

    public Vector2 GetMouseHalfTile()
    {
        Vector2 bPos = GetMouseBoardPlane(this);

        return new Vector2(Mathf.Round(bPos.x * 2) / 2f, Mathf.Round(bPos.y * 2) / 2f);
    }

    public Vector2 GetMouseBoardPlane(CameraController cc)
    {
        Ray ray = cc.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Plane basePlane = new Plane(Vector3.forward, Vector3.zero);
        float rayDistance = 0;
        basePlane.Raycast(ray, out rayDistance);

        Vector3 clickPoint = ray.GetPoint(rayDistance);

        return new Vector2(clickPoint.x, clickPoint.y);
    }

    public static void SetCameraMin(Vector2 min)
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        cc.minPanX = Mathf.RoundToInt(min.x);
        cc.minPanY = Mathf.RoundToInt(min.y);
    }

    public static void SetCameraMax(Vector2 max)
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        cc.maxPanX = Mathf.RoundToInt(max.x);
        cc.maxPanY = Mathf.RoundToInt(max.y);
    }

    public static void SetCamera(Vector2 pos)
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        cc.targetSet = true;
        cc.camTarget = new Vector3(pos.x, pos.y, -8);

        if (cc.camTarget.x < cc.minPanX) cc.camTarget.x = cc.minPanX;
        if (cc.camTarget.y < cc.minPanY) cc.camTarget.y = cc.minPanY;
        if (cc.camTarget.x > cc.maxPanX) cc.camTarget.x = cc.maxPanX;
        if (cc.camTarget.y > cc.maxPanY) cc.camTarget.y = cc.maxPanY;
    }
}
