using UnityEngine;
using UnityEditor;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isJumping = false;
    private bool canJump = true;
    private bool isTouchingWall = false;
    public bool canMove = true;
    public bool canAttack = true;
    public bool canSprint = false;
    private float jumpTimeCounter;
    private float staminaRegenTimer = 0.2f;
    private float staminaRegenCooldown = 0.2f;
    private bool isConsumingStamina = false;

    private float move = 0f;

    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI healthText;


    private bool isDamageFlashRunning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPosition = transform.position;
    }

    void Update()
    {
        move = 0f;

        // Bewegung Input
        if (canMove)
        {
            if (Input.GetKey(KeyCode.A))
                move = -1f;
            else if (Input.GetKey(KeyCode.D))
                move = 1f;
        }

        // Attack
        if (Input.GetKey(KeyCode.J))
        {
            UseStamina(10f * Time.deltaTime);
            playerAttack();
        }

        // Sprinten
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A) && stamina > 0 && canSprint ||
            Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.D) && stamina > 0 && canSprint)
        {
            if (stamina > 0.1f)
            {
                speed = 10f;
                UseStamina(2f * Time.deltaTime);
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = 5f;
        }

        // Springen
        if (Input.GetKeyDown(KeyCode.Space) && (canJump || isTouchingWall) && stamina >= 15)
        {
            UseStamina(5f);
            isJumping = true;
            jumpTimeCounter = maxJumpTime;

            if (!isTouchingWall)
            {
                canJump = false;
                canSprint = false;
            }
        }

        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                jumpTimeCounter -= Time.deltaTime;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
            isJumping = false;

        // Health Check
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

        void playerAttack()
        {
            // attack später
        }

        void staminaRegen()
        {
            if (canJump)
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
            Application.Quit();
            EditorApplication.ExitPlaymode();
            Debug.Log("Vorbei");
        }
    }

    void FixedUpdate()
    {
        // Bewegung
        if (canMove)
        {
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }

        // Springen
        if (isJumping && Input.GetKey(KeyCode.Space))
        {
            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, holdForce);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space) && (canJump || isTouchingWall) && stamina >= 15)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Fast Fall
        if (Input.GetKey(KeyCode.S) && !canJump)
        {
            if (rb.linearVelocity.y > -fastFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallSpeed);
        }
    }

    private void UseStamina(float amount)
    {
        stamina -= amount;
        if (stamina < 0f)
            stamina = 0f;
        isConsumingStamina = true;
    }

    IEnumerator DamageFlash()
    {
        isDamageFlashRunning = true;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.05f);

        if (spriteRenderer != null)
            spriteRenderer.color = Color.blue;

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
            print("Spieler Berührt Boden");
        }

        if (collision.gameObject.CompareTag("Wand"))
        {
            canMove = true;
            canJump = true;
            isTouchingWall = true;
        }

        if (collision.gameObject.CompareTag("Spike"))
        {
            stamina = 100;
            transform.position = new Vector3(-8f, 0f, 0f);
            rb.linearVelocity = Vector2.zero;
        }

        if (collision.gameObject.CompareTag("Dmg Spike"))
        {
            health -= 1;
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
        {
            canJump = true;
        }

        if (collision.gameObject.CompareTag("Wand"))
        {
            canSprint = true;
            if (rb.linearVelocity.y < -wallSlide)
                rb.linearVelocity = new Vector2(0, -wallSlide);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            canJump = true;
        }
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
            canSprint = false;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            canJump = true;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.blue;
            }
        }
    }
}