using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TankHealth : NetworkBehaviour
{
    [Header("Chỉ số")]
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);

    [Header("Giao diện")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private GameObject healthBarCanvas;

    [Header("Phần hiển thị")]
    [SerializeField] private SpriteRenderer tankSprite;     
    [SerializeField] private SpriteRenderer turretSprite;
    [SerializeField] private Collider2D tankCollider;

    public override void OnNetworkSpawn()
    {
       
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            isDead.Value = false;
        }

       
        currentHealth.OnValueChanged += OnHealthChanged;
        isDead.OnValueChanged += OnDeathStateChanged;

     
        SetHealthBarColor();
        UpdateHealthBar(currentHealth.Value);

        
        if (IsOwner)
        {
            ReturnToSpawnPoint();
        }
    
    }

   
    private void ReturnToSpawnPoint()
    {
        Transform targetSpawn = null;

        if (IsServer)
        {
            // Nếu là Host -> Tìm điểm SpawnPos_Host
            GameObject hostPos = GameObject.Find("SpawnPos_Host");
            if (hostPos != null) targetSpawn = hostPos.transform;
        }
        else
        {
            // Nếu là Client -> Tìm điểm SpawnPos_Client
            GameObject clientPos = GameObject.Find("SpawnPos_Client");
            if (clientPos != null) targetSpawn = clientPos.transform;
        }

        // Dịch chuyển
        if (targetSpawn != null)
        {
            transform.position = targetSpawn.position;
            // Reset luôn góc xoay về 0 cho ngay ngắn
            transform.rotation = Quaternion.identity;
        }
        else
        {
            // Nếu quên tạo điểm thì về 0,0
            transform.position = Vector3.zero;
            Debug.LogWarning("Không tìm thấy điểm Spawn (SpawnPos_Host/SpawnPos_Client)!");
        }
    }

    
    private void SetHealthBarColor()
    {
        if (IsOwner) healthBarFill.color = Color.green;
        else healthBarFill.color = Color.red;
    }

    private void OnHealthChanged(int oldVal, int newVal)
    {
        UpdateHealthBar(newVal);
    }

    private void UpdateHealthBar(int health)
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)health / maxHealth;
    }

    
    private void OnDeathStateChanged(bool oldVal, bool isDeadNow)
    {
        
        if (tankSprite != null) tankSprite.enabled = !isDeadNow;

      
        if (turretSprite != null) turretSprite.enabled = !isDeadNow;

     
        if (tankCollider != null) tankCollider.enabled = !isDeadNow;
        if (healthBarCanvas != null) healthBarCanvas.SetActive(!isDeadNow);

        

        
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = !isDeadNow;
            if (isDeadNow)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

      
        if (!isDeadNow && IsOwner)
        {
            ReturnToSpawnPoint();
        }
    }

   
    public void TakeDamage(int damage)
    {
        if (!IsServer || isDead.Value) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        isDead.Value = true;
        yield return new WaitForSeconds(3f); 
        currentHealth.Value = maxHealth;
        isDead.Value = false;
    }
}