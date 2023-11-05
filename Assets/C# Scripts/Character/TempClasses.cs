using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    
public class CharacterData
{
    public int Width {get;set;}
    public int Height {get;set;}
    public int MaxClimbSpeed {get; set;}
    public int MaxJumpTime {get; set;}
    public int MaxSlideSpeed {get; set;}
    public int Gravity {get; set;}
    public int ClimbAcceleration {get; set;}
    public int IdleSlide {get; set;}
    public int JumpSpeed {get; set;}

    public int FallSpeed {get; set;}
    public int CoyoteTime {get; set;}
    public int WallJumpForce {get; set;}
    public int JumpFalloff {get; set;}
    public int MinWallJumpTime {get; set;}
    public int Acceleration {get; set;}
    public int ApexControl {get; set;}
    public int ApexThreshold {get; set;}
    public int AirControl {get; set;}
    public int Friction {get; set;}
    public int Drag {get; set;}
    public int TopSpeed {get; set;}
    public int FloorSpeedGate {get; set;}
}

public class Velocity 
{
    public Vector2 Local {get; set;}
}

public class Character
{
    public EventHandler Activated {get;set;}
    public bool IsDead {get;set;}
} 

public class GroundTimers {
    public int AirTime {get; set;}
    public int CoyoteTime {get; set;}
}