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
    private Dictionary<string, IParam> _parameters;

    private interface IParam
    {
    }

    private struct ParamFloat : IParam
    {
        public ParamFloat(float v) { ParamValue = v; }

        public float ParamValue;
    }

    private struct ParamInt : IParam
    {
        public ParamInt(int v) { ParamValue = v; }
        public int ParamValue;
    }

    private struct ParamBool : IParam
    {
        public ParamBool(bool v) { ParamValue = v; }
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
