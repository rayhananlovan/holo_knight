using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyControllerX : MonoBehaviour
{
    [Header("Health Settings (Random Range)")]
    public int minHp = 20;
    public int maxHp = 50;
    public int maxHealth;
    public int currentHealth;
    public Slider healthSlider; 
    public GameObject healthCanvas; 

    [Header("XP Drop Settings")]
    [Range(0, 100)]
    public float xpDropPercentage = 20f;
    
    [Header("Combat Settings")]
    public int damage = 10;
    public float speed = 3f; 
    public float findRange = 5f;
    public float attackRange = 0.8f;
    public float attackCooldown = 1.5f;
    public float damageDelay = 0.3f; 

    [Header("Patrol Settings")]
    public bool enablePatrol = true; 
    public float patrolSpeed = 1.5f; 
    public float patrolRange = 3f;   
    public float waitTime = 2f;      
    
    private Vector2 startPoint;      
    private Vector2 roamPoint;       
    private float roamTimer;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] attackSounds;
    public AudioClip deathSound; // --- BARU: Slot untuk suara mati ---
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
    private float knockbackDuration = 0.2f; 
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

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        // Random HP
        maxHealth = Random.Range(minHp, maxHp + 1);
        currentHealth = maxHealth;

        if (healthSlider != null) { healthSlider.maxValue = maxHealth; healthSlider.value = currentHealth; }
        if (healthCanvas != null) originalCanvasScale = healthCanvas.transform.localScale;
        if (detectionPoint == null) detectionPoint = transform;
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null) player = GameObject.FindGameObjectWithTag("Player").transform;

        // Setup Patrol
        startPoint = transform.position; 
        PickNewRoamPoint();
        roamTimer = waitTime;
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

        if (healthCanvas != null)
        {
            if (transform.localScale.x < 0) healthCanvas.transform.localScale = new Vector3(-Mathf.Abs(originalCanvasScale.x), originalCanvasScale.y, originalCanvasScale.z);
            else healthCanvas.transform.localScale = new Vector3(Mathf.Abs(originalCanvasScale.x), originalCanvasScale.y, originalCanvasScale.z);
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Logika AI (Patrol vs Chase)
        if (distanceToPlayer <= findRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            
            UpdateAttackPointPosition(direction);
            UpdateAnimationDirection(direction);

            if (distanceToPlayer <= attackRange)
            {
                StopMoving();
                if (Time.time >= nextAttackTime && !isAttacking)
                {
                    StartCoroutine(PerformAttack());
                }
            }
            else
            {
                if (!isAttacking) Chasing(direction);
            }
        }
        else
        {
            if (enablePatrol && !isAttacking)
            {
                Patrolling();
            }
            else
            {
                StopMoving();
            }
        }
    }

    void Patrolling()
    {
        float distToRoamPoint = Vector2.Distance(transform.position, roamPoint);

        if (distToRoamPoint < 0.2f)
        {
            StopMoving(); 
            roamTimer -= Time.deltaTime;
            if (roamTimer <= 0)
            {
                PickNewRoamPoint(); 
                roamTimer = waitTime; 
            }
        }
        else
        {
            Vector2 dir = (roamPoint - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * patrolSpeed;
            anim.SetFloat("Speed", dir.sqrMagnitude);
            UpdateAnimationDirection(dir);
            CheckFlip(dir.x);
        }
    }

    void PickNewRoamPoint()
    {
        roamPoint = startPoint + Random.insideUnitCircle * patrolRange;
    }

    public void GetKnockback(Transform sender, float force)
    {
        if (isDead) return;

        isKnockback = true;
        knockbackCounter = knockbackDuration;
        rb.linearVelocity = Vector2.zero;
        Vector2 dir = (transform.position - sender.position).normalized;
        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControllerX playerScript = collision.gameObject.GetComponent<PlayerControllerX>();
            if (playerScript != null)
            {
                playerScript.KnockBack(transform);
                playerScript.ChangeHealth(-damage);
            }
        }
    }

    System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Attack");
        StartCoroutine(PlaySoundDelayed());
        yield return new WaitForSeconds(damageDelay); 

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
        if (audioSource != null && attackSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, attackSounds.Length);
            audioSource.PlayOneShot(attackSounds[randomIndex]);
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
        else 
        {
            attackPoint.localPosition = posisiSamping;
        }
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
        if (xValue > 0.1f && transform.localScale.x < 0) Flip();
        else if (xValue < -0.1f && transform.localScale.x > 0) Flip();
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
        if(healthCanvas != null) healthCanvas.SetActive(false);
        anim.SetTrigger("Die");
        
        // --- BAGIAN BARU: MAINKAN SUARA MATI ---
        if (deathSound != null)
        {
            // PlayClipAtPoint membuat suara tetap bunyi meskipun object musuhnya didestroy
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        }
        // ---------------------------------------

        if (player != null)
        {
            PlayerControllerX playerScript = player.GetComponent<PlayerControllerX>();
            if (playerScript != null)
            {
                float xpGained = (maxHealth * xpDropPercentage) / 100f;
                if (xpGained < 1) xpGained = 1;
                playerScript.AddExperience(xpGained);
            }
        }
        
        Destroy(gameObject, 1f); 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, findRange);
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint, patrolRange);
            Gizmos.DrawLine(transform.position, roamPoint);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, patrolRange);
        }
    }
}