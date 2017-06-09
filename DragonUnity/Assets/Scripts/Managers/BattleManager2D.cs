using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager2D : Singleton<BattleManager2D>
{
    //player charactor position
    public Vector2 PCPos { get; set; }

    private Action<Vector2> OnCameraPosUpdated;
    private Action<Vector2, Vector2> OnSceneBoundaryUpdated;
    private ParallexController _currScene;

    public void SetupParallexScene(ParallexController paraCtrlScene)
    {
        _currScene = paraCtrlScene;
        if (OnSceneBoundaryUpdated != null && _currScene)
            OnSceneBoundaryUpdated(_currScene.SceneCenter, _currScene.SceneSize);
    }

    public void UpdateCameraPos(Vector2 camPos)
    {
        if (OnCameraPosUpdated != null)
            OnCameraPosUpdated(camPos);
    }

    public void AddCamPosUpdateListoner(Action<Vector2> act)
    {
        OnCameraPosUpdated += act;
    }

    public void RemoveCamPosUpdateListoner(Action<Vector2> act)
    {
        OnCameraPosUpdated -= act;
    }

    public void AddSceneBoundaryUpdateListoner(Action<Vector2, Vector2> act)
    {
        OnSceneBoundaryUpdated += act;
        if (_currScene)
            act(_currScene.SceneCenter, _currScene.SceneSize);
    }

    public void RemoveSceneBoundaryUpdateListoner(Action<Vector2, Vector2> act)
    {
        OnSceneBoundaryUpdated -= act;
    }

    public void ClearAllListoners()
    {
        if (OnCameraPosUpdated != null) {
            var invocList = OnCameraPosUpdated.GetInvocationList();
            for (int i = 0; i < invocList.Length; i++) {
                OnCameraPosUpdated -= invocList[i] as Action<Vector2>;
            }
        }
        if (OnSceneBoundaryUpdated != null) {
            var invocList = OnSceneBoundaryUpdated.GetInvocationList();
            for (int i = 0; i < invocList.Length; i++) {
                OnSceneBoundaryUpdated -= invocList[i] as Action<Vector2, Vector2>;
            }
        }
    }
}
