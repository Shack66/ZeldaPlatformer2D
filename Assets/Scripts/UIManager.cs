using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;

    public Canvas gameCanvas;

    [Header("Player Health  UI")]
    public Slider playerHealthSlider;

    private void Awake()
    {
        if (gameCanvas == null)
        {
            gameCanvas = Object.FindFirstObjectByType<Canvas>();
        }
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

        // Verify if the character damaged is the Player (Link)
        if (character.CompareTag("Player") && playerHealthSlider != null)
        {
            Damageable playerHealth = character.GetComponent<Damageable>();

            if (playerHealth != null)
            {
                // Update Slider value
                playerHealthSlider.value = playerHealth.Health;
            }
        }
    }

    public void CharacterHealed(GameObject character, int healthRestored) 
    {
        // Create text at character hit
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);
        TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();
        tmpText.text = healthRestored.ToString();

        // Update Health Bar
        if (character.CompareTag("Player") && playerHealthSlider != null)
        {
            Damageable playerHealth = character.GetComponent<Damageable>();

            if (playerHealth != null)
            {
                // Update Slider value
                playerHealthSlider.value = playerHealth.Health;
            }
        }
    }

    public void OnExitGame(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
                Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            #endif

            #if (UNITY_EDITOR)
                UnityEditor.EditorApplication.isPlaying = false;
            #elif (UNITY_STANDALONE)
                Application.Quit();
            #elif (UNITY_WEBGL)
                SceneManager.LoadScene("QuitScene");
            #endif
        }
    }
}
