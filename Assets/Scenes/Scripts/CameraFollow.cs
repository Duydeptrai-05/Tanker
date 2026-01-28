//using UnityEngine;

//public class CameraFollow : MonoBehaviour
//{
//    public Transform target; 
//    public float smoothSpeed = 5f; 
//    public Vector3 offset = new Vector3(0, 0, -10);
//    public float zoomSize = 4f;

//    private Camera cam;

//    void Start()
//    {
//        cam = GetComponent<Camera>();
//        // Cài đặt độ zoom ngay khi vào game
//        if (cam != null)
//        {
//            cam.orthographicSize = zoomSize;
//        }
//    }

//    void LateUpdate()
//    {
//        if (target == null) return;
//        Vector3 desiredPosition = target.position + offset;
//        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
//        transform.position = smoothedPosition;
//    }
//}
using UnityEngine;
using Unity.Netcode; // Nhớ thêm dòng này để kiểm tra chủ nhân

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f; // Tốc độ mượt (Đừng để số 5 nhé!)
    public Vector3 offset = new Vector3(0, 0, -10);

    private void LateUpdate()
    {
        // 1. Nếu chưa có mục tiêu -> Tự đi tìm Xe Tăng của mình
        if (target == null)
        {
            FindMyTank();
            return;
        }

        // 2. Nếu đã có mục tiêu -> Bám theo
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    private void FindMyTank()
    {
        // Tìm tất cả xe tăng đang có trong game
        var allTanks = FindObjectsByType<TankController>(FindObjectsSortMode.None);

        foreach (var tank in allTanks)
        {
            // Kiểm tra xem xe nào là của "Tôi" (IsOwner)
            if (tank.IsOwner)
            {
                target = tank.transform;
                Debug.Log("CAMERA: Đã tự tìm thấy xe tăng của chủ nhân: " + tank.name);
                break;
            }
        }
    }

    // Hàm này giữ lại để hỗ trợ nếu cần, nhưng không bắt buộc nữa
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}