using UnityEngine;
using Photon.Pun;

/// <summary>
/// Spawns networked players when they join the game
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool spawnOnStart = true;

    [Header("Spawn Randomization")]
    [SerializeField] private bool randomizeSpawnPoint = true;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    private GameObject localPlayer;

    private void Start()
    {
        if (spawnOnStart && PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
    }

    /// <summary>
    /// Spawn the local player
    /// </summary>
    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Not connected to Photon network!");
            return;
        }

        // Get spawn position
        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = GetSpawnRotation();

        // Spawn player over network
        localPlayer = PhotonNetwork.Instantiate(
            playerPrefab.name, 
            spawnPosition, 
            spawnRotation
        );

        Debug.Log($"Spawned player: {PhotonNetwork.NickName} at {spawnPosition}");

        // You can setup camera to follow local player here
        SetupPlayerCamera();
    }

    /// <summary>
    /// Get spawn position based on settings
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex;
            
            if (randomizeSpawnPoint)
            {
                spawnIndex = Random.Range(0, spawnPoints.Length);
            }
            else
            {
                // Use player number to determine spawn point
                spawnIndex = (PhotonNetwork.CurrentRoom.PlayerCount - 1) % spawnPoints.Length;
            }

            return spawnPoints[spawnIndex].position + spawnOffset;
        }
        else
        {
            // Default spawn at origin with random offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-3f, 3f),
                0f,
                Random.Range(-3f, 3f)
            );
            
            return transform.position + randomOffset + spawnOffset;
        }
    }

    /// <summary>
    /// Get spawn rotation
    /// </summary>
    private Quaternion GetSpawnRotation()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = randomizeSpawnPoint 
                ? Random.Range(0, spawnPoints.Length)
                : (PhotonNetwork.CurrentRoom.PlayerCount - 1) % spawnPoints.Length;

            return spawnPoints[spawnIndex].rotation;
        }
        
        return Quaternion.identity;
    }

    /// <summary>
    /// Setup camera to follow local player
    /// </summary>
    private void SetupPlayerCamera()
    {
        if (localPlayer == null) return;

        // Find main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Example: Simple camera follow script
        // You can customize this based on your camera system
        var cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = localPlayer.transform;
        }
        else
        {
            // If no camera follow script exists, you can add one
            // Or manually set camera parent
            Debug.Log("No camera follow script found. Add one to track the player.");
        }
    }

    /// <summary>
    /// Despawn the local player
    /// </summary>
    public void DespawnPlayer()
    {
        if (localPlayer != null)
        {
            PhotonNetwork.Destroy(localPlayer);
            localPlayer = null;
            Debug.Log("Player despawned");
        }
    }

    #region Gizmos (Editor Only)

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // Draw spawn points in editor
        Gizmos.color = Color.green;
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 2f);
            }
        }
    }
#endif

    #endregion
}

/// <summary>
/// Simple camera follow script (optional)
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private bool lookAtTarget = true;

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Look at target
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
}
