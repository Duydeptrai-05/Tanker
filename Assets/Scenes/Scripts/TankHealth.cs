//using System.Collections;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.UI;

//public class TankHealth : NetworkBehaviour
//{
//    [Header("Chỉ số")]
//    public int maxHealth = 100;
//    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
//    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);

//    [Header("Giao diện")]
//    [SerializeField] private Image healthBarFill;
//    [SerializeField] private GameObject healthBarCanvas;

//    [Header("Phần hiển thị")]
//    [SerializeField] private SpriteRenderer tankSprite;     
//    [SerializeField] private SpriteRenderer turretSprite;
//    [SerializeField] private Collider2D tankCollider;

//    public override void OnNetworkSpawn()
//    {

//        if (IsServer)
//        {
//            currentHealth.Value = maxHealth;
//            isDead.Value = false;
//        }


//        currentHealth.OnValueChanged += OnHealthChanged;
//        isDead.OnValueChanged += OnDeathStateChanged;


//        SetHealthBarColor();
//        UpdateHealthBar(currentHealth.Value);


//        if (IsOwner)
//        {
//            ReturnToSpawnPoint();
//        }

//    }


//    private void ReturnToSpawnPoint()
//    {
//        Transform targetSpawn = null;

//        if (IsServer)
//        {
//            // Nếu là Host -> Tìm điểm SpawnPos_Host
//            GameObject hostPos = GameObject.Find("SpawnPos_Host");
//            if (hostPos != null) targetSpawn = hostPos.transform;
//        }
//        else
//        {
//            // Nếu là Client -> Tìm điểm SpawnPos_Client
//            GameObject clientPos = GameObject.Find("SpawnPos_Client");
//            if (clientPos != null) targetSpawn = clientPos.transform;
//        }

//        // Dịch chuyển
//        if (targetSpawn != null)
//        {
//            transform.position = targetSpawn.position;
//            // Reset luôn góc xoay về 0 cho ngay ngắn
//            transform.rotation = Quaternion.identity;
//        }
//        else
//        {
//            // Nếu quên tạo điểm thì về 0,0
//            transform.position = Vector3.zero;
//            Debug.LogWarning("Không tìm thấy điểm Spawn (SpawnPos_Host/SpawnPos_Client)!");
//        }
//    }


//    private void SetHealthBarColor()
//    {
//        if (IsOwner) healthBarFill.color = Color.blue;
//        else healthBarFill.color = Color.red;
//    }

//    private void OnHealthChanged(int oldVal, int newVal)
//    {
//        UpdateHealthBar(newVal);
//    }

//    private void UpdateHealthBar(int health)
//    {
//        if (healthBarFill != null)
//            healthBarFill.fillAmount = (float)health / maxHealth;
//    }


//    private void OnDeathStateChanged(bool oldVal, bool isDeadNow)
//    {

//        if (tankSprite != null) tankSprite.enabled = !isDeadNow;


//        if (turretSprite != null) turretSprite.enabled = !isDeadNow;


//        if (tankCollider != null) tankCollider.enabled = !isDeadNow;
//        if (healthBarCanvas != null) healthBarCanvas.SetActive(!isDeadNow);




//        var rb = GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.simulated = !isDeadNow;
//            if (isDeadNow)
//            {
//                rb.linearVelocity = Vector2.zero;
//                rb.angularVelocity = 0f;
//            }
//        }


//        if (!isDeadNow && IsOwner)
//        {
//            ReturnToSpawnPoint();
//        }
//    }


//    public void TakeDamage(int damage)
//    {
//        if (!IsServer || isDead.Value) return;

//        currentHealth.Value -= damage;

//        if (currentHealth.Value <= 0)
//        {
//            currentHealth.Value = 0;
//            StartCoroutine(RespawnRoutine());
//        }
//    }

//    IEnumerator RespawnRoutine()
//    {
//        isDead.Value = true;
//        yield return new WaitForSeconds(3f); 
//        currentHealth.Value = maxHealth;
//        isDead.Value = false;
//    }
//}
//using System.Collections;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;

//public class TankHealth : NetworkBehaviour
//{
//    [Header("Chỉ số")]
//    public int maxHealth = 100;
//    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
//    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);

//    // [MỚI THÊM] Biến này để chặn sát thương khi ăn khiên
//    public bool isShielded = false;

//    [Header("Giao diện")]
//    [SerializeField] private Image healthBarFill;
//    [SerializeField] private GameObject healthBarCanvas;

//    [Header("Phần hiển thị")]
//    [SerializeField] private SpriteRenderer tankSprite;
//    [SerializeField] private SpriteRenderer turretSprite;
//    [SerializeField] private Collider2D tankCollider;

//    public override void OnNetworkSpawn()
//    {
//        if (IsServer)
//        {
//            currentHealth.Value = maxHealth;
//            isDead.Value = false;
//        }

//        currentHealth.OnValueChanged += OnHealthChanged;
//        isDead.OnValueChanged += OnDeathStateChanged;

//        SetHealthBarColor();
//        UpdateHealthBar(currentHealth.Value);

//        if (IsOwner)
//        {
//            ReturnToSpawnPoint();
//        }
//    }

//    // [MỚI THÊM] Hàm hồi máu (TankEffectManager sẽ gọi hàm này)
//    public void Heal(int amount)
//    {
//        if (!IsServer) return;
//        currentHealth.Value += amount;
//        if (currentHealth.Value > maxHealth) currentHealth.Value = maxHealth;
//    }

//    public void TakeDamage(int damage)
//    {
//        if (!IsServer || isDead.Value) return;

//        // [MỚI THÊM] Nếu đang có khiên thì không trừ máu
//        if (isShielded) return;

//        currentHealth.Value -= damage;

//        if (currentHealth.Value <= 0)
//        {
//            currentHealth.Value = 0;
//            StartCoroutine(RespawnRoutine());
//        }
//    }

//    private void ReturnToSpawnPoint()
//    {
//        Transform targetSpawn = null;
//        if (IsServer)
//        {
//            GameObject hostPos = GameObject.Find("SpawnPos_Host");
//            if (hostPos != null) targetSpawn = hostPos.transform;
//        }
//        else
//        {
//            GameObject clientPos = GameObject.Find("SpawnPos_Client");
//            if (clientPos != null) targetSpawn = clientPos.transform;
//        }

//        if (targetSpawn != null)
//        {
//            transform.position = targetSpawn.position;
//            transform.rotation = Quaternion.identity;
//        }
//        else
//        {
//            transform.position = Vector3.zero;
//            Debug.LogWarning("Không tìm thấy điểm Spawn!");
//        }
//    }

//    private void SetHealthBarColor()
//    {
//        if (IsOwner) healthBarFill.color = Color.blue;
//        else healthBarFill.color = Color.red;
//    }

//    private void OnHealthChanged(int oldVal, int newVal)
//    {
//        UpdateHealthBar(newVal);
//    }

//    private void UpdateHealthBar(int health)
//    {
//        if (healthBarFill != null)
//            healthBarFill.fillAmount = (float)health / maxHealth;
//    }

//    private void OnDeathStateChanged(bool oldVal, bool isDeadNow)
//    {
//        if (tankSprite != null) tankSprite.enabled = !isDeadNow;
//        if (turretSprite != null) turretSprite.enabled = !isDeadNow;
//        if (tankCollider != null) tankCollider.enabled = !isDeadNow;
//        if (healthBarCanvas != null) healthBarCanvas.SetActive(!isDeadNow);

//        var rb = GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.simulated = !isDeadNow;
//            if (isDeadNow)
//            {
//                rb.linearVelocity = Vector2.zero;
//                rb.angularVelocity = 0f;
//            }
//        }

//        if (!isDeadNow && IsOwner)
//        {
//            ReturnToSpawnPoint();
//        }
//    }

//    IEnumerator RespawnRoutine()
//    {
//        isDead.Value = true;
//        yield return new WaitForSeconds(3f);
//        currentHealth.Value = maxHealth;
//        isDead.Value = false;
//    }
//}
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // [QUAN TRỌNG] Thêm thư viện này

public class TankHealth : NetworkBehaviour
{
    [Header("Chỉ số")]
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);

    // Biến chặn sát thương khi ăn khiên
    public bool isShielded = false;

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

        // --- ĐOẠN SỬA LỖI Ở ĐÂY ---
        // Nếu đang ở LobbyScene thì KHÔNG tìm điểm hồi sinh làm gì cả (tránh lỗi)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "LobbyScene") return;
        // -------------------------

        if (IsOwner)
        {
            ReturnToSpawnPoint();
        }
    }

    public void Heal(int amount)
    {
        if (!IsServer) return;
        currentHealth.Value += amount;
        if (currentHealth.Value > maxHealth) currentHealth.Value = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer || isDead.Value) return;

        if (isShielded) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            StartCoroutine(RespawnRoutine());
        }
    }

    private void ReturnToSpawnPoint()
    {
        // --- SỬA LẠI ĐOẠN NÀY ---
        // Lấy tên Scene hiện tại
        string currentScene = SceneManager.GetActiveScene().name;

        // Nếu KHÔNG PHẢI là "GameScene" (tức là đang ở Menu hoặc Lobby) -> Thì THOÁT NGAY
        if (currentScene != "GameScene") return;
        // -------------------------

        Transform targetSpawn = null;
        if (IsServer)
        {
            GameObject hostPos = GameObject.Find("SpawnPos_Host");
            if (hostPos != null) targetSpawn = hostPos.transform;
        }
        else
        {
            GameObject clientPos = GameObject.Find("SpawnPos_Client");
            if (clientPos != null) targetSpawn = clientPos.transform;
        }

        if (targetSpawn != null)
        {
            transform.position = targetSpawn.position;
            transform.rotation = Quaternion.identity;
        }
        else
        {
            // Chỉ báo lỗi nếu ĐÚNG LÀ GameScene mà vẫn không thấy điểm spawn
            Debug.LogWarning("Đang trong GameScene mà không tìm thấy điểm Spawn! Hãy kiểm tra lại Spawners.");
        }
    }

    private void SetHealthBarColor()
    {
        if (IsOwner) healthBarFill.color = Color.blue;
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

    IEnumerator RespawnRoutine()
    {
        isDead.Value = true;
        yield return new WaitForSeconds(3f);
        currentHealth.Value = maxHealth;
        isDead.Value = false;
    }
}