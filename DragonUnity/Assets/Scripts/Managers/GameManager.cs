using UnityEngine;
using System;

// 游戏的总管理器，执行优先级仅次于GameInitialize.cs的脚本
// 可以认为是游戏运行时每帧第一个执行的脚本（GameInitialize只在游戏启动时执行各管理器单件的初始化工作）
public class GameManager : MonoBehaviour
{
    private Action GameFixedUpdate;
    private Action GameUpdate;
    private Action GameLateUpdate;

    void Awake()
    {
    }

    void FixedUpdate()
    {
        if (GameFixedUpdate != null)
            GameFixedUpdate();
    }

    void Update()
    {
        if (GameUpdate != null)
            GameUpdate();
    }

    void LateUpdate()
    {
        if (GameLateUpdate != null)
            GameLateUpdate();
    }

    public void AddFixedUpdateListoner(Action updateAct) { GameFixedUpdate += updateAct; }
    public void RemoveFixedUpdateListoner(Action updateAct) { GameFixedUpdate -= updateAct; }
    public void AddUpdateListoner(Action updateAct) { GameUpdate += updateAct; }
    public void RemoveUpdateListoner(Action updateAct) { GameUpdate -= updateAct; }
    public void AddLateUpdateListoner(Action updateAct) { GameLateUpdate += updateAct; }
    public void RemoveLateUpdateListoner(Action updateAct) { GameLateUpdate -= updateAct; }
}
