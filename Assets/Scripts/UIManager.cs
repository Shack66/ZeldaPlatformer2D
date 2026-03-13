using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;

    public Canvas gameCanvas;

    private void Awake()
    {
        gameCanvas = Object.FindFirstObjectByType<Canvas>();
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += (CharacterTookDamage);
        CharacterEvents.characterHealed += (CharacterHealed);
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= (CharacterTookDamage);
        CharacterEvents.characterHealed -= (CharacterHealed);

    }

    public void CharacterTookDamage(GameObject character, int damageReceived)
    {
        // Look for the collider to find the center 
        Collider2D charCollider = character.GetComponent<Collider2D>();
        Vector3 spawnPosWorld = charCollider != null ? charCollider.bounds.center : character.transform.position;

        // Convert to screen coordinates 
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(spawnPosWorld);

        GameObject textObj = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform);
        TMP_Text tmpText = textObj.GetComponent<TMP_Text>();

        if (tmpText != null)
        {
            tmpText.text = damageReceived.ToString();
        }

        // Random effect
        HealthText healthText = textObj.GetComponent<HealthText>();
        if (healthText != null)
        {
            // Some numbers will spawn to the left and others to the right
            float randomX = Random.Range(-100f, 100f); // Side push
            healthText.moveSpeed = new Vector3(randomX, Random.Range(100f, 150f), 0);
        }
    }

    public void CharacterHealed(GameObject character, int healthRestored) 
    {
        // Create text at character hit
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();

        tmpText.text = healthRestored.ToString();
    }
}
