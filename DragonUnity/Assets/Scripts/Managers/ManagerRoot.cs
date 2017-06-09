using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 常驻的核心管理类
public class ManagerRoot : Singleton<ManagerRoot>
{
    public bool IsInited { get; private set; }

    private GameObject _rootObj;
    public GameObject RootObject {
        get {
            if (!_rootObj)
                _rootObj = new GameObject("[PersistentManager]");
            return _rootObj;
        }
    }

    private GameManager _gameManager;
    public GameManager GM {
        get {
            if (!_gameManager)
                _gameManager = RootObject.GetComponent<GameManager>();
                if (!_gameManager)
                    _gameManager = RootObject.AddComponent<GameManager>();

            return _gameManager;
        }
    }

    public ManagerRoot()
    {
        Init();
    }

    public void Init()
    {
        if (!IsInited) {
            _gameManager = RootObject.AddComponent<GameManager>();
            UnityEngine.Object.DontDestroyOnLoad(RootObject);
            IsInited = true;
        }
    }
}
