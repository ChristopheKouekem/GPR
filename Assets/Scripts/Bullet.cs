using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (other.CompareTag("Parry"))
        {
            Player player = other.GetComponentInParent<Player>();
            if (player != null && Input.GetKey(KeyCode.W))
            {

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


                Destroy(gameObject);
            }

        }
    }
}