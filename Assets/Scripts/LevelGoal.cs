using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() != null) GameManager.Instance.FinishLevel();
    }
}

