using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ChestSpawner : NetworkBehaviour
{
    [Header("--- CÀI ĐẶT RƯƠNG ---")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private float spawnInterval = 10f; // Bao lâu ra 1 rương
    [SerializeField] private int maxChests = 5;         // Tối đa bao nhiêu rương trên map

    [Header("--- PHẠM VI & ĐIỀU KIỆN ---")]
    [SerializeField] private Vector2 mapSize = new Vector2(18, 10); // Kích thước map (X, Y)
    [SerializeField] private float checkRadius = 0.8f; // Bán kính kiểm tra (Rương to bao nhiêu thì để bấy nhiêu)
    [SerializeField] private LayerMask obstacleLayer;   // Những lớp vật cản (Tường, Đá, Nước...)

    private float timer;

    public override void OnNetworkSpawn()
    {
        // Chỉ Server mới được quyền sinh rương
        if (IsServer)
        {
            timer = spawnInterval;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = spawnInterval;

            // Kiểm tra số lượng rương hiện tại, nếu ít hơn giới hạn thì mới sinh thêm
            if (GameObject.FindGameObjectsWithTag("Chest").Length < maxChests)
            {
                SpawnChestSafe();
            }
        }
    }

    private void SpawnChestSafe()
    {
        Vector2 spawnPos = Vector2.zero;
        bool foundPosition = false;

        // Thử tìm vị trí trống (Thử tối đa 20 lần để tránh treo máy)
        for (int i = 0; i < 20; i++)
        {
            // 1. Random tọa độ trong phạm vi Map
            float randomX = Random.Range(-mapSize.x / 2, mapSize.x / 2);
            float randomY = Random.Range(-mapSize.y / 2, mapSize.y / 2);
            Vector2 potentialPos = new Vector2(randomX, randomY);

            // 2. KIỂM TRA VẬT CẢN (Quan trọng nhất)
            // Quét 1 vòng tròn xem có chạm vào Layer Obstacle không
            Collider2D hit = Physics2D.OverlapCircle(potentialPos, checkRadius, obstacleLayer);

            // Nếu hit == null nghĩa là KHÔNG CHẠM GÌ -> Vị trí ngon
            if (hit == null)
            {
                spawnPos = potentialPos;
                foundPosition = true;
                break; // Tìm thấy rồi thì thoát vòng lặp
            }
        }

        // Nếu tìm được chỗ trống thì sinh rương
        if (foundPosition)
        {
            GameObject chest = Instantiate(chestPrefab, spawnPos, Quaternion.identity);
            chest.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy chỗ trống để sinh rương! Map quá chật?");
        }
    }

    // Vẽ hình chữ nhật phạm vi spawn để dễ căn chỉnh trong Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapSize.x, mapSize.y, 0));
    }
}