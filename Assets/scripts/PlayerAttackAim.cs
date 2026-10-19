using UnityEngine;

public class PlayerAimDirection : MonoBehaviour
{
    [Header("Target")]
    public Transform attackPoint;

    [Header("Atur Posisi Attack Point Tiap Arah")]
    // Kita pakai Vector3 agar kamu bisa atur X dan Y nya spesifik
    public Vector3 posisiAtas = new Vector3(0, 1f, 0);
    public Vector3 posisiBawah = new Vector3(0, -1f, 0);
    public Vector3 posisiKanan = new Vector3(1f, 0, 0);
    public Vector3 posisiKiri = new Vector3(1f, 0, 0); // Perhatikan ini nanti!

    private Vector3 lastDirection;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Cek input dan ubah posisi local attackPoint
        if (x > 0) // Kanan
        {
            attackPoint.localPosition = posisiKanan;
            lastDirection = Vector3.right;
        }
        else if (x < 0) // Kiri
        {
            attackPoint.localPosition = posisiKiri;
            lastDirection = Vector3.left;
        }
        
        if (y > 0) // Atas
        {
            attackPoint.localPosition = posisiAtas;
            lastDirection = Vector3.up;
        }
        else if (y < 0) // Bawah
        {
            attackPoint.localPosition = posisiBawah;
            lastDirection = Vector3.down;
        }
    }

    // Fitur Visualisasi biar gampang ngatur di Inspector
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.green;
        // Menggambar posisi prediksi relatif terhadap player
        Gizmos.DrawWireSphere(transform.TransformPoint(posisiAtas), 0.1f);
        Gizmos.DrawWireSphere(transform.TransformPoint(posisiBawah), 0.1f);
        Gizmos.DrawWireSphere(transform.TransformPoint(posisiKanan), 0.1f);
        Gizmos.DrawWireSphere(transform.TransformPoint(posisiKiri), 0.1f);
    }
}