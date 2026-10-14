using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class CharacterMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float acceleration = 48f;
    public float airAcceleration = 20f;
    public float minimumJumpSpeed = 6f;
    public float maximumJumpSpeed = 11f;
    public float doubleJumpSpeed = 9f;
    public float maximumChargeTime = 3f;
    public Transform view;
    public bool Grounded { get; private set; }
    public float Charge { get; private set; }
    public float PlanarSpeed => new Vector2(body.linearVelocity.x, body.linearVelocity.z).magnitude;
    public event Action DoubleJumped;
    public event Action Jumped;

    Rigidbody body;
    PlayerPowerups powerups;
    Vector2 input;
    bool running;
    bool charging;
    bool doubleJumpUsed;
    float lastGrounded = -10f;
    float lastJump = -10f;
    float queuedJump;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        powerups = GetComponent<PlayerPowerups>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        input = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
        running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (Input.GetKeyDown(KeyCode.Space)) BeginJump();
        if (charging)
        {
            Charge = Mathf.Min(1f, Charge + Time.deltaTime / maximumChargeTime);
            if (Input.GetKeyUp(KeyCode.Space) || Charge >= 1f) ReleaseJump();
            if (!Grounded && Time.time - lastGrounded > .12f) CancelCharge();
        }
    }

    public void BeginJump()
    {
        if (Grounded || Time.time - lastGrounded < .12f)
        {
            charging = true;
            Charge = 0f;
        }
        else if (!doubleJumpUsed && powerups != null && powerups.DoubleJumpActive && Time.time - lastJump > .1f)
        {
            doubleJumpUsed = true;
            queuedJump = doubleJumpSpeed;
            lastJump = Time.time;
            DoubleJumped?.Invoke();
            GameAudio.Play(Sound.Flip, transform.position);
        }
    }

    public void ReleaseJump()
    {
        if (!charging) return;
        if (Grounded || Time.time - lastGrounded < .12f)
        {
            queuedJump = Mathf.Lerp(minimumJumpSpeed, maximumJumpSpeed, Charge);
            lastJump = Time.time;
            Jumped?.Invoke();
            GameAudio.Play(Sound.Jump, transform.position);
        }
        CancelCharge();
    }

    public void CancelCharge() { charging = false; Charge = 0f; }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        Grounded = body.linearVelocity.y <= .5f && Time.time - lastJump > .12f &&
            Physics.SphereCast(transform.position + Vector3.up * .38f, .25f, Vector3.down,
                out RaycastHit ground, .24f, ~(1 << 8), QueryTriggerInteraction.Ignore) && ground.normal.y > .55f;
        if (Grounded) { lastGrounded = Time.time; doubleJumpUsed = false; }
        Vector3 forward = view != null ? view.forward : Vector3.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 direction = forward * input.y + right * input.x;
        float multiplier = powerups != null && powerups.SpeedActive ? 1.5f : 1f;
        Vector3 desired = direction * walkSpeed * (running ? 2f : 1f) * multiplier;
        Vector3 velocity = body.linearVelocity;
        Vector3 change = desired - new Vector3(velocity.x, 0f, velocity.z);
        body.AddForce(Vector3.ClampMagnitude(change, (Grounded ? acceleration : airAcceleration) * Time.fixedDeltaTime), ForceMode.VelocityChange);
        if (queuedJump > 0f)
        {
            body.AddForce(Vector3.up * (queuedJump - velocity.y), ForceMode.VelocityChange);
            queuedJump = 0f;
            Grounded = false;
        }
        // A slightly stronger fall keeps the rooftop jumps responsive.
        if (!Grounded && body.linearVelocity.y < 0f) body.AddForce(Physics.gravity * .45f, ForceMode.Acceleration);
        if (direction.sqrMagnitude > .01f)
            body.MoveRotation(Quaternion.Slerp(body.rotation, Quaternion.LookRotation(direction), 14f * Time.fixedDeltaTime));
    }
}

