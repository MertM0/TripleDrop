using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

namespace TripleDrop.PowerUps
{
    [RequireComponent(typeof(PhotonView))]
    public class PlayerPowerUpController : MonoBehaviourPun
    {
        [Header("Power-Up Settings")]
        public float speedUpDuration = 10f; public float speedUpMultiplier = 1.5f;
        public float jumpUpDuration = 10f; public float jumpUpMultiplier = 1.5f;
        public float throwPowerDuration = 10f; public float throwPowerMultiplier = 1.5f;
        public float lowGravityDuration = 10f; public float lowGravityScale = 0.5f;
        public float highGravityDuration = 10f; public float highGravityScale = 2f;
        public float heavyBallDuration = 10f; public float heavyBallMass = 3f;
        public float lightBallDuration = 10f; public float lightBallMass = 0.5f;
        public float fireBallDuration = 10f;
        public float platformPanicDuration = 5f;

        // Current active modifiers
        public float SpeedMultiplier { get; private set; } = 1f;
        public float JumpMultiplier { get; private set; } = 1f;
        public float ThrowPowerMultiplier { get; private set; } = 1f;
        public float GravityScale { get; private set; } = 1f;
        public float BallMassMultiplier { get; private set; } = 1f;
        public bool IsFireBallActive { get; private set; } = false;
        public bool IsPlatformPanicActive { get; private set; } = false;

        private Dictionary<PowerUpType, Coroutine> activePowerUps = new Dictionary<PowerUpType, Coroutine>();

        public void ApplyPowerUp(PowerUpType type)
        {
            // Sync activation to all clients
            photonView.RPC(nameof(RPC_ActivatePowerUp), RpcTarget.All, (int)type);
        }

        [PunRPC]
        private void RPC_ActivatePowerUp(int typeInt)
        {
            PowerUpType type = (PowerUpType)typeInt;

            Debug.Log($"[PowerUp] Player {photonView.Owner.NickName} collected: {type}");

            // Stop existing coroutine for this type to reset duration
            if (activePowerUps.ContainsKey(type) && activePowerUps[type] != null)
            {
                StopCoroutine(activePowerUps[type]);
            }

            float duration = 0f;

            switch (type)
            {
                case PowerUpType.SpeedUP:
                    SpeedMultiplier = speedUpMultiplier;
                    duration = speedUpDuration;
                    break;
                case PowerUpType.JumpUP:
                    JumpMultiplier = jumpUpMultiplier;
                    duration = jumpUpDuration;
                    break;
                case PowerUpType.PowerUP:
                    ThrowPowerMultiplier = throwPowerMultiplier;
                    duration = throwPowerDuration;
                    break;
                case PowerUpType.LowGravity:
                    if (activePowerUps.ContainsKey(PowerUpType.HighGravity)) { StopCoroutine(activePowerUps[PowerUpType.HighGravity]); activePowerUps.Remove(PowerUpType.HighGravity); }
                    GravityScale = lowGravityScale;
                    duration = lowGravityDuration;
                    break;
                case PowerUpType.HighGravity:
                    if (activePowerUps.ContainsKey(PowerUpType.LowGravity)) { StopCoroutine(activePowerUps[PowerUpType.LowGravity]); activePowerUps.Remove(PowerUpType.LowGravity); }
                    GravityScale = highGravityScale;
                    duration = highGravityDuration;
                    break;
                case PowerUpType.FireBall:
                    IsFireBallActive = true;
                    duration = fireBallDuration;
                    break;
                case PowerUpType.HeavyBall:
                    if (activePowerUps.ContainsKey(PowerUpType.LightBall)) { StopCoroutine(activePowerUps[PowerUpType.LightBall]); activePowerUps.Remove(PowerUpType.LightBall); }
                    BallMassMultiplier = heavyBallMass;
                    duration = heavyBallDuration;
                    break;
                case PowerUpType.LightBall:
                    if (activePowerUps.ContainsKey(PowerUpType.HeavyBall)) { StopCoroutine(activePowerUps[PowerUpType.HeavyBall]); activePowerUps.Remove(PowerUpType.HeavyBall); }
                    BallMassMultiplier = lightBallMass;
                    duration = lightBallDuration;
                    break;
                case PowerUpType.PlatformPanic:
                    IsPlatformPanicActive = true;
                    duration = platformPanicDuration;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // Spawn a moving wall
                        // PhotonNetwork.Instantiate("MovingWall", transform.position, Quaternion.identity);
                    }
                    break;
            }

            if (duration > 0)
            {
                Coroutine routine = StartCoroutine(PowerUpRoutine(type, duration));
                activePowerUps[type] = routine;
            }
        }

        private IEnumerator PowerUpRoutine(PowerUpType type, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemovePowerUp(type);
        }

        public void RemovePowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.SpeedUP:
                    SpeedMultiplier = 1f;
                    break;
                case PowerUpType.JumpUP:
                    JumpMultiplier = 1f;
                    break;
                case PowerUpType.PowerUP:
                    ThrowPowerMultiplier = 1f;
                    break;
                case PowerUpType.LowGravity:
                case PowerUpType.HighGravity:
                    GravityScale = 1f;
                    break;
                case PowerUpType.FireBall:
                    IsFireBallActive = false;
                    break;
                case PowerUpType.HeavyBall:
                case PowerUpType.LightBall:
                    BallMassMultiplier = 1f;
                    break;
                case PowerUpType.PlatformPanic:
                    IsPlatformPanicActive = false;
                    break;
            }

            if (activePowerUps.ContainsKey(type))
            {
                activePowerUps.Remove(type);
            }
        }

        public void ConsumeFireBall()
        {
            if (IsFireBallActive)
            {
                photonView.RPC(nameof(RPC_ConsumeFireBall), RpcTarget.All);
            }
        }

        [PunRPC]
        private void RPC_ConsumeFireBall()
        {
            IsFireBallActive = false;
        }
    }
}