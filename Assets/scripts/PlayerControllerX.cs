using UnityEngine;
using UnityEngine.UI; // Penting untuk Slider
using TMPro; 
using System.Collections; 

public class PlayerControllerX : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;       
    public float sprintBoost = 3f;     
    private float currentSpeed;        
    private Vector2 moveInput;
    private Vector2 lastMoveDirection; 

    [Header("Run Effect Settings")]
    public GameObject runEffectRight;  
    public GameObject runEffectLeft;   
    public Transform feetPos;          
    public float effectRate = 0.2f;    
    private float nextEffectTime = 0f;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;      
    public float dashDuration = 0.2f;  
    public float dashCooldown = 1f;    
    public bool invincibleWhileDashing = true; 
    public AudioClip dashSound;        
    
    private bool isDashing = false;
    private float nextDashTime = 0f;
    private Vector2 dashDir; 

    [Header("Combat Settings")]
    public int damage = 20;
    public float attackRange = 0.8f;
    public float attackRate = 2f;
    public float damageDelay = 0.2f; 
    public float knockbackPower = 10f; 
    
    public Transform attackPoint;     
    public LayerMask enemyLayer;      
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;    
    public AudioClip[] attackSounds;   

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public TMP_Text healthText;
    public Animator healthTextAnim;

    // --- BAGIAN BARU: LEVEL SYSTEM ---
    [Header("Level System")]
    public int currentLevel = 1;
    public float currentXp = 0;
    public float requiredXp = 100;     // Target XP untuk level selanjutnya
    
    [Header("Level UI")]
    public Slider xpSlider;            // Drag Slider XP di sini
    public TMP_Text levelText;         // Drag TextMeshPro Level di sini
    public AudioClip levelUpSound;     // Suara saat naik level (Opsional)

    [Header("Level Up Bonusses")]
    public int healthIncreasePerLevel = 20; // Tambah max HP per level
    public int damageIncreasePerLevel = 5;  // Tambah damage per level
    // ---------------------------------

    [Header("Visual Effects (Damage)")]
    public Color flashColor = Color.red; 
    public float flashDuration = 0.1f;   
    public int numberOfFlashes = 3;      
    public float iframesDuration = 1f;   
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isInvincible = false;   

    [Header("Knockback Received Settings")]
    public float hitKnockbackForce = 10f; 
    public float hitKnockbackTime = 0.2f; 
    
    public bool isKnockback = false; 
    private float knockbackCounter;
    private Vector2 knockbackDir; 

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator anim;

    private bool isDead = false;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        if (feetPos == null) feetPos = transform;

        currentHealth = maxHealth;
        UpdateHPText();
        
        // Inisialisasi UI Level
        UpdateLevelUI();
        
        lastMoveDirection = Vector2.down; 
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

        if (isDashing) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed = walkSpeed + sprintBoost;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        if (moveInput.magnitude > 0 && Time.time >= nextEffectTime)
        {
            SpawnRunEffect(moveX);
            float rate = (currentSpeed > walkSpeed) ? effectRate / 1.5f : effectRate;
            nextEffectTime = Time.time + rate;
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextDashTime)
        {
            StartCoroutine(PerformDash());
        }

        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking) 
            {
                StartCoroutine(PerformAttack());
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

        if (!isAttacking) AnimateMovement(moveX, moveY);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isKnockback)
        {
            Vector2 targetPos = rb.position + (knockbackDir * hitKnockbackForce * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
            return; 
        }

        if (isDashing)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            return; 
        }

        if (isAttacking) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput * currentSpeed;
    }

    // --- SISTEM EXP & LEVEL UP ---
    public void AddExperience(float amount)
    {
        currentXp += amount;
        
        // Cek apakah naik level
        while (currentXp >= requiredXp)
        {
            LevelUp();
        }

        UpdateLevelUI();
    }

    void LevelUp()
    {
        currentLevel++;
        currentXp -= requiredXp; // Sisa EXP dibawa ke level berikutnya
        
        // Naikkan target EXP untuk level selanjutnya (agar makin susah)
        // Contoh: Naik 20% setiap level
        requiredXp = Mathf.RoundToInt(requiredXp * 1.2f);

        // --- NAIKKAN STATS ---
        maxHealth += healthIncreasePerLevel;
        currentHealth = maxHealth; // Heal penuh saat naik level
        damage += damageIncreasePerLevel;
        // ---------------------

        // Update Text HP
        UpdateHPText();

        // Mainkan suara/efek
        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
        
        Debug.Log("LEVEL UP! Level: " + currentLevel + " | Damage: " + damage + " | MaxHP: " + maxHealth);
    }

    void UpdateLevelUI()
    {
        if (xpSlider != null)
        {
            xpSlider.maxValue = requiredXp;
            xpSlider.value = currentXp;
        }

        if (levelText != null)
        {
            levelText.text = "Lv. " + currentLevel;
        }
    }
    // -----------------------------

    void SpawnRunEffect(float moveX)
    {
        GameObject effectToSpawn = null;
        if (moveX > 0) effectToSpawn = runEffectRight;
        else if (moveX < 0) effectToSpawn = runEffectLeft;
        else effectToSpawn = (lastMoveDirection.x < 0) ? runEffectLeft : runEffectRight;

        if (effectToSpawn != null)
        {
            Instantiate(effectToSpawn, feetPos.position, Quaternion.identity);
        }
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;
        dashDir = moveInput != Vector2.zero ? moveInput : lastMoveDirection;

        if (audioSource != null && dashSound != null) audioSource.PlayOneShot(dashSound);

        if (invincibleWhileDashing)
        {
            isInvincible = true;
            if (spriteRenderer != null) spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        }

        SpawnRunEffect(dashDir.x);

        yield return new WaitForSeconds(dashDuration);

        if (invincibleWhileDashing)
        {
            isInvincible = false;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }
        
        isDashing = false;
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero; 
        anim.SetFloat("Speed", 0);
        anim.SetTrigger("isAttack");

        if (audioSource != null && attackSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, attackSounds.Length);
            audioSource.PlayOneShot(attackSounds[randomIndex]);
        }

        yield return new WaitForSeconds(damageDelay);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyControllerX enemyScript = enemy.GetComponent<EnemyControllerX>();
            if (enemyScript != null)
            {
                enemyScript.ChangeHealth(-damage);
                enemyScript.GetKnockback(transform, knockbackPower);
            }

            BossController bossScript = enemy.GetComponent<BossController>();
            if (bossScript != null)
            {
                bossScript.ChangeHealth(-damage);
                bossScript.GetKnockback(transform, knockbackPower);
            }
        }
        
        yield return new WaitForSeconds(0.1f);
        isAttacking = false;
    }

    public void KnockBack(Transform enemyTransform)
    {
        if (isDead || isInvincible) return;

        if (hitKnockbackTime <= 0) hitKnockbackTime = 0.2f;
        if (hitKnockbackForce <= 0) hitKnockbackForce = 10f;

        isKnockback = true;
        knockbackCounter = hitKnockbackTime;
        isAttacking = false; 

        knockbackDir = (transform.position - enemyTransform.position).normalized;
    }

    public void ChangeHealth(int amount)
    {
        if (isDead) return;
        if (isInvincible && amount < 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthTextAnim != null) healthTextAnim.Play("HP_Text");
        UpdateHPText();

        if (amount < 0) StartCoroutine(FlashAfterDamage());
        if (currentHealth <= 0) Die();
    }

    IEnumerator FlashAfterDamage()
    {
        isInvincible = true; 
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRenderer.color = flashColor; 
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor; 
            yield return new WaitForSeconds(flashDuration);
        }
        yield return new WaitForSeconds(iframesDuration - (numberOfFlashes * flashDuration * 2));
        if (!isDashing) isInvincible = false; 
    }

    void AnimateMovement(float x, float y)
    {
        anim.SetFloat("Horizontal", x);
        anim.SetFloat("Vertical", y);
        anim.SetFloat("Speed", moveInput.sqrMagnitude * currentSpeed); 

        if (x != 0 || y != 0)
        {
            anim.SetFloat("LastHorizontal", x);
            anim.SetFloat("LastVertical", y);
            lastMoveDirection = new Vector2(x, y).normalized;
            UpdateAttackPointPosition();
        }

        if (x > 0 && transform.localScale.x < 0) Flip();
        else if (x < 0 && transform.localScale.x > 0) Flip();
    }

    void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void UpdateAttackPointPosition()
    {
        if (attackPoint != null)
        {
            attackPoint.localPosition = lastMoveDirection * 0.5f;
        }
    }

    void UpdateHPText()
    {
        if (healthText != null) healthText.text = "HP: " + currentHealth + "/" + maxHealth;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        
        // Panggil fungsi Game Over di GameManager
        if (GameManagerX.instance != null)
        {
            GameManagerX.instance.TriggerGameOver();
        }

        // Matikan visual player tapi jangan destroy objectnya dulu
        // agar script ini masih bisa jalan sebentar jika perlu
        // atau cukup matikan sprite dan collider
        GetComponent<Collider2D>().enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        
        // Matikan script ini
        this.enabled = false;
    }
}