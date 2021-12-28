using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusChangeEvent : BaseEvent
{
    public readonly int value;

    public PlayerStatusChangeEvent(PlayerStatusChangeEventType type, int value) : base(type)
    {
        this.value = value;
    }
}

public enum PlayerStatusChangeEventType
{
    HP_CHANGE,
    MP_CHANGE,
    XP_CHANGE
}
