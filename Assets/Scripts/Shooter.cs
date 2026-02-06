using UnityEngine;

public class SimpleShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public int direction = -1;
    public float shootInterval = 2f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(bulletSpeed * direction, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}