using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isChasing = false;
    public Transform player;
    public float speed = 3f;
    public float damage = 0.2f;
    public float knockbackForce = 3f;
    public float knockbackStopTime = 0.3f;

    private Rigidbody2D rb;
    private Vector3 direction;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
            }
            return;
        }

        if (isChasing && player != null)
        {
            // Berechne Richtung zum Player
            direction = (player.position - transform.position).normalized;
        }
        else
        {
            direction = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (isKnockedBack)
        {
            return;
        }

        if (isChasing && player != null)
        {
            // Bewege Enemy in Richtung Player
            rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
        }
        else
        {
            // Stoppe Bewegung wenn nicht am Verfolgen
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player playerScript = collision.gameObject.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.health -= damage;
            }

            float direction = transform.position.x - collision.transform.position.x;
            direction = direction > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(direction * knockbackForce, 0);

            isKnockedBack = true;
            knockbackTimer = knockbackStopTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = true;
            player = other.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            player = null;
        }
    }
}