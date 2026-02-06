using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public float stamina = 100;
    public float speed = 5f;
    public float jumpForce = 5f;
    public float maxJumpTime = 0.2f;
    public float holdForce = 7f;
    public float fastFallSpeed = 7f;
    public float health = 10;
    public float damage = 2;
    public float wallSlide = 1f;
    public Vector3 spawnPosition;
    public float knockbackForce = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float wallJumpForceX = 8f;
    public float wallJumpForceY = 8f;

    [Header("Reflect / Parry")]
    [SerializeField] private float reflectWindow = 0.2f;
    [SerializeField] private float reflectIgnoreCollisionTime = 0.1f;

    private bool isReflecting = false;
    private float reflectCooldownTimer = 0f;

    [Header("UI - Parry Cooldown")]
    [SerializeField] private Image parryCooldownFillImage;     // Filled Image (fillAmount 0..1)
    [SerializeField] private float parryCooldownDuration = 2f; // 2 Sekunden Cooldown

    [Header("UI - Dash Cooldown")]
    [SerializeField] private Image dashCooldownFillImage;      // Filled Image (fillAmount 0..1)

    [Header("Parry Object (No Animation)")]
    [SerializeField] private GameObject parryObject;           // Child "Parry" (SpriteRenderer + BoxCollider2D IsTrigger)
    [SerializeField] private float parryActiveTime = 0.25f;    // wie lange Parry aktiv ist
    private Coroutine parryCoroutine;

    [Header("Block & Parry Colliders")]
    [SerializeField] private Collider2D parryCollider;
    [SerializeField] private Collider2D blockCollider;
    private bool isBlocking = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isJumping = false;
    private bool canJump = false;
    public bool isTouchingWall = false;
    public bool canMove = true;
    public bool canAttack = true;
    public bool canSprint = false;

    private float jumpTimeCounter;
    private float staminaRegenTimer = 0.2f;
    private float staminaRegenCooldown = 0.2f;
    private bool isConsumingStamina = false;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    private bool justWallJumped = false;
    private float wallJumpControlTimer = 0f;
    private float wallJumpControlDuration = 0.3f;

    private float move = 0f;

    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI healthText;



    private bool isDamageFlashRunning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPosition = transform.position;

        // UI initial "bereit"
        if (parryCooldownFillImage != null)
            parryCooldownFillImage.fillAmount = 1f;

        if (dashCooldownFillImage != null)
            dashCooldownFillImage.fillAmount = 1f;

        // Parry Objekt sicher aus
        if (parryObject != null)
            parryObject.SetActive(false);
    }

    void Update()
    {
        move = 0f;

        // Bewegung Input
        if (canMove && !isDashing && !justWallJumped)
        {
            if (Input.GetKey(KeyCode.A))
                move = -1f;
            else if (Input.GetKey(KeyCode.D))
                move = 1f;
        }

        // Wall Jump Control Timer
        if (justWallJumped)
        {
            wallJumpControlTimer -= Time.deltaTime;
            if (wallJumpControlTimer <= 0)
                justWallJumped = false;
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && stamina >= 10 && dashCooldownTimer <= 0 && !isDashing)
        {
            UseStamina(10f);
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            // Dash UI sofort leer
            if (dashCooldownFillImage != null)
                dashCooldownFillImage.fillAmount = 0f;
        }

        // Dash Timer
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                isDashing = false;
        }

        // Dash Cooldown runterzählen
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // Dash UI updaten
        if (dashCooldownFillImage != null)
        {
            if (dashCooldownTimer <= 0f)
                dashCooldownFillImage.fillAmount = 1f;
            else
                dashCooldownFillImage.fillAmount = Mathf.Clamp01(1f - (dashCooldownTimer / dashCooldown));
        }

        // Wand Stamina Verbrauch
        if (isTouchingWall && !canJump)
            UseStamina(5f * Time.deltaTime);

        // Parry Cooldown runterzählen
        if (reflectCooldownTimer > 0f)
            reflectCooldownTimer -= Time.deltaTime;

        // Parry UI updaten
        if (parryCooldownFillImage != null)
        {
            if (reflectCooldownTimer <= 0f)
                parryCooldownFillImage.fillAmount = 1f;
            else
                parryCooldownFillImage.fillAmount = Mathf.Clamp01(1f - (reflectCooldownTimer / parryCooldownDuration));
        }

        // Block/Parry (Taste W)
        if (Input.GetKeyDown(KeyCode.W))
        {
            isBlocking = true;
            spriteRenderer.color = Color.blue;

            // Parry nur wenn Cooldown bereit
            if (reflectCooldownTimer <= 0f)
            {
                ActivateParry(); // Standbild+Collider kurz aktivieren

                StartCoroutine(ReflectWindow());
                reflectCooldownTimer = parryCooldownDuration;

                if (parryCooldownFillImage != null)
                    parryCooldownFillImage.fillAmount = 0f;
            }
        }
        else
        {
            isBlocking = false;
            spriteRenderer.color = Color.white;
        }

        // Flip Sprite
        if (move < 0f && !justWallJumped)
            spriteRenderer.flipX = true;
        else if (move > 0f && !justWallJumped)
            spriteRenderer.flipX = false;

        // Attack
        if (Input.GetKey(KeyCode.J))
        {
            UseStamina(10f * Time.deltaTime);
            playerAttack();
        }

        // Springen
        if (Input.GetKeyDown(KeyCode.Space) && isTouchingWall)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            justWallJumped = true;
            wallJumpControlTimer = wallJumpControlDuration;

            float wallJumpDirection = spriteRenderer.flipX ? 1f : -1f;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpForceX, wallJumpForceY);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && canJump && stamina >= 15)
        {
            UseStamina(10f);
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            canJump = false;
            canSprint = false;
        }

        if (Input.GetKey(KeyCode.Space) && isJumping && !justWallJumped)
        {
            if (jumpTimeCounter > 0)
                jumpTimeCounter -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space))
            isJumping = false;

        if (health <= 0)
            gameOver();

        // stamina regenerieren
        if (!isConsumingStamina)
            staminaRegen();
        else
            staminaRegenTimer = staminaRegenCooldown;

        isConsumingStamina = false;

        if (staminaText != null)
            staminaText.text = "Stamina: " + Mathf.Round(stamina);

        if (healthText != null)
            healthText.text = "Health: " + Mathf.Round(health);

        // Animator
        bool isGrounded = canJump;
        bool inAir = !canJump;

        animator.SetBool("isWalking", move != 0f && isGrounded);
        animator.SetBool("isRunning", speed > 5f && move != 0f && isGrounded);
        animator.SetBool("isWallsliding", isTouchingWall);
        animator.SetBool("isJumping", inAir && !isTouchingWall);

        bool pressingS = Input.GetKey(KeyCode.S);
        bool fallingDown = rb.linearVelocity.y < -0.1f;
        bool doFastFall = pressingS && inAir && fallingDown && !isTouchingWall;
        animator.SetBool("isFastFalling", doFastFall);

        void playerAttack()
        {
            // attack später
        }

        void staminaRegen()
        {
            if (canJump || isTouchingWall)
            {
                staminaRegenTimer -= Time.deltaTime;

                if (staminaRegenTimer <= 0)
                {
                    stamina += 5;
                    staminaRegenTimer = staminaRegenCooldown;

                    if (stamina > 100)
                        stamina = 100;
                }
            }
        }

        void gameOver()
        {
            health = 10;
            stamina = 100;
            transform.position = new Vector3(-29f, 1.3f, 0f);
            spriteRenderer.color = Color.white;
            Debug.Log("Reset");
        }
    }

    void FixedUpdate()
    {
        // Bewegung
        if (canMove && !justWallJumped)
        {
            float currentSpeed = isDashing ? dashSpeed : speed;
            float dashDirection = spriteRenderer.flipX ? -1f : 1f;

            if (isDashing)
            {
                rb.linearVelocity = new Vector2(dashDirection * currentSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(move * currentSpeed, rb.linearVelocity.y);
            }
        }

        // Springen halten
        if (isJumping && Input.GetKey(KeyCode.Space) && !justWallJumped)
        {
            if (jumpTimeCounter > 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, holdForce);
        }

        // Fast Fall
        if (Input.GetKey(KeyCode.S) && !canJump && !isTouchingWall)
        {
            if (rb.linearVelocity.y > -fastFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallSpeed);
        }
    }

    private void ActivateParry()
    {
        if (parryObject == null) return;

        parryObject.SetActive(true);


        if (parryCoroutine != null)
            StopCoroutine(parryCoroutine);

        parryCoroutine = StartCoroutine(DisableParryAfterTime());
    }

    private IEnumerator DisableParryAfterTime()
    {
        yield return new WaitForSeconds(parryActiveTime);

        if (parryObject != null)
            parryObject.SetActive(false);

        parryCoroutine = null;
    }

    private void UseStamina(float amount)
    {
        stamina -= amount;
        if (stamina < 0f)
            stamina = 0f;
        isConsumingStamina = true;
    }

    private IEnumerator ReflectWindow()
    {
        isReflecting = true;
        yield return new WaitForSeconds(reflectWindow);
        isReflecting = false;
    }

    private IEnumerator ReenableCollision(Collider2D a, Collider2D b, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (a != null && b != null)
            Physics2D.IgnoreCollision(a, b, false);
    }

    IEnumerator DamageFlash()
    {
        isDamageFlashRunning = true;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.05f);

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        isDamageFlashRunning = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boden"))
        {
            canJump = true;
            isJumping = false;
            canMove = true;
            canSprint = true;
            justWallJumped = false;
            print("Spieler Berührt Boden");
        }

        if (collision.gameObject.CompareTag("Wand"))
        {
            canMove = true;
            isTouchingWall = true;
        }

        if (collision.gameObject.CompareTag("Bullet"))
        {
            if (isReflecting)
            {
                Rigidbody2D bulletRb = collision.rigidbody;
                if (bulletRb != null)
                {
                    Vector2 v = bulletRb.linearVelocity;

                    if (v.sqrMagnitude < 0.0001f)
                    {
                        v = spriteRenderer.flipX ? Vector2.left : Vector2.right;
                        bulletRb.linearVelocity = v * 10f;
                    }
                    else
                    {
                        bulletRb.linearVelocity = -v;
                    }

                    Collider2D bulletCol = collision.collider;
                    Collider2D playerCol = GetComponent<Collider2D>();
                    if (bulletCol != null && playerCol != null)
                    {
                        Physics2D.IgnoreCollision(bulletCol, playerCol, true);
                        StartCoroutine(ReenableCollision(bulletCol, playerCol, reflectIgnoreCollisionTime));
                    }

                    return;
                }
                return;
            }

            Destroy(collision.gameObject);
            health -= 1;
            if (!isDamageFlashRunning)
                StartCoroutine(DamageFlash());
        }

        if (collision.gameObject.CompareTag("Spike"))
        {
            stamina = 100;
            transform.position = new Vector3(-29f, 1.3f, 0f);
            rb.linearVelocity = Vector2.zero;
        }

        if (collision.gameObject.CompareTag("Dmg Spike"))
        {
            health -= 2;
            if (!isDamageFlashRunning)
                StartCoroutine(DamageFlash());
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            health -= 2;

            float direction = transform.position.x - collision.transform.position.x;
            direction = direction > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(direction * knockbackForce, knockbackForce * 0.3f);

            if (!isDamageFlashRunning)
                StartCoroutine(DamageFlash());
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boden"))
            canJump = true;

        if (collision.gameObject.CompareTag("Wand"))
        {
            isTouchingWall = true;
            if (rb.linearVelocity.y < -wallSlide)
                rb.linearVelocity = new Vector2(0, -wallSlide);
        }

        if (collision.gameObject.CompareTag("Enemy"))
            canJump = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boden"))
        {
            canJump = false;
            canSprint = false;
        }

        if (collision.gameObject.CompareTag("Wand"))
        {
            canMove = true;
            isTouchingWall = false;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            canJump = true;
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }

        if (collision.gameObject.CompareTag("Dmg Spike"))
        {
            canJump = true;
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }
    }
}
