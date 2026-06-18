using System.Collections.Generic;
using UnityEngine;

public class ScoreArea : MonoBehaviour
{
    [Header("VFX Settings")]
    public Transform vfxSpawnPoint;

    [Header("Score Settings")]
    public float scoreRadius = 0.3f;

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
            Vector2 ballXZ = new Vector2(pos.x, pos.z);
            Vector2 centerXZ = new Vector2(area.center.x, area.center.z);
            if (Vector2.Distance(ballXZ, centerXZ) > scoreRadius) continue;

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

    private void OnDrawGizmos()
    {
        Collider c = areaCollider != null ? areaCollider : GetComponent<Collider>();
        Vector3 center = c != null ? c.bounds.center : transform.position;

        Gizmos.color = new Color(0f, 1f, 0f, 1f);
        DrawCircle(center, scoreRadius);

        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Vector3 planeSize = new Vector3(scoreRadius * 2f, 0.001f, scoreRadius * 2f);
        Gizmos.DrawCube(center, planeSize);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        const int segments = 48;
        Vector3 previous = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
