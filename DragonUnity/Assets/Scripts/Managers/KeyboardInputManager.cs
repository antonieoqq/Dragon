using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class KeyboardInputManager : ManagerSingleton<KeyboardInputManager>
{
    private class KeyBuffer
    {
        public KeyBuffer(KeyCode keyCode, EInputCommand inputCmd)
        {
            KeyCodeValue = keyCode;
            CmdMessage = new InputCommandMessage();
            CmdMessage.InputCommand = inputCmd;
            CmdMessage.InputState = EInputState.Release;
        }

        public KeyCode KeyCodeValue { get; private set; }
        public InputCommandMessage CmdMessage { get; private set; }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCodeValue))
                CmdMessage.InputState = EInputState.Press;
            else if (Input.GetKey(KeyCodeValue))
                CmdMessage.InputState = EInputState.Hold;
            else if (Input.GetKeyUp(KeyCodeValue))
                CmdMessage.InputState = EInputState.Release;

            Instance.HandleKeyEvent(this);
        }
    }

    private Dictionary<KeyCode, KeyBuffer> keyBuffers = new Dictionary<KeyCode, KeyBuffer>();
    private Vector2 leftJoystick;
    private Action<Vector2> leftJoystickListoner;
    private Action<InputCommandMessage> buttonCommandListoner;

    protected override void InitExecute()
    {
        IsUpdating = false;

        leftJoystick.x = 0;
        leftJoystick.y = 0;

        // 方向键
        BindKey(KeyCode.A, EInputCommand.None);
        BindKey(KeyCode.D, EInputCommand.None);
        BindKey(KeyCode.W, EInputCommand.LiftUp);
        // 动作键
        BindKey(KeyCode.J, EInputCommand.Attack);
        BindKey(KeyCode.K, EInputCommand.Dodge);
        BindKey(KeyCode.L, EInputCommand.Item);
        BindKey(KeyCode.U, EInputCommand.LiftUp);
        BindKey(KeyCode.I, EInputCommand.Skill);
        BindKey(KeyCode.O, EInputCommand.Burst);
    }

    public void UpdateKeyInputs()
    {
        var keyIter = keyBuffers.GetEnumerator();
        while (keyIter.MoveNext())
            keyIter.Current.Value.Update();
    }

    public void AddLeftJoystickListoner(Action<Vector2> listoner)
    {
        leftJoystickListoner += listoner;
    }

    public void RemoveLeftJoystickListoner(Action<Vector2> listoner)
    {
        leftJoystickListoner -= listoner;
    }

    public void AddButtonMessageListoner(Action<InputCommandMessage> listoner)
    {
        buttonCommandListoner += listoner;
    }

    public void RemoveButtonMessageListoner(Action<InputCommandMessage> listoner)
    {
        buttonCommandListoner -= listoner;
    }

    private void BindKey(KeyCode keyCode, EInputCommand bindCmd)
    {
        if (!keyBuffers.ContainsKey(keyCode)) {
            keyBuffers.Add(keyCode, new KeyBuffer(keyCode, bindCmd));
            
        }
    }

    private void UpdateLeftJoystick()
    {
        leftJoystick.x = (keyBuffers[KeyCode.A].CmdMessage.InputState != EInputState.Release ? -1 : 0) +
                            (keyBuffers[KeyCode.D].CmdMessage.InputState != EInputState.Release ? 1 : 0);

        if (leftJoystickListoner != null)
            leftJoystickListoner(leftJoystick);
    }

    private void HandleKeyEvent(KeyBuffer inBuffer)
    {
        if (inBuffer.KeyCodeValue == KeyCode.A || inBuffer.KeyCodeValue == KeyCode.D) {
            UpdateLeftJoystick();
            return;
        }

        if (inBuffer.CmdMessage.InputCommand != EInputCommand.None && buttonCommandListoner != null)
            buttonCommandListoner(inBuffer.CmdMessage);
    }
}
