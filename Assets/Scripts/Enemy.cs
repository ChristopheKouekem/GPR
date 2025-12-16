using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isChasing = false;
    public Transform player;
    public float speed = 3f;
    public float damage = 0.2f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isChasing && player != null)
        {
            // Berechne Richtung zum Player
            Vector3 direction = (player.position - transform.position).normalized;

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