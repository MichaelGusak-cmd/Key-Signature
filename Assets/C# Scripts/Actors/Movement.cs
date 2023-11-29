using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Input;

public class Movement : MonoBehaviour
{
    public float JumpForce = 5f;
    public float TerminalVelocity = 10f;

    public float WalkSpeed = 5f;

    private float Width;
    private float Height;
    private Vector2 InputVelocity;

    public LayerMask GroundMask;

    public float FlooredDistanceThreshold = 0.1f;
    public event EventHandler Floored;
    public event EventHandler FloorExited;
    private bool isFloored;
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

        InputVelocity = new Vector2(0,0);

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
        rb.velocity = new Vector2(PlayerInput.MoveAxis.x*WalkSpeed, rb.velocity.y);
    }

    private void OnJumpButtonPressed(object sender, System.EventArgs e)
    {
        if (isFloored)
            rb.velocity = new Vector2(rb.velocity.x, JumpForce); // TODO: make the jump height tie to held duration?
        Debug.Log("Jump button pressed!");
    }
    private void OnJumpButtonReleased(object sender, System.EventArgs e)
    {
        Debug.Log("Jump button pressed!");
    }

    private void OnFloor(object sender, System.EventArgs e)
    {
        isFloored = true;
        Debug.Log("Entered Floor");
    }
    private void OnFloorExit(object sender, System.EventArgs e)
    {
        isFloored = false;
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

    // private void Walk()
    // {
    //     int direction = MoveAxis.x;
    //     float speed;

         
    //             speed = _data.Acceleration;
    //             break;
    //         default:
    //             if(Mathf.Abs(_velocity.Local.y) < _data.ApexThreshold && _data.ApexThreshold != 0)
    //             {
    //                 speed = _data.ApexControl != 0 ? _data.ApexControl : _data.Acceleration;
    //             }
    //             else speed = _data.AirControl != 0 ? _data.AirControl : _data.Acceleration;
    //             break;
    //     }

    //     float slowdown;
    //     switch(currentState)
    //     {
    //         case GroundState.Floor:
    //             if (isOnSlope) slowdown = 0;
    //             else slowdown = _data.Friction;
    //             break;
    //         default:
    //             slowdown = _data.Drag;
    //             break;
    //     }
        
    //     // stops character's horizontal velocity if they are attempting to run into a wall
    //     float v_sign = 0;
    //     if (_velocity.Local.x != 0) Mathf.Sign(_velocity.Local.x);
    //     if ((int)_detector.WallState == v_sign && v_sign != 0)
    //     {
    //         _velocity.Local = new Vector2(0, _velocity.Local.y);
    //         direction = 0;
    //     }

    //     // if player is on slope, use different movement style
    //     if (isOnSlope && _moveX != 0 && !_isJumping)
    //     {
    //         Vector2 dir = _detector.CalculateStairDirection(_moveX);
    //         _velocity.Local = 1/Mathf.Abs(dir.x) * _data.TopSpeed * dir;
    //         return;
    //     }

    //     float speedChange;

    //     switch (direction)
    //     {
    //         case -1: // moving left
    //             speedChange = -speed * Time.deltaTime;
    //             break;  
    //         case 1: // moving right
    //             speedChange = speed * Time.deltaTime;
    //             break;
    //         default: // not moving
    //             float topSpeed = _data.TopSpeed;
    //             float x_local = Mathf.Clamp(_velocity.Local.x * slowdown, -topSpeed, topSpeed);
    //             _velocity.Local = new Vector2(x_local, _velocity.Local.y);
    //             if(Mathf.Abs(_velocity.Local.x) < _data.FloorSpeedGate)
    //             {
    //                 _velocity.Local = new Vector2(0, _velocity.Local.y);
    //             }
    //             return;
    //     }

    //     // get new speed
    //     float x_velocity = _velocity.Local.x + speedChange;

    //     // set new speed + clamp
    //     float x = Mathf.Clamp(x_velocity, -_data.TopSpeed, _data.TopSpeed);
    //     _velocity.Local = new Vector2(x, _velocity.Local.y);
    // }
}
