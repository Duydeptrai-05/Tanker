//using Unity.Netcode;
//using UnityEngine;

//public class RewardChest : NetworkBehaviour
//{
//    [Header("--- CÀI ĐẶT THỜI GIAN ---")]
//    public float openDuration = 1.5f;
//    public float lifeTime = 15f;

//    [Header("--- CÀI ĐẶT VISUAL ---")]
//    [SerializeField] private Animator animator;
//    [SerializeField] private GameObject[] itemPrefabs;
//    [SerializeField] private SpriteRenderer itemVisualRenderer;
//    [SerializeField] private float floatSpeed = 1f;

//    private bool isOpened = false;
//    private TankController openerTank;
//    private Coroutine autoDestroyCoroutine;

//    public override void OnNetworkSpawn()
//    {
//        base.OnNetworkSpawn();
//        if (IsServer && lifeTime > 0)
//        {
//            autoDestroyCoroutine = StartCoroutine(AutoDestroyRoutine());
//        }
//    }

//    IEnumerator AutoDestroyRoutine()
//    {
//        yield return new WaitForSeconds(lifeTime);
//        if (!isOpened)
//        {
//            if (GetComponent<NetworkObject>() != null) GetComponent<NetworkObject>().Despawn();
//            else Destroy(gameObject);
//        }
//    }

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (!IsServer || isOpened) return;

//        if (other.CompareTag("Player"))
//        {
//            openerTank = other.GetComponent<TankController>();
//            if (openerTank != null)
//            {
//                isOpened = true;
//                if (autoDestroyCoroutine != null) StopCoroutine(autoDestroyCoroutine);

//                int randomIndex = 0;
//                if (itemPrefabs != null && itemPrefabs.Length > 0) randomIndex = Random.Range(0, itemPrefabs.Length);

//                OpenChestClientRpc(randomIndex);
//                StartCoroutine(GiveBuffRoutine(randomIndex));
//            }
//        }
//    }

//    IEnumerator GiveBuffRoutine(int index)
//    {
//        yield return new WaitForSeconds(openDuration);
//        if (openerTank != null) openerTank.ApplyBuffFromChest(index);

//        if (GetComponent<NetworkObject>() != null) GetComponent<NetworkObject>().Despawn();
//        else Destroy(gameObject);
//    }

//    [ClientRpc]
//    private void OpenChestClientRpc(int index)
//    {
//        if (animator != null) animator.SetTrigger("Open");

//        // [MỚI] PHÁT TIẾNG MỞ RƯƠNG
//        if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.chestOpenClip);

//        if (itemPrefabs != null && itemPrefabs.Length > index)
//        {
//            var spriteRen = itemPrefabs[index].GetComponent<SpriteRenderer>();
//            if (spriteRen != null && itemVisualRenderer != null)
//            {
//                itemVisualRenderer.sprite = spriteRen.sprite;
//                itemVisualRenderer.gameObject.SetActive(true);
//                StartCoroutine(AnimateItemFloatUp());
//            }
//        }
//    }

//    IEnumerator AnimateItemFloatUp()
//    {
//        float timer = 0;
//        while (timer < openDuration)
//        {
//            timer += Time.deltaTime;
//            if (itemVisualRenderer != null)
//                itemVisualRenderer.transform.localPosition += Vector3.up * floatSpeed * Time.deltaTime;
//            yield return null;
//        }
//    }
//}
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RewardChest : NetworkBehaviour
{
    [Header("--- CÀI ĐẶT THỜI GIAN ---")]
    [Tooltip("Thời gian item bay từ rương vào người chơi")]
    public float flyDuration = 1.0f; // Giảm xuống 1s cho nhanh gọn
    public float lifeTime = 15f;

    [Header("--- CÀI ĐẶT VISUAL ---")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private SpriteRenderer itemVisualRenderer;

    private bool isOpened = false;
    private TankController openerTank;
    private Coroutine autoDestroyCoroutine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer && lifeTime > 0)
        {
            autoDestroyCoroutine = StartCoroutine(AutoDestroyRoutine());
        }
    }

    IEnumerator AutoDestroyRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!isOpened)
        {
            if (GetComponent<NetworkObject>() != null) GetComponent<NetworkObject>().Despawn();
            else Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer || isOpened) return;

        if (other.CompareTag("Player"))
        {
            openerTank = other.GetComponent<TankController>();
            if (openerTank != null)
            {
                isOpened = true;
                if (autoDestroyCoroutine != null) StopCoroutine(autoDestroyCoroutine);

                int randomIndex = 0;
                if (itemPrefabs != null && itemPrefabs.Length > 0) randomIndex = Random.Range(0, itemPrefabs.Length);

                // Gửi thêm ID người mở rương để Client biết item bay vào ai
                OpenChestClientRpc(randomIndex, openerTank.OwnerClientId);

                StartCoroutine(GiveBuffRoutine(randomIndex));
            }
        }
    }

    IEnumerator GiveBuffRoutine(int index)
    {
        // Chờ item bay xong
        yield return new WaitForSeconds(flyDuration);

        // [QUAN TRỌNG] Kiểm tra xem Rương còn tồn tại trên mạng không?
        // Nếu nó đã bị xóa rồi thì DỪNG LẠI NGAY, không làm gì nữa.
        if (!IsSpawned) yield break;

        if (openerTank != null) openerTank.ApplyBuffFromChest(index);

        // Chỉ Despawn nếu nó chưa bị Despawn
        if (IsSpawned && NetworkObject != null)
        {
            NetworkObject.Despawn();
        }
    }

    // --- CLIENT RPC: XỬ LÝ HIỆU ỨNG BAY ---
    [ClientRpc]
    private void OpenChestClientRpc(int index, ulong openerId)
    {
        if (animator != null) animator.SetTrigger("Open");

        // Phát tiếng mở rương
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.chestOpenClip);

        // Tìm xe của người mở rương để item bay vào
        Transform targetTank = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(openerId, out NetworkObject netObj))
        {
            targetTank = netObj.transform;
        }

        if (itemPrefabs != null && itemPrefabs.Length > index)
        {
            var spriteRen = itemPrefabs[index].GetComponent<SpriteRenderer>();
            if (spriteRen != null && itemVisualRenderer != null)
            {
                itemVisualRenderer.sprite = spriteRen.sprite;
                itemVisualRenderer.gameObject.SetActive(true);

                // Bắt đầu bay
                StartCoroutine(AnimateItemFlyToTarget(targetTank));
            }
        }
    }

    // --- LOGIC BAY TỪ RƯƠNG -> XE TĂNG ---
    IEnumerator AnimateItemFlyToTarget(Transform target)
    {
        float timer = 0;
        Vector3 startPos = itemVisualRenderer.transform.position;
        Vector3 initialScale = itemVisualRenderer.transform.localScale;

        while (timer < flyDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flyDuration; // Giá trị từ 0 đến 1

            // Nếu xe còn đó thì bay vào xe, nếu xe chết/mất kết nối thì bay lên trời
            Vector3 endPos = (target != null) ? target.position : (startPos + Vector3.up * 2f);

            // Dùng Lerp để di chuyển mượt mà
            // Công thức Lerp đơn giản: Điểm hiện tại = Điểm đầu + (Điểm cuối - Điểm đầu) * tiến độ
            if (itemVisualRenderer != null)
            {
                itemVisualRenderer.transform.position = Vector3.Lerp(startPos, endPos, t);

                // Hiệu ứng thu nhỏ dần khi bay gần đến nơi (nhìn như bị hút vào)
                itemVisualRenderer.transform.localScale = Vector3.Lerp(initialScale, initialScale * 0.2f, t);
            }

            yield return null;
        }
    }
}