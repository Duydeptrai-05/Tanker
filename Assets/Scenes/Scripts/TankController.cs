using Unity.Netcode;
using UnityEngine;

public class TankController : NetworkBehaviour
{
    [Header("Thông số")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 150f;

    [Header("Giao diện (Skins)")]
    public Sprite[] tankSkins;      
    public Sprite[] turretSkins;

    [Header("Tham chiếu hiển thị")]
  
    [SerializeField] private SpriteRenderer turretRenderer;

    [Header("Cài đặt bắn")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

   
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private TankHealth tankHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tankHealth = GetComponent<TankHealth>();
    }

    public override void OnNetworkSpawn()
    {
       
        spriteRenderer.color = Color.white;
        if (turretRenderer != null) turretRenderer.color = Color.white; 

       
        ChangeSkinBasedOnID();
    }

    private void ChangeSkinBasedOnID()
    {
       
        int playerID = (int)OwnerClientId;

        
        if (tankSkins != null && tankSkins.Length > 0)
        {
            int bodyIndex = playerID % tankSkins.Length;
            spriteRenderer.sprite = tankSkins[bodyIndex];
        }

        
        if (turretSkins != null && turretSkins.Length > 0 && turretRenderer != null)
        {
            // Dùng chung công thức index để đảm bảo: Thân Xanh đi với Súng Xanh
            int turretIndex = playerID % turretSkins.Length;
            turretRenderer.sprite = turretSkins[turretIndex];
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (tankHealth != null && tankHealth.isDead.Value)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        float move = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Horizontal");

        float rotationAmount = -rotate * turnSpeed * Time.fixedDeltaTime;
        rb.rotation += rotationAmount;

        rb.linearVelocity = (Vector2)transform.up * move * moveSpeed;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (tankHealth != null && tankHealth.isDead.Value) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RequestFireServerRpc();
        }
    }

    [ServerRpc]
    private void RequestFireServerRpc()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        var bulletScript = bullet.GetComponent<BulletController>();
        if (bulletScript != null)
        {
            bulletScript.shooterId = OwnerClientId;
        }
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}