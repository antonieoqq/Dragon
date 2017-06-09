using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 游戏的最初起点，最高执行优先级的脚本
// 负责在启动时初始化常驻的核心管理类PersistentManager，以及其他所有管理类单件
public class GameInitialize : MonoBehaviour
{
    void Awake()
    {
        ManagerRoot.Instance.Init();

        KeyboardInputManager.Instance.Init();
        PlayerInputManager.Instance.Init();
    }
}
