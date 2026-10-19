using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject mainPanel;    // Drag Panel Menu Utama ke sini
    public GameObject settingsPanel; // Drag Panel Setting ke sini

    [Header("Scene Name")]
    public string gameplaySceneName = "SampleScene"; // Ganti dengan nama scene game kamu

    void Start()
    {
        // Pastikan saat game mulai: Main Menu NYALA, Setting MATI
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // --- FUNGSI NAVIGASI UI ---

    public void OpenSettings()
    {
        // Matikan Main, Nyalakan Setting
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void BackToMain()
    {
        // Matikan Setting, Nyalakan Main
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    // --- FUNGSI SCENE MANAGEMENT ---

    public void PlayGame()
    {
        // Pindah ke scene gameplay
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Keluar Game"); // Cek di editor
    }
}