////using System.Collections.Generic;
////using Unity.Netcode;
////using UnityEngine;
////using TMPro;
////using UnityEngine.UI;

////public class GameUIManager : MonoBehaviour
////{
////    [Header("Giao diện (Kéo thả vào đây)")]
////    public TextMeshProUGUI myScoreText;     
////    public TextMeshProUGUI enemyScoreText; 

////    public Transform myFlagContainer;      
////    public Transform enemyFlagContainer;    

////    [Header("Cài đặt")]
////    public GameObject flagIconPrefab;       
////    public Color myColor = Color.green;     
////    public Color enemyColor = Color.red;  

////    private void Update()
////    {

////        if (GameManager.Instance != null && NetworkManager.Singleton != null)
////        {
////            UpdateScores();
////            UpdateFlagIcons();
////        }
////    }

////    private void UpdateScores()
////    {
////        int hostScore = GameManager.Instance.hostScore.Value;
////        int clientScore = GameManager.Instance.clientScore.Value;



////        if (NetworkManager.Singleton.IsServer)
////        {

////            myScoreText.text = hostScore.ToString();
////            enemyScoreText.text = clientScore.ToString();
////        }
////        else
////        {
////            myScoreText.text = clientScore.ToString();
////            enemyScoreText.text = hostScore.ToString();
////        }


////        myScoreText.color = myColor;
////        enemyScoreText.color = enemyColor;
////    }

////    private void UpdateFlagIcons()
////    {

////        CaptureZone[] zones = FindObjectsByType<CaptureZone>(FindObjectsSortMode.None);

////        int myCount = 0;
////        int enemyCount = 0;
////        ulong myId = NetworkManager.Singleton.LocalClientId;


////        foreach (var zone in zones)
////        {
////            ulong owner = zone.ownerId.Value;

////            if (owner == 999) continue; 
////            if (owner == myId)
////                myCount++;
////            else
////                enemyCount++; 
////        }


////        RenderFlags(myFlagContainer, myCount, myColor);
////        RenderFlags(enemyFlagContainer, enemyCount, enemyColor);
////    }


////    private void RenderFlags(Transform container, int count, Color color)
////    {

////        if (container.childCount == count) return;


////        foreach (Transform child in container)
////        {
////            Destroy(child.gameObject);
////        }


////        for (int i = 0; i < count; i++)
////        {
////            GameObject icon = Instantiate(flagIconPrefab, container);

////            Image img = icon.GetComponent<Image>();
////            if (img != null) img.color = color;
////        }
////    }
////}
//using System.Collections.Generic;
//using Unity.Netcode;
//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class GameUIManager : MonoBehaviour
//{
//    // --- 1. PHẦN QUAN TRỌNG ĐỂ SỬA LỖI (Singleton) ---
//    public static GameUIManager Instance;

//    private void Awake()
//    {
//        // Nếu chưa có Instance thì gán là mình
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    // --- 2. PHẦN THÔNG BÁO (MỚI) ---
//    [Header("--- HỆ THỐNG THÔNG BÁO ---")]
//    [SerializeField] private TextMeshProUGUI notificationText; // Kéo Text Thông báo vào đây

//    public void ShowNotification(string message)
//    {
//        if (notificationText != null)
//        {
//            notificationText.text = message;
//            notificationText.gameObject.SetActive(true);
//            CancelInvoke(nameof(HideNotification));
//            Invoke(nameof(HideNotification), 3f);
//        }
//    }

//    private void HideNotification()
//    {
//        if (notificationText != null) notificationText.gameObject.SetActive(false);
//    }

//    // --- 3. PHẦN ĐIỂM SỐ & CỜ (CODE CŨ CỦA BẠN) ---
//    [Header("--- GIAO DIỆN ĐIỂM SỐ ---")]
//    public TextMeshProUGUI myScoreText;
//    public TextMeshProUGUI enemyScoreText;
//    public Transform myFlagContainer;
//    public Transform enemyFlagContainer;

//    [Header("--- CÀI ĐẶT ---")]
//    public GameObject flagIconPrefab;
//    public Color myColor = Color.green;
//    public Color enemyColor = Color.red;

//    private void Update()
//    {
//        if (GameManager.Instance != null && NetworkManager.Singleton != null)
//        {
//            UpdateScores();
//            UpdateFlagIcons();
//        }
//    }

//    private void UpdateScores()
//    {
//        int hostScore = GameManager.Instance.hostScore.Value;
//        int clientScore = GameManager.Instance.clientScore.Value;

//        if (NetworkManager.Singleton.IsServer)
//        {
//            myScoreText.text = hostScore.ToString();
//            enemyScoreText.text = clientScore.ToString();
//        }
//        else
//        {
//            myScoreText.text = clientScore.ToString();
//            enemyScoreText.text = hostScore.ToString();
//        }

//        myScoreText.color = myColor;
//        enemyScoreText.color = enemyColor;
//    }

//    private void UpdateFlagIcons()
//    {
//        CaptureZone[] zones = FindObjectsByType<CaptureZone>(FindObjectsSortMode.None);
//        int myCount = 0;
//        int enemyCount = 0;
//        ulong myId = NetworkManager.Singleton.LocalClientId;

//        foreach (var zone in zones)
//        {
//            ulong owner = zone.ownerId.Value;
//            if (owner == 999) continue;
//            if (owner == myId) myCount++;
//            else enemyCount++;
//        }

//        RenderFlags(myFlagContainer, myCount, myColor);
//        RenderFlags(enemyFlagContainer, enemyCount, enemyColor);
//    }

//    private void RenderFlags(Transform container, int count, Color color)
//    {
//        if (container.childCount == count) return;
//        foreach (Transform child in container) Destroy(child.gameObject);
//        for (int i = 0; i < count; i++)
//        {
//            GameObject icon = Instantiate(flagIconPrefab, container);
//            Image img = icon.GetComponent<Image>();
//            if (img != null) img.color = color;
//        }
//    }
//}
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    // --- 1. SINGLETON (Giữ nguyên để gọi từ nơi khác) ---
    public static GameUIManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- 2. HỆ THỐNG THÔNG BÁO ---
    [Header("--- HỆ THỐNG THÔNG BÁO ---")]
    [SerializeField] private TextMeshProUGUI notificationText;

    public void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideNotification));
            Invoke(nameof(HideNotification), 3f);
        }
    }

    private void HideNotification()
    {
        if (notificationText != null) notificationText.gameObject.SetActive(false);
    }

    // --- 3. GIAO DIỆN ĐIỂM SỐ (ĐÃ SỬA ĐỔI TÊN BIẾN) ---
    [Header("--- GIAO DIỆN ĐIỂM SỐ ---")]
    // Đổi tên biến cho dễ hiểu: P1 là Host, P2 là Client
    public TextMeshProUGUI scorePlayer1Text; // Kéo Text P1 vào đây
    public TextMeshProUGUI scorePlayer2Text; // Kéo Text P2 vào đây

    [Header("--- CỜ CHIẾM ĐÓNG (TÙY CHỌN) ---")]
    public Transform myFlagContainer;
    public Transform enemyFlagContainer;
    public GameObject flagIconPrefab;
    public Color p1Color = Color.green;
    public Color p2Color = Color.red;

    private void Update()
    {
        // KIỂM TRA AN TOÀN: Nếu chưa có GameManager thì chưa làm gì cả
        if (GameManager.Instance == null) return;

        UpdateScores();

        // Chỉ chạy Update cờ nếu có đủ điều kiện (tránh lỗi)
        if (flagIconPrefab != null)
        {
            UpdateFlagIcons();
        }
    }

    private void UpdateScores()
    {
        // 1. Kiểm tra xem đã kéo thả Text chưa (Tránh lỗi NullReference)
        if (scorePlayer1Text == null || scorePlayer2Text == null) return;

        // 2. Lấy điểm từ GameManager (Đúng tên biến ScoreP1, ScoreP2)
        int p1Score = GameManager.Instance.ScoreP1.Value;
        int p2Score = GameManager.Instance.ScoreP2.Value;

        // 3. Hiển thị lên màn hình
        scorePlayer1Text.text = "P1: " + p1Score.ToString();
        scorePlayer2Text.text = "P2: " + p2Score.ToString();

        // 4. Tô màu (Tùy chọn)
        scorePlayer1Text.color = p1Color;
        scorePlayer2Text.color = p2Color;
    }

    private void UpdateFlagIcons()
    {
        // Tìm tất cả các vùng chiếm đóng
        CaptureZone[] zones = FindObjectsByType<CaptureZone>(FindObjectsSortMode.None);
        if (zones == null || zones.Length == 0) return;

        int p1Flags = 0;
        int p2Flags = 0;

        // ID của Server luôn là 0 (Player 1)
        ulong p1Id = NetworkManager.ServerClientId;

        foreach (var zone in zones)
        {
            ulong owner = zone.ownerId.Value;
            if (owner == 999) continue; // Chưa ai chiếm

            if (owner == p1Id) p1Flags++; // Cờ của P1
            else p2Flags++;               // Cờ của P2 (bất kỳ ai không phải Host)
        }

        // Vẽ cờ (Chỉ vẽ nếu container đã được gán)
        if (myFlagContainer != null) RenderFlags(myFlagContainer, p1Flags, p1Color);
        if (enemyFlagContainer != null) RenderFlags(enemyFlagContainer, p2Flags, p2Color);
    }

    private void RenderFlags(Transform container, int count, Color color)
    {
        if (container.childCount == count) return;

        // Xóa cờ cũ
        foreach (Transform child in container) Destroy(child.gameObject);

        // Vẽ cờ mới
        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(flagIconPrefab, container);
            Image img = icon.GetComponent<Image>();
            if (img != null) img.color = color;
        }
    }
}