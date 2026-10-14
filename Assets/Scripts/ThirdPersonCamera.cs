using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.35f, 0f);
    public float distance = 4.8f;
    public float sensitivityX = 2.5f;
    public float sensitivityY = 2.1f;
    public float minPitch = -12f;
    public float maxPitch = 65f;
    public float smoothTime = .055f;
    float yaw;
    float pitch;
    Vector3 velocity;
    Camera lens;
    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        lens = GetComponent<Camera>();
        if (target == null)
        {
            CharacterMovement player = FindAnyObjectByType<CharacterMovement>();
            if (player != null) target = player.transform;
        }
        if (target != null)
        {
            CharacterMovement movement = target.GetComponent<CharacterMovement>();
            if (movement != null) movement.view = transform;
        }
    }
    void LateUpdate()
    {
        if (target == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying)) return;
        yaw += Input.GetAxis("Mouse X") * sensitivityX;
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivityY, minPitch, maxPitch);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + targetOffset;
        Vector3 offset = -(rotation * Vector3.forward);
        float clearDistance = distance;
        if (Physics.SphereCast(focus, .18f, offset, out RaycastHit hit, distance, ~(1 << 8), QueryTriggerInteraction.Ignore))
            clearDistance = Mathf.Max(.6f, hit.distance - .12f);
        Vector3 desired = focus + offset * clearDistance;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(focus);
        CharacterMovement movement = target.GetComponent<CharacterMovement>();
        if (lens != null && movement != null)
            lens.fieldOfView = Mathf.Lerp(lens.fieldOfView, 66f + Mathf.Clamp(movement.PlanarSpeed - 5f, 0f, 10f) * .8f, Time.deltaTime * 5f);
    }
}

