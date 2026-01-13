using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    public float speed = 10f;
    public int damage = 10;   
    public ulong shooterId;
    // -------------------------------------------

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GetComponent<Rigidbody2D>().linearVelocity = transform.up * speed;
            Invoke(nameof(DespawnBullet), 3f);
        }
    }

    private void DespawnBullet()
    {
        if(IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        GameObject hitObject = collision.gameObject;

        WallHealth wall = hitObject.GetComponent<WallHealth>();

        if (wall != null)
        {
            wall.TakeDame(damage);
            DespawnBullet();
            return;
        }
        TankHealth tank = hitObject.GetComponent<TankHealth>();
        if (tank != null)
        {
            tank.TakeDamage(damage);
            DespawnBullet();
            return;
        }
        DespawnBullet();
    }
}