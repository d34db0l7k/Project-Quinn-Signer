using UnityEngine;

public class InfinitePlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 20f;          // Base forward speed

    [Tooltip("How quickly we slide into a lane center")]
    public float laneChangeSpeed = 12f;

    [Header("Lanes")]
    public float laneWidth = 5f;              // Distance between lane centers
    public int minLane = -1;                  // -1 = left, 0 = middle, 1 = right
    public int maxLane =  1;
    public float middleLaneWorldX = 0f;       // World-X of the middle lane center

    [Header("Barrel Roll When Switching Lanes")]
    public Transform visual;                  // Optional: roll only the visual model (child). If null, uses this transform.
    public int   spinRevolutions = 1;         // 1 = 360°, 2 = 720°, etc.
    public float spinDuration    = 0.5f;      // Seconds for one lane-change spin
    public bool  spinMatchesInput = true;     // Left input = CCW, Right input = CW (if false, always same direction)

    [Header("Edge Tumble")]
    public float tumbleDuration  = 1.0f;      // Seconds to wobble on the edge
    public float tumbleAmplitude = 0.4f;      // How far outward it bumps during tumble
    public float tumbleTilt      = 20f;       // Visual tilt (degrees) during tumble
    public bool  lockInputDuringTumble = true;

    [Header("Mobile Controls (Swipe to change lanes)")]
    [Tooltip("Pixels of drag ignored at the start (dead zone)")]
    public float touchDeadZonePx = 4f;

    [Tooltip("Drag -> input smoothing (bigger damp = quicker decay)")]
    public float touchDamp = 14f;

    [Tooltip("How many pixels of horizontal drag count as a lane swipe")]
    public float swipeThresholdPx = 60f;

    [Tooltip("Minimum time between 2 lane changes (sec)")]
    public float laneChangeCooldown = 0.15f;

    [Header("Mobile Thrust (optional)")]
    [Tooltip("Two-finger = boost, three-finger = slow")]
    public bool enableTouchThrust = true;

    [Tooltip("Forward speed multiplier when 2 fingers held")]
    public float boostMultiplier = 1.35f;

    [Tooltip("Forward speed multiplier when 3+ fingers held")]
    public float slowMultiplier = 0.6f;

    // --- Internal state ---
    private int currentLane = 0;              // Start in the middle
    private bool isTumbling = false;          // In edge penalty animation?
    private bool isSpinning = false;          // In a barrel-roll spin?
    private float lastLaneChangeTime = -999f; // cooldown timer

    // Touch tracking (free-move logic adapted for swipes)
    private int activeTouchId = -1;
    private Vector2 lastTouchPos;
    private Vector2 touchDeltaSmoothed;
    private float accumulatedX;               // accumulate horizontal drag to detect a swipe

    private Transform TiltTarget => visual != null ? visual : transform;

    void Start()
    {
        if (laneWidth <= 0f) laneWidth = 5f;
        laneChangeSpeed = Mathf.Max(0.01f, laneChangeSpeed);
        spinDuration    = Mathf.Max(0.05f, spinDuration);
        spinRevolutions = Mathf.Max(1, spinRevolutions);
        swipeThresholdPx = Mathf.Max(8f, swipeThresholdPx);
    }

    void Update()
    {
        // --- Forward move with optional touch thrust multipliers ---
        float speedMul = 1f;
        if (enableTouchThrust)
        {
            int tc = Input.touchCount;
            if (tc >= 2 && tc < 3) speedMul = boostMultiplier; // two-finger boost
            else if (tc >= 3)      speedMul = slowMultiplier;  // three-finger slow
        }

        transform.Translate(Vector3.forward * (forwardSpeed * speedMul) * Time.deltaTime, Space.World);

        // --- Inputs: Keyboard + Mobile Swipe ---
        if (!isTumbling || !lockInputDuringTumble)
        {
            // Keyboard
            if (Input.GetKeyDown(KeyCode.A)) TryChangeLane(-1);
            if (Input.GetKeyDown(KeyCode.D)) TryChangeLane(+1);

            // Mobile swipe
            ReadMobileSwipe();
        }

        // --- Slide toward lane center (keep running while spinning) ---
        float targetX = LaneCenterX(currentLane);
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * laneChangeSpeed);
        transform.position = pos;

        // Keep Z flat when not spinning/tumbling
        if (!isSpinning && !isTumbling)
        {
            Vector3 e = TiltTarget.localEulerAngles;
            e.z = Mathf.LerpAngle(e.z, 0f, Time.deltaTime * laneChangeSpeed);
            TiltTarget.localEulerAngles = e;
        }
    }

    // --- Mobile swipe: one-finger horizontal drag triggers lane changes ---
    private void ReadMobileSwipe()
    {
        // Exponential decay of smoothed delta (prevents stuck input)
        touchDeltaSmoothed = Vector2.Lerp(touchDeltaSmoothed, Vector2.zero, 1f - Mathf.Exp(-touchDamp * Time.deltaTime));

        if (Input.touchCount == 0)
        {
            activeTouchId = -1;
            accumulatedX = 0f;
            return;
        }

        // maintain/choose a primary finger
        if (activeTouchId == -1)
        {
            Touch first = Input.GetTouch(0);
            activeTouchId = first.fingerId;
            lastTouchPos = first.position;
        }

        // fetch the primary touch
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
            // if lost, pick a new one
            Touch first = Input.GetTouch(0);
            activeTouchId = first.fingerId;
            lastTouchPos = first.position;
            accumulatedX = 0f;
            return;
        }

        Touch t = primary.Value;

        if (t.phase == TouchPhase.Began)
        {
            lastTouchPos = t.position;
            touchDeltaSmoothed = Vector2.zero;
            accumulatedX = 0f;
            return;
        }

        if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
        {
            Vector2 delta = t.position - lastTouchPos;

            // dead zone to ignore micro jitter
            if (delta.magnitude < touchDeadZonePx)
                delta = Vector2.zero;

            // smooth it
            touchDeltaSmoothed = Vector2.Lerp(
                touchDeltaSmoothed,
                delta,
                1f - Mathf.Exp(-touchDamp * Time.deltaTime)
            );

            lastTouchPos = t.position;

            // accumulate horizontal movement; when past threshold, trigger lane change
            accumulatedX += touchDeltaSmoothed.x;

            if (Mathf.Abs(accumulatedX) >= swipeThresholdPx)
            {
                int dir = accumulatedX > 0f ? +1 : -1;

                // respect a short cooldown so one long swipe doesn't spam lanes
                if (Time.time - lastLaneChangeTime >= laneChangeCooldown)
                {
                    TryChangeLane(dir);
                    lastLaneChangeTime = Time.time;
                }

                // reset accumulator so you can chain swipes within the same drag
                accumulatedX = 0f;
            }
        }

        if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            activeTouchId = -1;
            accumulatedX = 0f;
        }
    }

    private float LaneCenterX(int laneIndex) => middleLaneWorldX + laneIndex * laneWidth;

    private void TryChangeLane(int dir)
    {
        int desired = currentLane + dir;

        // Inside bounds -> move lanes normally and trigger spin
        if (desired >= minLane && desired <= maxLane)
        {
            currentLane = desired;

            // Start a 360° spin matching input direction (or fixed direction)
            if (!isSpinning && !isTumbling)
                StartCoroutine(Spin360(dir));

            return;
        }

        // Outside bounds -> trigger edge tumble (dir shows which edge we hit)
        if (!isTumbling)
            StartCoroutine(EdgeTumble(dir));
    }

    private System.Collections.IEnumerator Spin360(int dir)
    {
        isSpinning = true;

        // Determine spin sign (CCW = +, CW = - around local Z)
        float sign = (spinMatchesInput ? Mathf.Sign(dir) : 1f);

        // Cache start rotation
        Vector3 startEuler = TiltTarget.localEulerAngles;

        // We add N * 360 degrees to Z over spinDuration
        float totalDegrees = 360f * spinRevolutions * sign;

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);

            // Ease in/out (S-curve): 3t^2 - 2t^3
            float ease = (3f * t * t) - (2f * t * t * t);

            float z = startEuler.z + totalDegrees * ease;

            Vector3 e = TiltTarget.localEulerAngles;
            e.z = z;
            TiltTarget.localEulerAngles = e;

            yield return null;
        }

        // Land cleanly aligned (mod 360)
        Vector3 finalE = TiltTarget.localEulerAngles;
        finalE.z = Mathf.Repeat(startEuler.z + totalDegrees, 360f);
        TiltTarget.localEulerAngles = finalE;

        isSpinning = false;
    }

    private System.Collections.IEnumerator EdgeTumble(int dir)
    {
        isTumbling = true;

        float elapsed = 0f;
        float centerX = LaneCenterX(currentLane);       // edge lane center we’re on
        float outward = Mathf.Sign(dir);                // -1 if trying left at left edge, +1 if right at right edge

        while (elapsed < tumbleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tumbleDuration);

            // Sine pulse for bump 0→1→0
            float pulse = Mathf.Sin(t * Mathf.PI);

            // Bump position outward (small) then back
            Vector3 pos = transform.position;
            pos.x = centerX + outward * tumbleAmplitude * pulse;
            transform.position = pos;

            // Visual tilt during the bump
            Vector3 e = TiltTarget.eulerAngles;
            e.z = -outward * tumbleTilt * pulse;
            TiltTarget.eulerAngles = e;

            yield return null;
        }

        // Snap back to lane center and clear tilt
        Vector3 finalPos = transform.position;
        finalPos.x = centerX;
        transform.position = finalPos;

        Vector3 finalE = TiltTarget.eulerAngles;
        finalE.z = 0f;
        TiltTarget.eulerAngles = finalE;

        isTumbling = false;
    }
}
