using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 
/// <summary>
/// Adjust the Sprite used and the transform.z (which will affect how "distant" the background is and thus how fast it moves).
/// The sprite should initially be the width of one complete instance, but set to Draw Mode Tiled.
/// The could will note this original backgroundWidth, and then multiply the width by 3 so three instance are tiled.
/// This will allow the view to leave the center instance and then have the whole background seemlessly (and unnoticed) jump from a side instance to the matching position in the center one.
/// 
/// Remember to set the Sorting Layer to Background
/// 
/// Original from AdamCYounis https://www.youtube.com/watch?v=tMXgLBwtsvI
/// with modifications based on Chris' Tutorials https://www.youtube.com/watch?v=QHJlXSkwmjo
/// and Dani https://www.youtube.com/watch?v=zit45k6CUMk
/// </summary>
public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    //Starting position for the parallax game object
    Vector2 startingPosition;

    //Start Z value of the parallax game object
    float startingZ;

    //The width of the background sprite (just one instance, not counting tiling)
    private float backgroundWidth;

    private float parallaxEffect;

    //Distance that the camera has moved from the starting position of the parallax object
    private Vector2 GetCamMoveSinceStart()
    {
        return (Vector2)cam.transform.position - startingPosition;
    }

    private float GetZDistanceFromTarget()
    {
        return transform.position.z - followTarget.transform.position.z;
    }

    private float GetClippingPlane()
    {
        return (cam.transform.position.z + (GetZDistanceFromTarget() > 0 ? cam.farClipPlane : cam.nearClipPlane));
    }

    //The further the object from the player, the faster the ParallaxEffect object will move.  Drag its Z value closer to the target to make it move slower.
    private float GetParallaxFactor()
    {
        return Mathf.Abs(GetZDistanceFromTarget()) / GetClippingPlane();
    }

    void Start()
    {
        startingPosition = transform.position;  //Converts Vector3 to Vector2
        startingZ = transform.position.z;
        backgroundWidth = GetComponent<SpriteRenderer>().bounds.size.x;  //!!!
        parallaxEffect = GetParallaxFactor();   //!!! Since not changing

        //Stretch the sprite renderer width by 3x.  This results in 3 adjacent tiled images
        GetComponent<SpriteRenderer>().size = new Vector2(GetComponent<SpriteRenderer>().size.x * 3, GetComponent<SpriteRenderer>().size.y);
    }

    private void FixedUpdate()
    {
        float temp = cam.transform.position.x * (1.0f - parallaxEffect);
        float dist = cam.transform.position.x * parallaxEffect;

        transform.position = new Vector3(startingPosition.x + dist, startingPosition.y, startingZ);

        if (temp > startingPosition.x + backgroundWidth)
        {
            startingPosition.x += backgroundWidth;
        }
        else if (temp < startingPosition.x - backgroundWidth)
        {
            startingPosition.x -= backgroundWidth;
        }
    }
}
