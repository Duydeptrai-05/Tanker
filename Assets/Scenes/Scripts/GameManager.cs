//using TMPro;
//using Unity.Netcode;
//using UnityEngine;

//public class GameManager : NetworkBehaviour
//{
//    // Singleton để gọi từ nơi khác
//    public static GameManager Instance;

//    // --- [MỚI THÊM] PHẦN QUẢN LÝ VỊ TRÍ SINH RA ---
//    [Header("--- ĐIỂM SINH RA (SPAWN POINTS) ---")]
//    public Transform hostSpawnPoint;   // Kéo SpawnPos_Host vào đây
//    public Transform clientSpawnPoint; // Kéo SpawnPos_Client vào đây

//    // --- PHẦN QUẢN LÝ ĐIỂM SỐ (CODE CŨ CỦA BẠN) ---
//    [Header("Cài đặt Điểm Số")]
//    public NetworkVariable<int> ScoreP1 = new NetworkVariable<int>(0);
//    public NetworkVariable<int> ScoreP2 = new NetworkVariable<int>(0);

//    public int winScore = 5; // Điểm thắng

//    [Header("Giao Diện (Kéo UI trong GameScene vào)")]
//    [SerializeField] private TextMeshProUGUI txtScoreP1;
//    [SerializeField] private TextMeshProUGUI txtScoreP2;
//    [SerializeField] private GameObject winPanel;
//    [SerializeField] private TextMeshProUGUI txtWinnerText;

//    private void Awake()
//    {
//        // Tạo Singleton
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    public override void OnNetworkSpawn()
//    {
//        // Reset điểm khi vào game (chỉ Server làm)
//        if (IsServer)
//        {
//            ResetGame();
//        }

//        // Đăng ký sự kiện thay đổi điểm để cập nhật UI
//        ScoreP1.OnValueChanged += (oldVal, newVal) => UpdateUI();
//        ScoreP2.OnValueChanged += (oldVal, newVal) => UpdateUI();

//        UpdateUI();
//        if (winPanel != null) winPanel.SetActive(false);
//    }

//    private void ResetGame()
//    {
//        ScoreP1.Value = 0;
//        ScoreP2.Value = 0;
//    }

//    private void UpdateUI()
//    {
//        if (txtScoreP1 != null) txtScoreP1.text = "P1: " + ScoreP1.Value;
//        if (txtScoreP2 != null) txtScoreP2.text = "P2: " + ScoreP2.Value;
//    }

//    // Hàm cộng điểm
//    // Hàm cộng điểm
//    public void AddScore(ulong shooterId, int amount = 1)
//    {
//        if (!IsServer) return;

//        // SỬA LẠI ĐÚNG: Gọi trực tiếp ServerClientId
//        if (shooterId == NetworkManager.ServerClientId)
//        {
//            ScoreP1.Value += amount; // Host ghi điểm
//        }
//        else
//        {
//            ScoreP2.Value += amount; // Client ghi điểm
//        }

//        CheckWinCondition();
//    }

//    private void CheckWinCondition()
//    {
//        if (ScoreP1.Value >= winScore)
//        {
//            EndGameClientRpc("PLAYER 1 CHIẾN THẮNG!");
//        }
//        else if (ScoreP2.Value >= winScore)
//        {
//            EndGameClientRpc("PLAYER 2 CHIẾN THẮNG!");
//        }
//    }

//    [ClientRpc]
//    private void EndGameClientRpc(string message)
//    {
//        if (winPanel != null)
//        {
//            winPanel.SetActive(true);
//            txtWinnerText.text = message;
//            Time.timeScale = 0; // Dừng game
//        }
//    }
//}
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("--- ĐIỂM SINH RA ---")]
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;

    [Header("--- ĐIỂM SỐ ---")]
    public NetworkVariable<int> ScoreP1 = new NetworkVariable<int>(0);
    public NetworkVariable<int> ScoreP2 = new NetworkVariable<int>(0);
    public int winScore = 5;

    [Header("--- UI ---")]
    [SerializeField] private TextMeshProUGUI txtScoreP1;
    [SerializeField] private TextMeshProUGUI txtScoreP2;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI txtWinnerText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) ResetGame();
        ScoreP1.OnValueChanged += (oldVal, newVal) => UpdateUI();
        ScoreP2.OnValueChanged += (oldVal, newVal) => UpdateUI();
        UpdateUI();
        if (winPanel != null) winPanel.SetActive(false);
    }

    private void ResetGame()
    {
        ScoreP1.Value = 0;
        ScoreP2.Value = 0;
    }

    private void UpdateUI()
    {
        if (txtScoreP1 != null) txtScoreP1.text = "P1: " + ScoreP1.Value;
        if (txtScoreP2 != null) txtScoreP2.text = "P2: " + ScoreP2.Value;
    }

    public void AddScore(ulong shooterId, int amount = 1)
    {
        if (!IsServer) return;

        if (shooterId == NetworkManager.ServerClientId) ScoreP1.Value += amount;
        else ScoreP2.Value += amount;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (ScoreP1.Value >= winScore) EndGameClientRpc("PLAYER 1 CHIẾN THẮNG!");
        else if (ScoreP2.Value >= winScore) EndGameClientRpc("PLAYER 2 CHIẾN THẮNG!");
    }

    [ClientRpc]
    private void EndGameClientRpc(string message)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            txtWinnerText.text = message;
            Time.timeScale = 0;

            // [MỚI] TẮT NHẠC NỀN & PHÁT TIẾNG THẮNG
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlayMusic(null); // Tắt nhạc nền
                AudioManager.Instance.PlaySFX(AudioManager.Instance.winClip);
            }
        }
    }
}