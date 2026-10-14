using UnityEngine;

public enum PickupType { Score, Speed, ScoreBoost, Jump }

public class PlayerPowerups : MonoBehaviour
{
    float speedUntil;
    float scoreUntil;
    float jumpUntil;
    public float SpeedRemaining => Mathf.Max(0f, speedUntil - Time.time);
    public float ScoreRemaining => Mathf.Max(0f, scoreUntil - Time.time);
    public float JumpRemaining => Mathf.Max(0f, jumpUntil - Time.time);
    public bool SpeedActive => SpeedRemaining > 0f;
    public bool DoubleJumpActive => JumpRemaining > 0f;
    public int ScoreMultiplier => ScoreRemaining > 0f ? 2 : 1;
    public void Collect(PickupType type)
    {
        switch (type)
        {
            case PickupType.Score: GameManager.Instance.AddScore(50 * ScoreMultiplier); break;
            case PickupType.Speed: speedUntil = Time.time + 5f; break;
            case PickupType.ScoreBoost: scoreUntil = Time.time + 10f; break;
            case PickupType.Jump: jumpUntil = Time.time + 30f; break;
        }
    }
}

