using UnityEngine;

// Dòng này giúp tạo menu chuột phải để tạo file data mới
[CreateAssetMenu(fileName = "NewTankData", menuName = "Tank Game/Tank Data")]
public class TankData : ScriptableObject
{
    [Header("Giao diện")]
    public Sprite tankBody;      // Hình thân xe
    public Sprite tankTurret;    // Hình nòng súng

    [Header("Sức mạnh")]
    public GameObject bulletPrefab; // Loại đạn riêng của xe này
    public float moveSpeed = 3f;    // Tốc độ chạy
}