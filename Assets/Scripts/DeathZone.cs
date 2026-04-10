using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verify if the Player (Link) fell
        if (collision.CompareTag("Player"))
        {
            Damageable playerDamageable = collision.GetComponent<Damageable>();

            if (playerDamageable != null)
            {
                playerDamageable.KillInstantly();
            }
        }
    }
}
