//using Unity.Netcode;
//using UnityEngine;

//public class CaptureZone : NetworkBehaviour
//{
//    [Header("--- HÌNH ẢNH HIỂN THỊ ---")]
//    [SerializeField] private SpriteRenderer zoneSprite; // Code sẽ tự tìm cái này
//    [SerializeField] private Color myColor = Color.green;
//    [SerializeField] private Color enemyColor = Color.red;
//    [SerializeField] private Color neutralColor = Color.white;

//    [Header("--- CÀI ĐẶT CHIẾM ĐÓNG ---")]
//    public float captureTimeRequired = 3.0f;
//    private float currentCaptureTimer = 0f;

//    [Header("--- CÀI ĐẶT ĐIỂM SỐ ---")]
//    public float scoreInterval = 1.0f;
//    public int pointsPerInterval = 1;

//    // Biến mạng: 999 là chưa ai chiếm
//    public NetworkVariable<ulong> ownerId = new NetworkVariable<ulong>(999);
//    private float scoreTimer = 0f;

//    // --- [FIX LỖI QUAN TRỌNG] Tự động tìm Sprite ---
//    private void Awake()
//    {
//        if (zoneSprite == null)
//        {
//            zoneSprite = GetComponent<SpriteRenderer>();
//        }
//    }

//    public override void OnNetworkSpawn()
//    {
//        // Đăng ký sự kiện đổi màu
//        ownerId.OnValueChanged += OnOwnerChanged;

//        // Cập nhật màu ngay khi vào game
//        UpdateVisualColor(ownerId.Value);
//    }

//    public override void OnNetworkDespawn()
//    {
//        ownerId.OnValueChanged -= OnOwnerChanged;
//    }

//    private void OnTriggerStay2D(Collider2D other)
//    {
//        if (!IsServer) return;

//        if (other.CompareTag("Player"))
//        {
//            var tankObj = other.GetComponent<NetworkObject>();
//            if (tankObj != null)
//            {
//                ulong playerId = tankObj.OwnerClientId;

//                // Nếu người đứng trong vòng KHÔNG PHẢI chủ hiện tại -> Bắt đầu chiếm
//                if (ownerId.Value != playerId)
//                {
//                    currentCaptureTimer += Time.deltaTime;
//                    if (currentCaptureTimer >= captureTimeRequired)
//                    {
//                        ownerId.Value = playerId; // Đổi chủ -> Tự động đổi màu
//                        currentCaptureTimer = 0f;
//                    }
//                }
//                else
//                {
//                    currentCaptureTimer = 0f;
//                }
//            }
//        }
//    }

//    private void OnTriggerExit2D(Collider2D other)
//    {
//        if (!IsServer) return;
//        if (other.CompareTag("Player"))
//        {
//            currentCaptureTimer = 0f;
//        }
//    }

//    private void Update()
//    {
//        if (!IsServer) return;

//        // Logic cộng điểm
//        if (ownerId.Value != 999)
//        {
//            scoreTimer += Time.deltaTime;
//            if (scoreTimer >= scoreInterval)
//            {
//                scoreTimer = 0;
//                // Gọi GameManager để cộng điểm (Host tự tính)
//                if (GameManager.Instance != null)
//                {
//                    GameManager.Instance.AddScore(ownerId.Value, pointsPerInterval);
//                }
//            }
//        }
//    }

//    private void OnOwnerChanged(ulong oldId, ulong newId)
//    {
//        UpdateVisualColor(newId);
//    }

//    // --- HÀM ĐỔI MÀU (Đã sửa lại cho chắc chắn) ---
//    private void UpdateVisualColor(ulong currentOwner)
//    {
//        // Nếu không tìm thấy Sprite thì tìm lại lần nữa cho chắc
//        if (zoneSprite == null) zoneSprite = GetComponent<SpriteRenderer>();
//        if (zoneSprite == null) return; // Vẫn không thấy thì bó tay

//        ulong myId = NetworkManager.Singleton.LocalClientId;

//        if (currentOwner == 999)
//        {
//            zoneSprite.color = neutralColor;
//        }
//        else if (currentOwner == myId)
//        {
//            // Nếu là của MÌNH -> Màu Xanh
//            zoneSprite.color = myColor;
//        }
//        else
//        {
//            // Nếu là của ĐỊCH -> Màu Đỏ
//            zoneSprite.color = enemyColor;
//        }
//    }
//}
using Unity.Netcode;
using UnityEngine;

public class CaptureZone : NetworkBehaviour
{
    [Header("--- HÌNH ẢNH HIỂN THỊ ---")]
    [SerializeField] private SpriteRenderer zoneSprite;
    [SerializeField] private Color myColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;

    [Header("--- CÀI ĐẶT CHIẾM ĐÓNG ---")]
    public float captureTimeRequired = 3.0f;
    private float currentCaptureTimer = 0f;

    [Header("--- CÀI ĐẶT ĐIỂM SỐ ---")]
    public float scoreInterval = 1.0f;
    public int pointsPerInterval = 1;

    public NetworkVariable<ulong> ownerId = new NetworkVariable<ulong>(999);
    private float scoreTimer = 0f;

    private void Awake()
    {
        if (zoneSprite == null) zoneSprite = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        ownerId.OnValueChanged += OnOwnerChanged;
        UpdateVisualColor(ownerId.Value);
    }

    public override void OnNetworkDespawn()
    {
        ownerId.OnValueChanged -= OnOwnerChanged;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            var tankObj = other.GetComponent<NetworkObject>();
            if (tankObj != null)
            {
                ulong playerId = tankObj.OwnerClientId;
                if (ownerId.Value != playerId)
                {
                    currentCaptureTimer += Time.deltaTime;
                    if (currentCaptureTimer >= captureTimeRequired)
                    {
                        ownerId.Value = playerId;
                        currentCaptureTimer = 0f;
                    }
                }
                else
                {
                    currentCaptureTimer = 0f;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player")) currentCaptureTimer = 0f;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (ownerId.Value != 999)
        {
            scoreTimer += Time.deltaTime;
            if (scoreTimer >= scoreInterval)
            {
                scoreTimer = 0;
                if (GameManager.Instance != null) GameManager.Instance.AddScore(ownerId.Value, pointsPerInterval);
            }
        }
    }

    private void OnOwnerChanged(ulong oldId, ulong newId)
    {
        UpdateVisualColor(newId);

        // [MỚI] PHÁT TIẾNG CHIẾM ĐÓNG
        if (newId != 999 && AudioManager.Instance)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.captureClip);
        }
    }

    private void UpdateVisualColor(ulong currentOwner)
    {
        if (zoneSprite == null) zoneSprite = GetComponent<SpriteRenderer>();
        if (zoneSprite == null) return;

        ulong myId = NetworkManager.Singleton.LocalClientId;

        if (currentOwner == 999) zoneSprite.color = neutralColor;
        else if (currentOwner == myId) zoneSprite.color = myColor;
        else zoneSprite.color = enemyColor;
    }
}