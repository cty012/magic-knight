using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneEvent : BaseEvent
{
    public readonly string scene;

    public LoadSceneEvent(LoadSceneEventType type, string scene) : base(type)
    {
        this.scene = scene;
    }
}

public enum LoadSceneEventType
{
    LOAD_SCENE,
    LOAD_GAME_SCENE,
    PRELOAD_SCENE
}
