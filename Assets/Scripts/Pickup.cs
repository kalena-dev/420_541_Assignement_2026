using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public PickupType type;
    public Transform visual;
    public float respawnSeconds = 30f;
    Vector3 rest;
    Collider trigger;
    bool collected;
    void Awake()
    {
        trigger = GetComponent<Collider>();
        rest = visual != null ? visual.localPosition : Vector3.zero;
    }
    void Update()
    {
        if (visual == null) return;
        visual.localPosition = rest + Vector3.up * (Mathf.Sin(Time.time * 2.3f + transform.position.z) * .16f);
        visual.Rotate(0f, 65f * Time.deltaTime, 0f, Space.World);
    }
    void OnTriggerEnter(Collider other)
    {
        PlayerPowerups player = other.GetComponentInParent<PlayerPowerups>();
        if (collected || player == null || !GameManager.Instance.IsPlaying) return;
        collected = true;
        trigger.enabled = false;
        player.Collect(type);
        GameAudio.Play(type == PickupType.Score ? Sound.Pickup : Sound.Powerup, transform.position);
        if (visual != null) visual.gameObject.SetActive(false);
        if (type == PickupType.Score) Destroy(gameObject);
        else StartCoroutine(Respawn());
    }
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnSeconds);
        if (visual != null) visual.gameObject.SetActive(true);
        trigger.enabled = true;
        collected = false;
    }
}

