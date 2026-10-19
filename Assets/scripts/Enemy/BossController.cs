using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Required for SceneManager

public class BossController : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 500;
    public int currentHealth;
    public Slider healthSlider; 
    public GameObject healthCanvas; 

    // --- ADDED MISSING VARIABLE ---
    [Header("XP Drop Settings")]
    [Range(0, 100)]
    public float xpDropPercentage = 100f; // Boss usually gives full/high XP
    // ------------------------------

    [Header("Combat Settings")]
    public int damage = 25;
    public float speed = 3f;
    public float findRange = 7f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float damageDelay = 0.3f; 

    [Header("Boss Special Mechanics")]
    public float dashForce = 20f;
    public bool immuneToKnockback = true;
    
    public AudioSource musicSource; 
    public AudioClip bossTheme;     
    public AudioSource levelBGM;    

    private bool hasAggro = false;  

    [Header("SFX Settings")]
    public AudioSource sfxSource;   
    public AudioClip[] attackSounds;
    // --- ADDED MISSING VARIABLE ---
    public AudioClip deathSound; 
    // ------------------------------
    public float soundDelay = 0.1f; 

    [Header("Attack Point Positions")]
    public Vector3 posisiAtas = new Vector3(0, 0.8f, 0);
    public Vector3 posisiBawah = new Vector3(0, -0.8f, 0);
    public Vector3 posisiSamping = new Vector3(0.8f, 0, 0);

    [Header("Visual Effects")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    [Header("References")]
    public Transform player;
    public LayerMask playerLayer;
    public Transform attackPoint;
    public Transform detectionPoint; 

    private bool isKnockback = false;
    private float knockbackDuration = 0.1f; 
    private float knockbackCounter;

    private Vector3 originalCanvasScale;
    private float nextAttackTime = 0f;
    private bool isDead = false;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        currentHealth = maxHealth;
        
        if (healthSlider != null) { healthSlider.maxValue = maxHealth; healthSlider.value = currentHealth; }
        if (healthCanvas != null) originalCanvasScale = healthCanvas.transform.localScale;
        if (detectionPoint == null) detectionPoint = transform;
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null) player = GameObject.FindGameObjectWithTag("Player").transform;

        musicSource.loop = true; 
        musicSource.clip = bossTheme;
        musicSource.playOnAwake = false; 

        if (levelBGM == null)
        {
            GameObject bgmObj = GameObject.Find("LevelMusic");
            if (bgmObj != null) levelBGM = bgmObj.GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isDead) return;

        if (isKnockback)
        {
            knockbackCounter -= Time.deltaTime;
            if (knockbackCounter <= 0)
            {
                isKnockback = false;
                rb.linearVelocity = Vector2.zero; 
            }
            return; 
        }
        
        if (isAttacking) return; 

        if (healthCanvas != null)
        {
            if (transform.localScale.x < 0) healthCanvas.transform.localScale = new Vector3(-Mathf.Abs(originalCanvasScale.x), originalCanvasScale.y, originalCanvasScale.z);
            else healthCanvas.transform.localScale = new Vector3(Mathf.Abs(originalCanvasScale.x), originalCanvasScale.y, originalCanvasScale.z);
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        UpdateAttackPointPosition(direction);
        UpdateAnimationDirection(direction);

        if (!hasAggro && distanceToPlayer <= findRange)
        {
            hasAggro = true; 
            if (levelBGM != null) levelBGM.Stop();
            if (musicSource != null && bossTheme != null) musicSource.Play();
        }

        if (hasAggro)
        {
            if (distanceToPlayer <= attackRange)
            {
                StopMoving(); 
                if (Time.time >= nextAttackTime && !isAttacking)
                {
                    StartCoroutine(PerformAttack(direction));
                }
            }
            else
            {
                Chasing(direction);
            }
        }
        else
        {
            StopMoving();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        
        if (collision.CompareTag("Player"))
        {
            PlayerControllerX playerScript = collision.GetComponent<PlayerControllerX>();
            if (playerScript != null)
            {
                playerScript.KnockBack(transform);
                playerScript.ChangeHealth(-damage);
            }
        }
    }

    public void GetKnockback(Transform sender, float force)
    {
        if (isDead) return;
        if (immuneToKnockback) return;

        isKnockback = true;
        knockbackCounter = knockbackDuration;
        rb.linearVelocity = Vector2.zero;
        Vector2 dir = (transform.position - sender.position).normalized;
        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }

    System.Collections.IEnumerator PerformAttack(Vector2 dirToPlayer)
    {
        isAttacking = true;
        anim.SetTrigger("Attack");
        StartCoroutine(PlaySoundDelayed());
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(dirToPlayer * dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(damageDelay); 
        rb.linearVelocity = Vector2.zero; 

        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D p in hitPlayer)
        {
            PlayerControllerX playerScript = p.GetComponent<PlayerControllerX>();
            if (playerScript != null)
            {
                playerScript.KnockBack(transform); 
                playerScript.ChangeHealth(-damage);
            }
        }

        yield return new WaitForSeconds(attackCooldown - damageDelay);
        isAttacking = false;
        nextAttackTime = Time.time;
    }

    IEnumerator PlaySoundDelayed()
    {
        yield return new WaitForSeconds(soundDelay);
        if (sfxSource != null && attackSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, attackSounds.Length);
            sfxSource.PlayOneShot(attackSounds[randomIndex]);
        }
    }

    public void ChangeHealth(int amount)
    {
        if (isDead) return;
        currentHealth += amount;
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (amount < 0) StartCoroutine(FlashRed());
        if (currentHealth <= 0) Die();
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    void UpdateAttackPointPosition(Vector2 dir)
    {
        if (attackPoint == null) return;
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x)) 
        {
            if (dir.y > 0) attackPoint.localPosition = posisiAtas;
            else attackPoint.localPosition = posisiBawah;
        }
        else attackPoint.localPosition = posisiSamping;
    }

    void Chasing(Vector2 dir)
    {
        rb.linearVelocity = dir * speed;
        anim.SetFloat("Speed", dir.sqrMagnitude);
        CheckFlip(dir.x);
    }

    void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetFloat("Speed", 0);
    }

    void UpdateAnimationDirection(Vector2 dir)
    {
        anim.SetFloat("Horizontal", dir.x);
        anim.SetFloat("Vertical", dir.y);
    }

    void CheckFlip(float xValue)
    {
        if (xValue > 0.1f && transform.localScale.x > 0) Flip();
        else if (xValue < -0.1f && transform.localScale.x < 0) Flip();
    }

    void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        
        if (musicSource != null) musicSource.Stop();
        if (levelBGM != null) levelBGM.Play();
        if (healthCanvas != null) healthCanvas.SetActive(false);
        
        anim.SetTrigger("Die");
        
        // --- NOW THIS VARIABLE EXISTS ---
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        }

        if (player != null)
        {
            PlayerControllerX playerScript = player.GetComponent<PlayerControllerX>();
            if (playerScript != null)
            {
                // --- NOW THIS VARIABLE EXISTS ---
                float xpGained = (maxHealth * xpDropPercentage) / 100f;
                if (xpGained < 1) xpGained = 1;
                playerScript.AddExperience(xpGained);
            }
        }
        
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Win");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta; 
        Gizmos.DrawWireSphere(transform.position, findRange);
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}