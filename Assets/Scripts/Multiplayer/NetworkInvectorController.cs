using Fusion;
using UnityEngine;
using Invector.vCharacterController;

public class NetworkInvectorController : NetworkBehaviour
{
    [SerializeField] private vThirdPersonController controller;

    [Networked]
    private NetworkButtons PreviousButtons { get; set; }

    private Transform networkCameraReference;

    private LineFireSkill lineFireSkill;
    private BlackHoleSkill blackHoleSkill;
    private FireBallSkill fireBallSkill;
    private FireBulletBuffSkill fireBulletBuffSkill;
    private SmokeSkill smokeSkill;
    private LaserUltimateSkill laserUltimateSkill;
    private PlayerAim playerAim;
    private GunFireFX gunFireFX;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<vThirdPersonController>();

        lineFireSkill = GetComponentInChildren<LineFireSkill>(true);
        blackHoleSkill = GetComponentInChildren<BlackHoleSkill>(true);
        fireBallSkill = GetComponentInChildren<FireBallSkill>(true);

        fireBulletBuffSkill =
            GetComponentInChildren<FireBulletBuffSkill>(true);

        smokeSkill = GetComponentInChildren<SmokeSkill>(true);

        laserUltimateSkill =
            GetComponentInChildren<LaserUltimateSkill>(true);

        playerAim = GetComponentInChildren<PlayerAim>(true);
        gunFireFX = GetComponentInChildren<GunFireFX>(true);

        GameObject reference =
            new GameObject("NetworkCameraReference");

        reference.transform.SetParent(transform, false);
        networkCameraReference = reference.transform;
    }

    public override void Spawned()
    {
        if (controller == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy vThirdPersonController."
            );
            return;
        }

        controller.Init();
        controller.enabled = Object.HasStateAuthority;

        Debug.Log(
            $"{name}: InputAuthority={Object.HasInputAuthority}, " +
            $"StateAuthority={Object.HasStateAuthority}, " +
            $"ControllerEnabled={controller.enabled}"
        );
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || controller == null)
            return;

        if (!GetInput(out NetworkInputData inputData))
        {
            controller.input = Vector3.zero;
            RunInvector(null);
            return;
        }

        SetupCameraReference(inputData);

        Vector2 move = inputData.MoveInput;

        controller.input =
            new Vector3(move.x, 0f, move.y);

        NetworkButtons pressed =
            inputData.Buttons.GetPressed(PreviousButtons);

        NetworkButtons released =
            inputData.Buttons.GetReleased(PreviousButtons);

        if (pressed.IsSet((int)PlayerInputButton.Jump) &&
            CanJump())
        {
            controller.Jump();
        }

        if (pressed.IsSet((int)PlayerInputButton.Strafe))
            controller.Strafe();

        if (pressed.IsSet((int)PlayerInputButton.Sprint))
            controller.Sprint(true);

        if (released.IsSet((int)PlayerInputButton.Sprint))
            controller.Sprint(false);

        Vector3 aimDirection = GetAimDirection(inputData);

        if (pressed.IsSet((int)PlayerInputButton.SkillQ))
            RPC_UseSkillQ(aimDirection);

        if (pressed.IsSet((int)PlayerInputButton.SkillE))
            RPC_UseSkillE(aimDirection);

        if (pressed.IsSet((int)PlayerInputButton.Fire))
        {
            RPC_SetFireVisual(true);
            RPC_Shoot(aimDirection);
        }

        if (released.IsSet((int)PlayerInputButton.Fire))
            RPC_SetFireVisual(false);

        PreviousButtons = inputData.Buttons;

        RunInvector(networkCameraReference);
    }

    private void SetupCameraReference(NetworkInputData inputData)
    {
        if (networkCameraReference == null)
            return;

        Vector3 forward = inputData.CameraForward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = transform.forward;

        forward.Normalize();

        networkCameraReference.position = transform.position;
        networkCameraReference.rotation =
            Quaternion.LookRotation(forward, Vector3.up);

        controller.rotateTarget = networkCameraReference;
    }

    private Vector3 GetAimDirection(NetworkInputData inputData)
    {
        Vector3 direction = inputData.CameraForward;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        return direction.normalized;
    }

    private void RunInvector(Transform cameraReference)
    {
        controller.UpdateMotor();
        controller.ControlRotationType();
        controller.UpdateMoveDirection(cameraReference);
        controller.ControlLocomotionType();
        controller.UpdateAnimator();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UseSkillQ(Vector3 aimDirection)
    {
        if (lineFireSkill != null)
        {
            lineFireSkill.TryUseFromNetwork(aimDirection);
            return;
        }

        if (blackHoleSkill != null)
        {
            blackHoleSkill.TryUseFromNetwork(aimDirection);
            return;
        }

        if (fireBallSkill != null)
            fireBallSkill.TryUseFromNetwork(aimDirection);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UseSkillE(Vector3 aimDirection)
    {
        if (fireBulletBuffSkill != null)
        {
            fireBulletBuffSkill.TryUseFromNetwork(aimDirection);
            return;
        }

        if (smokeSkill != null)
        {
            smokeSkill.TryUseFromNetwork(aimDirection);
            return;
        }

        if (laserUltimateSkill != null)
            laserUltimateSkill.TryUseFromNetwork(aimDirection);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Shoot(Vector3 aimDirection)
    {
        if (gunFireFX != null)
            gunFireFX.ShootFromNetwork(aimDirection);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetFireVisual(bool fireHeld)
    {
        if (playerAim != null)
            playerAim.SetNetworkAim(fireHeld);
    }

    private bool CanJump()
    {
        return controller.isGrounded &&
               controller.GroundAngle() < controller.slopeLimit &&
               !controller.isJumping &&
               !controller.stopMove;
    }

    private void OnAnimatorMove()
    {
        if (Object == null ||
            !Object.IsValid ||
            !Object.HasStateAuthority ||
            controller == null)
        {
            return;
        }

        controller.ControlAnimatorRootMotion();
    }
}