using System.Collections.Generic;
using UnityEngine;

public class ScoreArea : MonoBehaviour
{
    [Header("VFX Settings")]
    public Transform vfxSpawnPoint;

    private const float TeleportThreshold = 5f;

    private Collider areaCollider;
    private BallController[] balls = new BallController[0];
    private float ballRefreshTimer;
    private readonly Dictionary<BallController, float> lastBallHeights = new Dictionary<BallController, float>();

    private void Awake()
    {
        areaCollider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        ballRefreshTimer -= Time.fixedDeltaTime;
        if (ballRefreshTimer <= 0f)
        {
            balls = FindObjectsByType<BallController>(FindObjectsSortMode.None);
            ballRefreshTimer = 1f;
        }

        Bounds area = areaCollider != null ? areaCollider.bounds : new Bounds(transform.position, Vector3.one);
        float planeY = area.center.y;

        foreach (BallController ball in balls)
        {
            if (ball == null) continue;

            float currentY = ball.transform.position.y;
            bool hasPrevious = lastBallHeights.TryGetValue(ball, out float previousY);
            lastBallHeights[ball] = currentY;
            if (!hasPrevious) continue;

            if (ball.isHeld) continue;
            if (Mathf.Abs(currentY - previousY) > TeleportThreshold) continue;

            Vector3 pos = ball.transform.position;
            bool insideXZ = pos.x >= area.min.x && pos.x <= area.max.x &&
                            pos.z >= area.min.z && pos.z <= area.max.z;
            if (!insideXZ) continue;

            bool crossedDown = previousY > planeY && currentY <= planeY;
            bool crossedUp = previousY < planeY && currentY >= planeY;

            if (crossedUp)
            {
                ball.enteredHoopFromBelow = true;
            }
            else if (crossedDown)
            {
                if (ball.enteredHoopFromBelow)
                {
                    ball.enteredHoopFromBelow = false;
                }
                else if (BasketballGameManager.Instance != null)
                {
                    Vector3 spawnPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
                    BasketballGameManager.Instance.HandleScore(ball, spawnPos);
                }
            }
        }
    }
}
