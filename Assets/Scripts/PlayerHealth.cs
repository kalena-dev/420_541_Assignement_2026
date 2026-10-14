using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int Health { get; private set; } = 3;
    public float killHeight = -12f;
    public bool Invulnerable => Time.time < invulnerableUntil;
    float invulnerableUntil;
    bool dead;
    Renderer[] visuals;
    void Awake() { visuals = GetComponentsInChildren<Renderer>(); }
    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (transform.position.y < killHeight) Die();
        bool visible = !Invulnerable || Mathf.FloorToInt(Time.time * 14f) % 2 == 0;
        foreach (Renderer visual in visuals) if (visual != null) visual.enabled = visible;
    }
    public void Damage()
    {
        if (dead || Invulnerable || !GameManager.Instance.IsPlaying) return;
        Health--;
        invulnerableUntil = Time.time + 1.1f;
        GameAudio.Play(Sound.Damage, transform.position);
        if (Health <= 0) Die();
    }
    void Die()
    {
        if (dead) return;
        dead = true;
        GameManager.Instance.RestartLevel();
    }
}

