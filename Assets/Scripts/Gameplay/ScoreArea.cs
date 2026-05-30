using UnityEngine;

public class ScoreArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null)
        {
            if (BasketballGameManager.Instance != null)
            {
                BasketballGameManager.Instance.HandleScore(ball);
            }
        }
    }
}