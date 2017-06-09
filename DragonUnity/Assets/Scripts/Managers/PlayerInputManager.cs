using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EInputCommand
{
    None,
    Attack,
    Dodge,
    LiftUp,
    Skill,
    Item,
    Burst
}

public enum EInputState
{
    Release,
    Press,
    Hold,
}

public class InputCommandMessage
{
    public EInputCommand InputCommand;
    public EInputState InputState;
}

public class InputCommandState
{
    public EInputCommand InputCommand { get; private set; }
    public EInputState CurrentState
    {
        get { return _currState; }
        private set
        {
            if (_currState != value)
            {
                _currState = value;
                PlayerInputManager.Instance.OnCommandStateChanged(this);
            }
            else if (_currState == EInputState.Hold)
            {
                PlayerInputManager.Instance.OnCommandStateChanged(this);
            }
        }
    }
    private EInputState _currState = EInputState.Release;

    public float HoldTime { get; private set; }

    private int _pressCount = 0;
    private int _holdCount = 0;

    public InputCommandState(EInputCommand inCmd)
    {
        InputCommand = inCmd;
        CurrentState = EInputState.Release;
        HoldTime = 0;
    }

    public void ResetInputCount()
    {
        _pressCount = 0;
        _holdCount = 0;
    }

    public void HandleInputMessage(InputCommandMessage inCmdMessage)
    {
        switch (inCmdMessage.InputState)
        {
            case EInputState.Release:
                break;
            case EInputState.Press:
                _pressCount++;
                break;
            case EInputState.Hold:
                _holdCount++;
                break;
        }
    }

    public void UpdateCommandState()
    {
        if (CurrentState != EInputState.Release)
            HoldTime += Time.deltaTime;

        EInputState newState;
        if (_pressCount > 0)
        {
            newState = EInputState.Press;
            HoldTime = Time.deltaTime;
        }
        else if (_holdCount > 0)
            newState = EInputState.Hold;
        else
            newState = EInputState.Release;

        CurrentState = newState;
    }
}

public sealed class PlayerInputManager : ManagerSingleton<PlayerInputManager>
{
    private Action<Vector2> OnLeftJoystick;
    private Action<InputCommandState> OnButtonCommand;

    private Dictionary<EInputCommand, InputCommandState> cmdStateDict;
    private bool _isListoningKeyboard = false;
    public bool IsListoningKeyboard
    {
        get { return _isListoningKeyboard; }
        set
        {
            if (_isListoningKeyboard != value)
            {
                _isListoningKeyboard = value;

                if (_isListoningKeyboard)
                {
                    KeyboardInputManager.Instance.AddLeftJoystickListoner(HandleLeftJoystick);
                    KeyboardInputManager.Instance.AddButtonMessageListoner(HandleCommandMessage);
                }
                else
                {
                    KeyboardInputManager.Instance.RemoveLeftJoystickListoner(HandleLeftJoystick);
                    KeyboardInputManager.Instance.RemoveButtonMessageListoner(HandleCommandMessage);
                }
            }
        }
    }

    protected override void InitExecute()
    {
        IsUpdating = true;
        IsListoningKeyboard = true;
        InitCommandStateDictionary();
    }

    public override void DoUpdate()
    {
        ResetCommandInputCountBeforeUpdate();
        if (IsListoningKeyboard)
            KeyboardInputManager.Instance.UpdateKeyInputs();

        UpdateAllCommandStatesFinally();
    }

    public void InitCommandStateDictionary()
    {
        cmdStateDict = new Dictionary<EInputCommand, InputCommandState>();
        var cmdEnums = Enum.GetValues(typeof(EInputCommand));
        for (int i = 0; i < cmdEnums.Length; i++)
        {
            var currEnum = (EInputCommand)Enum.ToObject(typeof(EInputCommand), cmdEnums.GetValue(i));
            cmdStateDict.Add(currEnum, new InputCommandState(currEnum));
        }
    }

    public void AddLeftJoystickListoner(Action<Vector2> leftJoystickListoner)
    {
        OnLeftJoystick += leftJoystickListoner;
    }

    public void RemoveLeftJoystickListoner(Action<Vector2> leftJoystickListoner)
    {
        OnLeftJoystick -= leftJoystickListoner;
    }

    public void AddButtonCommandListoner(Action<InputCommandState> buttonCommandListoner)
    {
        OnButtonCommand += buttonCommandListoner;
    }

    public void RemoveButtonCommandListoner(Action<InputCommandState> buttonCommandListoner)
    {
        OnButtonCommand -= buttonCommandListoner;
    }

    public void HandleLeftJoystick(Vector2 leftStick)
    {
        if (OnLeftJoystick != null)
            OnLeftJoystick(leftStick);
    }

    public void HandleCommandMessage(InputCommandMessage inCmdMessage)
    {
        InputCommandState targetState;
        if (cmdStateDict.TryGetValue(inCmdMessage.InputCommand, out targetState))
            targetState.HandleInputMessage(inCmdMessage);
    }

    public void OnCommandStateChanged(InputCommandState cmdState)
    {
        if (OnButtonCommand != null)
            OnButtonCommand(cmdState);
    }

    private void ResetCommandInputCountBeforeUpdate()
    {
        var iter = cmdStateDict.GetEnumerator();
        while (iter.MoveNext())
            iter.Current.Value.ResetInputCount();
    }

    private void UpdateAllCommandStatesFinally()
    {
        var iter = cmdStateDict.GetEnumerator();
        while (iter.MoveNext())
            iter.Current.Value.UpdateCommandState();
    }
}
