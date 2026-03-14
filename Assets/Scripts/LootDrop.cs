using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UIElements;

public class LootDrop : MonoBehaviour
{
    [SerializeField]
    GameObject tinyHeart;

    [SerializeField]
    float chance = 60;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Damageable damage = GetComponent<Damageable>();
        damage.characterDeath.AddListener(DropHeart);
    }

    private void DropHeart(GameObject enemy, Vector2 position)
    {
        int randomNumber = Random.Range(0, 100);
        Debug.Log("Número generado: " + randomNumber);

        if (randomNumber <= chance)
        {
            GameObject heartInstance = Instantiate(tinyHeart, position, Quaternion.identity);
            heartInstance.transform.SetParent(null);

            Rigidbody2D rb = heartInstance.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 heartImpulse = new Vector2(0, 5);
                rb.AddForce(heartImpulse, ForceMode2D.Impulse);
            }
        }
    }
}
