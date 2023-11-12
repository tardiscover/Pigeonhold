using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The Player Controller.
/// Based on https://www.youtube.com/playlist?list=PLyH-qXFkNSxmDU8ddeslEAtnXIDRLPd_V
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airWalkSpeed = 3f;
    public float climbSpeed = 1f;
    public float jumpImpulse = 10f;
    Vector2 moveInput;
    TouchingDirections touchingDirections;
    Damageable damageable;
    PlayerInput playerInput;
    Collider2D playerCollider;
    float playerTopOffset;   //The offset needed to add to the transform.position.y to get the playerTop.  Precalced to make calc of playerTop faster.
    float playerBtmOffset;   //The offset needed to add to the transform.position.y to get the playerBtm.  Precalced to make calc of playerBtm faster.
    bool freezeInput;

    public float CurrentMoveSpeed
    {
        get
        {
            if (CanMove)
            {
                if (IsMoving && !touchingDirections.IsOnWall)
                {
                    if (IsClimbing) //!!!Doesn't matter if IsGrounded?
                    {
                        return climbSpeed;
                    }
                    else
                    {
                        if (touchingDirections.IsGrounded)
                        {
                            if (IsRunning)
                            {
                                return runSpeed;
                            }
                            else
                            {
                                return walkSpeed;
                            }
                        }
                        else
                        {
                            //Air Move
                            return airWalkSpeed;
                        }
                    }
                }
                else
                {
                    return 0f;  //idle speed
                }
            }
            else
            {
                return 0f;  //movement locked
            }
        }
    }

    [SerializeField]
    private bool _isMoving = false;
    public bool IsMoving { 
        get 
        {
            return _isMoving;
        }
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, _isMoving);
        }
    }

    [SerializeField]
    private bool _isRunning = false;
    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
        private set
        {
            _isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value);
        }
    }

    [SerializeField]
    private bool _isClimbing = false;
    public bool IsClimbing
    {
        get
        {
            return _isClimbing;
        }
        private set
        {
            _isClimbing = value;
            animator.SetBool(AnimationStrings.isClimbing, value);
        }
    }

    public bool _isFacingRight = true;
    public bool IsFacingRight
    {
        get
        {
            return _isFacingRight;
        }
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
            }

            _isFacingRight = value;
        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public bool IsAlive
    {
        get
        {
            return animator.GetBool(AnimationStrings.isAlive);
        }
    }

    Rigidbody2D rb;
    Animator animator;

    [Header("Ladder Settings")]
    [HideInInspector] public Ladder ladder;     //If the player isNearLadder (for this ladder, this will be the ladder to be/being climbed.  null otherwise.
    [SerializeField] float climbSpriteHeight = 0.585f;  //Made consistant regardless of sprite animation.  I started with the collider height.  //!!!Used?
    // ladder climbing variables
    float playerBtm;    //bottom of the player (Renamed from transformY, which assumed player pivot at the bottom.)
    float playerTop;    //top of the player (Renamed from transformHY, which assumed player pivot at the bottom.)
    [SerializeField] bool isClimbingDown;
    [SerializeField] bool atLaddersEnd;
    [SerializeField] bool hasStartedClimbing;
    [SerializeField] bool startedClimbTransition;
    [SerializeField] bool finishedClimbTransition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
        playerInput = GetComponent<PlayerInput>();
        playerCollider = GetComponent<Collider2D>();    //!!!
    }

    private void Start()
    {
        CalcOffsets();
        InitializeActionMaps();
    }

    /// <summary>
    /// Precalculate offsets to make repeated calc of playerBtm and playerTop faster later.
    /// </summary>
    private void CalcOffsets()
    {
        playerBtmOffset = playerCollider.bounds.min.y - transform.position.y;
        playerTopOffset = playerCollider.bounds.max.y - transform.position.y;
    }

    /// <summary>
    /// Initializes the action maps for this player.  Note in this case we allow both the Player and UI ActionMaps simultaneously for the player
    /// on whatever device is assigned to them.  DON"T just create a second PlayerInput for the UI ActionMap, because this will cause issues when 
    /// there is more than one device.  (Each "player" will be assigned only one device.)
    /// https://www.youtube.com/watch?v=NZBAr_V7r0M
    /// </summary>
    private void InitializeActionMaps()
    {
        playerInput.actions.FindActionMap("Player").Enable();
        playerInput.actions.FindActionMap("UI").Enable();

        //Note actual code for UI is in UIManager script.
    }

    private void FixedUpdate()
    {
        if (!damageable.LockVelocity)
        {
            //These are for the ladder climbing but can be used elsewhere
            //Must be called before CheckIfClimbing().
            //transformY = transform.position.y;  //!!! Intended to be the bottom of the sprite.  Renamed to playerBtm.
            //transformHY = transformY + climbSpriteHeight;  //!!! Intended to be the top of the sprite.  Renamed to playerTop.
            playerTop = transform.position.y + playerTopOffset;
            playerBtm = transform.position.y + playerBtmOffset;

            CheckIfClimbing();  //!!!
            if (IsClimbing)
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Create subs

                Debug.Log($"playerBtmOffset = {playerCollider.bounds.min.y - transform.position.y}, playerTopOffset = {playerCollider.bounds.max.y - transform.position.y}");  //!!!

                // debug lines for our ladder handling  //!!!!!
                Debug.DrawLine(new Vector3(ladder.posX - 2f, ladder.posTopHandlerY, 0),
                    new Vector3(ladder.posX + 2f, ladder.posTopHandlerY, 0), Color.blue);
                Debug.DrawLine(new Vector3(ladder.posX - 2f, ladder.posBottomHandlerY, 0),
                    new Vector3(ladder.posX + 2f, ladder.posBottomHandlerY, 0), Color.blue);
                Debug.DrawLine(new Vector3(transform.position.x - 2f, playerTop, 0),
                    new Vector3(transform.position.x + 2f, playerTop, 0), Color.magenta);
                Debug.DrawLine(new Vector3(transform.position.x - 2f, playerBtm, 0),
                    new Vector3(transform.position.x + 2f, playerBtm, 0), Color.magenta);

                //// we just passed the top ladder handler position
                //if (playerTop > ladder.posTopHandlerY)
                //{
                //    // this should only happen when we're not climbing down
                //    // otherwise we get some real funky results!
                //    if (!isClimbingDown)
                //    {
                //        // start the climb transition animation
                //        if (!startedClimbTransition)
                //        {
                //            startedClimbTransition = true;
                //            ClimbTransition(true);
                //        }
                //        else if (finishedClimbTransition)
                //        {
                //            // we only want this block to happen once
                //            finishedClimbTransition = false;

                //            // we may not be completely touching the ground so setting
                //            // this to false will stop the jump landed audio clip
                //            //!!!isJumping = false;

                //            // climb transition has finished now reposition ourself
                //            // we kind of dip into the ground so we pad a little on our new y
                //            //!!!animator.Play("Player_Idle");
                //            transform.position = new Vector2(ladder.posX, ladder.posPlatformY + 0.005f);

                //            //!!!!!!!!!
                //            // at the top of the ladder
                //            if (!atLaddersEnd)
                //            {
                //                // reset climbing after a short delay
                //                // gives the rigidbody and ground check to settle
                //                atLaddersEnd = true;
                //                Invoke("ResetClimbing", 0.1f);  //Calls ResetClimbing() after 0.1 second delay
                //                Debug.Log($"***atLaddersEnd");    //!!!!!!!!!!!
                //            }
                //        }
                //    }
                //}
                //else if (playerBtm < ladder.posBottomHandlerY)
                //{
                //    // reaching this point means we have gone below of bottom handler
                //    // and haven't touched the ground so we should let go of the ladder
                //    ResetClimbing();
                //    Debug.Log($"***ResetClimbing! playerBtm = {playerBtm} + ladder.posBottomHandlerY = {ladder.posBottomHandlerY}");    //!!!!!!!!!!!
                //}
                ////else
                ////{
                ////    // this should only happen when we're not climbing down
                ////    // otherwise we get some real funky results!
                ////    if (!isClimbingDown)
                ////    {
                ////        // jump off the ladder as long as there is no vertical input
                ////        //!!!if (keyJump && keyVertical == 0)
                ////        if (moveInput.y == 0)
                ////        {
                ////            ResetClimbing();
                ////        }
                ////        // reached the ground by climbing down
                ////        else if (touchingDirections.IsGrounded && !hasStartedClimbing)
                ////        {
                ////            // we may not be completely touching the ground so setting
                ////            // this to false will stop the jump landed audio clip
                ////            //!!!isJumping = false;

                ////            // climbing has finished and now reposition ourself
                ////            // we kind of dip into the ground so we shave a little off our new y
                ////            animator.Play("Player_Idle");
                ////            transform.position = new Vector2(ladder.posX, ladder.posBottomY - 0.005f);

                ////            // at the bottom of the ladder
                ////            if (!atLaddersEnd)
                ////            {
                ////                // reset climbing after a short delay
                ////                // gives the rigidbody and ground check to settle
                ////                atLaddersEnd = true;
                ////                Invoke("ResetClimbing", 0.1f);
                ////            }
                ////        }
                ////        // somewhere in between the top and bottom of the ladder
                ////        else
                ////        {
                ////            // animate if we're moving in either direction
                ////            animator.speed = Mathf.Abs(moveInput.y);

                ////            // move on the ladder as long as we're not shooting/throwing
                ////            //!!!if (moveInput.y != 0 && !isShooting)
                ////            if (moveInput.y != 0)
                ////            {
                ////                // apply the direction and climb speed to our position
                ////                Vector3 climbDirection = new Vector3(0, climbSpeed) * moveInput.y;
                ////                transform.position = transform.position + climbDirection * Time.deltaTime;
                ////            }

                ////            //// if we're shooting or throwing then we can change our horizontal direction
                ////            //if (isShooting || isThrowing)
                ////            //{
                ////            //    // update the facing direction
                ////            //    if (moveInput.x < 0)
                ////            //    {
                ////            //        // facing right while shooting left - flip
                ////            //        if (isFacingRight)
                ////            //        {
                ////            //            Flip();
                ////            //        }
                ////            //    }
                ////            //    else if (moveInput.x > 0)
                ////            //    {
                ////            //        // facing left while shooting right - flip
                ////            //        if (!isFacingRight)
                ////            //        {
                ////            //            Flip();
                ////            //        }
                ////            //    }
                ////            //    // and then choose which animation to play
                ////            //    if (isShooting)
                ////            //    {
                ////            //        // play the shooting climb animation
                ////            //        animator.Play("Player_ClimbShoot");
                ////            //    }
                ////            //    else if (isThrowing)
                ////            //    {
                ////            //        // play the throwing climb animation
                ////            //        animator.Play("Player_ClimbThrow");
                ////            //    }
                ////            //}
                ////            //else
                ////            //{
                ////            //    // not shooting or throwing then we play
                ////            //    // the regular climbing animation
                ////            //    animator.Play("Player_Climb");
                ////            //}
                ////        }
                ////    }
                ////}

                //Animate (or don't) based on movement speed (whether vertical or horizontal)
                animator.speed = Mathf.Abs(moveInput.magnitude);
                rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, moveInput.y * CurrentMoveSpeed);
            }
            else
            {
                //Only horizontal velocity from input.  Vertical velocity from gravity or whatever.
                rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
            }
        }

        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (freezeInput)
        {
            moveInput = Vector2.zero;   //Move controls are temporarily frozen while preprogrammed behavior happens
            //!!! debug freezeInput works properly for other inputs
        }
        else
        {
            moveInput = context.ReadValue<Vector2>();
        }

        if (IsAlive)
        {
            IsMoving = moveInput != Vector2.zero;

            SetFacingDirection(moveInput);
        }
        else
        {
            IsMoving = false;
        }
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            //Face the right
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            //Face the left
            IsFacingRight = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
        }
        else if(context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        //TODO Check if alive as well
        if (context.started && touchingDirections.IsGrounded && CanMove)
        {
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log($"OnAttack");   //!!!!!!!!!
        //TODO Check if alive as well
        if (context.started)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);
        }
    }

    public void OnRangedAttack(InputAction.CallbackContext context)
    {
        Debug.Log($"OnRangedAttack");   //!!!!!!!!!
        //TODO Check if alive as well
        if (context.started)
        {
            animator.SetTrigger(AnimationStrings.rangedAttackTrigger);
        }
    }


    public void OnHit(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
    }

    //---------------------------------------------------

    //// wrapper for the StartedClimbingCo() coroutine below
    //void StartedClimbing()
    //{
    //    StartCoroutine(StartedClimbingCo());
    //}

    //// started climbing coroutine
    //// (this gives us a delay from the ground check giving false positives)
    //private IEnumerator StartedClimbingCo()
    //{
    //    hasStartedClimbing = true;
    //    yield return new WaitForSeconds(0.1f);
    //    hasStartedClimbing = false;
    //}

    // wrapper for the ClimbTransitionCo() coroutine below
    void ClimbTransition(bool movingUp)
    {
        StartCoroutine(ClimbTransitionCo(movingUp));
    }

    // climbing transition animation for when we move to the top of
    // the ladder or when we move down from the top of it
    private IEnumerator ClimbTransitionCo(bool movingUp)
    {
        Debug.Log("ClimbTransitionCo"); //!!!!!!!!!
        // we don't want any player input during this
        FreezeInput(true);

        // flag to signal we're not done performing the transition
        finishedClimbTransition = false;

        // there are two positions, going up and going down
        Vector3 newPos = Vector3.zero;
        if (movingUp)
        {
            // moving up we transition the top offset amount
            // (it looks like his body is half above the the ladder top)
            newPos = new Vector3(ladder.posX, playerBtm + ladder.handlerTopOffset, 0);
        }
        else
        {
            // moving down we first reposition our y (~position at the end of the moving up transition)
            // then we transition down the top offset amount so looks like we're climbing down from the top(ish)
            transform.position = new Vector3(ladder.posX, ladder.posTopHandlerY - climbSpriteHeight + ladder.handlerTopOffset, 0);
            newPos = new Vector3(ladder.posX, ladder.posTopHandlerY - climbSpriteHeight, 0);
        }

        while (transform.position != newPos)
        {
            // we are going to move towards the new position playing our other climb animation (the bent over look)
            transform.position = Vector3.MoveTowards(transform.position, newPos, climbSpeed * Time.deltaTime);
            animator.speed = 1;
            //!!!animator.Play("Player_ClimbTop");
            yield return null;
        }

        // done climbing down so those other code blocks can work again
        isClimbingDown = false;

        // now we're signaling that we finished the climb transition
        finishedClimbTransition = true;

        // give the player back their input
        FreezeInput(false);
    }

    void CheckIfClimbing()
    {
        // start ladder climbing here
        if (ladder != null)
        {
            Debug.Log($"CheckIfClimbing: isNearLadder = {ladder.isNearLadder}, moveInput.y = {moveInput.y}, playerTop = {playerTop}, ladder.posTopHandlerY = {ladder.posTopHandlerY}");
            // climbing up
            //if (ladder.isNearLadder && keyVertical > 0 && playerTop < ladder.posTopHandlerY)
            if (ladder.isNearLadder && moveInput.y > 0 && playerTop < ladder.posTopHandlerY)    
            {
                IsClimbing = true;
                isClimbingDown = false;
                //!!!animator.speed = 0;
                rb.bodyType = RigidbodyType2D.Kinematic;
                //!!!rb.velocity = Vector2.zero;
                Debug.Log($"transformY = {playerBtm}, transformY + 0.025f = {playerBtm + 0.025f}");  //!!!!!!!
                //!!!transform.position = new Vector3(ladder.posX, playerBtm + 0.025f, 0);
                transform.position = new Vector3(ladder.posX, transform.position.y + 0.025f, 0);
                //!!!StartedClimbing();
            }

            // climbing down
            //if (ladder.isNearLadder && keyVertical < 0 && touchingDirections.IsGrounded && playerTop > ladder.posTopHandlerY)
            if (ladder.isNearLadder && moveInput.y < 0 && touchingDirections.IsGrounded && playerTop > ladder.posTopHandlerY)
            {
                IsClimbing = true;
                isClimbingDown = true;
                //!!!animator.speed = 0;
                rb.bodyType = RigidbodyType2D.Kinematic;
                //!!!rb.velocity = Vector2.zero;
                //transform.position = new Vector3(ladder.posX, playerBtm, 0);
                transform.position = new Vector3(ladder.posX, transform.position.y, 0);
                //!!!ClimbTransition(false);
            }
        }
        else
        {
            ResetClimbing();
        }
    }

    //Reset our ladder climbing variables and 
    //put back the animator speed and rigidbody type
    void ResetClimbing()
    {
        // reset climbing if we're climbing
        if (IsClimbing)
        {
            IsClimbing = false;
            atLaddersEnd = false;
            startedClimbTransition = false;
            finishedClimbTransition = false;
            animator.speed = 1;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = Vector2.zero;
        }
    }

    public void FreezeInput(bool freeze)
    {
        // freeze/unfreeze user input
        freezeInput = freeze;
        //if (freeze)
        //{
        //    keyHorizontal = 0;
        //    keyVertical = 0;
        //    keyJump = false;
        //    keyShoot = false;
        //}
    }
}
