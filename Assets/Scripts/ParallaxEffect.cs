using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Original from AdamCYounis https://www.youtube.com/watch?v=tMXgLBwtsvI
public class ParallaxEffect : MonoBehaviour
{
    public Camera camera;
    public Transform followTarget;

    //Starting position for the parallax game object
    Vector2 startingPosition;

    //Start Z value of the parallax game object
    float startingZ;

    //Distance that the camera has moved from the starting position of the parallax object
    Vector2 camMoveSinceStart => (Vector2)camera.transform.position - startingPosition;

    float zDistanceFromTarget => transform.position.z - followTarget.transform.position.z;

    float clippingPlane => (camera.transform.position.z + (zDistanceFromTarget > 0 ? camera.farClipPlane : camera.nearClipPlane));

    //The further the object from the player, the faster the ParallaxEffect object will move.  Drag its Z value closer to the target to make it move slower.
    float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clippingPlane;

    void Start()
    {
        startingPosition = transform.position;
        startingZ = transform.position.z;
    }

    void Update()
    {
        Vector2 newPosition = startingPosition + camMoveSinceStart * parallaxFactor;

        transform.position = new Vector3(newPosition.x, newPosition.y, startingZ);
    }
}
