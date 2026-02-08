using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController_WheelCollider : MonoBehaviour
{
    [Header("Car Settings")]
    public float motorTorque = 2500f;
    public float maxSpeed = 120f;
    public float steeringAngle = 35f;
    public float brakeForce = 3000f;
    public float handbrakeForce = 2000f;
    public float driftStiffness = 0.5f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Center of Mass Offset")]
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    [Header("UI Controls")]
    public Button gasButton;
    public Button brakeButton;
    public Button handbrakeButton;
    public VirtualDPad dPad; // dùng để rẽ trái/phải qua UI

    private Rigidbody rb;
    private float inputVertical;
    private float inputHorizontal;
    private bool isHandbraking;
    private bool isDrifting;

    private bool gasHeld;
    private bool brakeHeld;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += centerOfMassOffset;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Gán sự kiện cho các nút UI (nếu có)
        if (gasButton != null)
            AddButtonHoldEvents(gasButton, () => gasHeld = true, () => gasHeld = false);

        if (brakeButton != null)
            AddButtonHoldEvents(brakeButton, () => brakeHeld = true, () => brakeHeld = false);

        if (handbrakeButton != null)
            AddButtonHoldEvents(handbrakeButton, () => isHandbraking = true, () => isHandbraking = false);
    }

    void Update()
    {
        // Bàn phím (fallback)
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        bool spacePressed = Input.GetKey(KeyCode.Space);

        // Nút tăng tốc / phanh (UI)
        if (gasHeld) verticalInput = 1f;
        else if (brakeHeld) verticalInput = -1f;

        // DPad điều khiển rẽ
        if (dPad != null)
        {
            Vector2 dpadValue = dPad.ReadValue();
            horizontalInput = dpadValue.x; // chỉ cần hướng ngang
        }

        // Handbrake (UI + phím)
        if (isHandbraking || spacePressed)
            isHandbraking = true;
        else
            isHandbraking = false;

        inputVertical = verticalInput;
        inputHorizontal = horizontalInput;

        UpdateAllWheelVisuals();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleHandbrake();
        LimitSpeed();
    }

    // ==== ĐỘNG CƠ ====
    void HandleMotor()
    {
        float motorForce = inputVertical * motorTorque;
        frontLeftCollider.motorTorque = motorForce;
        frontRightCollider.motorTorque = motorForce;
    }

    // ==== RẼ ====
    void HandleSteering()
    {
        float steer = inputHorizontal * steeringAngle;
        frontLeftCollider.steerAngle = steer;
        frontRightCollider.steerAngle = steer;
    }

    // ==== PHANH TAY / DRIFT ====
    void HandleHandbrake()
    {
        if (isHandbraking)
        {
            rearLeftCollider.brakeTorque = handbrakeForce;
            rearRightCollider.brakeTorque = handbrakeForce;

            var leftFriction = rearLeftCollider.sidewaysFriction;
            var rightFriction = rearRightCollider.sidewaysFriction;
            leftFriction.stiffness = driftStiffness;
            rightFriction.stiffness = driftStiffness;
            rearLeftCollider.sidewaysFriction = leftFriction;
            rearRightCollider.sidewaysFriction = rightFriction;

            isDrifting = true;
        }
        else
        {
            rearLeftCollider.brakeTorque = 0;
            rearRightCollider.brakeTorque = 0;

            var leftFriction = rearLeftCollider.sidewaysFriction;
            var rightFriction = rearRightCollider.sidewaysFriction;
            leftFriction.stiffness = Mathf.Lerp(leftFriction.stiffness, 1f, Time.fixedDeltaTime * 5f);
            rightFriction.stiffness = Mathf.Lerp(rightFriction.stiffness, 1f, Time.fixedDeltaTime * 5f);
            rearLeftCollider.sidewaysFriction = leftFriction;
            rearRightCollider.sidewaysFriction = rightFriction;

            isDrifting = false;
        }
    }

    // ==== GIỚI HẠN TỐC ĐỘ ====
    void LimitSpeed()
    {
        float speed = rb.linearVelocity.magnitude * 3.6f; // m/s → km/h
        if (speed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * (maxSpeed / 3.6f);
        }
    }

    // ==== CẬP NHẬT BÁNH XE ====
    void UpdateAllWheelVisuals()
    {
        UpdateWheelVisuals(frontLeftCollider, frontLeftMesh, true);
        UpdateWheelVisuals(frontRightCollider, frontRightMesh, false);
        UpdateWheelVisuals(rearLeftCollider, rearLeftMesh, true);
        UpdateWheelVisuals(rearRightCollider, rearRightMesh, false);
    }

    void UpdateWheelVisuals(WheelCollider collider, Transform mesh, bool isLeft)
    {
        if (collider == null || mesh == null) return;

        collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        Quaternion correction = Quaternion.Euler(0, 0, 90f);
        mesh.position = pos;
        mesh.rotation = rot * correction;
    }

    // ==== THÊM SỰ KIỆN GIỮ NÚT UI ====
    void AddButtonHoldEvents(Button button, System.Action onDown, System.Action onUp)
    {
        var trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var down = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
        };
        down.callback.AddListener((_) => onDown());
        trigger.triggers.Add(down);

        var up = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
        };
        up.callback.AddListener((_) => onUp());
        trigger.triggers.Add(up);
    }
}
