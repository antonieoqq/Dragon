using System;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

[RequireComponent(typeof(UnityArmatureComponent))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    public bool IsFacingRight {
        get {
            if (_armatureComponent != null) {
                return !_armatureComponent.armature.flipX;
            }
            return false;
        }
    }
    public Vector2 CenterPos { get { return _BBCollider.offset + (Vector2)transform.position; } }
    public bool IsGrounded { get; private set; }
    public bool IsRising { get { return _rigidBody.velocity.y > 0; } }

    private UnityArmatureComponent _armatureComponent = null;
    private DragonBones.AnimationState _animState = null;
    private Rigidbody2D _rigidBody = null;
    private BoxCollider2D _BBCollider = null;

    private float _validAcc { get { return IsGrounded ? GameDefine.StandardRunAcc : GameDefine.StandardGlideAcc; } }
    private bool _isJumping = false;
    private bool _doJumpBurst = false;
    private bool _doGlide = false;

    private int _moveDir = 0;
    private float _horiSpeed = 0;

    void Start()
    {
        _armatureComponent = GetComponent<UnityArmatureComponent>();
        _armatureComponent.AddEventListener(EventObject.FADE_IN_COMPLETE, AnimationEventHandler);
        _armatureComponent.AddEventListener(EventObject.FADE_OUT_COMPLETE, AnimationEventHandler);

        _animState = _armatureComponent.animation.FadeIn("idle");
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.freezeRotation = true;
        _rigidBody.gravityScale = GameDefine.GravityScale;
        _BBCollider = GetComponent<BoxCollider2D>();

        PlayerInputManager.Instance.AddLeftJoystickListoner(LeftJoystickHandler);
        PlayerInputManager.Instance.AddButtonCommandListoner(ButtonCommandHandler);
    }

    void FixedUpdate()
    {
        GroundCheck();
    }

    void Update()
    {
        UpdatePosition();
        UpdateFaceDirection();
        UpdateAnimation();

        BattleManager2D.Instance.PCPos = CenterPos;
    }

    void OnDestroy()
    {
        PlayerInputManager.Instance.RemoveLeftJoystickListoner(LeftJoystickHandler);
        PlayerInputManager.Instance.RemoveButtonCommandListoner(ButtonCommandHandler);
    }

    void GroundCheck()
    {
        Vector2 p1 = new Vector2(transform.position.x - _BBCollider.size.x / 2, transform.position.y + GameDefine.GroundCheckRadius);
        Vector2 p2 = new Vector2(transform.position.x + _BBCollider.size.x / 2, transform.position.y + GameDefine.GroundCheckRadius);
        Vector2 p3 = new Vector2(transform.position.x + _BBCollider.size.x / 2, transform.position.y - GameDefine.GroundCheckRadius);
        Vector2 p4 = new Vector2(transform.position.x - _BBCollider.size.x / 2, transform.position.y - GameDefine.GroundCheckRadius);
        Debug.DrawLine(p1, p2, Color.cyan);
        Debug.DrawLine(p2, p3, Color.cyan);
        Debug.DrawLine(p3, p4, Color.cyan);
        Debug.DrawLine(p4, p1, Color.cyan);

        var hit = Physics2D.OverlapArea(p1, p3, GameDefine.GroundLayerMask);

        //var hit = Physics2D.OverlapCircle(transform.position, _groundCheckRadius, GameDefine.GroundLayerMask);

        IsGrounded = hit != null;
        //_rigidBody.gravityScale = IsGrounded ? 2 : 5;
        Debug.DrawLine(transform.position, transform.position + Vector3.down, IsGrounded ? Color.green : Color.red);

    }

    void UpdatePosition()
    {
        if (_moveDir == 0 ) {
            if (_horiSpeed != 0) {
                bool moveRight = _horiSpeed > 0;
                if (_rigidBody.velocity.x == 0 || (!moveRight && _horiSpeed > 0) || (moveRight && _horiSpeed < 0))
                    _horiSpeed = 0;
                else
                    _horiSpeed += Time.deltaTime * _validAcc * (_horiSpeed < 0 ? 2 : -2);
            }
        }
        else {
            _horiSpeed += Time.deltaTime * _moveDir * _validAcc * (_moveDir * _horiSpeed < 0 ? 2 : 1);
            _horiSpeed = Mathf.Clamp(_horiSpeed, - GameDefine.StandardTopSpeed, GameDefine.StandardTopSpeed);
        }

        float vertSpeed = _rigidBody.velocity.y;
        if (_doJumpBurst) {
            vertSpeed = GameDefine.StandardJumpSpeed;
            _doJumpBurst = false;
        }
        if (_doGlide && vertSpeed < GameDefine.StandardGlideFallSpeed) {
            vertSpeed = GameDefine.StandardGlideFallSpeed;
        }
        _rigidBody.velocity = new Vector2(_horiSpeed, vertSpeed);

        //if (IsGrounded) {
        //    _currSpeed.y = 0;
        //    //var groundhit = Physics2D.Raycast(hit.point, Vector2.up, Mathf.Infinity, GameDefine.GroundLayerMask);
        //    //if (groundhit.collider) {
        //    //    transform.localPosition += Vector3.up * (groundhit.point - hit.point).y;
        //    //}
        //}
        //else {
        //    _currSpeed.y += GameDefine.Gravity * Time.deltaTime;
        //}

        //transform.localPosition += (Vector3)(_currSpeed * Time.deltaTime);
    }

    void UpdateFaceDirection()
    {
        if ((_moveDir > 0 && !IsFacingRight) || (_moveDir < 0 && IsFacingRight)) {
            _armatureComponent.armature.flipX = _moveDir < 0;
        }
    }

    void UpdateAnimation()
    {
        if (IsGrounded) {
            if (_isJumping) {
                if (_animState.name == "jump_4") return;
                if (_animState.name == "jump_3") {
                    _animState = _armatureComponent.animation.FadeIn("jump_4", -1, 1);
                    return;
                }
                if (!IsRising && _animState.name == "jump_2") {
                    _animState = _armatureComponent.animation.FadeIn("jump_3");
                }
            }

            else if (_moveDir == 0 && _animState.name != "idle") {
                _animState = _armatureComponent.animation.FadeIn("idle");
            }

            else if (_moveDir != 0 && _animState.name != "walk") {
                _animState = _armatureComponent.animation.FadeIn("walk", 0.016f);
            }

            if (_animState.name == "walk")
                _animState.timeScale = Mathf.Abs(4 * _horiSpeed / GameDefine.StandardTopSpeed);
        }
        else {
            if (!IsRising && _animState.name != "jump_3") {
                _isJumping = true;
                Debug.DrawLine(transform.position, (Vector2)transform.position + _rigidBody.velocity);
                _animState = _armatureComponent.animation.FadeIn("jump_3");
            }
        }
    }

    void LeftJoystickHandler(Vector2 leftJoystick)
    {
        _moveDir = (int)leftJoystick.x;
    }


    void ButtonCommandHandler(InputCommandState inCmdState)
    {
        //Debug.Log(inCmdState.InputCommand.ToString() + " " + inCmdState.CurrentState.ToString() + " " + inCmdState.HoldTime.ToString());
        switch (inCmdState.InputCommand) {
            case EInputCommand.None:
                break;
            case EInputCommand.Attack:
                break;
            case EInputCommand.Dodge:
                break;
            case EInputCommand.LiftUp:
                if (IsGrounded) {
                    _doGlide = false;
                    if (inCmdState.CurrentState == EInputState.Press) {
                        _isJumping = true;
                        _animState = _armatureComponent.animation.FadeIn("jump_1");
                    }
                }
                else {
                    _doGlide = (inCmdState.CurrentState != EInputState.Release);
                }
                break;
            case EInputCommand.Skill:
                break;
            case EInputCommand.Item:
                break;
            case EInputCommand.Burst:
                break;
            default:
                break;
        }
    }

    void AnimationEventHandler(string type, EventObject eventObject)
    {
        //Debug.Log(type + " : " + eventObject.animationState.name);
        switch (type) {
            case EventObject.FADE_IN_COMPLETE:
                if (eventObject.animationState.name == "jump_1") {
                    if (IsGrounded) {
                        _doJumpBurst = true;
                        _animState = _armatureComponent.animation.FadeIn("jump_2");
                        //_rigidBody.velocity = new Vector2(_rigidBody.velocity.x, _jumpSpeed);
                    }
                }
                else if (eventObject.animationState.name == "jump_4") {
                    _isJumping = false;
                }
                break;
            case EventObject.FADE_OUT_COMPLETE:
                break;
            default:
                break;
        }
    }


    //void OnTriggerEnter2D(Collider2D collider2D)
    //{
    //    if (1 << collider2D.gameObject.layer == GameDefine.GroundLayerMask) {
    //        Debug.Log(collider2D);
    //    }
    //}

    //void OnTriggerStay2D(Collider2D collider2D)
    //{
    //    if (1 << collider2D.gameObject.layer == GameDefine.GroundLayerMask) {
    //        Debug.Log(collider2D);
    //    }
    //}

    //void OnCollisionEnter2D(Collision2D coll)
    //{
    //    if (coll.collider) {
    //        if (coll.collider.gameObject.layer == GameDefine.GroundLayer) {
    //            collisionCount++;
    //        }
    //    }
    //}

    //void OnCollisionStay2D(Collision2D coll)
    //{
    //    if (coll.contacts.Length == 0) {
    //        return;
    //    }
    //    else if (coll.contacts.Length == 1) {
    //        var rayHit = Physics2D.Raycast(coll.contacts[0].point, Vector2.up, 100, GameDefine.GroundLayerMask);
    //        if (rayHit.collider != null) {
    //            //Debug.DrawLine(rayHit.point, coll.contacts[0].point);
    //            var offset = (rayHit.point - coll.contacts[0].point);
    //            offset.x = 0;
    //            transform.position += (Vector3)offset;
    //        }
    //    }
    //    else {
    //        for (int i = 0; i < coll.contacts.Length; i++) {
    //            int j = i + 1;
    //            if (j >= coll.contacts.Length) {
    //                break;
    //            }
    //            //Debug.DrawLine(coll.contacts[i].point, coll.contacts[j].point);
    //        }

    //    }

    //}

    //void OnCollisionExit2D(Collision2D coll)
    //{
    //    if (coll.collider) {
    //        if (coll.collider.gameObject.layer == GameDefine.GroundLayer) {
    //            collisionCount--;
    //        }
    //    }
    //}

}
