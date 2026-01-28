//using TMPro;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.UI;

//public class MenuController : MonoBehaviour
//{
//    [Header("--- PHẦN NHẬP TÊN (Cũ) ---")]
//    [SerializeField] private TMP_InputField nameInput;
//    [SerializeField] private Button btnHost;
//    [SerializeField] private Button btnClient;

//    [Header("--- PHẦN MỚI: CÁC PANEL ---")]
//    [SerializeField] private GameObject tutorialPanel; // Kéo TutorialPanel vào đây
//    [SerializeField] private GameObject creditsPanel;  // Kéo CreditsPanel vào đây

//    [Header("--- PHẦN MỚI: CÁC NÚT BẤM ---")]
//    [SerializeField] private Button btnOpenTutorial;   // Nút mở hướng dẫn
//    [SerializeField] private Button btnCloseTutorial;  // Nút đóng hướng dẫn

//    [SerializeField] private Button btnOpenCredits;    // Nút mở thông tin
//    [SerializeField] private Button btnCloseCredits;   // Nút đóng thông tin

//    private void Start()
//    {
//        // 1. Setup phần tên (Code cũ)
//        nameInput.text = PlayerPrefs.GetString("PlayerName", "Tanker_" + Random.Range(10, 99));
//        btnHost.onClick.AddListener(StartHostGame);
//        btnClient.onClick.AddListener(StartClientGame);

//        // 2. Setup sự kiện cho HƯỚNG DẪN (Code mới)
//        if (btnOpenTutorial != null && tutorialPanel != null)
//        {
//            btnOpenTutorial.onClick.AddListener(() =>
//            {
//                tutorialPanel.SetActive(true); // Bật lên
//            });
//        }

//        if (btnCloseTutorial != null && tutorialPanel != null)
//        {
//            btnCloseTutorial.onClick.AddListener(() =>
//            {
//                tutorialPanel.SetActive(false); // Tắt đi
//            });
//        }

//        // 3. Setup sự kiện cho THÔNG TIN (Code mới)
//        if (btnOpenCredits != null && creditsPanel != null)
//        {
//            btnOpenCredits.onClick.AddListener(() =>
//            {
//                creditsPanel.SetActive(true);
//            });
//        }

//        if (btnCloseCredits != null && creditsPanel != null)
//        {
//            btnCloseCredits.onClick.AddListener(() =>
//            {
//                creditsPanel.SetActive(false);
//            });
//        }
//    }

//    private void SaveName()
//    {
//        string playerName = nameInput.text;
//        if (string.IsNullOrEmpty(playerName)) playerName = "Unknown Tank";
//        PlayerPrefs.SetString("PlayerName", playerName);
//        PlayerPrefs.Save();
//    }

//    private void StartHostGame()
//    {
//        NetworkManager.Singleton.StartHost();

//        // CHÚ Ý: Đổi tên scene trong ngoặc kép thành "LobbyScene"
//        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
//    }

//    private void StartClientGame()
//    {
//        NetworkManager.Singleton.StartClient();
//    }
//}
//using TMPro;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement; // Thêm thư viện này cho chắc chắn

//public class MenuController : MonoBehaviour
//{
//    [Header("--- PHẦN NHẬP TÊN ---")]
//    [SerializeField] private TMP_InputField nameInput;
//    [SerializeField] private Button btnHost;
//    [SerializeField] private Button btnClient;

//    [Header("--- PHẦN GIAO DIỆN PHỤ ---")]
//    [SerializeField] private GameObject tutorialPanel;
//    [SerializeField] private GameObject creditsPanel;

//    [Header("--- CÁC NÚT BẤM ---")]
//    [SerializeField] private Button btnOpenTutorial;
//    [SerializeField] private Button btnCloseTutorial;
//    [SerializeField] private Button btnOpenCredits;
//    [SerializeField] private Button btnCloseCredits;

//    private void Start()
//    {
//        // 1. Load tên cũ lên ô nhập
//        nameInput.text = PlayerPrefs.GetString("PlayerName", "Tanker_" + Random.Range(10, 99));

//        // 2. Gán sự kiện cho nút Host/Client
//        btnHost.onClick.AddListener(StartHostGame);
//        btnClient.onClick.AddListener(StartClientGame);

//        // 3. Setup HƯỚNG DẪN
//        if (btnOpenTutorial != null && tutorialPanel != null)
//        {
//            btnOpenTutorial.onClick.AddListener(() => tutorialPanel.SetActive(true));
//            btnCloseTutorial.onClick.AddListener(() => tutorialPanel.SetActive(false));
//        }

//        // 4. Setup THÔNG TIN
//        if (btnOpenCredits != null && creditsPanel != null)
//        {
//            btnOpenCredits.onClick.AddListener(() => creditsPanel.SetActive(true));
//            btnCloseCredits.onClick.AddListener(() => creditsPanel.SetActive(false));
//        }
//    }

//    private void Start()
//    {
//        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
//        {
//            NetworkManager.Singleton.Shutdown();
//        }
//        1.Load tên cũ lên ô nhập
//        nameInput.text = PlayerPrefs.GetString("PlayerName", "Tanker_" + Random.Range(10, 99));

//        2.Gán sự kiện cho nút Host / Client
//        btnHost.onClick.AddListener(StartHostGame);
//        btnClient.onClick.AddListener(StartClientGame);

//        3.Setup HƯỚNG DẪN
//        if (btnOpenTutorial != null && tutorialPanel != null)
//        {
//            btnOpenTutorial.onClick.AddListener(() => tutorialPanel.SetActive(true));
//            btnCloseTutorial.onClick.AddListener(() => tutorialPanel.SetActive(false));
//        }

//        4.Setup THÔNG TIN
//        if (btnOpenCredits != null && creditsPanel != null)
//        {
//            btnOpenCredits.onClick.AddListener(() => creditsPanel.SetActive(true));
//            btnCloseCredits.onClick.AddListener(() => creditsPanel.SetActive(false));
//        }
//    }


//    Hàm lưu tên(Đã sửa để được gọi đúng lúc)
//    private void SaveName()
//    {
//        string playerName = nameInput.text;
//        if (string.IsNullOrEmpty(playerName)) playerName = "Unknown Tank"; // Chống tên rỗng

//        PlayerPrefs.SetString("PlayerName", playerName);
//        PlayerPrefs.Save();

//        Debug.Log("Đã lưu tên người chơi: " + playerName);
//    }

//    private void StartHostGame()
//    {
//        1.Kiểm tra an toàn: Nếu đang chạy rồi thì thôi, không làm gì cả
//        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
//        {
//            Debug.LogWarning("Mạng đang chạy rồi, không khởi động lại nữa.");
//            return;
//        }

//        2.Tắt nút đi để tránh người chơi bấm liên tục(Spam)
//        btnHost.interactable = false;

//        3.Lưu tên
//        SaveName();

//        4.Bắt đầu Host
//        Debug.Log("Đang khởi động Host...");
//        if (NetworkManager.Singleton.StartHost())
//        {
//            Debug.Log("Host đã khởi động thành công!");
//            NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
//        }
//        else
//        {
//            Debug.LogError("Không thể khởi động Host!");
//            Nếu lỗi thì bật lại nút cho người ta bấm lại
//            btnHost.interactable = true;
//        }
//    }

//    private void StartClientGame()
//    {
//        1.Lưu tên trước khi đi
//        SaveName();

//        2.Bắt đầu Client
//         QUAN TRỌNG: Chỉ StartClient, KHÔNG được LoadScene thủ công ở đây!
//        Debug.Log("Client đang kết nối...");
//        NetworkManager.Singleton.StartClient();

//    Mẹo: Bạn có thể hiện một dòng chữ "Connecting..." lên màn hình ở đây
//    Nếu Host chưa mở, Client sẽ đứng ở Menu này đợi(là ĐÚNG), không bị màn hình xanh.
//    }
//}

using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("--- NHẬP TÊN NGƯỜI CHƠI ---")]
    [SerializeField] private TMP_InputField nameInput;

    [Header("--- CÁC NÚT CHÍNH ---")]
    [SerializeField] private Button btnHost;   // Nút Tạo Phòng
    [SerializeField] private Button btnClient; // Nút Tham Gia

    [Header("--- GIAO DIỆN PHỤ (Hướng dẫn/Credits) ---")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Button btnOpenTutorial;
    [SerializeField] private Button btnCloseTutorial;
    [SerializeField] private Button btnOpenCredits;
    [SerializeField] private Button btnCloseCredits;

    private void Start()
    {
        // --- 1. TỰ ĐỘNG SỬA LỖI MẠNG (Quan trọng) ---
        // Nếu quay lại Menu mà mạng vẫn đang chạy ngầm -> Tắt ngay lập tức
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
        {
            Debug.Log("Phát hiện mạng cũ chưa tắt -> Đang Reset...");
            NetworkManager.Singleton.Shutdown();
        }
        // --------------------------------------------

        // 2. Load tên cũ
        if (nameInput != null)
        {
            nameInput.text = PlayerPrefs.GetString("PlayerName", "Tanker_" + Random.Range(10, 99));
        }

        // 3. Gán sự kiện cho nút (Reset lại nút để tránh lỗi click đúp)
        SetupButtons();
    }

    private void SetupButtons()
    {
        // Xóa sạch sự kiện cũ trước khi gán mới (để an toàn)
        btnHost.onClick.RemoveAllListeners();
        btnClient.onClick.RemoveAllListeners();

        btnHost.onClick.AddListener(StartHostGame);
        btnClient.onClick.AddListener(StartClientGame);

        // Setup nút Hướng dẫn
        if (btnOpenTutorial && tutorialPanel)
        {
            btnOpenTutorial.onClick.AddListener(() => tutorialPanel.SetActive(true));
            btnCloseTutorial.onClick.AddListener(() => tutorialPanel.SetActive(false));
        }

        // Setup nút Thông tin
        if (btnOpenCredits && creditsPanel)
        {
            btnOpenCredits.onClick.AddListener(() => creditsPanel.SetActive(true));
            btnCloseCredits.onClick.AddListener(() => creditsPanel.SetActive(false));
        }
    }

    private void SaveName()
    {
        if (nameInput == null) return;

        string playerName = nameInput.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Unknown Tank";

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        Debug.Log("Đã lưu tên: " + playerName);
    }

    // --- CHỨC NĂNG HOST (Tạo Phòng) ---
    private void StartHostGame()
    {
        SaveName();

        // Khóa nút để tránh bấm nhiều lần
        btnHost.interactable = false;

        Debug.Log("Đang khởi động Host...");

        // Bắt đầu Host
        bool startSuccess = NetworkManager.Singleton.StartHost();

        if (startSuccess)
        {
            Debug.Log("Host thành công! Đang chuyển sang LobbyScene...");
            // CHUYỂN CẢNH: Chỉ Host mới được dùng lệnh này
            NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Khởi động Host thất bại!");
            btnHost.interactable = true; // Mở lại nút cho bấm lại
        }
    }

    // --- CHỨC NĂNG CLIENT (Tham Gia) ---
    private void StartClientGame()
    {
        SaveName();

        // Khóa nút
        btnClient.interactable = false;

        Debug.Log("Client đang kết nối...");

        // Bắt đầu Client
        bool startSuccess = NetworkManager.Singleton.StartClient();

        if (!startSuccess)
        {
            Debug.LogError("Khởi động Client thất bại!");
            btnClient.interactable = true;
        }

        // LƯU Ý: Client KHÔNG có dòng LoadScene. 
        // Client sẽ đứng yên ở đây chờ Host "kéo" sang Lobby.
    }
}