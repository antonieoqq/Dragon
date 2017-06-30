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

public abstract class DBCondition
{
    public abstract bool IsTrue();
}

public class DBTransition
{
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
    private abstract class StateParamBase
    {
        public abstract bool IsComparable();
        public abstract bool IsBoolean();
    }

    private abstract class StateParam : StateParamBase
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
            switch (ParamType) {
                case EParamType.Scalar:
                case EParamType.Integer:
                    return true;
                case EParamType.Bool:
                default:
                    return false;
            }
        }

        public virtual bool CompareWith(ECompMode compMode, float v)
        {
            if (!IsComparable())
                Debug.LogWarning(ParamName + " is not comparable!");

            return false;
        }

        public virtual bool CompareWith(ECompMode compMode, int v)
        {
            if (!IsComparable())
                Debug.LogWarning(ParamName + " is not comparable!");

            return false;
        }

        public virtual bool IsTrue()
        {
            if (IsComparable())
                Debug.LogWarning(ParamName + " is not a boolean parameter!");

            return false;
        }

        protected bool CompareFloat(ECompMode compMode, float v1, float v2)
        {
            switch (compMode) {
                case ECompMode.Less: return v1 < v2;
                case ECompMode.LessOrEqual: return v1 <= v2;
                case ECompMode.Equal: return v1 == v2;
                case ECompMode.GreaterOrEqual: return v1 >= v2;
                case ECompMode.Greater: return v1 > v2;
                case ECompMode.NotEqual: return v1 != v2;
                default:
                    Debug.LogWarning("Compare failed: " + ParamName);
                    return false;
            }
        }

        protected bool CompareInt(ECompMode compMode, int v1, int v2)
        {
            switch (compMode) {
                case ECompMode.Less: return v1 < v2;
                case ECompMode.LessOrEqual: return v1 <= v2;
                case ECompMode.Equal: return v1 == v2;
                case ECompMode.GreaterOrEqual: return v1 >= v2;
                case ECompMode.Greater: return v1 > v2;
                case ECompMode.NotEqual: return v1 != v2;
                default:
                    Debug.LogWarning("Compare failed: " + ParamName);
                    return false;
            }
        }
    }

    private class ParamScalar : StateParam
    {
        public ParamScalar(string name, float v) : base(name, EParamType.Scalar)
        {
            ParamValue = v;
        }
        public float ParamValue;

        public override bool CompareWith(ECompMode compMode, float v)
        {
            return CompareFloat(compMode, ParamValue, v);
        }

        public override bool CompareWith(ECompMode compMode, int v)
        {
            return CompareInt(compMode , (int)ParamValue, v);
        }
    }

    private class ParamInt : StateParam
    {
        public ParamInt(string name, int v) : base(name, EParamType.Integer)
        {
            ParamValue = v;
        }
        public int ParamValue;

        public override bool CompareWith(ECompMode compMode, float v)
        {
            return CompareFloat(compMode, ParamValue, v);
        }

        public override bool CompareWith(ECompMode compMode, int v)
        {
            return CompareInt(compMode, ParamValue, v);
        }
    }

    private class ParamBool : StateParam
    {
        public ParamBool(string name, bool v) : base(name, EParamType.Bool)
        {
            ParamValue = v;
        }
        public bool ParamValue;

        public override bool IsTrue() { return ParamValue; }
    }

    public bool DoseParamExist(string paramName)
    {
        return _parameters.ContainsKey(paramName);
    }

    public void AddScalarParam(string paramName, float paramValue)
    {
        if (!DoseParamExist(paramName)) {
            _parameters.Add(paramName, new ParamScalar(paramName, paramValue));
            return;
        }
        LogKeyConfliction(paramName);
    }

    public void AddIntegerParam(string paramName, int paramValue)
    {
        if (!DoseParamExist(paramName)) {
            _parameters.Add(paramName, new ParamInt(paramName, paramValue));
            return;
        }
        LogKeyConfliction(paramName);
    }

    public void AddBoolParam(string paramName, bool paramValue)
    {
        if (!DoseParamExist(paramName)) {
            _parameters.Add(paramName, new ParamBool(paramName, paramValue));
            return;
        }
        LogKeyConfliction(paramName);
    }

    public bool RemoveParam(string name)
    {
        return _parameters.Remove(name);
    }

    public bool CompareParamWith(string name, ECompMode compMode, float v)
    {
        if (DoseParamExist(name))
            return _parameters[name].CompareWith(compMode, v);
        LogKeyNotFound(name);
        return false;
    }

    public bool CompareParamWith(string name, ECompMode compMode, int v)
    {
        if (DoseParamExist(name))
            return _parameters[name].CompareWith(compMode, v);
        LogKeyNotFound(name);
        return false;
    }

    public bool IsParamTrue(string name)
    {
        if (DoseParamExist(name))
            return _parameters[name].IsTrue();
        LogKeyNotFound(name);
        return false;
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
