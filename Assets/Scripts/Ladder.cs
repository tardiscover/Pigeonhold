using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for a climbable ladder.
/// Partially from https://www.youtube.com/watch?v=xoK0mPISY08, 
/// with some original code for tiling the (middle) ladder graphic for the GameObject's height.
/// </summary>
public class Ladder : MonoBehaviour
{
    //Temporary colored pixels that can be moved to see positions during play, such as the box collider corners.
    public Transform pixel1;
    public Transform pixel2;
    public Transform pixel3;
    public Transform pixel4;
    public Transform pixel5;

    //Sprite sections of the ladder.  LadderMid resizes, tiling the graphic.
    public RectTransform topSpriteRectTransform;
    public RectTransform midSpriteRectTransform;
    public RectTransform btmSpriteRectTransform;

    //Markers for changes in player behavior
    public Transform topStep;
    public Transform btmStep;
    public Transform platform;

    //Fields for resizing ladder image and collider
    private RectTransform rectTransform;
    private SpriteRenderer midSpriteRenderer;
    private BoxCollider2D ladderBoxCollider2D;

    //These offsets are for reaching the top (climb up)
    //and bottom (to let go and fall off) of the ladder
    public float handlerTopOffset = 0.04f;
    public float handlerBottomOffset = 0.04f;
    public float platform_offset = -0.04f;   //!!!

    private float nearEnoughToLadderCenter = 0.1f;

    // flag to let the player know that climbing is possible
    [HideInInspector] public bool isNearLadder;

    //These positions are for the player's ladder climbing logic
    [HideInInspector] public float posX;
    [HideInInspector] public float posTopY;
    [HideInInspector] public float posBottomY;
    [HideInInspector] public float posTopHandlerY;
    [HideInInspector] public float posBottomHandlerY;
    [HideInInspector] public float posPlatformY;

    void Awake()
    {
        //Init fields for resizing ladder image and collider
        rectTransform = GetComponent<RectTransform>();
        midSpriteRenderer = midSpriteRectTransform.GetComponent<SpriteRenderer>();
        ladderBoxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        ResizeLadderMid();
        InitPositioning();  //Must be after ResizeLadderMid
    }

    /// <summary>
    /// Resizes the middle portion of the ladder as a tiled sprite so that it fills the gap in the overall ladder height.
    /// </summary>
    void ResizeLadderMid()
    {
        float topSpriteTop = topSpriteRectTransform.localPosition.y - topSpriteRectTransform.rect.height;
        float topSpriteBtm = midSpriteRectTransform.localPosition.y;
        float newMidSpriteHeight = topSpriteTop - topSpriteBtm;
        float newColliderHeight = rectTransform.rect.height - 0.5f;
        float newColliderYOffset = newColliderHeight / 2.0f;

        midSpriteRenderer.size = new Vector2(midSpriteRenderer.size.x, newMidSpriteHeight);    //!!!!!!

        ladderBoxCollider2D.size = new Vector2(ladderBoxCollider2D.size.x, newColliderHeight);
        ladderBoxCollider2D.offset = new Vector2(ladderBoxCollider2D.offset.x, newColliderYOffset);

        //Show the bounds of the box collider
        pixel1.position = new Vector3(ladderBoxCollider2D.bounds.min.x, ladderBoxCollider2D.bounds.max.y, 0f);
        pixel2.position = new Vector3(ladderBoxCollider2D.bounds.min.x, ladderBoxCollider2D.bounds.min.y, 0f);
        pixel3.position = new Vector3(ladderBoxCollider2D.bounds.max.x, ladderBoxCollider2D.bounds.min.y, 0f);
        pixel4.position = new Vector3(ladderBoxCollider2D.bounds.max.x, ladderBoxCollider2D.bounds.max.y, 0f);
        pixel5.position = ladderBoxCollider2D.bounds.center;

    }

    private void InitPositioning()
    {
        // set up the positioning
        topStep.position = new Vector3(transform.position.x, transform.position.y + Mathf.Floor(rectTransform.rect.height - 0.1f), 0);  //This assumes the effective "top step" of the ladder is the integer just below the sprite's apparent top.
        btmStep.position = new Vector3(transform.position.x, transform.position.y, 0);  //transform position of ladder is center bottom, so is bottom step
        platform.position = new Vector3(topStep.position.x, topStep.position.y + platform_offset, 0);   //platform_offset is likely negative

        // set up the variables for the climber to use
        posX = transform.position.x;
        posTopY = topStep.transform.position.y;
        posBottomY = btmStep.transform.position.y;
        posPlatformY = platform.transform.position.y;
        posTopHandlerY = posTopY + handlerTopOffset;
        posBottomHandlerY = posBottomY + handlerBottomOffset;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // only ladder climber we have is the player
        if (other.gameObject.CompareTag("Player"))
        {
            // if the player is within the range (center-ish of the ladder)
            isNearLadder = (other.gameObject.transform.position.x > (posX - nearEnoughToLadderCenter) && other.gameObject.transform.position.x < (posX + nearEnoughToLadderCenter));  //Close to horiz center of ladder
            other.gameObject.GetComponent<PlayerController>().ladder = this;
            Debug.Log($"Ladder OnTriggerStay2D, isNearLadder = {isNearLadder}");    //!!!
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // outside of the trigger then no ladder to climb
            isNearLadder = false;
            other.gameObject.GetComponent<PlayerController>().ladder = null;
            Debug.Log($"Ladder OnTriggerExit2D, isNearLadder = {isNearLadder}");    //!!!
        }
    }
}
