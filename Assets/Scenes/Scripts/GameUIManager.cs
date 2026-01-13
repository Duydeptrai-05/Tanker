using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Giao diện (Kéo thả vào đây)")]
    public TextMeshProUGUI myScoreText;     
    public TextMeshProUGUI enemyScoreText; 

    public Transform myFlagContainer;      
    public Transform enemyFlagContainer;    

    [Header("Cài đặt")]
    public GameObject flagIconPrefab;       
    public Color myColor = Color.green;     
    public Color enemyColor = Color.red;  

    private void Update()
    {
        
        if (GameManager.Instance != null && NetworkManager.Singleton != null)
        {
            UpdateScores();
            UpdateFlagIcons();
        }
    }

    private void UpdateScores()
    {
        int hostScore = GameManager.Instance.hostScore.Value;
        int clientScore = GameManager.Instance.clientScore.Value;

       

        if (NetworkManager.Singleton.IsServer)
        {
          
            myScoreText.text = hostScore.ToString();
            enemyScoreText.text = clientScore.ToString();
        }
        else
        {
            myScoreText.text = clientScore.ToString();
            enemyScoreText.text = hostScore.ToString();
        }

       
        myScoreText.color = myColor;
        enemyScoreText.color = enemyColor;
    }

    private void UpdateFlagIcons()
    {
       
        CaptureZone[] zones = FindObjectsByType<CaptureZone>(FindObjectsSortMode.None);

        int myCount = 0;
        int enemyCount = 0;
        ulong myId = NetworkManager.Singleton.LocalClientId;

        
        foreach (var zone in zones)
        {
            ulong owner = zone.ownerId.Value;

            if (owner == 999) continue; 
            if (owner == myId)
                myCount++;
            else
                enemyCount++; 
        }

       
        RenderFlags(myFlagContainer, myCount, myColor);
        RenderFlags(enemyFlagContainer, enemyCount, enemyColor);
    }

 
    private void RenderFlags(Transform container, int count, Color color)
    {
  
        if (container.childCount == count) return;

        
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

    
        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(flagIconPrefab, container);
          
            Image img = icon.GetComponent<Image>();
            if (img != null) img.color = color;
        }
    }
}