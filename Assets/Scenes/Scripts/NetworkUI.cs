using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [Header("Nút bấm")]
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    [Header("Các Panel giao diện")]
    [SerializeField] private GameObject menuPanel; 
    [SerializeField] private GameObject gameHUD;  

    private void Awake()
    {
        
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            SwitchToGameMode(); 
        });

        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            SwitchToGameMode(); 
        });
    }

    private void Start()
    {
       
        menuPanel.SetActive(true);
        // Thêm câu lệnh kiểm tra: "Nếu gameHUD có tồn tại thì mới tắt"
        if (gameHUD != null)
        {
            gameHUD.SetActive(false);
        }
    }

    private void SwitchToGameMode()
    {
        // Kiểm tra menuPanel trước khi tắt
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // QUAN TRỌNG: Kiểm tra gameHUD trước khi bật
        if (gameHUD != null)
        {
            gameHUD.SetActive(true);
        }
    }
}