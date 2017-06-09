using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraContoller2D : MonoBehaviour
{
    public float FollowSpeed = 5;
    public Vector2 FrustumRect {
        get { return _frustumRect; }
        private set { _frustumRect = value; }
    }

    private Camera _cam;
    private Vector2 _frustumRect;
    private Vector2 _validBottomLeft;
    private Vector2 _validTopRight;

    void Awake()
    {
        //BattleManager2D.Instance.OnSceneBoundaryUpdated += OnSceneBoundaryUpdated;
        _cam = GetComponent<Camera>();
        transform.position = Vector3.forward * GameDefine.CameraDepth;
        _frustumRect.y = _cam.orthographicSize * 2; //2.0f * (-GameDefine.CameraDepth) * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        _frustumRect.x = _frustumRect.y * _cam.aspect;
        BattleManager2D.Instance.AddSceneBoundaryUpdateListoner(OnSceneBoundaryUpdated);
    }

    void Start()
    {
    }

    void LateUpdate()
    {
        var validTargetPosition = new Vector2(Mathf.Clamp(BattleManager2D.Instance.PCPos.x, _validBottomLeft.x, _validTopRight.x),
                                            Mathf.Clamp(BattleManager2D.Instance.PCPos.y, _validBottomLeft.y, _validTopRight.y));
        var planePos = Vector2.Lerp(transform.position, validTargetPosition, Time.deltaTime * FollowSpeed);
        transform.position = new Vector3(planePos.x, planePos.y, transform.position.z);

        BattleManager2D.Instance.UpdateCameraPos(transform.position);
    }

    void OnDestroy()
    {
        BattleManager2D.Instance.RemoveSceneBoundaryUpdateListoner(OnSceneBoundaryUpdated);
        //BattleManager2D.Instance.OnSceneBoundaryUpdated -= OnSceneBoundaryUpdated;
    }

    private void OnSceneBoundaryUpdated(Vector2 sceneCenter, Vector2 sceneSize)
    {
        _validBottomLeft = new Vector2(sceneCenter.x - (sceneSize.x - _frustumRect.x) * 0.5f, sceneCenter.y - (sceneSize.y - _frustumRect.y) * 0.5f);
        _validTopRight = new Vector2(sceneCenter.x + (sceneSize.x - _frustumRect.x) * 0.5f, sceneCenter.y + (sceneSize.y - _frustumRect.y) * 0.5f); 
        if (_validBottomLeft.x > _validTopRight.x) {
            _validBottomLeft.x = 0;
            _validTopRight.x = 0;
        }
        if (_validBottomLeft.y > _validTopRight.y) {
            _validBottomLeft.y = 0;
            _validTopRight.y = 0;
        }
        Debug.Log(_validBottomLeft.ToString() + "   " + _validTopRight.ToString());
    }
}
