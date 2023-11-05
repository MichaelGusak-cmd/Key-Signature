using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// Controllers are components in Ascension that take a character's input, and use it to
    /// affect the state of other character components. They are typically the central component in
    /// one of the Character System's namespaces (i.e. the movement controllers in the movement
    /// namespace are the primary components). <br/>
    /// There are three controller types; movement, ability, and combat. <br/>
    /// Movement is the primary controller type, processing input to move a character. Movement
    /// controllers are assumed to be on by default, with other controllers able to take over
    /// when needed.
    /// Ability controllers represent secondary movement options characters can have. <br/>
    /// Combat controllers allow characters to attack each other. <br/>
    /// The combat namespace also includes a stun mechanic, which overrides every controller
    /// temporarily.<br/>
    /// </summary>
    public abstract class Controller : MonoBehaviour
    {
        /// <summary>
        /// Reference to a character's Character component.
        /// </summary>
        [SerializeField] protected Character _character;

        /// <summary>
        /// Invoked when the Interrupt() method is called. Returns the length of the interrupt.
        /// </summary>
        public event EventHandler<float> Interrupted;

        /// <summary>
        /// Returns if the controller is interrupted. A controller does not run when it is
        /// interrupted. <br/>
        /// Derived classes are required to implement the functionality of this
        /// property.
        /// </summary>
        public bool IsInterrupted { private set; get; }

        /// <summary>
        /// Returns if the controller is active. Inactive controllers do not process anything
        /// while they're inactive. IsActive is set through OnActivation and OnDeactivation events
        /// in the Character component, but can also be set manually by other components, such as
        /// when the character dies.
        /// </summary>
        public bool IsActive { set; protected get; }

        private Coroutine _interruption;
        private float _interruptTimer;

        /// <summary>
        /// Disables/enables the controller. Default functionality is simply to set the active
        /// state of the game object the controller is on via gameObject.SetActive();
        /// </summary>
        public virtual void Enable(bool isEnabled)
        {
            gameObject.SetActive(isEnabled);
        }

        /// <summary>
        /// Processes the input for a character for the current frame. Called on the first line of
        /// a components Update() call. Intended to ease implementation of ReadInput().
        /// </summary>
        protected abstract void CacheInput();

        /// <summary>
        /// Temporarily halts the execution of the controller. If the controller is currently being
        /// interrupted, the old interrupt will be overriden.
        /// </summary>
        /// <param name="length">
        /// The amount of time the controller will be halted for (in seconds).
        /// </param>
        public void Interrupt(float length)
        {
            // stop old interrupt if it exists
            if (_interruption != null)
                StopCoroutine(_interruption);
            
            _interruption = StartCoroutine(InterruptController(length));
            OnInterrupted(length);

            // this implementation of interrupting is pretty bad but i wanted a solution that
            // was affected by Time.timeScale, so async/await wouldn't work
            IEnumerator InterruptController(float seconds)
            {
                IsInterrupted = true;
                _interruptTimer = 0;
                while (_interruptTimer < seconds)
                {
                    _interruptTimer += Time.deltaTime;
                    yield return null;
                }
                IsInterrupted = false;
                _interruption = null;
            }
        }

        /// <summary>
        /// Invokes the Interrupted event with the given length.
        /// </summary>
        /// <param name="length">
        /// The length of time of the interrupt (in seconds)
        /// </param>
        protected virtual void OnInterrupted(float length) => Interrupted?.Invoke(this, length);
    } 
}

