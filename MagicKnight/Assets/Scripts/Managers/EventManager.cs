using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public static readonly EventManager instance = new EventManager();
    private readonly Dictionary<string, Dictionary<int, Action<BaseEvent>>> listeners = new Dictionary<string, Dictionary<int, Action<BaseEvent>>>();
    
    private EventManager() { }

    // Registers a listener (a function)
    // Specify the tag you want to listen to
    // The returned id refers to the registered listener
    public int On(string tag, Action<BaseEvent> listener)
    {
        // Skip empty tags
        if ("".Equals(tag)) return 0;
        if (!this.listeners.ContainsKey(tag))
        {
            this.listeners[tag] = new Dictionary<int, Action<BaseEvent>>();
        }
        int id = Utils.GenerateId();
        this.listeners[tag].Add(id, listener);
        return id;
    }

    // Unregister a listener
    // Uses the id you get when registering
    public bool Off(int id)
    {
        foreach (string tag in listeners.Keys)
        {
            if (listeners[tag].ContainsKey(id))
            {
                listeners[tag].Remove(id);
                if (listeners[tag].Keys.Count == 0) listeners.Remove(tag);
                return true;
            }
        }
        return false;
    }

    // Emits an event
    // All listeners on the same tag will be invoked
    public void Emit(string tag, BaseEvent eventArgs)
    {
        if (!listeners.ContainsKey(tag)) return;
        foreach (Action<BaseEvent> action in this.listeners[tag].Values)
        {
            action.Invoke(eventArgs);
        }
    }
}

// All events should extend the BaseEvent
public class BaseEvent
{
    public readonly Enum type;

    public BaseEvent(Enum type)
    {
        this.type = type;
    }
}
