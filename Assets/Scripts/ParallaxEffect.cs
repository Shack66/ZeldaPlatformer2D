using UnityEngine;
using UnityEngine.Tilemaps;

public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    // Background's image width 
    private float length;

    //Starting position for the parallax game object
    Vector2 startingPosition;

    //Start Z value of the parallax game object
    float startingZ;

    Vector2 camMoveSinceStart;

    float zDistanceFromTarget => transform.position.z - followTarget.transform.position.z;

    float clippingPlane => (cam.transform.position.z + (zDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane));

    // The further the object from the player, the faster the ParallaxEffect object will move. Drag it's Z value closer to the target to make it slower.
    public float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clippingPlane;

    //Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;
        startingZ = transform.position.z;

        // Get the sprite's size of the X axis (SpriteRenderer or TilemapRenderer)
        if (GetComponent<SpriteRenderer>() != null)
        {
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }
        else if (GetComponent<TilemapRenderer>() != null)
        {
            length = GetComponent<Tilemap>().size.x * GetComponent<Tilemap>().cellSize.x;
        }

        length = 21f;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        // Calculate how much the background has moved for the looping effect
        float temp = (cam.transform.position.x * (1 - parallaxFactor));

        // Infinite loop's logic
        if (temp > startingPosition.x + length)
        {
            startingPosition.x += length; // If it came out on the right, move the anchor to the right 
        }
        else if (temp < startingPosition.x - length)
        {
            startingPosition.x -= length; // If it came out on the left, move the anchor to the left 
        }

        // Distance that the camera has moved from the starting position of the parallax object
        camMoveSinceStart = (Vector2)cam.transform.position - startingPosition;

        // When the target moves, move the parallax object the same distance times a multiplier
        Vector2 newPosition = startingPosition + camMoveSinceStart * parallaxFactor;

        // The X/Y position changes based on a target travel speed times the parallax factor, but Z stays consistent
        transform.position = new Vector3(newPosition.x, newPosition.y, startingZ);

    }
}
