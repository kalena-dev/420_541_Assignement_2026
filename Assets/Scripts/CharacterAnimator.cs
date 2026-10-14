using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Animator animator;
    public ParticleSystem flipEffect;
    CharacterMovement movement;
    static readonly int Speed = Animator.StringToHash("Speed");
    static readonly int Grounded = Animator.StringToHash("Grounded");
    static readonly int Flip = Animator.StringToHash("Flip");
    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (flipEffect == null)
        {
            GameObject effect = new GameObject("Double jump burst");
            effect.transform.SetParent(transform, false);
            effect.transform.localPosition = Vector3.up * .8f;
            flipEffect = effect.AddComponent<ParticleSystem>();
            flipEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = flipEffect.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = .3f;
            main.startLifetime = .3f;
            main.startSpeed = 3f;
            main.startSize = .09f;
            main.startColor = new Color(.12f, .55f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = flipEffect.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });
            var shape = flipEffect.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = .25f;
            effect.GetComponent<ParticleSystemRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Blue");
        }
    }
    void OnEnable() { if (movement != null) movement.DoubleJumped += DoubleJump; }
    void OnDisable() { if (movement != null) movement.DoubleJumped -= DoubleJump; }
    void Update()
    {
        if (animator == null || movement == null) return;
        animator.SetFloat(Speed, movement.PlanarSpeed / movement.walkSpeed, .1f, Time.deltaTime);
        animator.SetBool(Grounded, movement.Grounded);
    }
    void DoubleJump()
    {
        if (animator != null) animator.SetTrigger(Flip);
        if (flipEffect != null) flipEffect.Play();
    }
}

