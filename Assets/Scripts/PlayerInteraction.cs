using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public MouseItem mouseItem = new MouseItem();
    public bool toggleUI = false;      // Biến kiểm tra rương đang đóng hay mở
    public GameObject[] InventoryPanel;

    // Smooth panel animation
    public float panelMoveDuration = 0.25f; // thời gian trượt
    public AnimationCurve panelMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private Coroutine[] panelCoroutines;
    private RectTransform[] panelRects;

    public InventoryObject inventory;
    public float interactionDistance = 3f; 
    public LayerMask interactableLayer;   
    public RectTransform Crosshair;

    [Header("Animation Settings")]
    public float animationSpeed = 10f; // Tăng tốc độ này lên để thấy hiệu ứng mượt hơn

    void Start()
    {
        toggleUI = false;
        // Initialize panel rects and coroutines
        if (InventoryPanel != null)
        {
            panelCoroutines = new Coroutine[InventoryPanel.Length];
            panelRects = new RectTransform[InventoryPanel.Length];
            for (int i = 0; i < InventoryPanel.Length; i++)
            {
                if (InventoryPanel[i] != null)
                    panelRects[i] = InventoryPanel[i].GetComponent<RectTransform>();
            }
        }
        // Đặt các inventory panel về trạng thái "đóng" lúc bắt đầu
        SetInventoryPanels(toggleUI);
    }

    // Đặt vị trí các panel inventory theo trạng thái mở/đóng (animate mượt)
    private void SetInventoryPanels(bool open)
    {
        if (InventoryPanel == null || panelRects == null) return;
        for (int i = 0; i < InventoryPanel.Length; i++)
        {
            var rt = panelRects[i];
            if (rt == null) continue;
            float targetX;
            // index 0,1 là bên trái: đóng -> x = -1326, mở -> x = -665
            if (i == 0 || i == 1)
            {
                targetX = open ? -665f : -1326f;
            }
            // index 2,3 là bên phải: đóng -> x = 1746, mở -> x = 277.6147
            else if (i == 2 || i == 3)
            {
                targetX = open ? 277.6147f : 1746f;
            }
            else continue;

            StartPanelMove(i, rt, targetX);
        }
    }

    private void StartPanelMove(int index, RectTransform rt, float targetX)
    {
        if (panelCoroutines == null) return;
        if (panelCoroutines[index] != null) StopCoroutine(panelCoroutines[index]);
        Vector2 targetPos = new Vector2(targetX, rt.anchoredPosition.y);
        panelCoroutines[index] = StartCoroutine(MovePanelCoroutine(index, rt, targetPos, panelMoveDuration));
    }

    private System.Collections.IEnumerator MovePanelCoroutine(int index, RectTransform rt, Vector2 targetPos, float duration)
    {
        float elapsed = 0f;
        Vector2 startPos = rt.anchoredPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = panelMoveCurve != null ? panelMoveCurve.Evaluate(t) : t;
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);
            yield return null;
        }
        rt.anchoredPosition = targetPos;
        if (panelCoroutines != null && index >= 0 && index < panelCoroutines.Length)
            panelCoroutines[index] = null;
    }
    void Update()
    {
        ToggleUI();      // Lắng nghe phím Tab để đóng/mở rương

        if (Input.GetKeyDown(KeyCode.Space))
        {
            inventory.Save();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            inventory.Load();
        }
        // 1. Tạo tia Ray từ tâm màn hình
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Vẽ tia Ray trong cửa sổ Scene để bạn dễ debug (chỉ thấy khi đang chạy game)
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);

        // 2. Kiểm tra va chạm tia Ray
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            var _item = hit.collider.GetComponent<ItemWorld>();

            if (_item != null)
            {
                // Gọi hiệu ứng phóng to
                ScaleCrosshair(1.1f);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    bool added = inventory.AddItem(new Item(_item.item), 1);
                    if (added)
                    {
                        if (hit.collider != null)
                            hit.collider.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("Inventory is full!"); // TODO: show UI feedback
                    }
                }
            }
            else
            {
                // Có chạm nhưng không phải Item
                ScaleCrosshair(0.6f);
            }
        }
        else
        {
            // KHÔNG chạm vào bất cứ thứ gì thuộc interactableLayer
            ScaleCrosshair(0.6f);
        }
    }
    private void ScaleCrosshair(float targetScale)
    {
        Vector3 target = new Vector3(targetScale, targetScale, targetScale);
        // Sử dụng tốc độ cao hơn (ví dụ: 10f) để thấy rõ sự thay đổi
        Crosshair.localScale = Vector3.Lerp(Crosshair.localScale, target, animationSpeed * Time.deltaTime);
    }

    // Hàm xử lý logic đóng/mở giao diện và con trỏ chuột
    public void ToggleUI()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) // Kiểm tra phím Tab
        {
            toggleUI = !toggleUI;
            // Di chuyển các panel theo trạng thái mới
            SetInventoryPanels(toggleUI);
        }

        if (toggleUI)
        {
            // MỞ RƯƠNG: Hiện chuột và cho phép di chuyển chuột tự do trên màn hình
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; 
        }
        else
        {
            // ĐÓNG RƯƠNG: Khóa chuột vào tâm màn hình và ẩn đi (dành cho game góc nhìn thứ nhất/thứ ba)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnApplicationQuit()    // Được gọi tự động khi bạn nhấn Stop (tắt game) hoặc thoát ứng dụng
    {
        inventory.Container.Items = new InventorySlot[50];
    }
}