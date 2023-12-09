using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM
{
    public class PlayerCamera : MonoBehaviour
    {
        // CAMERA IMPLEMENTATION BASED ON SEBASTIAN GRAVES' ELDEN RING TUTORIAL



        public static PlayerCamera instance;
        public AnimationAndMovementManager player; // UPDATE IN FUTURE, SHOULD REFERENCE SEPERATE SCRIPT FOR PLAYER IDENTIFICATION
        public Camera cameraObject;
        [SerializeField] Transform cameraPivotTransform;


        [Header("CAMERA SETTINGS")]
        public float cameraSmoothSpeed = 1; // THE LARGER THE VALUE, THE LONGER FOR THE CAMERA TO REACH ITS POSITION
        [SerializeField] float leftAndRightRotationSpeed = 220f;
        [SerializeField] float upAndDownRotationSpeed = 220f;
        [SerializeField] float minimumPivot = -30f; // THE LOWEST POINT YOU CAN LOOK DOWN
        [SerializeField] float maximumPivot = 60f; // THE HIGHEST POINT YOU CAN LOOK UP
        [SerializeField] float cameraCollsionRadius = 0.2f;
        [SerializeField] LayerMask collideWithLayers;


        private Vector3 cameraVelocity;
        private Vector3 cameraObjectPosition; // USED FOR CAMERA COLLISIONS (RELOCATES CAMERA OBJECT ON COLLISION)
        [SerializeField] float leftAndRightLookAngle;
        [SerializeField] float upAndDownLookAngle;
        private float cameraZposition; // CAMERA COLLISION VALUES
        private float targetCameraZPosition; // CAMERA COLLISION VALUES


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                {
                    Destroy(gameObject);
                }
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            cameraZposition = cameraObject.transform.localPosition.z;
        }

        public void TargetFollow()
        {
            Vector3 targetCameraPosition = Vector3.SmoothDamp(transform.position, player.transform.position, ref cameraVelocity, cameraSmoothSpeed * Time.deltaTime);
            transform.position = targetCameraPosition;
        }

        public void TargetRotation()
        {
            // LOCK ON ROTATIONS



            // NORMAL ROTATIONS

            // ROTATE BASED ON HORIZONTAL INPUT
            leftAndRightLookAngle += (player.cameraHorizontalInput * leftAndRightRotationSpeed) * Time.deltaTime;
            // ROTATE BASED ON VERTICAL INPUT
            upAndDownLookAngle -= (player.cameraVerticalInput * upAndDownRotationSpeed) * Time.deltaTime;
            // CLAMP VERTICAL LOOK ANGLE
            upAndDownLookAngle = Mathf.Clamp(upAndDownLookAngle, minimumPivot, maximumPivot);

            Vector3 cameraRotation = Vector3.zero;
            Quaternion targetRotation;

            // ROTATE THE THIS GAME OBJECT LEFT/RIGHT [PlayerCamera]
            cameraRotation.y = leftAndRightLookAngle;
            targetRotation = Quaternion.Euler(cameraRotation);
            transform.rotation = targetRotation;

            // ROTATE THE PIVOT GAME OBJECT UP/DOWN [CameraPivot]
            cameraRotation = Vector3.zero;
            cameraRotation.x = upAndDownLookAngle;
            targetRotation = Quaternion.Euler(cameraRotation);
            cameraPivotTransform.localRotation = targetRotation;
        }

        public void HandleCollisions()
        {
            targetCameraZPosition = cameraZposition;
            RaycastHit hit;
            Vector3 direction = cameraObject.transform.position - cameraPivotTransform.position;
            direction.Normalize();


            // CHECK FOR OBJECTS WITHIN DESIRED DIRECTION
            if (Physics.SphereCast(cameraPivotTransform.position, cameraCollsionRadius, direction, out hit, Mathf.Abs(targetCameraZPosition), collideWithLayers))
            {
                // IF COLLIDED, GET DISTANCE
                float distanceFromHitObject = Vector3.Distance(cameraPivotTransform.position, hit.point);
                // UPDATE TARGET Z POSITION BASED ON COLLISION DISTANCE
                targetCameraZPosition = -(distanceFromHitObject - cameraCollsionRadius);
            }

            // IF TARGET Z POSITION LESS THAN COLLSION RADIUS SUBTRACT THE COLLISION RADIUS
            if (Mathf.Abs(targetCameraZPosition) < cameraCollsionRadius)
            {
                targetCameraZPosition = -cameraCollsionRadius;
            }

            // APPLY POSITION CHANGE
            cameraObjectPosition.z = Mathf.Lerp(cameraObject.transform.localPosition.z, targetCameraZPosition, 0.2f);
            cameraObject.transform.localPosition = cameraObjectPosition;
        }
    }
}