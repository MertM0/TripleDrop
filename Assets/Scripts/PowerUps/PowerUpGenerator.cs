using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace TripleDrop.PowerUps
{
    [System.Serializable]
    public class PowerUpSetting
    {
        public PowerUpType powerUpType;
        public string prefabName;
        [Range(0f, 100f)]
        public float spawnWeight = 10f;
    }

    public class PowerUpGenerator : MonoBehaviourPunCallbacks
    {
        public List<PowerUpSetting> powerUpSettings = new List<PowerUpSetting>();
        public Vector3 spawnAreaCenter;
        public Vector3 spawnAreaSize = new Vector3(10, 0, 10);
        public float spawnHeight = 1f;
        public float spawnInterval = 15f;
        public int maxConcurrentPowerUps = 2;

        private float nextSpawnTime;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (powerUpSettings == null || powerUpSettings.Count == 0)
            {
                powerUpSettings = new List<PowerUpSetting>
                {
                    new PowerUpSetting { powerUpType = PowerUpType.SpeedUP,      prefabName = "PowerUp_SpeedUP",      spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.JumpUP,       prefabName = "PowerUp_JumpUP",       spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.PowerUP,      prefabName = "PowerUp_PowerUP",      spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.LowGravity,   prefabName = "PowerUp_LowGravity",   spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.HighGravity,  prefabName = "PowerUp_HighGravity",  spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.HeavyBall,    prefabName = "PowerUp_HeavyBall",    spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.LightBall,    prefabName = "PowerUp_LightBall",    spawnWeight = 10f },
                    new PowerUpSetting { powerUpType = PowerUpType.FireBall,     prefabName = "PowerUp_FireBall",     spawnWeight = 5f  },
                    new PowerUpSetting { powerUpType = PowerUpType.PlatformPanic,prefabName = "PowerUp_PlatformPanic",spawnWeight = 5f  }
                };
            }
        }
#endif

        private void Start()
        {
            nextSpawnTime = Time.time + spawnInterval;
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (newMasterClient.IsLocal)
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

            if (FindObjectsByType<PowerUp>(FindObjectsSortMode.None).Length >= maxConcurrentPowerUps) return;

            float totalWeight = 0f;
            foreach (var setting in powerUpSettings) totalWeight += setting.spawnWeight;

            if (totalWeight <= 0f) return;

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

            if (string.IsNullOrEmpty(selectedSetting.prefabName)) return;

            Vector3 spawnPos = new Vector3(
                spawnAreaCenter.x + Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                spawnHeight,
                spawnAreaCenter.z + Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            );

            object[] data = new object[] { (int)selectedSetting.powerUpType };

            PhotonNetwork.InstantiateRoomObject(selectedSetting.prefabName, spawnPos, Quaternion.identity, 0, data);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Vector3 gizmoSize = new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.z);
            Gizmos.DrawCube(new Vector3(spawnAreaCenter.x, spawnHeight, spawnAreaCenter.z), gizmoSize);
        }
    }
}
