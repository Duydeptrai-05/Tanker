using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("--- CÀI ĐẶT ---")]
    [SerializeField] private RectTransform background; // Vòng tròn nền
    [SerializeField] private RectTransform handle;     // Cái núm xoay ở giữa
    [SerializeField] private float handleRange = 1f;   // Phạm vi di chuyển của núm

    // Giá trị trả về cho TankController (-1 đến 1)
    public Vector2 InputDirection { get; private set; } = Vector2.zero;

    private Vector2 startPos;

    private void Start()
    {
        // Lưu vị trí ban đầu (nếu cần dùng sau này)
        if (background != null)
        {
            startPos = background.position;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData); // Khi chạm vào là tính như đang kéo luôn
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        Vector2 position = Vector2.zero;

        // Tính toán vị trí ngón tay so với tâm Joystick
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            // Chuẩn hóa vị trí (về khoảng -1 đến 1)
            // Dùng t.x và t.y thay vì chia trực tiếp để code gọn hơn
            position.x /= background.sizeDelta.x;
            position.y /= background.sizeDelta.y;

            // Tính toán hướng (Input)
            float x = position.x * 2;
            float y = position.y * 2;

            InputDirection = new Vector2(x, y);

            // Giới hạn độ dài vector không quá 1 (để đi chéo không bị nhanh hơn)
            if (InputDirection.magnitude > 1)
            {
                InputDirection = InputDirection.normalized;
            }

            // Di chuyển cái núm (Handle) theo ngón tay
            handle.anchoredPosition = new Vector2(
                InputDirection.x * (background.sizeDelta.x / 2) * handleRange,
                InputDirection.y * (background.sizeDelta.y / 2) * handleRange
            );
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Khi thả tay ra -> Reset về 0
        InputDirection = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }
}