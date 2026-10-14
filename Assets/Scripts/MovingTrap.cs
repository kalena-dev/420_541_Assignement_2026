using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingTrap : MonoBehaviour
{
    public Vector3 travel = new Vector3(3f, 0f, 0f);
    public float period = 3f;
    public float phase;
    Vector3 origin;
    Rigidbody body;
    void Awake() { origin = transform.position; body = GetComponent<Rigidbody>(); }
    void FixedUpdate()
    {
        body.MovePosition(origin + travel * Mathf.Sin(Time.time * Mathf.PI * 2f / period + phase));
    }
}

