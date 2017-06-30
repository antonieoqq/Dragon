using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ECompMode
{
    Less,
    LessOrEqual,
    Equal,
    GreaterOrEqual,
    Greater,
    NotEqual,
}

public enum ECheckMode
{
    Compare,
    IsTrue,
}

public abstract class DBCondition
{
    public DBCondition(string name) { CheckParamName = name; }
    public string CheckParamName { get; private set; }
    public abstract bool Pass();
}

public class DBConditionScalar : DBCondition
{
    public DBConditionScalar(string name, ECompMode mode, float v) : base(name)
    {
        CompMode = mode;
        CompareValue = v;
    }
    public ECompMode CompMode { get; private set; }
    public float CompareValue { get; private set; }

    public override bool Pass()
    {
        throw new NotImplementedException();
    }
}

public class DBConditionInteger : DBCondition
{
    public DBConditionInteger(string name, ECompMode mode, int v) : base(name)
    {
        CompMode = mode;
        CompareValue = v;
    }

    public ECompMode CompMode { get; private set; }
    public int CompareValue { get; private set; }

    public override bool Pass()
    {
        throw new NotImplementedException();
    }
}

public class DBConditionBool : DBCondition
{
    public DBConditionBool(string name, bool v) : base(name)
    {
        RequiredValue = v;
    }

    public bool RequiredValue { get; private set; }

    public override bool Pass()
    {
        throw new NotImplementedException();
    }
}

public class DBTransition
{
    private DBState _targetState;
    private LinkedList<DBCondition> _conditions;
}

public class DBState
{
    private LinkedList<DBTransition> _transitions;
}

public class DBStateMachine
{
    private Dictionary<string, StateParam> _parameters;
    private Dictionary<string, DBState> _states;
    private DBState _currentState;

    public enum EParamType
    {
        Scalar,
        Integer,
        Bool,
    }

    // 为了避免box/unbox，将3种基本类型的参数封装成类
    public abstract class StateParamBase
    {
        public abstract bool IsComparable();
        public abstract bool IsBoolean();
    }

    public abstract class StateParam : StateParamBase
    {
        public StateParam(string name, EParamType type) { ParamName = name; ParamType = type; }

        public string ParamName { get; private set; }
        public EParamType ParamType { get; private set; }

        public override sealed bool IsComparable()
        {
            switch (ParamType) {
                case EParamType.Scalar:
                case EParamType.Integer:
                    return true;
                case EParamType.Bool:
                default:
                    return false;
            }
        }

        public override sealed bool IsBoolean()
        {
            return !IsComparable();
        }

        //protected bool CompareFloat(ECompMode compMode, float v1, float v2)
        //{
        //    switch (compMode) {
        //        case ECompMode.Less: return v1 < v2;
        //        case ECompMode.LessOrEqual: return v1 <= v2;
        //        case ECompMode.Equal: return v1 == v2;
        //        case ECompMode.GreaterOrEqual: return v1 >= v2;
        //        case ECompMode.Greater: return v1 > v2;
        //        case ECompMode.NotEqual: return v1 != v2;
        //        default:
        //            Debug.LogWarning("Compare failed: " + ParamName);
        //            return false;
        //    }
        //}

        //protected bool CompareInt(ECompMode compMode, int v1, int v2)
        //{
        //    switch (compMode) {
        //        case ECompMode.Less: return v1 < v2;
        //        case ECompMode.LessOrEqual: return v1 <= v2;
        //        case ECompMode.Equal: return v1 == v2;
        //        case ECompMode.GreaterOrEqual: return v1 >= v2;
        //        case ECompMode.Greater: return v1 > v2;
        //        case ECompMode.NotEqual: return v1 != v2;
        //        default:
        //            Debug.LogWarning("Compare failed: " + ParamName);
        //            return false;
        //    }
        //}
    }

    public class ParamScalar : StateParam
    {
        public ParamScalar(string name, float v) : base(name, EParamType.Scalar)
        {
            ParamValue = v;
        }
        public float ParamValue;
    }

    public class ParamInt : StateParam
    {
        public ParamInt(string name, int v) : base(name, EParamType.Integer)
        {
            ParamValue = v;
        }
        public int ParamValue;
    }

    public class ParamBool : StateParam
    {
        public ParamBool(string name, bool v) : base(name, EParamType.Bool)
        {
            ParamValue = v;
        }
        public bool ParamValue;
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

    private void LogKeyConfliction(string paramName)
    {
        Debug.LogWarning("DB State Machine: parameter already exists!\t----\t" + paramName);
    }

    private void LogKeyNotFound(string paramName)
    {
        Debug.LogWarning("DB State Machine: parameter is not found!\t----\t" + paramName);

    }
}
