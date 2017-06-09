using UnityEngine;
using System.Collections;
using System;

public abstract class Singleton<T> where T : new()
{
    private static T _instance;
    static object _lock = new object();
    public static T Instance {
        get {
            if (_instance == null) {
                lock (_lock) {
                    if (_instance == null)
                        _instance = new T();
                }
            }
            return _instance;
        }
    }
}

// 各种管理器单件类的基类，继承后应该修饰为sealed，Instance类型T只能是第一级继承时的类型
// 多级继承后，Instance的接口调用不满足多态
public abstract class ManagerSingleton<T> : Singleton<T> where T : new()
{
    public bool IsInited { get; protected set; }

    private bool _isFixedUpdating = false;
    public bool IsFixedUpdating {
        get { return _isFixedUpdating; }
        set {
            if (_isFixedUpdating != value) {
                switch (value) {
                    case true: ManagerRoot.Instance.GM.AddFixedUpdateListoner(DoFixedUpdate); break;
                    case false: ManagerRoot.Instance.GM.RemoveFixedUpdateListoner(DoFixedUpdate); break;
                    default: break;
                }
                _isFixedUpdating = value;
            }
        }
    }

    private bool _isUpdating = false;
    public bool IsUpdating {
        get { return _isUpdating; }
        set {
            if (_isUpdating != value) {
                switch (value) {
                    case true: ManagerRoot.Instance.GM.AddUpdateListoner(DoUpdate); break;
                    case false: ManagerRoot.Instance.GM.RemoveUpdateListoner(DoUpdate); break;
                    default: break;
                }
                _isUpdating = value;
            }
        }
    }

    private bool _isLateUpdating = false;
    public bool IsLateUpdating {
        get { return _isLateUpdating; }
        set {
            if (_isLateUpdating != value) {
                switch (value) {
                    case true: ManagerRoot.Instance.GM.AddLateUpdateListoner(DoLateUpdate); break;
                    case false: ManagerRoot.Instance.GM.RemoveLateUpdateListoner(DoLateUpdate); break;
                    default: break;
                }
            }

        }
    }

    // 注意：子类请勿重写此函数，应通过实现InitExecute()方法执行具体的初始化行为
    // 初始化函数,每个Manager单件的初始化行为都应该在GameInitialize.cs里调用
    // 单件是否要监听GameManager的GameUpdate也建议在Init中初始化
    public virtual void Init()
    {
        if (!IsInited) {
            IsInited = true;
            InitExecute();
        }
    }

    // 具体的初始化执行函数，每个子类都必须各自实现此函数
    protected abstract void InitExecute();

    // 如果设置为isFixedUpdating，那么单件就会监听GameManager的GameFixedUpdate，在每帧FixedUpdate时执行DoFixedUpdate
    public virtual void DoFixedUpdate() { }

    // 如果设置为isUpdating，那么单件就会监听GameManager的GameUpdate，在每帧Update时执行DoUpdate
    public virtual void DoUpdate() { }

    public virtual void DoLateUpdate() { }
}
