using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameDefine
{
    public const float CameraDepth = -100;

    public const int InvalidId = -1;
    public const int GroundLayer = 8;
    public const int GroundLayerMask = 1 << GroundLayer;

    public const float GroundCheckRadius = 0.25f;
    public const float StandardTopSpeed = 16;
    public const float StandardRunAcc = 96;
    public const float StandardGlideAcc = 16;
    public const float StandardGlideFallSpeed = -1;
    public const float StandardJumpSpeed = 30;
    public const float GravityScale = 8;
    //public const float Gravity = -30f;
}
