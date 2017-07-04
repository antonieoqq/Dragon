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
        public DBCondition(string name) { CheckParamName = name; }
        public string CheckParamName { get; private set; }
    }

    public abstract class DBConditionComparable : DBCondition
    {
        public DBConditionComparable(string name) : base(name) { }

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
                    Debug.LogWarning("Compare failed: " + CheckParamName);
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
                    Debug.LogWarning("Compare failed: " + CheckParamName);
                    return false;
            }
        }
    }

    public class DBConditionScalar : DBConditionComparable
    {
        public DBConditionScalar(string name, EDBCompMode mode, float v) : base(name)
        {
            CompMode = mode;
            CompareValue = v;
        }
        public EDBCompMode CompMode { get; private set; }
        public float CompareValue { get; private set; }
    }

    public class DBConditionInteger : DBCondition
    {
        public DBConditionInteger(string name, EDBCompMode mode, int v) : base(name)
        {
            CompMode = mode;
            CompareValue = v;
        }

        public EDBCompMode CompMode { get; private set; }
        public int CompareValue { get; private set; }
    }

    public class DBConditionBool : DBCondition
    {
        public DBConditionBool(string name, bool v) : base(name)
        {
            RequiredValue = v;
        }

        public bool RequiredValue { get; private set; }
    }

    public class DBTransition
    {
        public DBTransition()
        {
            Conditions = new LinkedList<DBCondition>();
        }

        public string TargetStateName { get; private set; }
        public LinkedList<DBCondition> Conditions { get; private set; }
    }

    public class DBState
    {
        public DBState()
        {
            Transitions = new LinkedList<DBTransition>();
        }

        public LinkedList<DBTransition> Transitions { get; private set; }
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

    public class ParamScalar : StateParam
    {
        public ParamScalar(string name, float v) : base(name, EDBParamType.Scalar)
        {
            ParamValue = v;
        }
        public float ParamValue;
    }

    public class ParamInt : StateParam
    {
        public ParamInt(string name, int v) : base(name, EDBParamType.Integer)
        {
            ParamValue = v;
        }
        public int ParamValue;
    }

    public class ParamBool : StateParam
    {
        public ParamBool(string name, bool v) : base(name, EDBParamType.Bool)
        {
            ParamValue = v;
        }
        public bool ParamValue;
    }


    private Dictionary<string, StateParam> _parameters;
    private Dictionary<string, DBState> _states;
    private DBState _currentState;

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

    public ParamScalar AddScalarParam(string paramName, float paramValue)
    {
        if (!DoseParamExist(paramName)) {
            var newParam = new ParamScalar(paramName, paramValue);
            _parameters.Add(paramName, newParam);
            return newParam;
        }
        LogKeyConfliction(paramName);
        return null;
    }

    public ParamInt AddIntegerParam(string paramName, int paramValue)
    {
        if (!DoseParamExist(paramName)) {
            var newParam = new ParamInt(paramName, paramValue);
            _parameters.Add(paramName, newParam);
            return newParam;
        }
        LogKeyConfliction(paramName);
        return null;
    }

    public ParamBool AddBoolParam(string paramName, bool paramValue)
    {
        if (!DoseParamExist(paramName)) {
            var newParam = new ParamBool(paramName, paramValue);
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

    public void AddNewState(string name)
    {
        if (!_states.ContainsKey(name))
            _states.Add(name, new DBState());
    }

    public void RemoveState(string name)
    {
        if (_states.ContainsKey(name))
            _states.Remove(name);
    }

    private DBState GetState(string name)
    {
        DBState currState;
        _states.TryGetValue(name, out currState);
        return currState;
    }

    private void LogKeyConfliction(string paramName)
    {
        Debug.LogWarning("DB State Machine: parameter already exists!\t----\t" + paramName);
    }

    private void LogKeyNotFound(string paramName)
    {
        Debug.LogWarning("DB State Machine: parameter is not found!\t----\t" + paramName);

    }
}
