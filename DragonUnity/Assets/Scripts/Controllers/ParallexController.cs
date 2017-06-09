using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 控制场景多层的视差滚动
[RequireComponent(typeof(BoxCollider2D))]
public class ParallexController : MonoBehaviour
{
    [Range(-1, 1)]
    public float CloseOffsetFactor = -0.5f;
    [Range(-1, 1)]
    public float MiddleOffsetFactor = 0.2f;
    [Range(-1, 1)]
    public float FarOffsetFactor = 0.95f;

    public Vector2 SceneCenter { get { return BBox.offset; } }
    public Vector2 SceneSize { get { return BBox.size; } }

    private BoxCollider2D BBox;
    private Transform _closePivot;
    private Transform _focusPivot;
    private Transform _middlePivot;
    private Transform _farPivot;
    private Transform _horizonPivot;

    void Awake()
    {
        //BattleManager2D.Instance.OnCameraPosUpdated += OnCameraPosUpdated;
        BattleManager2D.Instance.AddCamPosUpdateListoner(OnCameraPosUpdated);
    }

    // Use this for initialization
    void Start()
    {
        transform.position = Vector3.zero;
        BBox = GetComponent<BoxCollider2D>();

        _closePivot = transform.FindChild("CloseLayer");
        _closePivot.position = Vector3.forward * -1;

        _focusPivot = transform.FindChild("FocusLayer");
        _focusPivot.position = Vector3.forward * 1;

        _middlePivot = transform.FindChild("MiddleLayer");
        _middlePivot.position = Vector3.forward * 2;

        _farPivot = transform.FindChild("FarLayer");
        _farPivot.position = Vector3.forward * 3;

        _horizonPivot = transform.FindChild("HorizonLayer");
        _horizonPivot.position = Vector3.forward * 4;

        BattleManager2D.Instance.SetupParallexScene(this);
        //BattleManager2D.Instance.OnSceneBoundaryUpdated(BBox.offset, BBox.size);
        //BattleManager2D.Instance.SetSceneBoundaries(BBox.offset, BBox.size);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        //BattleManager2D.Instance.OnCameraPosUpdated -= OnCameraPosUpdated;
        BattleManager2D.Instance.RemoveCamPosUpdateListoner(OnCameraPosUpdated);
        BattleManager2D.Instance.SetupParallexScene(null);
    }

    void OnCameraPosUpdated(Vector2 camPos)
    {
        _closePivot.position = new Vector3(camPos.x * CloseOffsetFactor, camPos.y * CloseOffsetFactor, _closePivot.position.z);
        _middlePivot.position = new Vector3(camPos.x * MiddleOffsetFactor, camPos.y * MiddleOffsetFactor, _middlePivot.position.z);
        _farPivot.position = new Vector3(camPos.x * FarOffsetFactor, camPos.y * FarOffsetFactor, _farPivot.position.z);
        _horizonPivot.position = new Vector3(camPos.x, camPos.y, _horizonPivot.position.z);
    }
}
