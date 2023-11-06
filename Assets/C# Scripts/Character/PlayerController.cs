using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Characters;

namespace Characters.Movement
{
    /// <summary>
    /// The movement controller specifically designed for the player character. <br/>
    /// </summary>
    public class PlayerController : MovementController
    {
        /// <summary>
        /// PlayerMovement dependency. Allows ground-reliant characters to react to the ground.
        /// </summary>
        [Tooltip("PlayerMovement dependency. Allows ground-reliant characters to react to the " +
                 "ground.")]
        [SerializeField] protected GroundDetector _detector;

        /// <summary>
        /// PlayerMovement dependency. Allows a character keep track of the time they've spent in
        /// each possible GroundState.
        /// </summary>
        [Tooltip("PlayerMovement dependency. Allows a character keep track of the time they've " +
                "spent in each possible GroundState.")]
        [SerializeField] protected GroundTimers _timer;

        public override bool AbilityOverride
        {
            get { return _abilityOverride; }
            set
            {
                if(!_abilityOverride && value)
                {
                    CancelJump();
                }
                _abilityOverride = value;
            }
        }

        /// <summary>
        /// Wrapper event for the event of the same name in the GroundDetector component. Called
        /// when the character is on the floor, when they weren't on the floor during the last
        /// call.
        /// </summary>
        public event EventHandler Floored
        {
            add { _detector.Floored += value; }
            remove {_detector.Floored -= value; }
        }

        /// <summary>
        /// Wrapper event for the event of the same name in the GroundDetector component. Called
        /// when the player is not on the floor, when they were on the call during the last call.
        /// </summary>
        public event EventHandler FloorExited
        {
            add { _detector.FloorExited += value; }
            remove { _detector.FloorExited -= value; }
        }

        /// <summary>
        /// Called on the first frame the player begins climbing.
        /// </summary>
        public event EventHandler Climbing;

        /// <summary>
        /// Called on the frame the player exists a climb.
        /// </summary>
        public event EventHandler ClimbExited;

        /// <summary>
        /// Called on the first frame when a player jumps.
        /// </summary>
        public event EventHandler Jumping;

        /// <summary>
        /// Called on the first frame when a player jumps from the floor.
        /// </summary>
        public event EventHandler FloorJumping;

        /// <summary>
        /// Called on the first frame when a player jumps from a wall.
        /// </summary>
        public event EventHandler WallJumping;

        /// <summary>
        /// Returns true when the player is in the jumping state. Not to be confused with the
        /// hangtime from being in the air.
        /// </summary>
        public bool IsJumping => _isJumping;

        /// <summary>
        /// Returns true when the player is in the climbing state.
        /// </summary>
        public bool IsClimbing => _isClimbing;

        /// <summary>
        /// The horizontal input from the player. This property should only equal 1, 0, or -1.
        /// </summary>
        public int XInput => _moveX;

        /// <summary>
        /// The vertical input from the player. This property should only equal 1, 0, or -1.
        /// </summary>
        public int YInput => _moveY;
        
        private bool _canCoyoteJump;
        private bool _isJumping;
        private bool _isClimbing;
        private bool _isWallJump;
        private int _moveX;
        private int _moveY;
        private bool _abilityOverride;
        private Coroutine _jumpProcess;

        private void Awake()
        {
            _character.Activated += character_Activated;
        }
        private void Update()
        {
            if (IsInterrupted) return;
            if (AbilityOverride)
            {
                CancelJump();
                return;
            }

            CacheInput();
            CheckForCoyoteTime();
        }

        private void FixedUpdate()
        {
            if (IsInterrupted)
            {
                OnInterrupt();
                return;
            }
            if (AbilityOverride) return;
            
            Walk();
            Climb();
            ApplyGravity();
        }

        /// <summary>
        /// Sets the x input. <br/>
        /// PlayerMovement will only process X input as a positive (1), negative (-1), or neutral
        /// (0) integer value. The float argument will clamp to 1 or -1 unless it equals 0
        /// directly, then it will be set to 0.
        /// </summary>
        /// <param name="value">
        /// The new value of the x input.
        /// </param>
        public void SetXInput(float value)
        {
            if (value == 0)
            {
                _moveX = 0;
                return;
            }
            _moveX = (int)Mathf.Sign(value);
        }

        /// <summary>
        /// Sets the x input <br/>
        /// PlayerMovement will only process x input as a positive (1), negative (-1), or neutral
        /// (0) integer value. The integer argument will clamp to 1 or -1 unless it equals 0.
        /// </summary>
        /// <param name="value">
        /// The new value of the x input.
        /// </param>
        public void SetXInput(int value) => SetXInput((float)value);

        /// <summary>
        /// Sets the y input. <br/>
        /// PlayerMovement will only process y input as a positive (1), negative (-1), or neutral
        /// (0) integer value. The float argument will clamp to 1 or -1 unless it equals 0
        /// directly, then it will be set to 0.
        /// </summary>
        /// <param name="value">
        /// The new value of the y input.
        /// </param>
        public void SetYInput(float value)
        {
            if (value == 0)
            {
                _moveY = 0;
                return;
            }
            _moveY = (int)Mathf.Sign(value);
        }

        /// <summary>
        /// Sets the y input <br/>
        /// PlayerMovement will only process y input as a positive (1), negative (-1), or neutral
        /// (0) integer value. The integer argument will clamp to 1 or -1 unless it equals 0.
        /// </summary>
        /// <param name="value">
        /// The new value of the y input.
        /// </param>
        public void SetYInput(int value) => SetYInput((float)value);

        protected override void CacheInput()
        {
            if (_character.IsDead)
            {
                _moveX = 0;
                _moveY = 0;
                return;
            }

            if(_moveX != 0) LastX = _moveX;
        }
        
        private void Climb()
        {
            // cache fields
            SideState climbState = _detector.ClimbState;
            SideState edgeState = _detector.EdgeState;

            if(_isClimbing)
            {
                if(CannotClimb())
                {
                    ClimbExited?.Invoke(this, null);
                    _isClimbing = false;
                    return;
                }
            }
            else
            {
                if(CanClimb())
                {
                    Climbing?.Invoke(this, null);
                    _isClimbing = true;
                    CancelJump();
                }
                else return;
            }


            // prevent player from accidentally climbing up or down in an inaccessable location
            if (_moveY != 0 && !_detector.CornerCheck((int)climbState, _moveY, _detector.ClimbMask))
            {
                _velocity.Local = Vector2.zero;
;               return;
            }

            // climbing movement
            switch (_moveY)
            {
                case -1: // character sliding down wall
                    if(_velocity.Local.y > _data.MaxSlideSpeed)
                        _velocity.Local = new Vector2(0, _velocity.Local.y +_data.Gravity * Time.deltaTime);
                    else _velocity.Local = new Vector2(0, _data.MaxSlideSpeed);
                    break;
                case 1: // character climbing up
                        if(_velocity.Local.y < _data.MaxClimbSpeed)
                            _velocity.Local = new Vector2(0, _velocity.Local.y + _data.ClimbAcceleration * Time.deltaTime);
                        else _velocity.Local = new Vector2(0, _data.MaxClimbSpeed);

                    break;
                default: // case 0, no input
                    if(_velocity.Local.y > _data.IdleSlide)
                        _velocity.Local = new Vector2(0, _velocity.Local.y +_data.Gravity * Time.deltaTime);
                    else _velocity.Local = new Vector2(0, _data.IdleSlide);
                    break;
            }

            bool CanClimb()
            {
                if(!_detector.IsFullContactClimb) // not touching a wall
                {
                    return false;
                }
                if((int)climbState == -_moveX) // character holding direction opposite from wall
                {
                    return false;
                }
                if(_isWallJump || _isJumping)
                {
                    return false;
                }

                return true;
            }

            bool CannotClimb()
            {
                if (_detector.WallState == SideState.Both || _detector.WallState == SideState.None)
                {
                    return true;
                }
                if ((int)climbState == -_moveX) // character holding direction opposite from wall
                {
                    return true;
                }
                if (_isWallJump || _isJumping)
                {
                    return true;
                }

                return false;
            }
        }

        private void Walk()
        {
            if(_isClimbing)
                return;

            GroundState currentState = _detector.State;

            int direction = _moveX;
            float speed;
            bool isOnSlope = _detector.StairState == SideState.Left ||
                             _detector.StairState == SideState.Right;

            switch (currentState)
            {
                case GroundState.Floor:
                    speed = _data.Acceleration;
                    break;
                default:
                    if(Mathf.Abs(_velocity.Local.y) < _data.ApexThreshold && _data.ApexThreshold != 0)
                    {
                        speed = _data.ApexControl != 0 ? _data.ApexControl : _data.Acceleration;
                    }
                    else speed = _data.AirControl != 0 ? _data.AirControl : _data.Acceleration;
                    break;
            }

            float slowdown;
            switch(currentState)
            {
                case GroundState.Floor:
                    if (isOnSlope) slowdown = 0;
                    else slowdown = _data.Friction;
                    break;
                default:
                    slowdown = _data.Drag;
                    break;
            }
            
            // stops character's horizontal velocity if they are attempting to run into a wall
            float v_sign = 0;
            if (_velocity.Local.x != 0) Mathf.Sign(_velocity.Local.x);
            if ((int)_detector.WallState == v_sign && v_sign != 0)
            {
                _velocity.Local = new Vector2(0, _velocity.Local.y);
                direction = 0;
            }

            // if player is on slope, use different movement style
            if (isOnSlope && _moveX != 0 && !_isJumping)
            {
                Vector2 dir = _detector.CalculateStairDirection(_moveX);
                _velocity.Local = 1/Mathf.Abs(dir.x) * _data.TopSpeed * dir;
                return;
            }

            float speedChange;

            switch (direction)
            {
                case -1: // moving left
                    speedChange = -speed * Time.deltaTime;
                    break;  
                case 1: // moving right
                    speedChange = speed * Time.deltaTime;
                    break;
                default: // not moving
                    float topSpeed = _data.TopSpeed;
                    float x_local = Mathf.Clamp(_velocity.Local.x * slowdown, -topSpeed, topSpeed);
                    _velocity.Local = new Vector2(x_local, _velocity.Local.y);
                    if(Mathf.Abs(_velocity.Local.x) < _data.FloorSpeedGate)
                    {
                        _velocity.Local = new Vector2(0, _velocity.Local.y);
                    }
                    return;
            }

            // get new speed
            float x_velocity = _velocity.Local.x + speedChange;

            // set new speed + clamp
            float x = Mathf.Clamp(x_velocity, -_data.TopSpeed, _data.TopSpeed);
            _velocity.Local = new Vector2(x, _velocity.Local.y);
        }
        private void ApplyGravity()
        {
            // gates
            if(_isJumping) return;
            if(_isClimbing) return;
            if(AbilityOverride) return;

            _velocity.Local = new Vector2(_velocity.Local.x, _velocity.Local.y + _data.Gravity * Time.deltaTime);

            if (_detector.StairState != SideState.None &&
                _detector.State == GroundState.Floor)
            {
                _velocity.Local = new Vector2(_velocity.Local.x, 0.0f);
                return;
            }

            if (_detector.State == GroundState.Floor && !_detector.WallCheck)
            {
                _velocity.Local = new Vector2(_velocity.Local.x, 0.0f);
            }

            // default; regular fall speed
            if(_velocity.Local.y > _data.FallSpeed)
                _velocity.Local = new Vector2(_velocity.Local.x, _velocity.Local.y + _data.Gravity * Time.deltaTime);
            else _velocity.Local = new Vector2(_velocity.Local.x, _data.FallSpeed);
        }

        private void CheckForCoyoteTime()
        {
            bool isTouchingFloor = _detector.FloorCheck;
            SideState wallState = _detector.WallState;

            if (isTouchingFloor && !_isJumping && wallState == SideState.None)
            {
                _canCoyoteJump = true;
            }

            if (_timer.AirTime > _data.CoyoteTime)
            {
                _canCoyoteJump = false;
            }
        }

        /// <summary>
        /// Attempts to have the player jump. If successful, returns true. If not, returns false.
        /// </summary>
        /// <returns>
        /// If the jump was successful or not.
        /// </returns>
        public bool TryJump()
        {
            if (IsInterrupted) return false;
            if (AbilityOverride) return false;
            if (_isJumping) return false;

            // ground checks
            GroundState currentState = _detector.State;
            bool isTouchingFloor = _detector.FloorCheck;
            bool isTouchingWall = _detector.WallCheck;
            bool isTouchingRoof = _detector.RoofCheck;
            SideState edgeState = _detector.EdgeState;
            SideState climbState = _detector.ClimbState;
            SideState wallState = _detector.WallState;
            SideState stairState = _detector.StairState;

            // climbing bypasses all of the checks used when not climbing, because the climbing
            // state is an automatically valid position to jump from
            if (!IsClimbing)
            {
                if (_timer.AirTime > _data.CoyoteTime) return false;

                // don't even try jumping if player is touching roof (and it's not a wall lol)
                // ignore for wall jumps
                if (isTouchingRoof && !isTouchingWall ||
                    isTouchingRoof && wallState != SideState.None && !IsClimbing)
                    return false;

                if (isTouchingFloor)
                {
                    if (edgeState != SideState.Both &&
                        wallState != SideState.None &&
                        stairState == SideState.None)
                    {
                        return false;
                    }
                }

                if (edgeState != SideState.Both &&
                    wallState != SideState.None &&
                    stairState == SideState.None)
                {
                    return false;
                }

                // prevent coyote jumps from happening if flag isn't true
                if (!IsClimbing && !isTouchingFloor)
                {
                    if (!_canCoyoteJump) return false;
                }
            }

            _isJumping = true;
            _canCoyoteJump = false;

            _jumpProcess = StartCoroutine(JumpingProcess(IsClimbing));
            Jumping?.Invoke(this, null);

            if (!IsClimbing)
            { 
                FloorJumping?.Invoke(this, null);
            }
            else WallJumping?.Invoke(this, null);

            return true;
        }

        /// <summary>
        /// Coroutine used to handle the jumping process.
        /// </summary>
        /// <param name="isWallJump">
        /// If the jump is a wall jump.
        /// </param>
        private IEnumerator JumpingProcess(bool isWallJump)
        {
            float currentJumpTime = 0;

            SideState climbState = _detector.ClimbState;
            float sideJumpForce = -(int)climbState * _data.WallJumpForce;

            while (currentJumpTime < _data.MaxJumpTime)
            {

                bool isTouchingWall = _detector.WallCheck;
                bool isTouchingRoof = _detector.RoofCheck;
                SideState wallState = _detector.WallState;
                
                float x_velocity =
                    isWallJump ?
                    sideJumpForce + Mathf.Sign(sideJumpForce) * _data.JumpFalloff * Time.deltaTime :
                    _velocity.Local.x;

                float y_velocity = _data.JumpSpeed + _data.JumpFalloff * currentJumpTime;

                // cancel horizontal wall movement if player is attempting to move in a different direction
                if (Mathf.Sign(sideJumpForce) == -_moveX &&
                    _data.MinWallJumpTime < currentJumpTime)
                    isWallJump = false;

                Vector2 nextVelocity = new Vector2(x_velocity, y_velocity);
                _velocity.Local = nextVelocity;

                // cancel jump if player's head is touching roof 
                if (isTouchingRoof && !isTouchingWall ||
                    isTouchingRoof && wallState != SideState.None && !isWallJump)
                {
                    currentJumpTime = _data.MaxJumpTime + Mathf.Epsilon;
                }

                // iterate
                currentJumpTime += Time.deltaTime;
                yield return null;
            }

            CancelJump();
        }

        // Resets all jumping variables back to normal
        // Used by climbing code to fix issue.
        public void CancelJump()
        {
            _isJumping = false;
            _isWallJump = false;

            if (_jumpProcess != null) StopCoroutine(_jumpProcess);
        }

        protected void OnInterrupt()
        {
            CancelJump();
        }
    }
}        

