using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EValueComp
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

    public enum EParamType
    {
        Scalar,
        Integer,
        Bool,
    }

    // 为了避免box/unbox，将3种基本类型的参数封装成类
    private abstract class StateParam
    {
        public EParamType ParamType { get; protected set; }
        public abstract bool IsComparable();
        public abstract bool IsBoolean();
        public virtual bool CompareWith(EValueComp compMode, float v) { return false; }
        public virtual bool CompareWith(EValueComp compMode, int v) { return false; }
        public virtual bool IsTrue() { return false; }
    }

    private class ParamBase : StateParam
    {
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
    }

    private class ParamFloat : ParamBase
        {
        public ParamFloat(float v)
        {
            ParamType = EParamType.Scalar;
            ParamValue = v;
        }
        public float ParamValue;
    }

    private class ParamInt : ParamBase
        {
        public ParamInt(int v)
        {
            ParamType = EParamType.Integer;
            ParamValue = v;
        }
        public int ParamValue;
    }

    private class ParamBool : ParamBase
        {
        public ParamBool(bool v)
        {
            ParamType = EParamType.Bool;
            ParamValue = v;
        }
        public bool ParamValue;
    }

    public bool DoseParamExist(string paramName)
    {
        return _parameters.ContainsKey(paramName);
    }

    public void AddNewParam(string paramName, float paramValue)
    {
        if (!DoseParamExist(paramName)) {
            _parameters.Add(paramName, new ParamFloat(paramValue));
            return;
        }
        LogKeyConfliction(paramName);
    }

    public void AddNewParam(string paramName, int paramValue)
    {
        if (!DoseParamExist(paramName)) {
            _parameters.Add(paramName, new ParamInt(paramValue));
            return;
        }
        LogKeyConfliction(paramName);
    }

    public void AddNewParam(string paramName, bool paramValue)
    {
        if (!DoseParamExist(paramName)) {
            _parameters.Add(paramName, new ParamBool(paramValue));
            return;
        }
        LogKeyConfliction(paramName);
    }

    private void LogKeyConfliction(string paramName)
    {
        Debug.LogWarning("DB State Machine: state " + paramName + " already exists.");
    }
}
