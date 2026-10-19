using System;
using Unity.VisualScripting;
using UnityEngine;

public class Elevation_Entery : MonoBehaviour
{
    public Collider2D[] mountainColiders;
    public Collider2D[] bounderyColiders;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // === Jika Player Masuk ===
        if (collision.CompareTag("Player"))
        {
            foreach (Collider2D mountain in mountainColiders)
            {
                mountain.enabled = false;
            }

            foreach (Collider2D bound in bounderyColiders)
            {
                bound.enabled = true;
            }

            collision.GetComponent<SpriteRenderer>().sortingOrder = 15;
        }

        // === Jika Enemy Masuk ===
        if (collision.CompareTag("Enemy"))
        {
            foreach (Collider2D mountain in mountainColiders)
            {
                mountain.enabled = false;
            }

            foreach (Collider2D bound in bounderyColiders)
            {
                bound.enabled = true;
            }

            // Jika enemy punya sprite renderer juga
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 15;
            }
        }
    }
}
