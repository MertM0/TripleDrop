using UnityEngine;

namespace TripleDrop.PowerUps
{
    [CreateAssetMenu(fileName = "NewPowerUpData", menuName = "TripleDrop/PowerUps/PowerUpData", order = 1)]
    public class PowerUpData : ScriptableObject
    {
        [Tooltip("The type of power-up.")]
        public PowerUpType powerUpType;

        [Tooltip("Duration in seconds for the power-up effect. (0 for instant/permanent)")]
        public float duration = 10f;

        [Tooltip("Modifier value applied by this power-up (e.g., speed multiplier, gravity scale).")]
        public float modifierValue = 1.5f;
    }
}