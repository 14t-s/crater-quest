using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterJumping : MonoBehaviour
{
    private Rigidbody2D rb;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private Controls playerControlsAction;

    void Awake()
    {
        //playerControls = GetComponent<Controls>();

        //Reference
        /*
        PlayerInputActions playerInputActions = new PlayerInputActions();
        controls.Gameplay.Jump.performed += Jump;
        */

        playerControlsAction = new Controls();
        playerControlsAction.Gameplay.Enable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(rb.velocity.y < 0)
        {
            rb.velocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        }
        else if (rb.velocity.y > 0 && !(playerControlsAction.Gameplay.Jump.ReadValue<float>()==1f ? true : false))
        {
            rb.velocity += (lowJumpMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        }
    }
}
