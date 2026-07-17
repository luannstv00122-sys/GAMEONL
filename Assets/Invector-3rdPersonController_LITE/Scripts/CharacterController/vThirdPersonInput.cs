using UnityEngine;
using UnityEngine.InputSystem;

namespace Invector.vCharacterController
{
    public class vThirdPersonInput : MonoBehaviour
    {
        [Header("Camera Input")]
        public float mouseSensitivity = 1f;

        [HideInInspector] public vThirdPersonController cc;
        [HideInInspector] public vThirdPersonCamera tpCamera;
        [HideInInspector] public Camera cameraMain;

        protected virtual void Start()
        {
            InitilizeController();
            InitializeTpCamera();
        }

        protected virtual void FixedUpdate()
        {
            cc.UpdateMotor();
            cc.ControlLocomotionType();
            cc.ControlRotationType();
        }

        protected virtual void Update()
        {
            InputHandle();
            cc.UpdateAnimator();
        }

        public virtual void OnAnimatorMove()
        {
            cc.ControlAnimatorRootMotion();
        }

        protected virtual void InitilizeController()
        {
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
                cc.Init();
        }

        protected virtual void InitializeTpCamera()
        {
            if (tpCamera == null)
            {
                tpCamera = FindFirstObjectByType<vThirdPersonCamera>();

                if (tpCamera == null)
                    return;

                tpCamera.SetMainTarget(transform);
                tpCamera.Init();
            }
        }

        protected virtual void InputHandle()
        {
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
            JumpInput();
        }

        public virtual void MoveInput()
        {
            if (Keyboard.current == null)
            {
                cc.input.x = 0f;
                cc.input.z = 0f;
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontal = -1f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontal = 1f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                vertical = 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                vertical = -1f;

            cc.input.x = horizontal;
            cc.input.z = vertical;
        }

        protected virtual void CameraInput()
        {
            if (!cameraMain)
            {
                if (!Camera.main)
                {
                    Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                }
                else
                {
                    cameraMain = Camera.main;
                    cc.rotateTarget = cameraMain.transform;
                }
            }

            if (cameraMain)
                cc.UpdateMoveDirection(cameraMain.transform);

            if (tpCamera == null)
                return;

            float x = 0f;
            float y = 0f;

            if (Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                x = mouseDelta.x * mouseSensitivity;
                y = mouseDelta.y * mouseSensitivity;
            }

            tpCamera.RotateCamera(x, y);
        }

        protected virtual void StrafeInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.tabKey.wasPressedThisFrame)
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
                cc.Sprint(true);

            if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
                cc.Sprint(false);
        }

        protected virtual bool JumpConditions()
        {
            return cc.isGrounded &&
                   cc.GroundAngle() < cc.slopeLimit &&
                   !cc.isJumping &&
                   !cc.stopMove;
        }

        protected virtual void JumpInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && JumpConditions())
                cc.Jump();
        }
    }
}