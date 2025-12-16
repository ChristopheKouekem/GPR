using UnityEngine;
using UnityEditor;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.InputSystem;

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

    public TextMeshProUGUI staminaText;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPosition = transform.position;
    }

    void Update()
    {
        float move = 0f;

        // Bewegung
        if (canMove)
        {
            if (Input.GetKey(KeyCode.A))
                move = -1f;
            else if (Input.GetKey(KeyCode.D))
                move = 1f;

            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }

        // Attack
        if (Input.GetKey(KeyCode.J))
        {
            UseStamina(10f * Time.deltaTime);
            playerAttack();
        }

        // Sprinten
        // if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && canSprint)
        // {
        //     // if (stamina > 0.1f)
        //     // {
        //     //     speed = 10f;
        //     //     UseStamina(2f * Time.deltaTime);
        //     // }

        // }
        // Sprinten
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A) && stamina > 0 && canSprint || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.D) && stamina > 0 && canSprint)
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

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
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, holdForce);
                jumpTimeCounter -= Time.deltaTime;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
            isJumping = false;

        // Fast Fall
        if (Input.GetKey(KeyCode.S) && !canJump)
        {
            if (rb.linearVelocity.y > -fastFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallSpeed);
        }

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
                    stamina += 2;
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

    private void UseStamina(float amount)
    {
        stamina -= amount;
        if (stamina < 0f)
            stamina = 0f;
        isConsumingStamina = true;
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
            canJump = true; // an der Wand immer springen können
            isTouchingWall = true;
        }

        if (collision.gameObject.CompareTag("Spike"))
        {
            transform.position = new Vector3(-8f, 0f, 0f);
            rb.linearVelocity = Vector2.zero;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            health -= 2;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
            }
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
            if (rb.linearVelocity.y < -wallSlide)
                rb.linearVelocity = new Vector2(0, -wallSlide);
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
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.blue;
            }
        }
    }
}