using UnityEngine;
using Unity.Cinemachine;

public class ArenaLock : MonoBehaviour
{
    [Header("Walls")]
    [SerializeField] private GameObject _entranceWall;
    [SerializeField] private GameObject _exitWall;

    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera _explorationCamera;
    [SerializeField] private CinemachineCamera _bossCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Only Link can activate the closure
        {
            ActivateLock();
        }
    }

    private void ActivateLock()
    {
        // Activate physic walls so that Link can't go back
        _entranceWall.SetActive(true);
        if (_exitWall != null)
        {
            _exitWall.SetActive(true);
        }

        // Activate the bossCamera by increasing its priority
        if (_bossCamera != null)
        {
            _bossCamera.Priority = 2;
        }

        // Deactivate the BoxCollider2D trigger so it don't execute more times
        GetComponent<BoxCollider2D>().enabled = false;
    }
}