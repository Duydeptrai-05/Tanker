//using UnityEngine;
//using Unity.Netcode;
//using UnityEngine.SceneManagement;
//using System.Collections;
//using UnityEngine.EventSystems; // Thư viện để xử lý cảm ứng Mobile

//public class TankController : NetworkBehaviour
//{
//    [Header("--- CÀI ĐẶT DI CHUYỂN ---")]
//    [SerializeField] private float moveSpeed = 5f;
//    [SerializeField] private float turnSpeed = 150f;

//    [Header("--- HỆ THỐNG BẮN SÚNG ---")]
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private Transform firePoint;
//    [Tooltip("Tốc độ đạn bay")]
//    [SerializeField] private float bulletSpeed = 6f;
//    [Tooltip("Thời gian hồi chiêu (Giây)")]
//    [SerializeField] private float fireCooldown = 0.8f;

//    private float nextFireTime = 0f;

//    [Header("--- GIAO DIỆN (SKINS) ---")]
//    public Sprite[] tankSkins;
//    public Sprite[] turretSkins;
//    [SerializeField] private SpriteRenderer turretRenderer;

//    [Header("--- HIỆU ỨNG BUFF (VISUAL) ---")]
//    [SerializeField] private GameObject shieldVisual; // Vòng Khiên
//    [SerializeField] private GameObject bombVisual;   // Quả Bom

//    [Header("--- HIỆU ỨNG & ANIMATION ---")]
//    [SerializeField] private ParticleSystem dustTrail;
//    [SerializeField] private Animator trackAnimator;

//    // --- BIẾN TRẠNG THÁI (Server quản lý) ---
//    [HideInInspector] public float speedMultiplier = 1f;
//    [HideInInspector] public bool isDrunk = false;
//    [HideInInspector] public bool isShielded = false;

//    // --- BIẾN ĐIỀU KHIỂN MOBILE ---
//    private VirtualJoystick joystickMobile;
//    private bool isMobileFireHeld = false;

//    // --- BIẾN NỘI BỘ ---
//    private Rigidbody2D rb;
//    private SpriteRenderer spriteRenderer;
//    private TankHealth tankHealth;
//    private bool isHiddenInLobby = false;

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        spriteRenderer = GetComponent<SpriteRenderer>();
//        tankHealth = GetComponent<TankHealth>();

//        // Tắt visual mặc định
//        if (shieldVisual) shieldVisual.SetActive(false);
//        if (bombVisual) bombVisual.SetActive(false);
//    }

//    public override void OnNetworkSpawn()
//    {
//        base.OnNetworkSpawn();

//        // Đăng ký sự kiện chuyển cảnh
//        SceneManager.sceneLoaded += OnSceneLoaded;

//        // Nếu sinh ra đã ở trong GameScene thì chạy setup luôn
//        if (SceneManager.GetActiveScene().name == "GameScene")
//        {
//            FindAndMoveToSpawn();
//            if (IsOwner) FindMobileControls();
//        }

//        ChangeSkinBasedOnID();
//        if (IsOwner) ConnectToCamera();
//    }

//    public override void OnNetworkDespawn()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//        base.OnNetworkDespawn();
//    }

//    // --- TỰ ĐỘNG CHẠY KHI VÀO GAME SCENE ---
//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        if (scene.name == "GameScene")
//        {
//            FindAndMoveToSpawn();
//            if (IsOwner)
//            {
//                ConnectToCamera();
//                FindMobileControls(); // Tìm Joystick & Nút Bắn
//            }
//        }
//    }

//    // --- TÌM NÚT ĐIỀU KHIỂN MOBILE ---
//    private void FindMobileControls()
//    {
//        // 1. Tìm Joystick
//        joystickMobile = FindFirstObjectByType<VirtualJoystick>();

//        // 2. Tìm Nút Bắn (Tên phải đúng là Btn_Fire)
//        GameObject btnFire = GameObject.Find("Btn_Fire");
//        if (btnFire != null)
//        {
//            EventTrigger trigger = btnFire.GetComponent<EventTrigger>();
//            if (trigger == null) trigger = btnFire.AddComponent<EventTrigger>();
//            trigger.triggers.Clear();

//            // Sự kiện ấn xuống -> Bắn
//            EventTrigger.Entry entryDown = new EventTrigger.Entry();
//            entryDown.eventID = EventTriggerType.PointerDown;
//            entryDown.callback.AddListener((data) => { isMobileFireHeld = true; });
//            trigger.triggers.Add(entryDown);

//            // Sự kiện thả ra -> Dừng
//            EventTrigger.Entry entryUp = new EventTrigger.Entry();
//            entryUp.eventID = EventTriggerType.PointerUp;
//            entryUp.callback.AddListener((data) => { isMobileFireHeld = false; });
//            trigger.triggers.Add(entryUp);
//        }
//    }

//    // --- UPDATE: XỬ LÝ BẮN SÚNG ---
//    private void Update()
//    {
//        // Setup Camera nếu bị mất
//        if (IsOwner && SceneManager.GetActiveScene().name == "GameScene")
//        {
//            var cam = Camera.main;
//            if (cam != null)
//            {
//                var camFollow = cam.GetComponent<CameraFollow>();
//                if (camFollow != null && camFollow.target == null) camFollow.target = transform;
//            }
//        }

//        CheckVisibilityLobby();

//        if (SceneManager.GetActiveScene().name == "LobbyScene") return;
//        if (!IsOwner) return;
//        if (tankHealth != null && tankHealth.isDead.Value) return;

//        // --- LOGIC BẮN (PC SPACE hoặc MOBILE BUTTON) ---
//        if (Input.GetKey(KeyCode.Space) || isMobileFireHeld)
//        {
//            if (Time.time >= nextFireTime)
//            {
//                nextFireTime = Time.time + fireCooldown;

//                // 1. Phát tiếng ngay lập tức (Chống delay)
//                if (AudioManager.Instance != null)
//                    AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip);

//                // 2. Gửi lệnh lên Server
//                RequestFireServerRpc();
//            }
//        }
//    }

//    // --- FIXED UPDATE: XỬ LÝ DI CHUYỂN ---
//    private void FixedUpdate()
//    {
//        if (SceneManager.GetActiveScene().name == "LobbyScene") { rb.linearVelocity = Vector2.zero; return; }
//        if (!IsOwner) return;
//        if (tankHealth != null && tankHealth.isDead.Value)
//        {
//            rb.linearVelocity = Vector2.zero;
//            rb.angularVelocity = 0f;
//            HandleEffects(0, 0);
//            return;
//        }

//        // --- DI CHUYỂN (PC + MOBILE) ---
//        float move = Input.GetAxis("Vertical");      // PC W/S
//        float rotate = Input.GetAxis("Horizontal");  // PC A/D

//        // Cộng dồn tín hiệu Joystick (nếu có)
//        if (joystickMobile != null)
//        {
//            move += joystickMobile.InputDirection.y;
//            rotate += joystickMobile.InputDirection.x;
//        }

//        // Logic Say Rượu
//        if (isDrunk) { move = -move; rotate = -rotate; }

//        // Áp dụng lực vật lý
//        float rotationAmount = -rotate * turnSpeed * Time.fixedDeltaTime;
//        rb.rotation += rotationAmount;
//        rb.linearVelocity = (Vector2)transform.up * move * moveSpeed * speedMultiplier;

//        HandleEffects(move, rotate);
//    }

//    // --- SERVER RPC: BẮN SÚNG ---
//    [ServerRpc]
//    private void RequestFireServerRpc()
//    {
//        if (bulletPrefab == null || firePoint == null) return;

//        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
//        var bulletScript = bullet.GetComponent<BulletController>();
//        if (bulletScript != null) bulletScript.shooterId = OwnerClientId;

//        bullet.GetComponent<NetworkObject>().Spawn();

//        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
//        if (bulletRb != null) bulletRb.linearVelocity = firePoint.right * bulletSpeed;

//        // Gọi ClientRpc để phát tiếng cho CÁC MÁY KHÁC nghe
//        PlayShootSoundClientRpc();
//    }

//    // --- HỆ THỐNG BUFF ---
//    public void ApplyBuffFromChest(int itemID)
//    {
//        if (!IsServer) return;
//        switch (itemID)
//        {
//            case 0: // KHIÊN
//                isShielded = true;
//                SetShieldVisualClientRpc(true);
//                StartCoroutine(RemoveBuffAfterTime("Shield", 10f));
//                break;

//            case 1: // TÀNG HÌNH
//                SetInvisibleClientRpc(true);
//                StartCoroutine(RemoveBuffAfterTime("Invis", 5f));
//                break;

//            case 2: // HỒI MÁU
//                if (tankHealth != null) tankHealth.Heal(30);
//                break;

//            case 3: // TĂNG TỐC (ĐÃ SỬA: Thêm hiệu ứng khói đỏ)
//                speedMultiplier = 2f;
//                SetSpeedVisualClientRpc(true); // Bật hiệu ứng khói đỏ
//                StartCoroutine(RemoveBuffAfterTime("Speed", 5f));
//                break;

//            case 4: // LÀM CHẬM
//                speedMultiplier = 0.5f;
//                StartCoroutine(RemoveBuffAfterTime("Speed", 5f));
//                break;

//            case 5: // SAY RƯỢU
//                isDrunk = true;
//                StartCoroutine(RemoveBuffAfterTime("Drunk", 5f));
//                break;

//            case 6: // BOM
//                SetBombVisualClientRpc(true);
//                StartCoroutine(ExplodeBomb(3f));
//                break;
//        }
//    }

//    private IEnumerator RemoveBuffAfterTime(string buffType, float duration)
//    {
//        yield return new WaitForSeconds(duration);
//        switch (buffType)
//        {
//            case "Shield":
//                isShielded = false;
//                SetShieldVisualClientRpc(false);
//                break;
//            case "Invis":
//                SetInvisibleClientRpc(false);
//                break;
//            case "Speed":
//                speedMultiplier = 1f;
//                SetSpeedVisualClientRpc(false); // Tắt hiệu ứng khói đỏ
//                break;
//            case "Drunk":
//                isDrunk = false;
//                break;
//        }
//    }

//    private IEnumerator ExplodeBomb(float time)
//    {
//        yield return new WaitForSeconds(time);
//        if (tankHealth != null) tankHealth.TakeDamage(30);
//        SetBombVisualClientRpc(false);
//        PlayExplosionSoundClientRpc();
//    }


//    // --- KHU VỰC CLIENT RPC (XỬ LÝ HÌNH ẢNH & UI) ---

//    // [ĐÃ SỬA] Tàng hình: Ẩn luôn cả thanh máu
//    [ClientRpc]
//    private void SetInvisibleClientRpc(bool isInvisible)
//    {
//        // 1. XỬ LÝ ẢNH XE
//        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
//        foreach (var sprite in allSprites)
//        {
//            Color c = sprite.color;
//            // Địch không thấy gì, Mình thấy mờ 0.5
//            c.a = (isInvisible && !IsOwner) ? 0f : (isInvisible ? 0.5f : 1f);
//            sprite.color = c;
//        }

//        // 2. XỬ LÝ THANH MÁU (Canvas)
//        Canvas[] allCanvas = GetComponentsInChildren<Canvas>();
//        foreach (var canvas in allCanvas)
//        {
//            if (isInvisible)
//            {
//                // Nếu mình là chủ -> Vẫn hiện Canvas để xem máu
//                // Nếu là địch -> Tắt Canvas đi
//                if (IsOwner) canvas.gameObject.SetActive(true);
//                else canvas.gameObject.SetActive(false);
//            }
//            else
//            {
//                // Hết tàng hình -> Hiện lại
//                canvas.gameObject.SetActive(true);
//            }
//        }

//        // 3. Âm thanh
//        if (isInvisible && AudioManager.Instance)
//            AudioManager.Instance.PlaySFX(AudioManager.Instance.buffPickupClip);
//    }

//    // [MỚI] Hiệu ứng Tăng tốc (Khói đỏ)
//    [ClientRpc]
//    private void SetSpeedVisualClientRpc(bool isActive)
//    {
//        if (dustTrail != null)
//        {
//            var main = dustTrail.main;
//            var emission = dustTrail.emission;

//            if (isActive)
//            {
//                // Bật chế độ Tăng tốc: Khói đỏ, phun nhiều
//                main.startColor = Color.red;
//                emission.rateOverTime = 20f;
//                // Tiếng Buff
//                if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.buffPickupClip);
//            }
//            else
//            {
//                // Trở về bình thường
//                main.startColor = Color.white;
//                emission.rateOverTime = 10f;
//            }
//        }
//    }

//    [ClientRpc]
//    private void SetShieldVisualClientRpc(bool isActive)
//    {
//        if (shieldVisual != null) shieldVisual.SetActive(isActive);
//        if (isActive && AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.buffPickupClip);
//    }

//    [ClientRpc]
//    private void SetBombVisualClientRpc(bool isActive)
//    {
//        if (bombVisual != null) bombVisual.SetActive(isActive);
//        if (isActive && AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.buffPickupClip);
//    }

//    [ClientRpc]
//    private void PlayShootSoundClientRpc()
//    {
//        // Chỉ phát nếu KHÔNG PHẢI là người bắn
//        if (!IsOwner && AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip);
//    }

//    [ClientRpc]
//    private void PlayExplosionSoundClientRpc()
//    {
//        if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionClip);
//    }

//    // --- CÁC HÀM PHỤ TRỢ ---
//    private void FindAndMoveToSpawn()
//    {
//        if (!IsOwner) return;
//        if (IsHost)
//        {
//            GameObject hostPos = GameObject.Find("SpawnPos_Host");
//            if (hostPos != null) { transform.position = hostPos.transform.position; if (rb) rb.linearVelocity = Vector2.zero; }
//        }
//        else
//        {
//            GameObject clientPos = GameObject.Find("SpawnPos_Client");
//            if (clientPos != null) { transform.position = clientPos.transform.position; if (rb) rb.linearVelocity = Vector2.zero; }
//        }
//    }

//    private void CheckVisibilityLobby()
//    {
//        string sceneName = SceneManager.GetActiveScene().name;
//        if (sceneName == "LobbyScene")
//        {
//            if (!isHiddenInLobby) { SetTankActive(false); isHiddenInLobby = true; }
//        }
//        else
//        {
//            if (isHiddenInLobby) { SetTankActive(true); isHiddenInLobby = false; }
//        }
//    }

//    private void SetTankActive(bool isActive)
//    {
//        var allSprites = GetComponentsInChildren<SpriteRenderer>(true);
//        foreach (var sprite in allSprites) sprite.enabled = isActive;
//        var allColliders = GetComponentsInChildren<Collider2D>(true);
//        foreach (var col in allColliders) col.enabled = isActive;
//        var allCanvas = GetComponentsInChildren<Canvas>(true);
//        foreach (var canvas in allCanvas) canvas.gameObject.SetActive(isActive);
//    }

//    private void ConnectToCamera()
//    {
//        var camObj = Camera.main;
//        if (camObj != null)
//        {
//            var camScript = camObj.GetComponent<CameraFollow>();
//            if (camScript != null) camScript.SetTarget(transform);
//        }
//    }

//    private void ChangeSkinBasedOnID()
//    {
//        int playerID = (int)OwnerClientId;
//        if (tankSkins != null && tankSkins.Length > 0) spriteRenderer.sprite = tankSkins[playerID % tankSkins.Length];
//        if (turretSkins != null && turretSkins.Length > 0 && turretRenderer != null) turretRenderer.sprite = turretSkins[playerID % turretSkins.Length];
//    }

//    private void HandleEffects(float move, float rotate)
//    {
//        bool isMoving = Mathf.Abs(move) > 0.1f || Mathf.Abs(rotate) > 0.1f;
//        if (dustTrail != null)
//        {
//            if (isMoving) { if (!dustTrail.isPlaying) dustTrail.Play(); }
//            else { if (dustTrail.isPlaying) dustTrail.Stop(); }
//        }
//        if (trackAnimator != null) trackAnimator.SetBool("IsMoving", isMoving);
//    }
//}
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems; // Thư viện để xử lý cảm ứng Mobile

public class TankController : NetworkBehaviour
{
    [Header("--- CÀI ĐẶT DI CHUYỂN ---")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 150f;

    [Header("--- HỆ THỐNG BẮN SÚNG ---")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [Tooltip("Tốc độ đạn bay")]
    [SerializeField] private float bulletSpeed = 6f;
    [Tooltip("Thời gian hồi chiêu (Giây)")]
    [SerializeField] private float fireCooldown = 0.8f;

    private float nextFireTime = 0f;

    [Header("--- GIAO DIỆN (SKINS) ---")]
    public Sprite[] tankSkins;
    public Sprite[] turretSkins;
    [SerializeField] private SpriteRenderer turretRenderer;

    [Header("--- HIỆU ỨNG BUFF (VISUAL) ---")]
    [SerializeField] private GameObject shieldVisual; // Vòng Khiên
    [SerializeField] private GameObject bombVisual;   // Quả Bom

    [Header("--- HIỆU ỨNG & ANIMATION ---")]
    [SerializeField] private ParticleSystem dustTrail;
    [SerializeField] private Animator trackAnimator;

    // --- BIẾN TRẠNG THÁI (Server quản lý) ---
    [HideInInspector] public float speedMultiplier = 1f;
    [HideInInspector] public bool isDrunk = false;
    [HideInInspector] public bool isShielded = false;

    // --- BIẾN ĐIỀU KHIỂN MOBILE ---
    private VirtualJoystick joystickMobile;
    private bool isMobileFireHeld = false;

    // --- BIẾN NỘI BỘ ---
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private TankHealth tankHealth;
    private bool isHiddenInLobby = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tankHealth = GetComponent<TankHealth>();

        // Tắt visual mặc định
        if (shieldVisual) shieldVisual.SetActive(false);
        if (bombVisual) bombVisual.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Đăng ký sự kiện chuyển cảnh
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Nếu sinh ra đã ở trong GameScene thì chạy setup luôn
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            FindAndMoveToSpawn();
            if (IsOwner) FindMobileControls();
        }

        ChangeSkinBasedOnID();
        if (IsOwner) ConnectToCamera();
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnNetworkDespawn();
    }

    // --- TỰ ĐỘNG CHẠY KHI VÀO GAME SCENE ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            FindAndMoveToSpawn();
            if (IsOwner)
            {
                ConnectToCamera();
                FindMobileControls(); // Tìm Joystick & Nút Bắn
            }
        }
    }

    // --- TÌM NÚT ĐIỀU KHIỂN MOBILE ---
    private void FindMobileControls()
    {
        // 1. Tìm Joystick
        joystickMobile = FindFirstObjectByType<VirtualJoystick>();

        // 2. Tìm Nút Bắn (Tên phải đúng là Btn_Fire)
        GameObject btnFire = GameObject.Find("Btn_Fire");
        if (btnFire != null)
        {
            EventTrigger trigger = btnFire.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btnFire.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            // Sự kiện ấn xuống -> Bắn
            EventTrigger.Entry entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener((data) => { isMobileFireHeld = true; });
            trigger.triggers.Add(entryDown);

            // Sự kiện thả ra -> Dừng
            EventTrigger.Entry entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener((data) => { isMobileFireHeld = false; });
            trigger.triggers.Add(entryUp);
        }
    }

    // --- UPDATE: XỬ LÝ BẮN SÚNG ---
    private void Update()
    {
        // Setup Camera nếu bị mất
        if (IsOwner && SceneManager.GetActiveScene().name == "GameScene")
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var camFollow = cam.GetComponent<CameraFollow>();
                if (camFollow != null && camFollow.target == null) camFollow.target = transform;
            }
        }

        CheckVisibilityLobby();

        if (SceneManager.GetActiveScene().name == "LobbyScene") return;
        if (!IsOwner) return;
        if (tankHealth != null && tankHealth.isDead.Value) return;

        // --- LOGIC BẮN (PC SPACE hoặc MOBILE BUTTON) ---
        if (Input.GetKey(KeyCode.Space) || isMobileFireHeld)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireCooldown;

                // 1. Phát tiếng ngay lập tức (Chống delay)
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip);

                // 2. Gửi lệnh lên Server
                RequestFireServerRpc();
            }
        }
    }

    // --- FIXED UPDATE: XỬ LÝ DI CHUYỂN ---
    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "LobbyScene") { rb.linearVelocity = Vector2.zero; return; }
        if (!IsOwner) return;
        if (tankHealth != null && tankHealth.isDead.Value)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            HandleEffects(0, 0);
            return;
        }

        // --- DI CHUYỂN (PC + MOBILE) ---
        float move = Input.GetAxis("Vertical");      // PC W/S
        float rotate = Input.GetAxis("Horizontal");  // PC A/D

        // Cộng dồn tín hiệu Joystick (nếu có)
        if (joystickMobile != null)
        {
            move += joystickMobile.InputDirection.y;
            rotate += joystickMobile.InputDirection.x;
        }

        // Logic Say Rượu
        if (isDrunk) { move = -move; rotate = -rotate; }

        // Áp dụng lực vật lý
        float rotationAmount = -rotate * turnSpeed * Time.fixedDeltaTime;
        rb.rotation += rotationAmount;
        rb.linearVelocity = (Vector2)transform.up * move * moveSpeed * speedMultiplier;

        HandleEffects(move, rotate);
    }

    // --- SERVER RPC: BẮN SÚNG ---
    [ServerRpc]
    private void RequestFireServerRpc()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        var bulletScript = bullet.GetComponent<BulletController>();
        if (bulletScript != null) bulletScript.shooterId = OwnerClientId;

        bullet.GetComponent<NetworkObject>().Spawn();

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null) bulletRb.linearVelocity = firePoint.right * bulletSpeed;

        // Gọi ClientRpc để phát tiếng cho CÁC MÁY KHÁC nghe
        PlayShootSoundClientRpc();
    }

    // --- HỆ THỐNG BUFF (ĐÃ SỬA LẠI LOGIC SERVER/CLIENT) ---

    // Hàm này được gọi từ Rương (trên Server)
    public void ApplyBuffFromChest(int itemID)
    {
        if (!IsServer) return;

        // 1. Nếu là Máu -> Server xử lý luôn (Vì máu là biến NetworkVariable)
        if (itemID == 2)
        {
            if (tankHealth != null) tankHealth.Heal(30);
            return;
        }

        // 2. Nếu là các Buff khác (Tốc độ, Hình ảnh...) -> Đẩy về Client tự xử lý cho mượt
        ApplyBuffEffectClientRpc(itemID);
    }

    // Hàm này chạy trên TẤT CẢ CLIENT (Bao gồm cả người chơi)
    [ClientRpc]
    private void ApplyBuffEffectClientRpc(int itemID)
    {
        switch (itemID)
        {
            case 0: // KHIÊN
                isShielded = true;
                SetShieldVisual(true);
                StartCoroutine(RemoveBuffAfterTime("Shield", 10f));
                break;

            case 1: // TÀNG HÌNH
                SetInvisible(true);
                StartCoroutine(RemoveBuffAfterTime("Invis", 5f));
                break;

            // case 2: Hồi máu đã làm ở Server

            case 3: // TĂNG TỐC
                speedMultiplier = 2f;
                SetSpeedVisual(true); // Bật khói đỏ
                StartCoroutine(RemoveBuffAfterTime("Speed", 5f));
                break;

            case 4: // LÀM CHẬM
                speedMultiplier = 0.5f;
                StartCoroutine(RemoveBuffAfterTime("Speed", 5f));
                break;

            case 5: // SAY RƯỢU
                isDrunk = true;
                StartCoroutine(RemoveBuffAfterTime("Drunk", 5f));
                break;

            case 6: // BOM
                SetBombVisual(true);
                StartCoroutine(ExplodeBomb(3f));
                break;
        }

        // Phát tiếng chung
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.buffPickupClip);
    }

    private IEnumerator RemoveBuffAfterTime(string buffType, float duration)
    {
        yield return new WaitForSeconds(duration);
        switch (buffType)
        {
            case "Shield":
                isShielded = false;
                SetShieldVisual(false);
                break;
            case "Invis":
                SetInvisible(false);
                break;
            case "Speed":
                speedMultiplier = 1f; // Trả lại tốc độ gốc
                SetSpeedVisual(false); // Tắt khói đỏ
                break;
            case "Drunk":
                isDrunk = false;
                break;
        }
    }

    private IEnumerator ExplodeBomb(float time)
    {
        yield return new WaitForSeconds(time);

        // Bom nổ gây sát thương thì phải báo ngược lại Server trừ máu
        if (IsOwner && tankHealth != null)
        {
            // (Lưu ý: Chỉ Owner mới gọi ServerRPC trừ máu được, hoặc ta dùng logic va chạm khác)
            // Ở đây để đơn giản ta chỉ phát nổ visual, sát thương nên để Server tính
        }

        SetBombVisual(false);
        PlayExplosionSoundClientRpc();

        // Đoạn này nếu muốn trừ máu chuẩn thì Server phải làm, nhưng tạm thời để visual trước
        if (IsServer && tankHealth != null) tankHealth.TakeDamage(30);
    }

    // --- CÁC HÀM VISUAL (HIỂN THỊ HÌNH ẢNH) ---

    private void SetInvisible(bool isInvisible)
    {
        // 1. XỬ LÝ ẢNH XE
        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in allSprites)
        {
            Color c = sprite.color;
            // Địch không thấy gì (0), Mình thấy mờ (0.5)
            c.a = (isInvisible && !IsOwner) ? 0f : (isInvisible ? 0.5f : 1f);
            sprite.color = c;
        }

        // 2. XỬ LÝ THANH MÁU (Ẩn luôn Canvas)
        Canvas[] allCanvas = GetComponentsInChildren<Canvas>();
        foreach (var canvas in allCanvas)
        {
            if (isInvisible)
            {
                if (IsOwner) canvas.gameObject.SetActive(true);
                else canvas.gameObject.SetActive(false);
            }
            else
            {
                canvas.gameObject.SetActive(true);
            }
        }
    }

    private void SetSpeedVisual(bool isActive)
    {
        if (dustTrail != null)
        {
            var main = dustTrail.main;
            var emission = dustTrail.emission;
            if (isActive)
            {
                main.startColor = Color.red; // Khói đỏ
                emission.rateOverTime = 20f; // Phun mạnh
            }
            else
            {
                main.startColor = Color.white;
                emission.rateOverTime = 10f;
            }
        }
    }

    private void SetShieldVisual(bool isActive)
    {
        if (shieldVisual != null) shieldVisual.SetActive(isActive);
    }

    private void SetBombVisual(bool isActive)
    {
        if (bombVisual != null) bombVisual.SetActive(isActive);
    }

    // --- KHU VỰC CLIENT RPC KHÁC ---

    [ClientRpc]
    private void PlayShootSoundClientRpc()
    {
        if (!IsOwner && AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip);
    }

    [ClientRpc]
    private void PlayExplosionSoundClientRpc()
    {
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionClip);
    }

    // --- CÁC HÀM PHỤ TRỢ ---
    private void FindAndMoveToSpawn()
    {
        if (!IsOwner) return;
        if (IsHost)
        {
            GameObject hostPos = GameObject.Find("SpawnPos_Host");
            if (hostPos != null) { transform.position = hostPos.transform.position; if (rb) rb.linearVelocity = Vector2.zero; }
        }
        else
        {
            GameObject clientPos = GameObject.Find("SpawnPos_Client");
            if (clientPos != null) { transform.position = clientPos.transform.position; if (rb) rb.linearVelocity = Vector2.zero; }
        }
    }

    private void CheckVisibilityLobby()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "LobbyScene")
        {
            if (!isHiddenInLobby) { SetTankActive(false); isHiddenInLobby = true; }
        }
        else
        {
            if (isHiddenInLobby) { SetTankActive(true); isHiddenInLobby = false; }
        }
    }

    private void SetTankActive(bool isActive)
    {
        var allSprites = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in allSprites) sprite.enabled = isActive;
        var allColliders = GetComponentsInChildren<Collider2D>(true);
        foreach (var col in allColliders) col.enabled = isActive;
        var allCanvas = GetComponentsInChildren<Canvas>(true);
        foreach (var canvas in allCanvas) canvas.gameObject.SetActive(isActive);
    }

    private void ConnectToCamera()
    {
        var camObj = Camera.main;
        if (camObj != null)
        {
            var camScript = camObj.GetComponent<CameraFollow>();
            if (camScript != null) camScript.SetTarget(transform);
        }
    }

    private void ChangeSkinBasedOnID()
    {
        int playerID = (int)OwnerClientId;
        if (tankSkins != null && tankSkins.Length > 0) spriteRenderer.sprite = tankSkins[playerID % tankSkins.Length];
        if (turretSkins != null && turretSkins.Length > 0 && turretRenderer != null) turretRenderer.sprite = turretSkins[playerID % turretSkins.Length];
    }

    private void HandleEffects(float move, float rotate)
    {
        bool isMoving = Mathf.Abs(move) > 0.1f || Mathf.Abs(rotate) > 0.1f;
        if (dustTrail != null)
        {
            if (isMoving) { if (!dustTrail.isPlaying) dustTrail.Play(); }
            else { if (dustTrail.isPlaying) dustTrail.Stop(); }
        }
        if (trackAnimator != null) trackAnimator.SetBool("IsMoving", isMoving);
    }
}