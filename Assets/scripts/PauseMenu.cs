using UnityEngine;
using UnityEngine.SceneManagement; // Wajib untuk pindah scene

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel; // Drag Panel Pause kamu ke sini
    public string mainMenuScene = "MainMenu"; // Ganti dengan nama scene main menu kamu

    // Status apakah game sedang pause atau tidak
    public static bool isPaused = false;

    void Start()
    {
        // Pastikan saat mulai, panel mati dan waktu berjalan normal
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        // Jika tombol ESC ditekan
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true); // Munculkan panel
        Time.timeScale = 0f; // Hentikan waktu (Game berhenti)
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false); // Sembunyikan panel
        Time.timeScale = 1f; // Jalankan waktu kembali normal
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // PENTING: Kembalikan waktu ke normal sebelum reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload scene saat ini
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // PENTING: Kembalikan waktu ke normal sebelum pindah
        SceneManager.LoadScene(mainMenuScene);
    }
}