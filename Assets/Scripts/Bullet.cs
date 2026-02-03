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
        print("Collided with " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Wand"))
        {
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Boden"))
        {
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Spike"))
        {
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        print("Triggered with " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Block") && Input.GetKey(KeyCode.W))
        {
            Destroy(gameObject);
        }
    }


}