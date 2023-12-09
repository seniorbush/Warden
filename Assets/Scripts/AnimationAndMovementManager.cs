using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PM
{
    public class AnimationAndMovementManager : MonoBehaviour

    {

        // REFERENCE VARIABLES
        public CharacterController characterController;
        public Animator animator;
        PlayerControls playerControls;


        // INPUT SYSTEM
        [Header("PLAYER MOVEMENT INPUT")]
        [SerializeField] Vector2 movementInput;
        [SerializeField] float horizontalInput;
        [SerializeField] float verticalInput;
        public float moveAmount;


        [Header("CAMERA MOVEMENT INPUT")]
        [SerializeField] Vector2 cameraMovementInput;
        public float cameraHorizontalInput;
        public float cameraVerticalInput;


        [Header("PLAYER ACTION INPUT")]
        [SerializeField] bool dodgeInput = false;
        [SerializeField] bool sprintInput = false;


        // VALUES FROM INPUTS
        private Vector3 movementDirection;
        private Vector3 targetRotationDirection;


        [Header("MOVEMENT SETTINGS")]
        [SerializeField] float walkingSpeed = 1f;
        [SerializeField] float runningSpeed = 2f;
        [SerializeField] float sprintingSpeed = 6f;
        [SerializeField] float roationSpeed = 10f;


        [Header("DODGE SETTINGS")]
        private Vector3 rollDirection;


        [Header("FLAGS")]
        public bool isPerformingAction = false;
        public bool applyRootMotion = false;
        public bool canRotate = true;
        public bool canMove = true;
        public bool isSprinting = false;


        [Header("HASHES")]
        private int horizontal;
        private int vertical;



        //
        //
        // 
        //
        //



        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            // STRING TO HASH
            horizontal = Animator.StringToHash("Horizontal");
            vertical = Animator.StringToHash("Vertical");
        }


        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();
                // READ THE PLAYERS LEFT STICK/WASD INPUT VALUE, STORE THE VALUE IN 'movementInput'
                playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

                // READ THE PLAYERS RIGHT STICK/MOUSED INPUT VALUE, STORE THE VALUE IN 'cameraInput'
                playerControls.PlayerCamera.Movement.performed += i => cameraMovementInput = i.ReadValue<Vector2>();

                // READ THE PLAYERS GAMEPAD EAST/C PRESSED INPUT VALUE, STORE THE VALUE IN 'dodgeInput'
                playerControls.PlayerActions.Dodge.performed += i => dodgeInput = true;

                // READ THE PLAYERS GAMEPAD EAST/L-SHIFT HELD INPUT VALUE, STORE THE VALUE IN 'sprintInput'
                playerControls.PlayerActions.Sprint.performed += i => sprintInput = true; // TRUE IF HELD
                playerControls.PlayerActions.Sprint.canceled += i => sprintInput = false; // FALSE IF RELEASED
            }

            playerControls.Enable();
        }


        private void Update()
        {
            HandlePlayerMovementInput();
            HandleCameraMovementInput();
            HandleDodgeInput();
            HandleSprintInput();
            HandleMovement();
            HandleRotation();
        }


        private void HandlePlayerMovementInput()
        {
            // SPLIT THE X AND Y INPUT VALUES
            verticalInput = movementInput.y;
            horizontalInput = movementInput.x;

            // RETURNS THE ABSOLUTE VALUE TO REDUCE ERRORS
            moveAmount = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));

            // CLAMPS THE MOVEMENT AMOUNT TO BE TARGET VALUE (0, 0.5, 1)
            if (moveAmount <= 0.5 && moveAmount > 0)
            {
                moveAmount = 0.5f;
            }
            else if (moveAmount > 0.5 && moveAmount <= 1)
            {
                moveAmount = 1f;
            }

            // HORIZONTAL = 0, 
            // HORIZONTAL ONLY USED WHEN STRAFING
            UpdateAnimatorMovementParameters(0, moveAmount, isSprinting);


            // IF LOCKED ON THEN PASS HORIZONTAL AND VERTICAL VALUES
        }


        private void HandleCameraMovementInput()
        {
            // SPLIT THE X AND Y INPUT VALUES
            cameraVerticalInput = cameraMovementInput.y;
            cameraHorizontalInput = cameraMovementInput.x;
        }


        private void HandleDodgeInput() // TO-DO: COMBINE WITH AttemptToPerformDodge
        {
            if (dodgeInput)
            {
                // FUTURE NOTE: RETURN (DO NOTHING) IF MENU OR UI WINDOW OPEN

                // PERFORM DODGE
                dodgeInput = false;
                AttemptToPerformDodge();
            }
        }


        private void HandleSprintInput() // TO-DO: COMBINE WITH HandlingSprinting
        {
            if (sprintInput)
            {
                HandleSprinting();
            }
            else
            {
                isSprinting = false;
            }
        }


        private void HandleMovement()
        {
            if (!canMove)
            {
                return;
            }

            // MOVEMENT DIRECTION BASED ON CAMERA DIRECTION AND PLAYER INPUT
            movementDirection = PlayerCamera.instance.transform.forward * verticalInput;//
            movementDirection = movementDirection + PlayerCamera.instance.transform.right * horizontalInput;//
            movementDirection.Normalize();
            movementDirection.y = 0;

            if (isSprinting)
            {
                characterController.Move(movementDirection * sprintingSpeed * Time.deltaTime);
            }
            else
            {
                if (moveAmount > 0.5f)
                {
                    characterController.Move(movementDirection * runningSpeed * Time.deltaTime);
                }
                else if (moveAmount <= 0.5f)
                {
                    characterController.Move(movementDirection * walkingSpeed * Time.deltaTime);
                }
            }
        }


        private void HandleRotation()
        {
            if (!canRotate)
            {
                return;
            }

            targetRotationDirection = Vector3.zero;
            targetRotationDirection = PlayerCamera.instance.cameraObject.transform.forward * verticalInput;//
            targetRotationDirection = targetRotationDirection + PlayerCamera.instance.cameraObject.transform.right * horizontalInput;//
            targetRotationDirection.Normalize();
            targetRotationDirection.y = 0;

            if (targetRotationDirection == Vector3.zero)
            {
                targetRotationDirection = transform.forward;
            }

            Quaternion newRotation = Quaternion.LookRotation(targetRotationDirection);
            Quaternion targetRoation = Quaternion.Slerp(transform.rotation, newRotation, roationSpeed * Time.deltaTime);
            transform.rotation = targetRoation;
        }


        public void HandleSprinting()
        {
            if (isPerformingAction)
            {
                return;
            }

            // IF NO STAMINA, SPRINTING = FALSE

            if (moveAmount > 0) // IF PLAYER IS MOVING, ENABLE SRPINT
            {
                isSprinting = true;
            }
        }


        private void AttemptToPerformDodge()
        {
            if (isPerformingAction)
            {
                return;
            }

            // IF MOVING, PERFORM ROLL
            if (moveAmount > 0)
            {
                rollDirection = PlayerCamera.instance.cameraObject.transform.forward * verticalInput;
                rollDirection += PlayerCamera.instance.cameraObject.transform.right * horizontalInput;
                rollDirection.y = 0;
                rollDirection.Normalize();

                Quaternion playerRotation = Quaternion.LookRotation(rollDirection);
                transform.rotation = playerRotation;

                //PERFORM ROLL
                PlayTargetActionAnimation("Roll_Forward_01", true, true, false, false);
            }
            // IF STATIONARY, PERFORM BACKSTEP
            else
            {
                //PERFORM BACKSTEP ANIMATION
                PlayTargetActionAnimation("Backstep_01", true, true, false, false);

            }
        }


        private void UpdateAnimatorMovementParameters(float horizontalMovement, float verticalMovement, bool isSprinting)
        {
            float horizontalValue = horizontalMovement;
            float verticalValue = verticalMovement;

            if (isSprinting)
            {
                verticalValue = 2f;
            }

            animator.SetFloat(horizontal, horizontalValue, 0.1f, Time.deltaTime); // TO-DO: CONVERT STRING TO HASH
            animator.SetFloat(vertical, verticalValue, 0.1f, Time.deltaTime); // TO-DO: CONVERT STRING TO HASH
        }


        public void PlayTargetActionAnimation(string targetAnimation, bool actionReady, bool rootMotionReady, bool rotateReady, bool moveReady)
        {
            animator.applyRootMotion = rootMotionReady;
            animator.CrossFade(targetAnimation, 0.2f);

            // USED TO STOP CHARACTER FROM ATTEMPTING A NEW ACTION DURING AN ACTION
            isPerformingAction = actionReady;
            applyRootMotion = rootMotionReady;
            canRotate = rotateReady;
            canMove = moveReady;
        }


        private void LateUpdate()
        {
            // HANDLE CAMERA UPDATES
            PlayerCamera.instance.TargetFollow();
            PlayerCamera.instance.TargetRotation();
            PlayerCamera.instance.HandleCollisions();
        }
    }



}