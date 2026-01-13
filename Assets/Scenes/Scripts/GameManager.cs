using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.VisualScripting;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Cài đặt Game")]
    public int scoreToWin = 200; 

    [Header("Điểm số (Server quản lý)")]
    public NetworkVariable<int> hostScore = new NetworkVariable<int>(0);
    public NetworkVariable<int> clientScore = new NetworkVariable<int>(0);

    [Header("Giao diện")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winMessage;

    private void Awake()
    {
        if ( Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        hostScore.OnValueChanged += (oldVal, newVal) => UpdateUI();
        clientScore.OnValueChanged += (oldVal, newVal) => UpdateUI();
        UpdateUI();
    }

    private void UpdateUI()
    {
        scoreText.text = $"HOST: {hostScore.Value} | CLIENT: {clientScore.Value}";
        CheckWinner();
    }

    private void CheckWinner()
    {
        if (hostScore.Value >= scoreToWin) EndGame("HOST CHIẾN THẮNG!");
        else if (clientScore.Value >= scoreToWin) EndGame("CLIENT CHIẾN THẮNG!");
    }

    private void EndGame(string message)
    {
        winPanel.SetActive(true);
        winMessage.text = message;
        Time.timeScale = 0; 
    }

    
    public void AddScore(ulong playerId, int amount)
    {
        if (!IsServer) return; 

        if (playerId == NetworkManager.ServerClientId)
            hostScore.Value += amount;
        else
            clientScore.Value += amount;
    }
}
