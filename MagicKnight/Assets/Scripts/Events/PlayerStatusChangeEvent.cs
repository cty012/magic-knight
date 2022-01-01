using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusChangeEvent : BaseEvent
{
    public readonly int value;
    public readonly int maxValue;

    public PlayerStatusChangeEvent(PlayerStatusChangeEventType type, int value) : base(type)
    {
        this.value = value;
    }

    public PlayerStatusChangeEvent(PlayerStatusChangeEventType type, int value, int maxValue) : base(type)
    {
        this.value = value;
        this.maxValue = maxValue;
    }
}

public enum PlayerStatusChangeEventType
{
    HP_CHANGE,
    MP_CHANGE,
    XP_CHANGE
}
