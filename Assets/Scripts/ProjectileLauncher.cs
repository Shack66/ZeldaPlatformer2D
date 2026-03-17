using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public Transform launchPoint;
    public GameObject projectilePrefab;

    public void FireProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, projectilePrefab.transform.rotation);
        Vector3 origScale = projectile.transform.localScale;

        // Flip the projectiles' facing direction and movement based on the direction the character is facing at the time of launch
        projectile.transform.localScale = new Vector3(
            origScale.x * transform.localScale.x > 0 ? 6 : -6,
            origScale.y,
            origScale.z
            );
    }

}