using System;
using Unity.VisualScripting;
using UnityEngine;

public class Elevation_Exit : MonoBehaviour
{
    public Collider2D[] mountainColiders;
    public Collider2D[] bounderyColiders;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // === Jika Player Keluar ===
        if (collision.CompareTag("Player"))
        {
            foreach (Collider2D mountain in mountainColiders)
            {
                mountain.enabled = true;
            }

            foreach (Collider2D bound in bounderyColiders)
            {
                bound.enabled = false;
            }

            // Kembalikan sorting order
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 5;
            }
        }

        // === Jika Enemy Keluar ===
        if (collision.CompareTag("Enemy"))
        {
            foreach (Collider2D mountain in mountainColiders)
            {
                mountain.enabled = true;
            }

            foreach (Collider2D bound in bounderyColiders)
            {
                bound.enabled = false;
            }

            // Kembalikan sorting order Enemy
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 5;
            }
        }
    }
}
