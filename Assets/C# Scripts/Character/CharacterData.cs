using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Characters.Movement;

namespace Characters.Movement
{
    /// <summary>
    /// List of different movement controllers.
    /// </summary>
    public enum MovementType
    {
        /// <summary>
        /// Deprecated. Special case where a character isn't using the movement subsystem.
        /// </summary>
        Static = -1,

        /// <summary>
        /// Special case for Ascension; the player's movement.
        /// </summary>
        Player = 1,

        /// <summary>
        /// Grounded character.
        /// </summary>
        Ground,

        /// <summary>
        /// Air-based character. Mimics the movement of a top-down character. Unaffected by
        /// gravity.
        /// </summary>
        Air,
    }
}

namespace Characters
{
    /// <summary>
    /// Holds information about a character that may need to be referenced from other classes.
    /// Character components in Ascension typically store the data they need via private
    /// serialized fields, but for situations where fields need to be synced across multiple
    /// components, or if a value needs to be referenced outside of the Character system,
    /// CharacterData is used.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Characters/CharacterData", order = 1)]
    public partial class CharacterData : MonoBehaviour
    {
        [SerializeField] private string _characterName;
        public string Name => _characterName;
        [SerializeField] private Sprite defaultSprite;

        [SerializeField] private string _title;

        public string Title => _title;
        public Sprite DefaultSprite => this.defaultSprite;
        [SerializeField] private float width;
        public float Width
        {
            get => this.width;
        }
        [SerializeField] private float height;
        public float Height
        {
            get => this.height;
        }

        [SerializeField] private bool _activateOnSceneLoad;
        public bool ActivateOnSceneLoad => _activateOnSceneLoad;
        [SerializeField] private string _dialoguePrefix;
        public string DialoguePrefix => _dialoguePrefix;
        [SerializeField] private float _spawnOffset;
        public float SpawnOffset => _spawnOffset;
        [SerializeField] private MovementType movementType;
        public MovementType MovementType
        {
            get => this.movementType;
        }
        [SerializeField] private float gravity;
        public float Gravity 
        { 
            get => this.gravity;
        }
        [SerializeField] private float fallSpeed;
        public float FallSpeed
        {
            get => this.fallSpeed;
        }
        [SerializeField] private float acceleration;
        public float Acceleration
        {
            get => this.acceleration;
        }
        [SerializeField] private float topSpeed;
        public float TopSpeed
        {
            get => this.topSpeed;
        }
        [SerializeField] private float drag;
        public float Drag
        {
            get => this.drag; 
        }
        [SerializeField] private float friction;
        public float Friction
        {
            get => this.friction;
        }
        [SerializeField] private float jumpSpeed;
        public float JumpSpeed
        {
            get => this.jumpSpeed;
        }
        [SerializeField] private float maxJumpTime;
        public float MaxJumpTime
        {
            get => this.maxJumpTime;
        }
        [SerializeField] private float _jumpTime;
        public float JumpTime => _jumpTime;
        [SerializeField] private float jumpFalloff;
        public float JumpFalloff
        {
            get => this.jumpFalloff;
        }
        [SerializeField] private float airControl;
        public float AirControl
        {
            get => this.airControl;
        }
        [SerializeField] private float coyoteTime;
        public float CoyoteTime
        {
            get => this.coyoteTime;
        }
        [SerializeField] private float jumpBufferTime;
        public float JumpBufferTime
        {
            get => this.jumpBufferTime;
        }
        [SerializeField] private float apexControl;
        public float ApexControl
        {
            get => this.apexControl;
        }
        [SerializeField] private float apexThreshold;
        public float ApexThreshold
        {
            get => this.apexThreshold;
        }

        [SerializeField] private float distanceBuffer; // not implemented
        public float DistanceBuffer
        {
            get => this.distanceBuffer;
        }

        [SerializeField] private float floorSpeedGate;
        public float FloorSpeedGate => this.floorSpeedGate;
        [SerializeField] private float floorGravity;
        public float FloorGravity => this.floorGravity;

        #region Climbing
        [SerializeField] private bool _enableClimbing;
        public bool EnableClimbing => _enableClimbing;
        [SerializeField] private float _climbAcceleration;
        public float ClimbAcceleration => _climbAcceleration;
        [SerializeField] private float _maxClimbSpeed;
        public float MaxClimbSpeed => _maxClimbSpeed;
        [SerializeField] private float _maxSlideSpeed;
        public float MaxSlideSpeed => _maxSlideSpeed;
        [SerializeField] private float _idleSlide;
        public float IdleSlide => _idleSlide;
        [SerializeField] private float _wallJumpForce;
        public float WallJumpForce => _wallJumpForce;
        [SerializeField] private float _minWallJumpTime;
        public float MinWallJumpTime => _minWallJumpTime;

        #endregion

        [SerializeField] private bool _isLifeStealOverridden;
        public bool IsLifeStealOverridden => _isLifeStealOverridden;
        
        [SerializeField] private int _lifeStealOverride;
        public int LifeStealOverride => _lifeStealOverride;
    }
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