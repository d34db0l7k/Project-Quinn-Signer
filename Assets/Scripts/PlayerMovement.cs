using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [Tooltip("Speed when holding Space // two-finger hold")]
    public float forwardSpeed = 8f;
    [Tooltip("Speed when holding Shift // three-finger hold")]
    public float reverseSpeed = 6f;

    [Header("Rotation Speeds")]
    [Tooltip("Yaw (turn) speed, degrees per second")]
    public float yawSpeed = 120f;
    [Tooltip("Pitch (look up/down) speed, degrees per second")]
    public float pitchSpeed = 90f;

    [Header("Rotation Smoothing")]
    [Tooltip("How fast the ship turns toward its target rotation")]
    public float rotationSmoothSpeed = 12f;

    [Header("Lean / Tilt")]
    [Tooltip("Max roll angle when turning")]
    public float maxLeanAngle = 25f;
    [Tooltip("How quickly the lean settles (0 <–> 1)")]
    [Range(0.01f, 1f)]
    public float leanSmooth = 0.2f;

    [Header("Mobile Controls")]
    [Tooltip("Pixels of drag ignored at the start (dead zone)")]
    public float touchDeadZonePx = 4f;
    [Tooltip("Drag -> input sensitivity (bigger = more responsive)")]
    public float touchSensitivity = 0.045f;
    [Tooltip("How quickly drag influence decays when you stop moving")]
    public float touchDamp = 14f;

    // Reference to your ship’s visible model (for roll)
    private Transform model;

    // Internal rotation state
    private float yaw;
    private float pitch;

    // Mobile input state
    private int activeTouchId = -1;
    private Vector2 lastTouchPos;
    private Vector2 touchDeltaSmoothed; // damped delta for stable control

    void Start()
    {
        // assume the first child is the mesh/model
        if (transform.childCount > 0)
            model = transform.GetChild(0);
        else
            Debug.LogWarning("PlayerMovement: no child model found!");

        // initialize from current rotation
        Vector3 e = transform.eulerAngles;
        yaw   = e.y;
        pitch = e.x;
    }

    void Update()
    {
        float yawInput = 0f;
        float pitchInput = 0f;
        bool forwardHeld = false;
        bool reverseHeld = false;

        /* --- PC controls (unchanged) --- */
        if (Input.GetKey(KeyCode.A))        yawInput   = -1f;
        else if (Input.GetKey(KeyCode.D))   yawInput   = +1f;

        if (Input.GetKey(KeyCode.W))        pitchInput = -1f;
        else if (Input.GetKey(KeyCode.S))   pitchInput = +1f;

        if (Input.GetKey(KeyCode.Space))                    forwardHeld = true;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) reverseHeld = true;

        /* --- Mobile controls (additive, only when touches exist) --- */
        // if (Application.isMobilePlatform || Input.touchSupported)
        // {
            ReadMobileLook(ref yawInput, ref pitchInput);
            ReadMobileThrust(ref forwardHeld, ref reverseHeld);
        // }

        /* --- Apply rotation --- */
        yaw   += yawInput   * yawSpeed   * Time.deltaTime;
        pitch += pitchInput * pitchSpeed * Time.deltaTime;
        pitch  = Mathf.Clamp(pitch, -45f, 45f);

        Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSmoothSpeed * Time.deltaTime
        );

        /* --- Apply movement --- */
        Vector3 move = Vector3.zero;
        if (forwardHeld) move += transform.forward * forwardSpeed;
        if (reverseHeld) move -= transform.forward * reverseSpeed;
        transform.position += move * Time.deltaTime;

        /* --- Visual lean (roll) from yaw input --- */
        if (model != null)
        {
            float targetRoll = -yawInput * maxLeanAngle;
            Vector3 le = model.localEulerAngles;
            float currentRoll = le.z > 180f ? le.z - 360f : le.z; // map 0..360 to -180..180
            float newRoll = Mathf.Lerp(currentRoll, targetRoll, leanSmooth);
            model.localEulerAngles = new Vector3(le.x, le.y, newRoll);
        }
    }

    /* One-finger drag to aim (yaw/pitch) */
    private void ReadMobileLook(ref float yawInput, ref float pitchInput)
    {
        // Decay smoothed delta every frame (prevents "stuck" input)
        touchDeltaSmoothed = Vector2.Lerp(touchDeltaSmoothed, Vector2.zero, 1f - Mathf.Exp(-touchDamp * Time.deltaTime));

        if (Input.touchCount == 0)
        {
            activeTouchId = -1;
            return;
        }

        // find/maintain a primary finger
        if (activeTouchId == -1)
        {
            // choose the first non-UI touch as primary
            activeTouchId = Input.GetTouch(0).fingerId;
            lastTouchPos = Input.GetTouch(0).position;
        }

        // get the touch with that id
        Touch? primary = null;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).fingerId == activeTouchId)
            {
                primary = Input.GetTouch(i);
                break;
            }
        }

        if (primary == null)
        {
            // primary lost(finger picked up); pick another
            activeTouchId = Input.GetTouch(0).fingerId;
            lastTouchPos = Input.GetTouch(0).position;
            return;
        }

        var t = primary.Value;

        if (t.phase == TouchPhase.Began)
        {
            lastTouchPos = t.position;
            touchDeltaSmoothed = Vector2.zero;
            return;
        }

        if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
        {
            Vector2 delta = t.position - lastTouchPos;

            // Dead zone to ignore micro jitter
            if (delta.magnitude < touchDeadZonePx)
                delta = Vector2.zero;

            // Smooth the delta for stable control
            touchDeltaSmoothed = Vector2.Lerp(touchDeltaSmoothed, delta, 1f - Mathf.Exp(-touchDamp * Time.deltaTime));
            lastTouchPos = t.position;

            // Convert to normalized inputs (-1..1-ish), horizontal = yaw, vertical = pitch
            float x = touchDeltaSmoothed.x * touchSensitivity; // right drag → +yaw
            float y = touchDeltaSmoothed.y * touchSensitivity; // up drag    → +pitch (invert if you prefer flight)
            // Flight-style invert (drag up = pitch down). Comment out if unwanted:
            y = -y;

            // Combine with keyboard values (mobile adds on top)
            yawInput = Mathf.Clamp(yawInput + x, -1f, 1f);
            pitchInput = Mathf.Clamp(pitchInput + y, -1f, 1f);
        }

        if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            activeTouchId = -1;
        }
    }

    // Two fingers = forward thrust, three fingers = reverse
    private void ReadMobileThrust(ref bool forwardHeld, ref bool reverseHeld)
    {
        int tc = Input.touchCount;
        if (tc >= 2 && tc < 3) forwardHeld = true;
        if (tc >= 3)           reverseHeld = true;
    }
}
