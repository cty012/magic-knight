using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This event is ONLY used by the GameManager to indicate that a scene finish loading.
// DO NOT use it to load a new scene. Use GameManager.LoadScene instead
public class LoadSceneEvent : BaseEvent
{
    public readonly Scene scene;
    public readonly LoadSceneMode mode;

    public LoadSceneEvent(LoadSceneEventType type, Scene scene, LoadSceneMode mode) : base(type)
    {
        this.scene = scene;
        this.mode = mode;
    }
}

public enum LoadSceneEventType
{
    LOAD_SCENE,
    LOAD_GAME_SCENE
}
