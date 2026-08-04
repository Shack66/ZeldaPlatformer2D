using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool _activated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_activated && collision.CompareTag("Player"))
        {
            _activated = true;
            // Saves the checkpoint's position
            GameOverManager.lastCheckpointPosition = transform.position;
            GameOverManager.hasCheckpoint = true;

            Debug.Log("Checkpoint!");
        }
    }
}