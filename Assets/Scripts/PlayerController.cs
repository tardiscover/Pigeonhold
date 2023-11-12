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
    [HideInInspector] public Ladder ladder;     //If isClimbing, this will be the ladder being climbed.  null otherwise.
    [SerializeField] float climbSpriteHeight = 0.585f;  //Made consistant regardless of sprite animation.  I started with the collider height.
    // ladder climbing variables
    float transformY;   //bottom of the player (assumes player pivot at the bottom)
    float transformHY;  //transform y + height of sprite - top of player
    bool isClimbingDown;
    bool atLaddersEnd;
    bool hasStartedClimbing;
    bool startedClimbTransition;
    bool finishedClimbTransition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        InitializeActionMaps();
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
            CheckIfClimbing();  //!!!
            if (IsClimbing)
            {
                rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, moveInput.y * CurrentMoveSpeed);
            }
            else
            {
                rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
            }
        }

        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

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

    //// reset our ladder climbing variables and 
    //// put back the animator speed and rigidbody type
    //void ResetClimbing()
    //{
    //    // reset climbing if we're climbing
    //    if (IsClimbing)
    //    {
    //        IsClimbing = false;
    //        atLaddersEnd = false;
    //        startedClimbTransition = false;
    //        finishedClimbTransition = false;
    //        animator.speed = 1;
    //        rb.bodyType = RigidbodyType2D.Dynamic;
    //        rb.velocity = Vector2.zero;
    //    }
    //}

    //// wrapper for the ClimbTransitionCo() coroutine below
    //void ClimbTransition(bool movingUp)
    //{
    //    StartCoroutine(ClimbTransitionCo(movingUp));
    //}

    //// climbing transition animation for when we move to the top of
    //// the ladder or when we move down from the top of it
    //private IEnumerator ClimbTransitionCo(bool movingUp)
    //{
    //    // we don't want any player input during this
    //    //!!!FreezeInput(true);

    //    // flag to signal we're not done performing the transition
    //    finishedClimbTransition = false;

    //    // there are two positions, going up and going down
    //    Vector3 newPos = Vector3.zero;
    //    if (movingUp)
    //    {
    //        // moving up we transition the top offset amount
    //        // (it looks like his body is half above the the ladder top)
    //        newPos = new Vector3(ladder.posX, transformY + ladder.handlerTopOffset, 0);
    //    }
    //    else
    //    {
    //        // moving down we first reposition our y (~position at the end of the moving up transition)
    //        // then we transition down the top offset amount so looks like we're climbing down from the top(ish)
    //        transform.position = new Vector3(ladder.posX, ladder.posTopHandlerY - climbSpriteHeight + ladder.handlerTopOffset, 0);
    //        newPos = new Vector3(ladder.posX, ladder.posTopHandlerY - climbSpriteHeight, 0);
    //    }

    //    while (transform.position != newPos)
    //    {
    //        // we are going to move towards the new position playing our other climb animation (the bent over look)
    //        transform.position = Vector3.MoveTowards(transform.position, newPos, climbSpeed * Time.deltaTime);
    //        animator.speed = 1;
    //        animator.Play("Player_ClimbTop");
    //        yield return null;
    //    }

    //    // done climbing down so those other code blocks can work again
    //    isClimbingDown = false;

    //    // now we're signaling that we finished the climb transition
    //    finishedClimbTransition = true;

    //    // give the player back their input
    //    //!!!FreezeInput(false);
    //}

    void CheckIfClimbing()
    {
        // start ladder climbing here
        if (ladder != null)
        {
            // climbing up
            //if (ladder.isNearLadder && keyVertical > 0 && transformHY < ladder.posTopHandlerY)
            if (ladder.isNearLadder && moveInput.y > 0 && transformHY < ladder.posTopHandlerY)    
            {
                IsClimbing = true;
                isClimbingDown = false;
                //!!!animator.speed = 0;
                rb.bodyType = RigidbodyType2D.Kinematic;
                //!!!rb.velocity = Vector2.zero;
                Debug.Log($"transformY = {transformY}, transformY + 0.025f = {transformY + 0.025f}");  //!!!!!!!
                //!!!transform.position = new Vector3(ladder.posX, transformY + 0.025f, 0);
                transform.position = new Vector3(ladder.posX, transform.position.y + 0.025f, 0);
                //!!!StartedClimbing();
            }

            // climbing down
            //if (ladder.isNearLadder && keyVertical < 0 && touchingDirections.IsGrounded && transformHY > ladder.posTopHandlerY)
            if (ladder.isNearLadder && moveInput.y < 0 && touchingDirections.IsGrounded && transformHY > ladder.posTopHandlerY)
            {
                IsClimbing = true;
                isClimbingDown = true;
                //!!!animator.speed = 0;
                rb.bodyType = RigidbodyType2D.Kinematic;
                //!!!rb.velocity = Vector2.zero;
                //transform.position = new Vector3(ladder.posX, transformY, 0);
                transform.position = new Vector3(ladder.posX, transform.position.y, 0);
                //!!!ClimbTransition(false);
            }
        }
        else
        {
            //!!!!!!!!!!!!
            if (IsClimbing)
            {
                //No ladder, stop climbing
                IsClimbing = false;
                isClimbingDown = false;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }
}
