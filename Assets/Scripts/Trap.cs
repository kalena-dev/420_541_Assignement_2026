using UnityEngine;

public class Trap : MonoBehaviour
{
    void OnTriggerEnter(Collider other) { Hurt(other); }
    void OnTriggerStay(Collider other) { Hurt(other); }
    void Hurt(Collider other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player != null) player.Damage();
    }
}

