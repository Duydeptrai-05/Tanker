using Unity.Netcode;
using UnityEngine;

public class WallHealth : NetworkBehaviour
{
    [Header("Cài đặt Máu")]
    public int maxHealth = 30;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    [Header("Hình ảnh vỡ (Kéo 7 ảnh vào đây)")]
    public Sprite[] wallStates;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        currentHealth.OnValueChanged += OnHealthChanged;
        UpdateWallSprite(currentHealth.Value);
    }
    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }
    private void OnHealthChanged(int oldVal,int newVal)
    {
        UpdateWallSprite(newVal);
    }
    private void UpdateWallSprite(int health)
    {
        if (wallStates == null || wallStates.Length == 0) return;
        if ( health <= 0) return;

        float healthPercent = (float)health / maxHealth;

        int maxIndex = wallStates.Length -1;
        int spriteIndex = Mathf.RoundToInt((1 - healthPercent) * maxIndex);

        spriteIndex = Mathf.Clamp(spriteIndex, 0, maxIndex);

        if(spriteRenderer != null)
        {
            spriteRenderer.sprite = wallStates[spriteIndex];
        }
    }
    public void TakeDame(int Damage)
    {
        if (!IsServer) return;
        currentHealth.Value -= Damage;

        if (currentHealth.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
