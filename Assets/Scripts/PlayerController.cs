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
    public bool MenuOpenInput { get; private set; }

    Vector2 moveInput;
    TouchingDirections touchingDirections;
    Damageable damageable;
    PlayerInput playerInput;
    Collider2D playerCollider;
    float playerTopOffset;   //The offset needed to add to the transform.position.y to get the playerTop.  Precalced to make calc of playerTop faster.
    float playerBtmOffset;   //The offset needed to add to the transform.position.y to get the playerBtm.  Precalced to make calc of playerBtm faster.

    private InputAction menuOpenAction;

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
    //!!![SerializeField] float climbSpriteHeight = 0.585f;  //Made consistant regardless of sprite animation.  I started with the collider height.  //!!!Used?
    // ladder climbing variables
    float playerBtm;    //bottom of the player (Renamed from transformY, which assumed player pivot at the bottom.)
    float playerTop;    //top of the player (Renamed from transformHY, which assumed player pivot at the bottom.)
    //!!![SerializeField] bool isClimbingDown;
    //!!![SerializeField] bool atLaddersEnd;
    [SerializeField] bool hasStartedClimbing;
    //!!![SerializeField] bool startedClimbTransition;
    //!!![SerializeField] bool finishedClimbTransition;

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
    }

    /// <summary>
    /// Precalculate offsets to make repeated calc of playerBtm and playerTop faster later.
    /// </summary>
    private void CalcOffsets()
    {
        playerBtmOffset = playerCollider.bounds.min.y - transform.position.y;
        playerTopOffset = playerCollider.bounds.max.y - transform.position.y;
    }

    private void FixedUpdate()
    {
        if (!damageable.LockVelocity)
        {
            //These are for the ladder climbing but can be used elsewhere
            //Must be called before CheckIfClimbing().
            //transformY = transform.position.y;  //!!! Intended to be the bottom of the sprite.  Renamed to playerBtm.
            //!!!transformHY = transformY + climbSpriteHeight;  //!!! Intended to be the top of the sprite.  Renamed to playerTop.
            playerTop = transform.position.y + playerTopOffset;
            playerBtm = transform.position.y + playerBtmOffset;

            CheckIfClimbing();  //!!!
            if (IsClimbing)
            {
                //// debug lines for our ladder handling  //!!!!!
                //Debug.DrawLine(new Vector3(ladder.posX - 2f, ladder.posTopHandlerY, 0),
                //    new Vector3(ladder.posX + 2f, ladder.posTopHandlerY, 0), Color.blue);
                //Debug.DrawLine(new Vector3(ladder.posX - 2f, ladder.posBottomHandlerY, 0),
                //    new Vector3(ladder.posX + 2f, ladder.posBottomHandlerY, 0), Color.blue);
                //Debug.DrawLine(new Vector3(transform.position.x - 2f, playerTop, 0),
                //    new Vector3(transform.position.x + 2f, playerTop, 0), Color.magenta);
                //Debug.DrawLine(new Vector3(transform.position.x - 2f, playerBtm, 0),
                //    new Vector3(transform.position.x + 2f, playerBtm, 0), Color.magenta);

                //Animate (or don't) based on movement speed (whether vertical or horizontal).
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
        if (context.started && CanMove)
        {
            if (touchingDirections.IsGrounded || IsClimbing)
            {
                ResetClimbing();
                animator.SetTrigger(AnimationStrings.jumpTrigger);
                rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && IsAlive)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);
        }
    }

    public void OnRangedAttack(InputAction.CallbackContext context)
    {
        if (context.started && IsAlive)
        {
            animator.SetTrigger(AnimationStrings.rangedAttackTrigger);
        }
    }


    public void OnHit(int damage, Vector2 knockback)
    {
        ResetClimbing();
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
    }

    public void OnDeath()
    {
        GameManager.Instance.GameState = GameStateType.GameOverLost;
    }

    //---------------------------------------------------

    void CheckIfClimbing()
    {
        // start ladder climbing here
        if (ladder != null)
        {
            // climbing up
            //if (ladder.isNearLadder && keyVertical > 0 && playerTop < ladder.posTopHandlerY)
            if (ladder.isNearLadder && moveInput.y > 0 && playerTop < ladder.posTopHandlerY)    
            {
                IsClimbing = true;
                //!!!isClimbingDown = false;
                //!!!animator.speed = 0;
                rb.bodyType = RigidbodyType2D.Kinematic;
                //!!!rb.velocity = Vector2.zero;
                transform.position = new Vector3(ladder.posX, transform.position.y + 0.025f, 0);
            }

            // climbing down
            //if (ladder.isNearLadder && keyVertical < 0 && touchingDirections.IsGrounded && playerTop > ladder.posTopHandlerY)
            if (ladder.isNearLadder && moveInput.y < 0 && touchingDirections.IsGrounded)
            {
                if (playerTop > ladder.posTopHandlerY)
                {
                    IsClimbing = true;
                    //!!!isClimbingDown = true;
                    //!!!animator.speed = 0;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    //!!!rb.velocity = Vector2.zero;
                    transform.position = new Vector3(ladder.posX, transform.position.y, 0);
                }
                else
                {
                    //Reached the bottom while moving down. Stop climbing if you were.
                    ResetClimbing();
                }
            }
        }
        else
        {
            //Not near a ladder.  Stop climbing if you were.
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
            //!!!atLaddersEnd = false;
            //!!!startedClimbTransition = false;
            //!!!finishedClimbTransition = false;
            animator.speed = 1;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = Vector2.zero;
        }
    }
}
