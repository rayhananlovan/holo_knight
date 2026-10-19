using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider volumeSlider; // Drag Slider Volume kamu ke sini

    void Start()
    {
        // 1. Ambil nilai volume yang tersimpan (Default 1 jika belum ada)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        
        // 2. Terapkan ke AudioListener (Suara Global)
        AudioListener.volume = savedVolume;
        
        // 3. Update posisi Slider agar sesuai dengan nilai tersimpan
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            
            // Tambahkan listener otomatis agar kita tidak perlu setting OnValueChanged di Inspector
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // Fungsi ini dipanggil otomatis saat Slider digeser
    public void SetVolume(float volume)
    {
        // Ubah volume global
        AudioListener.volume = volume;
        
        // Simpan settingan ke memori
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
}