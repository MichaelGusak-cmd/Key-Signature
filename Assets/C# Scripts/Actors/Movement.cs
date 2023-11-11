using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Input;

public class Movement : MonoBehaviour
{
    public float JumpForce = 5f;
    public float TerminalVelocity = 10f;

    private float Width;
    private float Height;

    public LayerMask GroundMask;

    public float FlooredDistanceThreshold = 0.1f;
    public event EventHandler Floored;
    public event EventHandler FloorExited;
    private bool FloorCheck;
    private bool OnFloorLastFrame = false;

    private PlayerGameplayInput PlayerInput;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Global Global;

    private void Start()
    {
        // Assuming you have a reference to the JumpButtonScript instance
        PlayerInput = GetComponent<PlayerGameplayInput>();
        rb = GetComponent<Rigidbody2D>();
        Global = GameObject.Find("Global").GetComponent<Global>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null)
        {
            // Get the width and height of the collider
            Width = boxCollider.size.x;
            Height = boxCollider.size.y;
        }
        else Debug.LogError("BoxCollider2D component not found.");

        // Subscribe functions to Events:
        if (PlayerInput != null)
        {
            PlayerInput.JumpButtonPressed += OnJumpButtonPressed;
            PlayerInput.JumpButtonReleased += OnJumpButtonReleased;
        }
        else Debug.LogError("PlayerGameplayInput component not found.");

        Floored += OnFloor;
        FloorExited += OnFloorExit;

    }
    
    private void FixedUpdate()
    {
        rb.gravityScale = Global.Gravity;




        FloorCheck = CheckForFloor(GroundMask);
        CheckForRoof(GroundMask); // TODO: make rigidbody y component <= 0
        ProcessGroundState();
    }

    private void OnJumpButtonPressed(object sender, System.EventArgs e)
    {
        rb.velocity = new Vector2(rb.velocity.x, JumpForce); // TODO: make the jump height tie to held duration?
        Debug.Log("Jump button pressed!");
    }
    private void OnJumpButtonReleased(object sender, System.EventArgs e)
    {
        Debug.Log("Jump button pressed!");
    }

    private void OnFloor(object sender, System.EventArgs e)
    {
        Debug.Log("Entered Floor");
    }
    private void OnFloorExit(object sender, System.EventArgs e)
    {
        Debug.Log("Exited Floor");
    }

    
    private void OnDestroy()
    {
        if (PlayerInput != null)
        {
            // Unsubscribe from events
            PlayerInput.JumpButtonPressed -= OnJumpButtonPressed;
            PlayerInput.JumpButtonReleased -= OnJumpButtonReleased;
        }
    }



    private void ProcessGroundState()
    {
        if(FloorCheck)
        {
            if(!OnFloorLastFrame)
            {
                Floored?.Invoke(this, null);
            }
            OnFloorLastFrame = true;
        }
        else
        {
            if(OnFloorLastFrame)
            {
                FloorExited?.Invoke(this, null);
            }
            OnFloorLastFrame = false;
        }
    }
    
    private bool CheckForFloor(LayerMask mask)
    {
        float x = transform.position.x;
        float y = transform.position.y;

        Vector2 floorCheckPos = new Vector2(x, y - (Height / 2.0f));
        Vector2 floorCheckSize = new Vector2(Width, FlooredDistanceThreshold);
        return ColliderOverlapCheck(floorCheckPos, floorCheckSize, mask);
    }
    
    private bool CheckForRoof(LayerMask mask)
    {
        float x = transform.position.x;
        float y = transform.position.y;

        Vector2 roofPos = new Vector2(x, y + (Height / 2.0f));
        Vector2 roofSize = new Vector2(Width - FlooredDistanceThreshold, FlooredDistanceThreshold);
        return ColliderOverlapCheck(roofPos, roofSize, mask);
    }

    private bool ColliderOverlapCheck(Vector2 position, Vector2 size, LayerMask mask)
    {
        Collider2D check = Physics2D.OverlapBox(position, size, 0, mask);
        return check != null;
    }
}
