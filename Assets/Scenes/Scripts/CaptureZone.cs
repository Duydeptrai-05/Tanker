using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CaptureZone : NetworkBehaviour
{
    [Header("Cài đặt")]
    public float timeToCapture = 3f;
    public float scoreInterval = 1f;

    [Header("Màu sắc")]
    public Color myColor = Color.blue; 
    public Color enemyColor = Color.red;  
    public Color neutralColor = Color.white;

   
    public NetworkVariable<ulong> ownerId = new NetworkVariable<ulong>(999);

    private SpriteRenderer spriteRen;
    private float scoreTimer = 0;
    private float captureTimer = 0;

   
    private List<ulong> playersInZone = new List<ulong>();

    private void Awake()
    {
        spriteRen = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
       
        ownerId.OnValueChanged += (oldId, newId) => UpdateColor(newId);
        
        UpdateColor(ownerId.Value);
    }

    private void Update()
    {
        if (IsServer)
        {
            ServerHandleCapture();
            ServerHandleScore();
        }
    }

    private void ServerHandleCapture()
    {
       
        for (int i = playersInZone.Count - 1; i >= 0; i--)
        {
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(playersInZone[i]) ||
                NetworkManager.Singleton.ConnectedClients[playersInZone[i]].PlayerObject == null ||
                !NetworkManager.Singleton.ConnectedClients[playersInZone[i]].PlayerObject.gameObject.activeInHierarchy)
            {
                playersInZone.RemoveAt(i);
            }
        }

       
        if (playersInZone.Count > 0)
        {
     
            ulong occupierId = playersInZone[playersInZone.Count - 1];

            
            if (occupierId != ownerId.Value)
            {
                captureTimer += Time.deltaTime;
                if (captureTimer >= timeToCapture)
                {
                    ownerId.Value = occupierId; 
                    captureTimer = 0;
                    Debug.Log($"Người chơi {occupierId} đã chiếm cứ điểm!");
                }
            }
        }
        else
        {
      
            captureTimer = 0;
        }
    }

    private void ServerHandleScore()
    {
        if (ownerId.Value != 999)
        {
            scoreTimer += Time.deltaTime;
            if (scoreTimer >= scoreInterval)
            {
                GameManager.Instance.AddScore(ownerId.Value, 1);
                scoreTimer = 0;
            }
        }
    }

   
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            ulong id = other.GetComponent<NetworkObject>().OwnerClientId;
            if (!playersInZone.Contains(id))
            {
                playersInZone.Add(id);
                captureTimer = 0;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            ulong id = other.GetComponent<NetworkObject>().OwnerClientId;
            if (playersInZone.Contains(id))
            {
                playersInZone.Remove(id);
                captureTimer = 0;
            }
        }
    }

    private void UpdateColor(ulong owner)
    {
        

        if (owner == 999)
        {
            spriteRen.color = neutralColor;
        }
        else if (owner == NetworkManager.Singleton.LocalClientId)
        {
            spriteRen.color = myColor;
        }
        else
        {
            spriteRen.color = enemyColor; 
        }
    }
}