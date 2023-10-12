using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Original from AdamCYounis https://www.youtube.com/watch?v=tMXgLBwtsvI
public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    //Starting position for the parallax game object
    Vector2 startingPosition;

    //Start Z value of the parallax game object
    float startingZ;

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
    }

    private void FixedUpdate()
    {
        Vector2 newPosition = startingPosition + GetCamMoveSinceStart() * GetParallaxFactor();

        transform.position = new Vector3(newPosition.x, newPosition.y, startingZ);
    }
}
