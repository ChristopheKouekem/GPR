using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    public GameObject parryCollider;  // Der innere Collider
    public GameObject destroyCollider; // Der äußere Collider

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bullet") && Input.GetKey(KeyCode.W))
        {
            if (gameObject == parryCollider)
            {

                Rigidbody2D bulletRb = other.gameObject.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.linearVelocity = -bulletRb.linearVelocity * 1.5f;
                }
            }
            else if (gameObject == destroyCollider)
            {

                Destroy(other.gameObject);
            }
        }

    }
}