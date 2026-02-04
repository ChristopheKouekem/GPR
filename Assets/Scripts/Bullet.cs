using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Zerstöre Bullet bei Wand, Boden, etc.
        if (collision.gameObject.CompareTag("Wand") || 
            collision.gameObject.CompareTag("Boden") || 
            collision.gameObject.CompareTag("Enemy") || 
            collision.gameObject.CompareTag("Spike") ||
            collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Prüfe ob es der Parry oder Block Collider ist
        if (other.CompareTag("Parry"))
        {
            Player player = other.GetComponentInParent<Player>();
            if (player != null && Input.GetKey(KeyCode.W))
            {
                // PARRY: Reflektiere die Bullet
                if (rb != null)
                {
                    rb.linearVelocity = -rb.linearVelocity;
                }
            }
        }
        else if (other.CompareTag("Block"))
        {
            Player player = other.GetComponentInParent<Player>();
            if (player != null && Input.GetKey(KeyCode.W))
            {
                // BLOCK: Zerstöre die Bullet
                Destroy(gameObject);
            }
        }
    }
}