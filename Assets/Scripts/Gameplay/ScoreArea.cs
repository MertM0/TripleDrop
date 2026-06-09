using UnityEngine;

public class ScoreArea : MonoBehaviour
{
    [Header("VFX Settings")]
    public Transform vfxSpawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null)
        {
            if (BasketballGameManager.Instance != null)
            {
                Vector3 spawnPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
                BasketballGameManager.Instance.HandleScore(ball, spawnPos);
            }
        }
    }
}
