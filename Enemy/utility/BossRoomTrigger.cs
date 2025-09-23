using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("Boss Spawning")]
    public EnemySpawnPoint bossSpawnPoint; // Kéo EnemySpawnPoint vào đây
    [Header("Door Control")]
    public DoorController doorController;
    public string doorGroupName;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;

            // Spawn boss khi player vào
            if (bossSpawnPoint != null)
                bossSpawnPoint.StartSpawning();

            // Đóng cửa
            if (doorController != null && !string.IsNullOrEmpty(doorGroupName))
                doorController.CloseDoors(doorGroupName);
        }
    }
}
