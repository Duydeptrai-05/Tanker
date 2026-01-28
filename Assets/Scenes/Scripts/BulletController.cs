using UnityEngine;
using Unity.Netcode;

public class BulletController : NetworkBehaviour
{
    [Header("Cài đặt chỉ số đạn")]
    public float speed = 10f;
    public int damage = 10;
    public float maxRange = 15f;

    [HideInInspector]
    public ulong shooterId; // ID của người bắn đạn này

    private Vector2 startPos;
    private bool daVaCham = false; // Biến cờ để tránh va chạm kép

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            startPos = transform.position;
            // Unity 6 dùng linearVelocity. Nếu Unity cũ thì dùng velocity
            // Đổi 'up' thành 'right' để bay theo mũi tên Đỏ
            GetComponent<Rigidbody2D>().linearVelocity = transform.right * speed;

            // Tự hủy sau 5 giây nếu không trúng gì
            Invoke(nameof(DespawnBullet), 5f);
        }
    }

    private void Update()
    {
        if (!IsServer || daVaCham) return;

        // Kiểm tra tầm bắn tối đa
        if (Vector2.Distance(startPos, transform.position) > maxRange)
        {
            DespawnBullet();
        }
    }

    // Dùng OnTriggerEnter2D để xử lý va chạm (Nhớ tích Is Trigger ở Prefab)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Chỉ Server mới được xử lý va chạm
        if (!IsServer || daVaCham) return;

        // 2. BỎ QUA NGƯỜI BẮN (Để không tự bắn trúng mình lúc vừa sinh ra)
        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.OwnerClientId == shooterId) return;

        GameObject hitObject = other.gameObject;

        // --- XỬ LÝ CÁC LOẠI VA CHẠM ---

        // TRƯỜNG HỢP 1: TRÚNG ĐÁ / MÔI TRƯỜNG (Tag "Environment")
        // Đây là đoạn xử lý cho cục đá "stone" của bạn
        if (hitObject.CompareTag("Environment"))
        {
            daVaCham = true;
            DespawnBullet(); // Xóa đạn ngay lập tức
            return;
        }

        // TRƯỜNG HỢP 2: TRÚNG TƯỜNG CÓ MÁU (Script WallHealth)
        WallHealth wall = hitObject.GetComponent<WallHealth>();
        if (wall != null)
        {
            wall.TakeDamage(damage);
            daVaCham = true;
            DespawnBullet();
            return;
        }

        // TRƯỜNG HỢP 3: TRÚNG XE TĂNG ĐỊCH
        TankHealth tank = hitObject.GetComponent<TankHealth>();
        if (tank != null)
        {
            tank.TakeDamage(damage);
            daVaCham = true;
            DespawnBullet();
            return;
        }
    }

    private void DespawnBullet()
    {
        // Kiểm tra xem đạn còn tồn tại không trước khi xóa
        if (IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}