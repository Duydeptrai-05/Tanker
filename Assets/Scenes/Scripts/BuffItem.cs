using Unity.Netcode;
using UnityEngine;

public class BuffItem : NetworkBehaviour
{
    [Header("Chọn loại hiệu ứng")]
    public BuffType type;

    // Biến cờ để đảm bảo chỉ ăn 1 lần
    private bool isEaten = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ Server mới xử lý
        if (!IsServer || isEaten) return;

        if (other.CompareTag("Player"))
        {
            var effectManager = other.GetComponent<TankEffectManager>();
            if (effectManager != null)
            {
                isEaten = true; // Đánh dấu đã ăn

                // 1. Kích hoạt hiệu ứng
                effectManager.ApplyBuff(type);

                // 2. Ẩn vật phẩm đi ngay lập tức (để người chơi thấy là đã ăn)
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;

                // 3. Đợi một xíu (0.1s) rồi mới xóa hẳn khỏi Server
                // Việc này giúp Netcode kịp đồng bộ lệnh Spawn trước khi Despawn
                Invoke(nameof(DespawnItem), 0.1f);
            }
        }
    }

    private void DespawnItem()
    {
        if (IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}