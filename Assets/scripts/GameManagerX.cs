using UnityEngine;
using UnityEngine.SceneManagement; // Wajib untuk pindah scene

public class GameManagerX : MonoBehaviour
{
    public static GameManagerX instance; // Singleton agar mudah dipanggil

    [Header("UI Settings")]
    public GameObject gameOverPanel; // Drag Panel Game Over ke sini

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void TriggerGameOver()
    {
        // Munculkan panel Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // OPSI: Hentikan waktu agar game pause
            Time.timeScale = 0f; 
        }
    }

    // Fungsi untuk Tombol Restart
    public void RestartGame()
    {
        // Kembalikan waktu ke normal (penting jika tadi dipause)
        Time.timeScale = 1f;
        
        // Reload scene yang sedang aktif saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Fungsi untuk Tombol Main Menu
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Fungsi untuk Pindah ke Scene Win (Dipanggil Boss)
    public void LoadWinScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Win");
    }
}