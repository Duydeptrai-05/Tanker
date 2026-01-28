using UnityEngine;
using Unity.Netcode;

public class WallHealth : NetworkBehaviour
{
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(30);

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            // Nếu tường là NetworkObject thì dùng Despawn
            if (IsSpawned)
            {
                GetComponent<NetworkObject>().Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}