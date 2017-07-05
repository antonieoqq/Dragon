using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EDBCompMode
{
    Less,
    LessOrEqual,
    Equal,
    GreaterOrEqual,
    Greater,
    NotEqual,
}

public enum EDBCheckMode
{
    Compare,
    IsTrue,
}

//为了减少运行时反射检查类型，采用此枚举进行优化
public enum EDBParamType
{
    Scalar,
    Integer,
    Bool,
}

public class DBStateMachine
{

    public abstract class DBCondition
    {
        public EDBParamType CheckParamType { get; protected set; }
    }

    public class DBConditionScalar : DBCondition
    {
        public EDBCompMode CompMode { get; private set; }
        public float CompareValue { get; private set; }

        public DBConditionScalar(EDBCompMode mode, float v)
        {
            CompMode = mode;
            CompareValue = v;
            CheckParamType = EDBParamType.Scalar;
        }
    }

    public class DBConditionInteger : DBCondition
    {
        public EDBCompMode CompMode { get; private set; }
        public int CompareValue { get; private set; }

        public DBConditionInteger(EDBCompMode mode, int v)
        {
            CompMode = mode;
            CompareValue = v;
            CheckParamType = EDBParamType.Integer;
        }
    }

    public class DBConditionBool : DBCondition
    {
        public bool RequiredValue { get; private set; }

        public DBConditionBool(bool v)
        {
            RequiredValue = v;
            CheckParamType = EDBParamType.Bool;
        }
    }

    public class DBTransition
    {
        public Dictionary<string, DBCondition> Conditions { get; private set; }
        public bool HasExitTime;
        public float ExitTime;

        public DBTransition()
        {
            Conditions = new Dictionary<string, DBCondition>();
        }

        public void AddScalarCondition(string name, EDBCompMode compMode, float compValue)
        {
            if (!Conditions.ContainsKey(name))
                Conditions.Add(name, new DBConditionScalar(compMode, compValue));
        }

        public void AddIntegerCondition(string name, EDBCompMode compMode, int compValue)
        {
            if (!Conditions.ContainsKey(name))
                Conditions.Add(name, new DBConditionInteger(compMode, compValue));
        }

        public void AddBoolCondition(string name, bool compValue)
        {
            if (!Conditions.ContainsKey(name))
                Conditions.Add(name, new DBConditionBool(compValue));
        }

        public void RemoveCondition(string name)
        {
            Conditions.Remove(name);
        }
    }

    public class DBState
    {
        public Dictionary<string, DBTransition> Transitions { get; private set; }
        public float Speed = 1;

        public DBState()
        {
            Transitions = new Dictionary<string, DBTransition>();
        }

        public DBTransition AddTransition(string targetStateName)
        {
            var newTrans = new DBTransition();
            Transitions.Add(targetStateName, newTrans);
            return newTrans;
        }

        public void RemoveTransition(string targetStateName)
        {
            Transitions.Remove(targetStateName);
        }
    }

    // 为了避免box/unbox，将3种基本类型的参数封装成类
    public abstract class StateParamBase
    {
        public abstract bool IsComparable();
        public abstract bool IsBoolean();
    }

    public abstract class StateParam : StateParamBase
    {
        public StateParam(string name, EDBParamType type) { ParamName = name; ParamType = type; }

        public string ParamName { get; private set; }
        public EDBParamType ParamType { get; private set; }

        public override sealed bool IsComparable()
        {
            switch (ParamType) {
                case EDBParamType.Scalar:
                case EDBParamType.Integer:
                    return true;
                case EDBParamType.Bool:
                default:
                    return false;
            }
        }

        public override sealed bool IsBoolean()
        {
            return !IsComparable();
        }
    }

    public class ScalarParam : StateParam
    {
        public ScalarParam(string name, float v) : base(name, EDBParamType.Scalar)
        {
            ParamValue = v;
        }
        public float ParamValue;
    }

    public class IntParam : StateParam
    {
        public IntParam(string name, int v) : base(name, EDBParamType.Integer)
        {
            ParamValue = v;
        }
        public int ParamValue;
    }

    public class BoolParam : StateParam
    {
        public BoolParam(string name, bool v) : base(name, EDBParamType.Bool)
        {
            ParamValue = v;
        }
        public bool ParamValue;
    }

    public string OriginStateName = "";
    public DBState AnyState { get; private set; }

    private DragonBones.Animation _DBAnimation;
    private DragonBones.AnimationState _DBAnimState;

    private Dictionary<string, StateParam> _parameters = new Dictionary<string, StateParam>();
    private Dictionary<string, DBState> _states = new Dictionary<string, DBState>();
    private DBState _currentState = null;

    public DBStateMachine(DragonBones.Animation dbAnim)
    {
        AnyState = new DBState();
        _DBAnimation = dbAnim;
    }

    public void Update()
    {
        if (_states.Count < 0) return;

        if (_currentState == null) {
            if (OriginStateName.Length > 0 && _states.ContainsKey(OriginStateName)) {
                _currentState = _states[OriginStateName];
                _DBAnimation.FadeIn(OriginStateName);
            }
            else {
                Debug.LogError("[DBStateMachine] No origin state!\t----\t" + OriginStateName);
            }
        }

        if (_currentState == null) {
            Debug.LogError("[DBStateMachine] Current state is not valid!");
            return;
        }

        var transIter = _currentState.Transitions.GetEnumerator();
        while (transIter.MoveNext()) {
            var currTrans = transIter.Current.Value;
            var condIter = currTrans.Conditions.GetEnumerator();
            bool isAllPass = true;
            while (condIter.MoveNext()) {
                var checkParamName = condIter.Current.Key;
                var currCond = condIter.Current.Value;

                isAllPass = isAllPass && CheckParameterByCondition(checkParamName, currCond);
            }
        }
    }

    public bool DoseParamExist(string paramName)
    {
        return _parameters.ContainsKey(paramName);
    }

    public StateParam GetParam(string name)
    {
        if (DoseParamExist(name)) {
            return _parameters[name];
        }
        return null;
    }

    public ScalarParam AddScalarParam(string paramName, float paramValue)
    {
        if (!DoseParamExist(paramName)) {
            var newParam = new ScalarParam(paramName, paramValue);
            _parameters.Add(paramName, newParam);
            return newParam;
        }
        LogKeyConfliction(paramName);
        return null;
    }

    public IntParam AddIntegerParam(string paramName, int paramValue)
    {
        if (!DoseParamExist(paramName)) {
            var newParam = new IntParam(paramName, paramValue);
            _parameters.Add(paramName, newParam);
            return newParam;
        }
        LogKeyConfliction(paramName);
        return null;
    }

    public BoolParam AddBoolParam(string paramName, bool paramValue)
    {
        if (!DoseParamExist(paramName)) {
            var newParam = new BoolParam(paramName, paramValue);
            _parameters.Add(paramName, newParam);
            return newParam;
        }
        LogKeyConfliction(paramName);
        return null;
    }

    public bool RemoveParam(string name)
    {
        return _parameters.Remove(name);
    }

    public void SetScalarParam(string paramName, float paramValue)
    {
        if (DoseParamExist(paramName)) {
            var currentParam = _parameters[paramName] as ScalarParam;
            if (currentParam != null) {
                currentParam.ParamValue = paramValue;
                return;
            }
            Debug.LogError("[DBStateMachine] paramter is not a scalar\t----\t" + paramName);
        }
        Debug.LogError("[DBStateMachine] paramter is not found\t----\t" + paramName);
    }

    public void SetIntegerParam(string paramName, int paramValue)
    {
        if (DoseParamExist(paramName)) {
            var currentParam = _parameters[paramName] as IntParam;
            if (currentParam != null) {
                currentParam.ParamValue = paramValue;
                return;
            }
            Debug.LogError("[DBStateMachine] paramter is not an integer\t----\t" + paramName);
        }
        Debug.LogError("[DBStateMachine] paramter is not found\t----\t" + paramName);
    }

    public void SetBoolParam(string paramName, bool paramValue)
    {
        if (DoseParamExist(paramName)) {
            var currentParam = _parameters[paramName] as BoolParam;
            if (currentParam != null) {
                currentParam.ParamValue = paramValue;
                return;
            }
            Debug.LogError("[DBStateMachine] paramter is not a boolean\t----\t" + paramName);
        }
        Debug.LogError("[DBStateMachine] paramter is not found\t----\t" + paramName);
    }

    public DBState AddNewState(string name)
    {
        DBState newState;
        if (!_states.ContainsKey(name)) {
            newState = new DBState();
            _states.Add(name, newState);
        }
        else
            newState = GetState(name);

        return newState;
    }

    public void RemoveState(string name)
    {
        if (_states.ContainsKey(name))
            _states.Remove(name);
    }

    public DBState GetState(string name)
    {
        DBState currState;
        _states.TryGetValue(name, out currState);
        return currState;
    }

    protected bool CompareFloat(EDBCompMode compMode, float v1, float v2)
    {
        switch (compMode) {
            case EDBCompMode.Less: return v1 < v2;
            case EDBCompMode.LessOrEqual: return v1 <= v2;
            case EDBCompMode.Equal: return v1 == v2;
            case EDBCompMode.GreaterOrEqual: return v1 >= v2;
            case EDBCompMode.Greater: return v1 > v2;
            case EDBCompMode.NotEqual: return v1 != v2;
            default:
                Debug.LogWarning("[DBStateMachine] Compare failed!");
                return false;
        }
    }

    protected bool CompareInt(EDBCompMode compMode, int v1, int v2)
    {
        switch (compMode) {
            case EDBCompMode.Less: return v1 < v2;
            case EDBCompMode.LessOrEqual: return v1 <= v2;
            case EDBCompMode.Equal: return v1 == v2;
            case EDBCompMode.GreaterOrEqual: return v1 >= v2;
            case EDBCompMode.Greater: return v1 > v2;
            case EDBCompMode.NotEqual: return v1 != v2;
            default:
                Debug.LogWarning("[DBStateMachine] Compare failed!");
                return false;
        }
    }

    private bool CheckParameterByCondition(string paramName, DBCondition condition)
    {
        var currParam = GetParam(paramName);
        if (currParam == null) {
            Debug.LogError("[DBStateMachine] Parameter is not found: " + paramName);
            return false;
        }

        switch (condition.CheckParamType) {
            case EDBParamType.Scalar:
                var scalarCond = condition as DBConditionScalar;
                var scalarParam = currParam as ScalarParam;
                if (scalarParam != null)
                    return CompareFloat(scalarCond.CompMode, scalarParam.ParamValue, scalarCond.CompareValue);
                break;
            case EDBParamType.Integer:
                var intCond = condition as DBConditionInteger;
                var intParam = currParam as IntParam;
                if (intParam != null)
                    return CompareInt(intCond.CompMode, intParam.ParamValue, intCond.CompareValue);
                break;
            case EDBParamType.Bool:
                var boolCond = condition as DBConditionBool;
                var boolParam = currParam as BoolParam;
                if (boolParam != null)
                    return boolCond.RequiredValue == boolParam.ParamValue;
                break;
            default:
                break;
        }

        Debug.LogError("[DBStateMachine] Parameter check failure: parameter type is not matching!\t----\t" + paramName);
        return false;
    }

private void LogKeyConfliction(string paramName)
    {
        Debug.LogWarning("[DBStateMachine] Parameter already exists!\t----\t" + paramName);
    }

    private void LogKeyNotFound(string paramName)
    {
        Debug.LogWarning("[DBStateMachine] Parameter is not found!\t----\t" + paramName);

    }
}
