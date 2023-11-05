using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters;


namespace Characters.Movement
{
    /// <summary>
    /// Base class for character movement. Lets other classes interact with all character
    /// controllers.
    /// </summary>
    public abstract class MovementController : Controller
    {   
        [Tooltip("Dependency from MovementController. Allows access to the Velocity component")]
        [SerializeField] 
        protected Velocity _velocity;
        
        [Tooltip("Dependency from MovementController. Allows access to a character's data.")]
        [SerializeField] 
        protected CharacterData _data;

        public Vector2 LocalVelocity => _velocity.Local;
        /// <summary>
        /// Provides the last non-zero X input a character has done.
        /// </summary>
        public int LastX
        {
            get => _lastX;
            protected set
            {
                if(value != _lastX)
                {
                    _lastX = value;
                }
            }
        }

        private int _lastX = 1;
        /// <summary>
        /// Prevents a Movement Controller from running while true. Intended to be set by abilities
        /// that bypass a character's standard movement.
        /// </summary>
        public virtual bool AbilityOverride { get; set; }

        /// <summary>
        /// Allows direct access to a character's local velocity.
        /// </summary>
        public virtual void SetVelocity(Vector2 velocity)
        {
            _velocity.Local = velocity;
        }

        /// <summary>
        /// Resets a movement controller to it's default state during character activation.
        /// </summary>
        protected virtual void character_Activated(object sender, EventArgs e)
        {
            AbilityOverride = false;
            SetVelocity(Vector2.zero);
        }
    }
}
    
