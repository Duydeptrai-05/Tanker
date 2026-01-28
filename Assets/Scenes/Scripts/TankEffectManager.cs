using System.Collections;
using Unity.Netcode;
using UnityEngine;

// Định nghĩa danh sách các loại Buff
public enum BuffType
{
    Shield,         // 0: Khiên
    Invisibility,   // 1: Tàng hình
    Heal,           // 2: Hồi máu
    SpeedUp,        // 3: Tăng tốc
    Slow,           // 4: Làm chậm (Nhựa đường)
    Drunk,          // 5: Say rượu (Đảo ngược nút)
    TimeBomb        // 6: Bom hẹn giờ
}

public class TankEffectManager : NetworkBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] private TankController controller;
    [SerializeField] private TankHealth health;
    [SerializeField] private SpriteRenderer[] renderers; // Kéo tất cả Sprite (thân, súng) vào đây để làm tàng hình

    [Header("Hiệu ứng Visual (Kéo Prefab VFX vào nếu có)")]
    public GameObject shieldVFX;    // Hình cái khiên
    public GameObject bombVFX;      // Hình quả bom trên đầu
    public GameObject explosionVFX; // Hiệu ứng nổ bùm

    private void Awake()
    {
        // Tự tìm component nếu quên kéo
        if (controller == null) controller = GetComponent<TankController>();
        if (health == null) health = GetComponent<TankHealth>();
        if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    // --- HÀM NHẬN BUFF (GỌI TỪ SERVER) ---
    public void ApplyBuff(BuffType type)
    {
        if (!IsServer) return;

        Debug.Log($"Người chơi {OwnerClientId} nhận buff: {type}");

        switch (type)
        {
            case BuffType.Heal:
                health.Heal(30); // Hồi 30 máu
                break;

            case BuffType.Shield:
                StartCoroutine(ShieldRoutine(5f)); // Bất tử 5 giây
                break;

            case BuffType.Invisibility:
                ToggleInvisibilityClientRpc(true, 5f); // Tàng hình 5 giây
                break;

            case BuffType.SpeedUp:
                ApplySpeedClientRpc(2f, 5f); // Tốc độ x2 trong 5 giây
                break;

            case BuffType.Slow:
                ApplySpeedClientRpc(0.5f, 3f); // Tốc độ giảm một nửa trong 3 giây
                break;

            case BuffType.Drunk:
                ApplyDrunkClientRpc(4f); // Đảo ngược nút trong 4 giây
                break;

            case BuffType.TimeBomb:
                StartCoroutine(TimeBombRoutine(3f)); // Nổ sau 3 giây
                break;
        }
    }

    // --- CÁC LOGIC XỬ LÝ CHI TIẾT ---

    // 1. KHIÊN (Shield)
    IEnumerator ShieldRoutine(float duration)
    {
        health.isShielded = true; // Bật cờ bất tử bên TankHealth
        SetVisualClientRpc(BuffType.Shield, true); // Bật hình ảnh khiên
        yield return new WaitForSeconds(duration);
        health.isShielded = false;
        SetVisualClientRpc(BuffType.Shield, false);
    }

    // 2. BOM HẸN GIỜ (TimeBomb)
    IEnumerator TimeBombRoutine(float duration)
    {
        SetVisualClientRpc(BuffType.TimeBomb, true); // Hiện bom trên đầu
        yield return new WaitForSeconds(duration);
        SetVisualClientRpc(BuffType.TimeBomb, false);

        // Nổ gây sát thương cho chính mình
        health.TakeDamage(30);

        // Tạo hiệu ứng nổ
        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            vfx.GetComponent<NetworkObject>().Spawn();
        }
    }

    // 3. TÀNG HÌNH (Invisibility)
    [ClientRpc]
    void ToggleInvisibilityClientRpc(bool isHidden, float duration)
    {
        StartCoroutine(InvisRoutine(isHidden, duration));
    }
    IEnumerator InvisRoutine(bool isHidden, float duration)
    {
        float alpha = isHidden ? (IsOwner ? 0.5f : 0f) : 1f; // Mình thấy mờ mờ, địch không thấy

        foreach (var r in renderers)
            r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);

        yield return new WaitForSeconds(duration);

        // Trả về bình thường
        foreach (var r in renderers)
            r.color = new Color(r.color.r, r.color.g, r.color.b, 1f);
    }

    // 4. TỐC ĐỘ (Speed / Slow)
    [ClientRpc]
    void ApplySpeedClientRpc(float multiplier, float duration)
    {
        if (IsOwner) StartCoroutine(SpeedRoutine(multiplier, duration));
    }
    IEnumerator SpeedRoutine(float multiplier, float duration)
    {
        controller.speedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        controller.speedMultiplier = 1f;
    }

    // 5. SAY RƯỢU (Drunk)
    [ClientRpc]
    void ApplyDrunkClientRpc(float duration)
    {
        if (IsOwner) StartCoroutine(DrunkRoutine(duration));
    }
    IEnumerator DrunkRoutine(float duration)
    {
        controller.isDrunk = true;
        yield return new WaitForSeconds(duration);
        controller.isDrunk = false;
    }

    // --- HÀM HỖ TRỢ BẬT TẮT VFX ---
    [ClientRpc]
    void SetVisualClientRpc(BuffType type, bool active)
    {
        if (type == BuffType.Shield && shieldVFX != null) shieldVFX.SetActive(active);
        if (type == BuffType.TimeBomb && bombVFX != null) bombVFX.SetActive(active);
    }
}