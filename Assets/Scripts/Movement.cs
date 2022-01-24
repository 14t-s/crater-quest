using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D player;
    private AnimationScript anim;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;

    public float kaioatTime = 1f;
    public float kaioatTimeCounter;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;

    [Space]

    private bool groundTouch;
    private bool hasDashed;

    public int side = 1;

    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    [Space]
    [Header("Smoothdamp")]
    public Vector2 currentInputVector;
    public Vector2 smoothInputVelocity;
    public float smoothInputTime = 0.4f;
    public float smoothedX;
    public float smoothedY;

    // wall grab test
    [SerializeField] public bool stopWallGrab;

    //private Controls playerControls;
    private Controls playerControlsAction;
    private PlayerInput playerInput;
    private CharacterController controller;

    void Awake()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        //playerControls = GetComponent<Controls>();
        playerInput = GetComponent<PlayerInput>();

        playerControlsAction = new Controls();
        playerControlsAction.Gameplay.Enable();
        //playerControlsAction.Gameplay.Movement.performed += Movement_performed;
        playerControlsAction.Gameplay.Jump.performed += HandleJump;
        playerControlsAction.Gameplay.Dash.performed += HandleDash;
        playerControlsAction.Gameplay.GrabWall.performed += GrabWall;
    }

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        player = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        // keys:
        // arrow keys - movement
        // z - jump
        // shift - wall grab
        // x - melee / s - ranged attckes
        // c - special/dash
        // d - ability

        Vector2 updatedInputVector = playerControlsAction.Gameplay.Movement.ReadValue<Vector2>();


        //currentInputVector = Vector2.SmoothDamp(currentInputVector, updatedInputVector, ref smoothInputVelocity, smoothInputTime);
        currentInputVector = Vector2.Lerp(currentInputVector, updatedInputVector, 0.2f);

        Debug.Log("currentvelocity X:" + smoothInputVelocity.x + " " + "currentvelocity Y:" + smoothInputVelocity.y);
        Debug.Log("currentInputVector X: " + currentInputVector.x + " currentInputVector" + currentInputVector.y);

        // Vector2 move = new Vector2(currentInputVector.x, currentInputVector.y);
        // controller.Move(move * Time.deltaTime * speed);



        smoothedX = currentInputVector.x;
        smoothedY = currentInputVector.y;

        float x = smoothedX;
        float y = smoothedY;

        // Sets X to 0 if X is basically 0
        if (smoothedX < 0.01 && smoothedX > 0)
        {
            smoothedX = 0;
        }
        if (smoothedX > -0.01 && smoothedX < 0)
        {
            smoothedX = 0;
        }
        // Sets Y to 0 if Y is basically 0
        if (smoothedY < 0.01 && smoothedY > 0)
        {
            smoothedY = 0;
        }
        if (smoothedY > -0.01 && smoothedY < 0)
        {
            smoothedY = 0;
        }

        // Movement
        Vector2 dir = new Vector2(smoothedX, smoothedY);
        Walk(dir);
        anim.SetHorizontalMovement(smoothedX, smoothedY, player.velocity.y);


        // Resets coyote time/counts down the grace period
        if (coll.onGround == true)
        {
            kaioatTimeCounter = kaioatTime;
        }
        else
        {
            kaioatTimeCounter -= Time.deltaTime;
        }


        if (stopWallGrab == true || !coll.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }


        // After player stops pressing wall grab
        if (stopWallGrab == true)
        {
            // Resets stopWallGrab
            if (coll.onGround == true)
            {
                stopWallGrab = false;
            }
            else
            {
                // Prevents wallslide and wallgrab in invalid scenarios
                if (!coll.onWall || !canMove)
                {
                    wallGrab = false;
                    wallSlide = false;
                }
                // Wallslide after wall grab button is released
                if (coll.onWall && !coll.onGround)
                {
                    if (getXRaw() != 0 && !wallGrab)
                    {
                        wallSlide = true;
                        WallSlide();
                    }
                }
            }

        }

        // Resets better jumping?
        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }


        if (wallGrab && !isDashing)
        {
            player.gravityScale = 0;
            if (x > .2f || x < -.2f)
                player.velocity = new Vector2(player.velocity.x, 0);

            float speedModifier;
            if (coll.reachLedge == true)
                speedModifier = y > 0 ? 0f : 1;
            else
                speedModifier = y > 0 ? .5f : 1;


            player.velocity = new Vector2(player.velocity.x, y * (speed * speedModifier));
        }
        else
        {
            player.gravityScale = 3;
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if (!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        if (x > 0)
        {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }


    }

    //Returns whether x is positive/negative or 0
    public int getXRaw()
    {
        Vector2 updatedInputVector = playerControlsAction.Gameplay.Movement.ReadValue<Vector2>();
        float x = updatedInputVector.x;

        if (x > 0)
        {
            return 1;
        }
        else if (x < 0)
            return -1;
        else
            return 0;
    }

    public int getYRaw()
    {
        Vector2 updatedInputVector = playerControlsAction.Gameplay.Movement.ReadValue<Vector2>();
        float y = updatedInputVector.y;

        if (y > 0)
        {
            return 1;
        }
        else if (y < 0)
            return -1;
        else
            return 0;
    }

    /*
    public void Movement_performed(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        Vector2 dir = new Vector2(smoothedX, smoothedY);

        Walk(dir);


        anim.SetHorizontalMovement(smoothedX, smoothedY, player.velocity.y);
    }
    */

    public void HandleJump(InputAction.CallbackContext context)
    {
        anim.SetTrigger("jump");

        if (kaioatTimeCounter > 0f)
        {
            Jump(Vector2.up, false);
        }

        if (coll.onWall && !coll.onGround)
            WallJump();
    }
    // DASH CODE BEGIN
    public void HandleDash(InputAction.CallbackContext context)
    {
        Vector2 updatedInputVector = playerControlsAction.Gameplay.Movement.ReadValue<Vector2>();
        float xRaw = getXRaw();
        float yRaw = getYRaw();

        if ((xRaw != 0 || yRaw != 0) && !hasDashed)
            Dash(xRaw, yRaw, side);
    }
    private void Dash(float x, float y, float xdir)
    {
        Debug.Log("Dash");
        if (x == -xdir && wallGrab) return;
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;

        anim.SetTrigger("dash");

        player.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        player.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        Debug.Log("Dash wait");

        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        player.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        player.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        Debug.Log("Ground dash");
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }
    // DASH CODE END

    // Handles the wall grabbing 
    public void GrabWall(InputAction.CallbackContext context)
    {
        stopWallGrab = false;

        if (coll.onWall && canMove)
        {
            if (side != coll.wallSide)
            {
                side *= -1;
                anim.Flip(side);
            }
            wallGrab = true;
            wallSlide = false;
        }

        // Wall slide after release wall grab
        if (context.canceled == true)
        {
            Debug.Log(context.phase);
            stopWallGrab = true;
        }
    }


    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }



    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if (coll.wallSide != side)
            anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if ((player.velocity.x > 0 && coll.onRightWall) || (player.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : player.velocity.x;

        player.velocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            player.velocity = new Vector2(dir.x * speed, player.velocity.y);
        }
        else
        {
            player.velocity = Vector2.Lerp(player.velocity, (new Vector2(dir.x * speed, player.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        player.velocity = new Vector2(player.velocity.x, 0);
        player.velocity += dir * jumpForce;

        particle.Play();

        kaioatTimeCounter = 0f;
    }

    private void CornerCorrect(float yVelocity)
    {
        //RaycastHit2D hit = Physics2D.Raycast(transform.position - innerRaycastOffset + Vector3.up * topRaycastLength, Vector3.left, topRaycastLength, groundTouch)
        return;
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        player.drag = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }

    // Enable player only if GameState is in Gameplay
    private void OnGameStateChanged(GameState newGameState)
    {
        enabled = newGameState == GameState.Gameplay;
    }
}
