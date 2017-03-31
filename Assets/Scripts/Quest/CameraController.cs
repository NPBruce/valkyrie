using UnityEngine;
using System.Collections;

// Class to control the game camera
// Used to pan/zoom around the board
public class CameraController : MonoBehaviour {

    // How fast to move the screen when arrows used
    static float keyScrollRate = 0.3f;
    // How much to zoom in/out with wheel
    static int mouseWheelScrollRate = 6;
    // Max zoom in
    static int maxZoom = -1;
    // Max zoom out
    static int minZoom = -25;

    public bool minLimit = false;
    public bool maxLimit = false;
    // These are defaults, replaced by the quest
    public int minPanX = -50;
    public int minPanY = -50;
    public int maxPanX = 50;
    public int maxPanY = 50;

    // Units to move per second
    public static float autoPanSpeed = 12;

    // Are we moving to a target position?
    public bool targetSet = false;
    // Target position
    public Vector3 camTarget;

    // Camera position on mouse down
    public Vector3 mouseDownCamPosition;
    // Mouse position on mouse down
    public Vector2 mouseDownMousePosition;
    // camera pan disable
    public bool panDisable = false;


    public Game game;

    // Called by Unity
    void Awake()
    {
        mouseDownCamPosition = gameObject.transform.position;
        mouseDownMousePosition = Vector2.zero;
        game = Game.Get();
    }

    // FixedUpdate is not tied to frame rate
    // Scrolling by keys go here to be at a fixed rate
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
        if (minLimit)
        {
            if (pos.x < minPanX) pos.x = minPanX;
            if (pos.y < minPanY) pos.y = minPanY;
        }
        if (maxLimit)
        {
            if (pos.x > maxPanX) pos.x = maxPanX;
            if (pos.y > maxPanY) pos.y = maxPanY;
        }
        gameObject.transform.position = pos;
    }

    // Called by unity every frame
    // Scrolling by mouse/scripts goes here, rate is fixed, display needs to be smooth
    void Update()
    {
        // latch positions on mouse down
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownCamPosition = gameObject.transform.position;
            mouseDownMousePosition = GetMouseBoardPlane();
        }
        // If mouse is held down update camera
        if (Input.GetMouseButton(0) && !panDisable)
        {
            Vector2 bPos = GetMouseBoardPlane();
            gameObject.transform.Translate(new Vector3(mouseDownMousePosition.x - bPos.x,
                mouseDownMousePosition.y - bPos.y, 0), Space.World);
        }
        // Limit camera position
        Vector3 pos = gameObject.transform.position;
        if (minLimit)
        {
            if (pos.x < minPanX) pos.x = minPanX;
            if (pos.y < minPanY) pos.y = minPanY;
        }
        if (maxLimit)
        {
            if (pos.x > maxPanX) pos.x = maxPanX;
            if (pos.y > maxPanY) pos.y = maxPanY;
        }
        gameObject.transform.position = pos;

        // If we are moving to a target position
        if (targetSet)
        {
            // Calculate distance to target
            float camJumpDist = Vector3.Distance(gameObject.transform.position, camTarget);
            // Are we close?
            if (camJumpDist > 0.1f)
            {
                // How many units to move this frame
                float moveDist = Time.deltaTime * autoPanSpeed;
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, camTarget, moveDist);
            }
            else
            { // Close enough to target, finish move
                targetSet = false;
            }
        }
    }

    // Get mouse position in board space coordinates, rounded for tile location
    public Vector2 GetMouseTile()
    {
        Vector2 bPos = GetMouseBoardPlane();

        // return rounded results
        return new Vector2(Mathf.Round(bPos.x), Mathf.Round(bPos.y));
    }

    // Get mouse position in board space coordinates, rounded to arbitrary distance
    public Vector2 GetMouseBoardRounded(float round)
    {
        // Get position divided by rounding
        Vector2 bPos = GetMouseBoardPlane() / round;

        // Return rounded results, re multiplied
        return new Vector2(Mathf.Round(bPos.x), Mathf.Round(bPos.y)) * round;
    }

    // Get mouse position in board space coordinates
    public Vector2 GetMouseBoardPlane()
    {
        // Ray from mouse position
        Ray ray = gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        // Plane at board
        Plane basePlane = new Plane(Vector3.forward, Vector3.zero);
        float rayDistance = 0;
        // Find intersection of plane and ray
        basePlane.Raycast(ray, out rayDistance);

        // Get coordinates of intersection
        Vector3 clickPoint = ray.GetPoint(rayDistance);

        return new Vector2(clickPoint.x, clickPoint.y);
    }

    // Update minimum camera pan
    public static void SetCameraMin(Vector2 min)
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        cc.minPanX = Mathf.RoundToInt(min.x);
        cc.minPanY = Mathf.RoundToInt(min.y);
        cc.minLimit = true;
    }

    // Update maximum camera pan
    public static void SetCameraMax(Vector2 max)
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        cc.maxPanX = Mathf.RoundToInt(max.x);
        cc.maxPanY = Mathf.RoundToInt(max.y);
        cc.maxLimit = true;
    }

    // Set camera target position
    public static void SetCamera(Vector2 pos)
    {
        CameraController cc = GameObject.FindObjectOfType<CameraController>();
        cc.targetSet = true;
        cc.camTarget = new Vector3(pos.x, pos.y, -8);

        if (cc.minLimit)
        {
            if (cc.camTarget.x < cc.minPanX) cc.camTarget.x = cc.minPanX;
            if (cc.camTarget.y < cc.minPanY) cc.camTarget.y = cc.minPanY;
        }
        if (cc.maxLimit)
        {
            if (cc.camTarget.x > cc.maxPanX) cc.camTarget.x = cc.maxPanX;
            if (cc.camTarget.y > cc.maxPanY) cc.camTarget.y = cc.maxPanY;
        }
    }
}
