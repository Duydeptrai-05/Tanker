using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    [Header("Cài đặt Rương")]
    public GameObject chestPrefab;   // Kéo Prefab Rương vào đây
    public float chestLifetime = 15f; // Rương tồn tại 15s rồi tự mất
    public float respawnDelay = 5f;   // Sau khi mất, đợi 5s mới ra cái mới

    [Header("Phạm vi Map")]
    public float mapSizeX = 8f;
    public float mapSizeY = 4f;

    // Biến nội bộ để theo dõi
    private GameObject currentChest;
    private float timer;
    private bool hasChest = false; // Kiểm tra xem map đang có rương không

    public override void OnNetworkSpawn()
    {
        // Khi game bắt đầu, Server sẽ sinh rương ngay lập tức
        if (IsServer)
        {
            SpawnChest();
        }
    }

    private void Update()
    {
        if (!IsServer) return; // Chỉ Server được quyền tính toán

        // TRƯỜNG HỢP 1: Map ĐANG CÓ rương
        if (currentChest != null)
        {
            hasChest = true;
            timer += Time.deltaTime;

            // Nếu hết giờ mà chưa ai ăn -> Tự hủy
            if (timer >= chestLifetime)
            {
                currentChest.GetComponent<NetworkObject>().Despawn(false);
                Destroy(currentChest);

                // Reset timer để chuẩn bị cho pha hồi sinh tiếp theo
                timer = 0;
                hasChest = false;
            }
        }
        // TRƯỜNG HỢP 2: Map KHÔNG CÓ rương (Vừa bị ăn hoặc vừa tự hủy)
        else
        {
            // Nếu frame trước có rương, mà giờ mất -> Reset đồng hồ
            if (hasChest)
            {
                hasChest = false;
                timer = 0;
            }

            // Đếm ngược để sinh rương mới
            timer += Time.deltaTime;
            if (timer >= respawnDelay)
            {
                SpawnChest();
            }
        }
    }

    private void SpawnChest()
    {
        // 1. Random vị trí
        float randomX = Random.Range(-mapSizeX, mapSizeX);
        float randomY = Random.Range(-mapSizeY, mapSizeY);
        Vector2 spawnPos = new Vector2(randomX, randomY);

        // 2. Tạo rương
        currentChest = Instantiate(chestPrefab, spawnPos, Quaternion.identity);
        currentChest.GetComponent<NetworkObject>().Spawn();

        // 3. Reset các thông số
        timer = 0;
        hasChest = true;

        // 4. Báo cho tất cả người chơi biết
        ShowNotificationClientRpc();
    }

    [ClientRpc]
    private void ShowNotificationClientRpc()
    {
        // Gọi UI hiện chữ lên
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowNotification("RƯƠNG THƯỞNG ĐÃ XUẤT HIỆN!");
        }
    }
}