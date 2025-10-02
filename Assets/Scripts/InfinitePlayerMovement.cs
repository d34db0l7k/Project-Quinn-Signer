using UnityEngine;

public class InfinitePlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 10f;        // Constant forward speed
    public float laneChangeSpeed = 12f;     // How quickly we slide into a lane center

    [Header("Lanes")]
    public float laneWidth = 5f;            // Distance between lane centers
    public int minLane = -1;                // -1 = left, 0 = middle, 1 = right
    public int maxLane =  1;
    public float middleLaneWorldX = 0f;     // World-X of the middle lane center

    [Header("Edge Tumble")]
    public float tumbleDuration = 1.0f;     // Seconds to wobble on the edge
    public float tumbleAmplitude = 0.4f;    // How far outward it bumps during tumble
    public float tumbleTilt = 20f;          // Visual tilt (degrees) during tumble
    public bool  lockInputDuringTumble = true;

    private int currentLane = 0;            // Start in the middle
    private bool isTumbling = false;        // In edge penalty animation?

    void Update()
    {
        // Always move forward
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);

        // Read inputs (keyboard). You can add swipe/touch here too.
        if (!isTumbling || !lockInputDuringTumble)
        {
            if (Input.GetKeyDown(KeyCode.A)) TryChangeLane(-1);
            if (Input.GetKeyDown(KeyCode.D)) TryChangeLane(+1);
        }

        // Smoothly slide to the lane center when NOT tumbling
        if (!isTumbling)
        {
            float targetX = LaneCenterX(currentLane);
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * laneChangeSpeed);
            transform.position = pos;

            // Reset any tilt we may have applied during a previous tumble
            Vector3 e = transform.eulerAngles;
            e.z = Mathf.LerpAngle(e.z, 0f, Time.deltaTime * laneChangeSpeed);
            transform.eulerAngles = e;
        }

        // (Optional) Very simple mobile swipe detection:
        // DetectSwipe(); // uncomment and implement if you want touch too
    }

    private float LaneCenterX(int laneIndex)
    {
        return middleLaneWorldX + laneIndex * laneWidth;
    }

    private void TryChangeLane(int dir)
    {
        int desired = currentLane + dir;

        // Inside bounds -> move lanes normally
        if (desired >= minLane && desired <= maxLane)
        {
            currentLane = desired;
            return;
        }

        // Outside bounds -> trigger edge tumble (dir shows which edge we hit)
        if (!isTumbling)
            StartCoroutine(EdgeTumble(dir));
    }

    private System.Collections.IEnumerator EdgeTumble(int dir)
    {
        isTumbling = true;

        float elapsed = 0f;
        float centerX = LaneCenterX(currentLane);       // edge lane center we’re on
        float outward = Mathf.Sign(dir);                // -1 if trying left at left edge, +1 if right at right edge

        // Animate a quick outward bump + tilt, then settle back
        while (elapsed < tumbleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tumbleDuration);

            // Use a sine pulse for a bump that goes out and back
            float pulse = Mathf.Sin(t * Mathf.PI); // 0→1→0

            // Bump position outward (small) then back
            Vector3 pos = transform.position;
            pos.x = centerX + outward * tumbleAmplitude * pulse;
            transform.position = pos;

            // Add a little roll tilt during the bump for feedback
            Vector3 e = transform.eulerAngles;
            e.z = -outward * tumbleTilt * pulse;
            transform.eulerAngles = e;

            yield return null;
        }

        // Snap back to lane center and clear tilt
        Vector3 finalPos = transform.position;
        finalPos.x = centerX;
        transform.position = finalPos;

        Vector3 finalE = transform.eulerAngles;
        finalE.z = 0f;
        transform.eulerAngles = finalE;

        isTumbling = false;
    }

    // --- Optional swipe detection (very lightweight) ---
    // private Vector2 swipeStart;
    // private void DetectSwipe()
    // {
    //     if (Input.touchCount == 0) return;
    //     Touch t = Input.GetTouch(0);
    //     if (t.phase == TouchPhase.Began) swipeStart = t.position;
    //     if (t.phase == TouchPhase.Ended)
    //     {
    //         Vector2 delta = t.position - swipeStart;
    //         if (Mathf.Abs(delta.x) > 60f && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
    //         {
    //             if (!isTumbling || !lockInputDuringTumble)
    //                 TryChangeLane(delta.x > 0 ? +1 : -1);
    //         }
    //     }
    // }
}
