using Fusion;
using UnityEngine;

public enum PlayerInputButton
{
    Jump = 0,
    Sprint = 1,
    Strafe = 2,
    SkillQ = 3,
    SkillE = 4,
    Fire = 5
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 MoveInput;
    public Vector3 CameraForward;
    public Vector3 CameraRight;
    public NetworkButtons Buttons;
}