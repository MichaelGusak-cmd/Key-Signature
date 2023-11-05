using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Movement
{
    public enum GroundState
    {
        Floor, 
        Wall,
        Air
    }

    public enum SideState
    {
        Left,
        Right,
        Both,
        None
    }


    /// <summary>
    /// Caulates a ground-based character's collision movement states every frame.<br/>
    /// GroundDetector uses Physics2D.OverlapBox() under the assumption that the charater
    /// it is attached to uses a BoxCollider2D to interact with world geometry. Due to the
    /// imprecision of Unity's physics system, there is a maximum acceptable length (called the
    /// distance threshold) that a character can be from the ground and be considered touching it.
    /// <br/>
    /// GroundDetector uses 8 possible ground checks to determine movement states. Half (4) of
    /// them extend outwards from each side of the character's collision box, and the other half
    /// cover the corners. The checks extending from the sides are called the floor, roof, left,
    /// and right checks. The corner checks are named based on the checks beside them (e.g.
    /// top-left and bottom-right corner checks.) <br/>
    /// Outside of detecting ground, GroundDetector comes with the ability to check for climbable
    /// and slope collisions, for characters that can climb, and who may or may not need special
    /// functionality for climbing up slopes.
    /// </summary>
    public class GroundDetector : MonoBehaviour
    {
        [Tooltip("Used to access the width and height of a character. Characters in Ascension " +
            "are created with retangular collision boxes in mind.")]
        [SerializeField] private CharacterData _data;
        [SerializeField] private LayerMask _mask;
        [SerializeField] private LayerMask _climbMask;
        [SerializeField] private LayerMask _stairMask;


        public LayerMask Mask => _mask;
        public LayerMask ClimbMask => _climbMask;

        public LayerMask StairMask => _stairMask;
        /// <summary>
        /// Called when the character is on the floor, when they weren't on the floor during the
        /// last call.
        /// </summary>
        public event EventHandler Floored;

        /// <summary>
        /// Called when the character is not on the floor, when they were on the call during the
        /// last call.
        /// </summary>
        public event EventHandler FloorExited;

        /// <summary>
        /// The minimum length of collision checks. The distance threshold refers to the maximum
        /// distance a character from be from the ground and still be considered touching it.
        /// </summary>
        public float DistanceThreshold => _distT;

        /// <summary>
        /// The current ground state of the character. There are three possible states: Floor, Wall,
        /// and Air. Floor and Wall are both checked. Floor takes priority over Wall if both return
        /// true. If neither are true, Air is returned. Wall only returns true on climable walls.
        /// </summary>
        public GroundState State { get; private set;}

        /// <summary>
        /// Checks for ground on the left and right of a character.
        /// </summary>
        public SideState WallState { get; private set;}

        /// <summary>
        /// Checks for climbable ground to the left and right of a character.
        /// </summary>
        public SideState ClimbState { get; private set; }

        /// <summary>
        /// Splits the floor collision check in half, returning a SideState enum based on which
        /// checks return true. Detects ledges, and slopes.
        /// </summary>
        public SideState EdgeState { get; private set; }

        /// <summary>
        /// Uses the stair mask to calculate which direction the slope is for a character.
        /// </summary>
        public SideState StairState { get; private set; }

        /// <summary>
        /// Returns true if the character feet are on the ground.
        /// </summary>
        public bool FloorCheck { get; private set; }

        /// <summary>
        /// Returns true if the character's head is touching ground. Useful for cancelling
        /// actions when the characters's head bumps into something (jumping, abilities, etc.)
        /// </summary>
        public bool RoofCheck { get; private set; }

        /// <summary>
        /// Returns true if there is ground touching the left or the right of a character.
        /// </summary>
        public bool WallCheck { get; private set;}

        /// <summary>
        /// Returns true if WallState is either SideState.Left or SideState.Right, and if the
        /// corner checks are also true for that side. <br/>
        /// e.g. If WallState == SideState.Left, this will only return true if the top-left and
        /// bottom-left corner check both equal true.
        /// </summary>
        public bool IsFullContactWall { get; private set; }

        /// <summary>
        /// Returns true if there is climable ground touching the left or the right of a character.
        /// </summary>
        public bool IsTouchingClimb { get; private set; }

        /// <summary>
        /// Returns true if ClimbState is true (not SideState.None), and if the corner checks are
        /// true for the side making contact. <br/>
        /// e.g. If ClimbState == SideState.Left, this will only return true if the top-left and
        /// bottom-left corner check both equal true.
        /// </summary>
        public bool IsFullContactClimb { get; private set; }

        /// <summary>
        /// The minimum length of collision checks.
        /// </summary>
        readonly float _distT = 0.125f;
        private bool _onFloorLastFrame;

        /// <summary>
        /// TO DO - Fires a raycast facing downwards, looking for ground. SideState parameter
        /// specifies if the raycast is fired at the bottom left or right of a character. Useful
        /// for CharacterAI's, to let them check if they can safely fall down.
        /// </summary>
        public float DistanceFromFloor(int x)
        {
            float x_pos = x * _data.Width / 2 + transform.position.x;
            float y_pos = transform.position.y - _data.Height / 2;
            Vector2 pos = new Vector2(x_pos, y_pos);

            RaycastHit2D mainHit = Physics2D.Raycast(pos, -Vector2.up, Mathf.Infinity, _mask);
            return mainHit.distance;
        }

        /// <summary>
        /// Does a ground check in one of the ordinal directions, by doing a collision check of
        /// the smallest acceptable size (based on the distance threshold).
        /// </summary>
        public bool CornerCheck(int x, int y, LayerMask mask)
        {
            float x_pos = x * _data.Width / 2 + transform.position.x;
            float y_pos = y * _data.Height / 2 + transform.position.y;

            Vector2 position = new Vector2(x_pos, y_pos);
            Vector2 size = new Vector2(_distT, _distT);
            return GroundCheck(position, size, mask);
        }

        /// <summary>
        /// Calculates the direction needed to move parallel to sloped ground in the Stair layer.
        /// Intended to be used by ground-based movement controllers who need special
        /// functionality for walking up and down slopes. By default, characters simply move through
        /// Rigidbodies, but this causes issues for precise movement up/down stairs.<br/>
        /// This method will calculate Vector2.zero on the very edges of a slope. If this happens,
        /// this method will instead return (x, -0.2f), where x is the argument provided during
        /// invoke.
        /// </summary>
        /// <param name="x">
        /// The horizontal direction that the character is heading in.
        /// </param>
        /// <returns>
        /// A Vector2 representing the direction the character needs to move to be aligned to the ground.
        /// </returns>
        public Vector2 CalculateStairDirection(int x)
        {
            // position ray cast
            float x_pos = x * _data.Width / 2 + transform.position.x;
            float y_pos = transform.position.y - _data.Height / 2;
            Vector2 pos = new Vector2(x_pos, y_pos);

            // get normal from raycast
            RaycastHit2D mainHit = Physics2D.Raycast(pos, -Vector2.up, Mathf.Infinity, _stairMask);
            Vector2 normal = mainHit.normal;

            // turn normal so it's parallel in the right direction.
            // code taken from https://answers.unity.com/questions/661383/whats-the-most-efficient-way-to-rotate-a-vector2-o.html
            // modified from an answer written by user DDP
            // this code doesn't really work that well but whatever
            float sin = Mathf.Sin(x * -90 * Mathf.Deg2Rad);
            float cos = Mathf.Cos(x * -90 * Mathf.Deg2Rad);
            
            float tx = normal.x;
            float ty = normal.y;
            float new_x = (cos * tx) - (sin * ty);
            float new_y = (sin * tx) + (cos * ty);

            Vector2 output = new Vector2(new_x, new_y);
            if (output == Vector2.zero)
            {
                output = new Vector2(x, -0.2f).normalized;
            }
            output.Normalize();
            return output;
        }

        private void FixedUpdate()
        {
            FloorCheck = CheckForFloor(_mask);
            RoofCheck = CheckForRoof();

            WallState = CheckForWall(false);
            bool leftWallCheck = WallState == SideState.Left || WallState == SideState.Both;
            bool rightWallCheck = WallState == SideState.Right || WallState == SideState.Both;

            ClimbState = CheckForWall(true);
            bool leftClimbCheck = ClimbState == SideState.Left || ClimbState == SideState.Both;
            bool rightClimbCheck = ClimbState == SideState.Right || ClimbState == SideState.Both;

            bool q1 = CornerCheck(1, 1, _mask);
            bool q2 = CornerCheck(-1, 1, _mask);
            bool q3 = CornerCheck(-1, -1, _mask);
            bool q4 = CornerCheck(1, -1, _mask);

            ClimbState = ClimbState;
            EdgeState = CheckForFloorEdge();

            WallCheck = WallState != SideState.None;
            IsFullContactWall = (q2 && q3 && leftWallCheck) ||
                                 (q1 && q4 && rightWallCheck);

            IsTouchingClimb = ClimbState != SideState.None;
            IsFullContactClimb = (q2 && q3 && leftClimbCheck) ||
                                  (q1 && q4 && rightClimbCheck);

            StairState = CalculateSlopeState();
            State = ProcessGroundState();
        }

        /// <summary>
        /// Simplies ground checks, to avoid the boilerplate of constantly writing
        /// Physics2D.OverlapBox.
        /// </summary>
        /// <param name="position"> The position of the check. </param>
        /// <param name="size"> The size of the check. Size is calculated from position, going
        /// outwards. </param>
        /// <param name="mask"> The layers to check for. </param>
        /// <returns></returns>
        private bool GroundCheck(Vector2 position, Vector2 size, LayerMask mask)
        {
            Collider2D check = Physics2D.OverlapBox(position, size, 0, mask);
            return check != null;
        }

        /// <summary>
        /// Uses the stair mask to calculate which direction the slope is for a character.
        /// </summary>
        /// <returns>
        /// The direction the slope is for a character, if they're on a slope.
        /// </returns>
        private SideState CalculateSlopeState()
        {
            bool isOnLeftSlope = CheckForFloor(_stairMask) && CornerCheck(-1, -1, _stairMask);
            bool isOnRightSlope = CheckForFloor(_stairMask) && CornerCheck(1, -1, _stairMask);

            if (isOnLeftSlope && isOnRightSlope) return SideState.Both;
            if (isOnLeftSlope) return SideState.Left;
            if (isOnRightSlope) return SideState.Right;
            return SideState.None;
        }

        /// <summary>
        /// Checks the bottom of the character's collision box for ground. The width of the check
        /// is the width of the character, while the height is the distance threshold.
        /// </summary>
        /// <returns> True if the character is considered to be touching the ground. False
        /// otherwise </returns>
        private bool CheckForFloor(LayerMask mask)
        {
            float x = transform.position.x;
            float y = transform.position.y;

            Vector2 floorCheckPos = new Vector2(x, y - (_data.Height / 2));
            Vector2 floorCheckSize = new Vector2(_data.Width, _distT);
            bool isFloor = GroundCheck(floorCheckPos, floorCheckSize, mask);
            if (isFloor) return true;
            else return false;
        }

        /// <summary>
        /// Checks the bottom of the character's collision for ground. Uses two separate collision
        /// checks, one on the left and the right of the bottom of the character. Can be thought
        /// of as a floor check cut in half.
        /// </summary>
        /// <returns> The edge state of the character. </returns>
        private SideState CheckForFloorEdge()
        {
            float x = transform.position.x;
            float y = transform.position.y;

            // left foot check
            Vector2 leftCheckPos = new Vector2(x - (_data.Width / 4), y - (_data.Height / 2));
            Vector2 leftCheckSize = new Vector2(_data.Width / 2, _distT);
            bool isLeft = GroundCheck(leftCheckPos, leftCheckSize, _mask);

            // right foot check
            Vector2 rightCheckPos = new Vector2(x + (_data.Width / 4), y - (_data.Height / 2));
            Vector2 rightCheckSize = new Vector2(_data.Width / 2, _distT);
            bool isRight = GroundCheck(rightCheckPos, rightCheckSize, _mask);

            if(isLeft && isRight) return SideState.Both;
            if(isLeft) return SideState.Left;
            if(isRight) return SideState.Right;
            return SideState.None;   
        }

        /// <summary>
        /// Checks the left and right of a character's collision box for ground. Both wall states
        /// are the same size, with the width being the distance threshold, and the height being
        /// the character height minus the distance threshold. The wall checks are done to the left
        /// and right of the character's collision box respectively.
        /// </summary>
        /// <param name="climbCheck"> If true, checks for climbable walls rather than ground.
        /// </param>
        /// <returns> The SideState representing the character's wall state. </returns>
        private SideState CheckForWall(bool climbCheck)
        {
            LayerMask mask = climbCheck ? _climbMask : _mask; 
            float x = transform.position.x;
            float y = transform.position.y;
            
            // left wall check
            Vector2 leftPos = new Vector2(x - (_data.Width / 2), y); // subtract x
            Vector2 leftSize = new Vector2(_distT, _data.Height - _distT);
            bool isLeft = GroundCheck(leftPos, leftSize, mask);

            // right wall check
            Vector2 rightPos = new Vector2(x + (_data.Width / 2), y); // add x
            Vector2 rightSize = new Vector2(_distT, _data.Height - _distT);
            bool isRight = GroundCheck(rightPos, rightSize, mask);

            if(isLeft && isRight) return SideState.Both;
            if(isLeft) return SideState.Left;
            if(isRight) return SideState.Right;
            return SideState.None;   
        }

        /// <summary>
        /// Checks the top of the character's collision box for ground. The width of the check is
        /// the width of the character, while the height is the distance threshold.
        /// </summary>
        /// <returns> True if the top of the character's collision box is considered to be touching
        /// ground. </returns>
        private bool CheckForRoof()
        {
            float x = transform.position.x;
            float y = transform.position.y;

            Vector2 roofPos = new Vector2(x, y + (_data.Height / 2));
            Vector2 roofSize = new Vector2(_data.Width - _distT, _distT);
            return GroundCheck(roofPos, roofSize, _mask);
        }

        /// <summary>
        /// Calculates GroundState. As well, checks if OnFloor and OnFloorExit should be called,
        /// then calls them.
        /// </summary>
        /// <returns> The character's current GroundState. </returns>
        private GroundState ProcessGroundState()
        {
            if(FloorCheck)
            {
                if(!_onFloorLastFrame)
                {
                    Floored?.Invoke(this, null);
                }
                _onFloorLastFrame = true;
                return GroundState.Floor;
            }

            if(_onFloorLastFrame)
            {
                FloorExited?.Invoke(this, null);
            }
            _onFloorLastFrame = false;

            if(IsTouchingClimb) return GroundState.Wall;
            
            // air check...? lol
            return GroundState.Air;
        }
    } 
}
