using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

namespace TripleDrop.PowerUps
{
    [System.Serializable]
    public class PowerUpSetting
    {
        public PowerUpType powerUpType;
        [Range(0f, 100f)]
        public float spawnWeight = 10f;
    }

    public class PowerUpGenerator : MonoBehaviourPunCallbacks
    {
        public List<PowerUpSetting> powerUpSettings = new List<PowerUpSetting>();
        public Vector3 spawnAreaCenter;
        public Vector3 spawnAreaSize = new Vector3(10, 0, 10);
        public float spawnInterval = 15f;
        public string powerUpPrefabName = "PowerUp";

        private float nextSpawnTime;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (powerUpSettings == null || powerUpSettings.Count == 0)
            {
                powerUpSettings = new List<PowerUpSetting>
                {
                    new PowerUpSetting { powerUpType = PowerUpType.SpeedUP, spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.JumpUP, spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.PowerUP, spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.LowGravity, spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.HighGravity, spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.HeavyBall, spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.LightBall, spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.FireBall, spawnWeight = 5f }, // FireBall is slightly rarer
                    new PowerUpSetting { powerUpType = PowerUpType.PlatformPanic, spawnWeight = 5f }
                };
            }
        }
#endif

        private void Start()
        {
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (Time.time >= nextSpawnTime)
            {
                SpawnPowerUp();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        private void SpawnPowerUp()
        {
            if (powerUpSettings == null || powerUpSettings.Count == 0) return;

            Vector3 randomPos = spawnAreaCenter + new Vector3(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f),
                Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            );

            float totalWeight = 0f;
            foreach (var setting in powerUpSettings) totalWeight += setting.spawnWeight;

            float randomWeight = Random.Range(0f, totalWeight);
            PowerUpSetting selectedSetting = powerUpSettings[0];

            foreach (var setting in powerUpSettings)
            {
                randomWeight -= setting.spawnWeight;
                if (randomWeight <= 0f)
                {
                    selectedSetting = setting;
                    break;
                }
            }

            object[] data = new object[]
            {
                (int)selectedSetting.powerUpType
            };

            PhotonNetwork.InstantiateRoomObject(powerUpPrefabName, randomPos, Quaternion.identity, 0, data);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
        }
    }
}