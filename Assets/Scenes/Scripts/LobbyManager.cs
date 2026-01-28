using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Cần để chuyển cảnh

public class LobbyManager : NetworkBehaviour
{
    [Header("UI Hiển Thị")]
    [SerializeField] private TextMeshProUGUI p1StatusText; // Kéo Text P1 vào đây
    [SerializeField] private TextMeshProUGUI p2StatusText; // Kéo Text P2 vào đây

    [Header("Nút Bấm")]
    [SerializeField] private Button btnReady;     // Kéo nút Sẵn Sàng vào đây
    [SerializeField] private Button btnStartGame; // Kéo nút Vào Trận vào đây

    // Biến mạng: True là Sẵn sàng, False là Chưa
    private NetworkVariable<bool> isP1Ready = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isP2Ready = new NetworkVariable<bool>(false);

    private void Start()
    {
        // SỬA: Thay vì ẩn đi (SetActive false), ta cứ để nó hiện nhưng làm mờ (interactable false)
        btnStartGame.gameObject.SetActive(true);
        btnStartGame.interactable = false;

        btnReady.onClick.AddListener(OnReadyClicked);
        btnStartGame.onClick.AddListener(OnStartClicked);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Host thì nút sẽ sáng lên (nếu đủ điều kiện)
            btnStartGame.gameObject.SetActive(true);
        }
        else
        {
            // Client thì ẩn nút này đi cho đỡ rối
            btnStartGame.gameObject.SetActive(false);
        }

        // ... (Giữ nguyên các dòng đăng ký biến isP1Ready...)
        isP1Ready.OnValueChanged += (oldVal, newVal) => UpdateUI();
        isP2Ready.OnValueChanged += (oldVal, newVal) => UpdateUI();
        UpdateUI();
    }

    // Khi bấm nút "SẴN SÀNG"
    private void OnReadyClicked()
    {
        if (IsServer) // Nếu là Host
        {
            isP1Ready.Value = !isP1Ready.Value; // Tự đổi trạng thái của mình
        }
        else // Nếu là Khách
        {
            ToggleReadyServerRpc(); // Gửi thư xin Server đổi hộ
        }
    }

    // Server nhận lệnh từ Khách
    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc()
    {
        isP2Ready.Value = !isP2Ready.Value;
    }

    // Cập nhật giao diện (Màu sắc, Chữ)
    private void UpdateUI()
    {
        // Xử lý P1
        if (isP1Ready.Value)
        {
            p1StatusText.text = "PLAYER 1: ĐÃ SẴN SÀNG!";
            p1StatusText.color = Color.green;
        }
        else
        {
            p1StatusText.text = "PLAYER 1: CHƯA SẴN SÀNG";
            p1StatusText.color = Color.red;
        }

        // Xử lý P2
        if (isP2Ready.Value)
        {
            p2StatusText.text = "PLAYER 2: ĐÃ SẴN SÀNG!";
            p2StatusText.color = Color.green;
        }
        else
        {
            p2StatusText.text = "PLAYER 2: CHƯA SẴN SÀNG";
            p2StatusText.color = Color.red;
        }

        // Logic nút Start (Chỉ Host mới check)
        if (IsServer)
        {
            // CÁCH 1: Test một mình (Chỉ cần P1 sẵn sàng là đi luôn)
            btnStartGame.interactable = isP1Ready.Value;

            // CÁCH 2: Khi nào nộp bài hoặc test 2 người thì dùng dòng dưới này (bỏ 2 dấu gạch chéo đi)
            // btnStartGame.interactable = isP1Ready.Value && isP2Ready.Value;
        }
    }

    // Khi Host bấm "VÀO TRẬN"
    private void OnStartClicked()
    {
        // Chuyển tất cả sang Scene số 2 (GameScene)
        // ĐÚNG:
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}