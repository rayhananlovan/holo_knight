using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Settings")]
    public int healAmount = 20; // Berapa banyak darah yang ditambah
    
    [Header("Effects")]
    public GameObject pickupEffect; // Partikel saat diambil (Opsional)
    public AudioClip pickupSound;   // Suara saat diambil (Opsional)

    [Header("Animation (Floating)")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.2f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Efek melayang naik turun biar keren (Floating)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cek apakah yang menabrak adalah Player
        if (collision.CompareTag("Player"))
        {
            PlayerControllerX player = collision.GetComponent<PlayerControllerX>();

            if (player != null)
            {
                // Cek apakah darah player penuh?
                // Jika kamu mau orb tidak bisa diambil kalau darah penuh, buka komentar di bawah ini:
                /*
                if (player.currentHealth >= player.maxHealth)
                {
                    return; // Jangan ambil orb
                }
                */

                // 1. Tambah Darah Player (Angka Positif)
                player.ChangeHealth(healAmount);

                // 2. Mainkan Suara (Menggunakan PlayClipAtPoint agar suara tidak putus saat object hancur)
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);
                }

                // 3. Spawn Partikel (Jika ada)
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // 4. Hancurkan Orb
                Destroy(gameObject);
            }
        }
    }
}