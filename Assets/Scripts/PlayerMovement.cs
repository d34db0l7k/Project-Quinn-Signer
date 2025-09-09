using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [Tooltip("Speed when holding Space")]
    public float forwardSpeed;
    [Tooltip("Speed when holding Shift")]
    public float reverseSpeed;

    [Header("Rotation Speeds")]
    [Tooltip("Yaw (turn) speed, degrees per second")]
    public float yawSpeed;

    [Header("Rotation Smoothing")]
    [Tooltip("How fast the ship turns toward its target rotation")]
    public float rotationSmoothSpeed; // larger = snappier

    [Tooltip("Pitch (look up/down) speed, degrees per second")]
    public float pitchSpeed;

    [Header("Lean / Tilt")]
    [Tooltip("Max roll angle when turning")]
    public float maxLeanAngle;
    [Tooltip("How quickly the lean settles (0–1)")]
    [Range(0.01f,1f)]
    public float leanSmooth;

    // Reference to your ship’s visible model (for roll)
    private Transform model;

    // Internal rotation state
    private float yaw;
    private float pitch;

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
        // 1) Read arrow‑key input for rotation
        float yawInput   = 0f;
        float pitchInput = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))  yawInput   = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) yawInput   = +1f;

        if (Input.GetKey(KeyCode.UpArrow))    pitchInput = -1f;
        else if (Input.GetKey(KeyCode.DownArrow))  pitchInput = +1f;

        // apply to yaw & pitch
        yaw   += yawInput   * yawSpeed   * Time.deltaTime;
        pitch += pitchInput * pitchSpeed * Time.deltaTime;
        pitch  = Mathf.Clamp(pitch, -45f, 45f);

        // commit rotation (no roll here; roll is on the child model)
        Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSmoothSpeed * Time.deltaTime
        );

        // 2) Movement via Space (forward) and Shift (reverse)
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.Space))
            move += transform.forward * forwardSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            move -= transform.forward * reverseSpeed;

        transform.position += move * Time.deltaTime;

        // 3) Lean (roll) the visual model based on yawInput
        if (model != null)
        {
            float targetRoll = -yawInput * maxLeanAngle;
            Vector3 le = model.localEulerAngles;
            // convert z to -180..+180
            float currentRoll = le.z > 180f ? le.z - 360f : le.z;
            float newRoll = Mathf.Lerp(currentRoll, targetRoll, leanSmooth);
            model.localEulerAngles = new Vector3(le.x, le.y, newRoll);
        }
    }
}
