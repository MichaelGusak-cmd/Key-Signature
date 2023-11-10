using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Input;

public class Movement : MonoBehaviour
{
    public float TerminalVelocity = 10f;

    public LayerMask GroundMask;

    public float FlooredDistanceThreshold = 0.1f;
    public event EventHandler Floored;
    public event EventHandler FloorExited;

    private PlayerGameplayInput PlayerInput;
    private Rigidbody2D rb;
    private Global Global;

    private void Start()
    {
        // Assuming you have a reference to the JumpButtonScript instance
        PlayerInput = GetComponent<PlayerGameplayInput>();
        rb = GetComponent<Rigidbody2D>();
        Global = GameObject.Find("Global").GetComponent<Global>();

        // Subscribe to the JumpButtonPressed event
        if (PlayerInput != null)
        {
            PlayerInput.JumpButtonPressed += OnJumpButtonPressed;
            PlayerInput.JumpButtonReleased += OnJumpButtonReleased;
        }
    }

    public void FixedUpdate() 
    {
        rb.gravityScale = Global.Gravity;
    }

    private void OnJumpButtonPressed(object sender, System.EventArgs e)
    {
        // Handle the jump button pressed event
        Debug.Log("Jump button pressed!");
    }

    private void OnJumpButtonReleased(object sender, System.EventArgs e)
    {
        // Handle the jump button released event
        Debug.Log("Jump button pressed!");
    }

    
    private void OnDestroy()
    {
        if (PlayerInput != null)
        {
            // Unsubscribe from events
            PlayerInput.JumpButtonPressed -= OnJumpButtonPressed;
        }
    }
}
