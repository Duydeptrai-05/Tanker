using UnityEngine;
using UnityEngine.SceneManagement; // Cần thư viện này để check màn chơi

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("--- NGUỒN PHÁT (AUDIO SOURCE) ---")]
    public AudioSource musicSource; // Nhớ tích Loop
    public AudioSource sfxSource;   // Nhớ BỎ tích Loop

    [Header("--- NHẠC NỀN (BGM) ---")]
    public AudioClip bgmMenu;       // Nhạc nhẹ nhàng (Lobby/Menu)
    public AudioClip bgmGame;       // Nhạc chiến đấu (GameScene)

    [Header("--- HIỆU ỨNG (SFX) ---")]
    public AudioClip shootClip;
    public AudioClip explosionClip;
    public AudioClip chestOpenClip;
    public AudioClip buffPickupClip;
    public AudioClip captureClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip clickClip;

    private void Awake()
    {
        // Singleton: Giữ AudioManager sống mãi
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Đăng ký sự kiện chuyển cảnh
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- HÀM TỰ ĐỘNG ĐỔI NHẠC KHI CHUYỂN CẢNH ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nếu đang ở Menu hoặc Lobby -> Chơi nhạc Menu
        if (scene.name == "MenuScene" || scene.name == "LobbyScene")
        {
            PlayMusic(bgmMenu);
        }
        // Nếu đang ở Game -> Chơi nhạc Chiến đấu
        else if (scene.name == "GameScene")
        {
            PlayMusic(bgmGame);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        // Nếu bài nhạc mới trùng bài đang phát thì không cần phát lại (để tránh bị ngắt quãng)
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayClickSound()
    {
        if (clickClip != null) PlaySFX(clickClip);
    }
}