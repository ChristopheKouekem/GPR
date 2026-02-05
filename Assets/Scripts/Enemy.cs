using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private bool isChasing = false;
    public Transform player;

    public float speed = 3f;
    public float damage = 0.2f;
    public float knockbackForce = 3f;
    public float knockbackStopTime = 0.3f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector3 direction;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        // ✅ Start immer Idle
        isChasing = false;
        player = null;
        animator.SetBool("isRolling", false);
    }

    void Update()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
                isKnockedBack = false;

            animator.SetBool("isRolling", false);
            return;
        }

        if (isChasing && player != null)
        {
            direction = (player.position - transform.position).normalized;

            // ✅ FLIP HIER (richtiger Spot)
            if (direction.x > 0.01f)
                sr.flipX = false; // schaut nach rechts
            else if (direction.x < -0.01f)
                sr.flipX = true;  // schaut nach links
        }
        else
        {
            direction = Vector3.zero;
        }

        // ✅ Roll-Animation nur beim Chasen
        animator.SetBool("isRolling", isChasing && player != null);
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;

        if (isChasing && player != null)
        {
            rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Player playerScript = collision.gameObject.GetComponent<Player>();
        if (playerScript != null)
            playerScript.health -= damage;

        float dir = transform.position.x - collision.transform.position.x;
        dir = dir > 0 ? 1 : -1;

        rb.linearVelocity = new Vector2(dir * knockbackForce, 0);

        isKnockedBack = true;
        knockbackTimer = knockbackStopTime;

        animator.SetBool("isRolling", false);

        if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isChasing = true;
        player = other.transform;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isChasing = false;
        player = null;
    }
}

