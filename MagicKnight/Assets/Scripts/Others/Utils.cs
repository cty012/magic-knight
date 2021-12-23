using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    private static readonly object idLock = new object();
    private static int nextId = 1;
    public static readonly Vector2 size = new Vector2(1920, 1080);

    // Generate an id unique in the game session
    public static int GenerateId()
    {
        lock (idLock)
        {
            return Utils.nextId++;
        }
    }

    // Get the rough direction (1, -1, or 0 for each dimension) of two points
    public static Vector2 GetDirection(Vector2 from, Vector2 to)
    {
        return new Vector2(
            (from.x == to.x) ? 0 : ((from.x < to.x) ? 1 : -1),
            (from.y == to.y) ? 0 : ((from.y < to.y) ? 1 : -1));
    }

    // Identify game objects
    public static bool IsPlayer(GameObject obj)
    {
        return obj.GetComponent<PlayerController>() != null;
        // return "Player".Equals(obj?.name);
    }
    public static bool IsEnemy(GameObject obj)
    {
        return obj.GetComponent<EnemyController>() != null;
        // return "Enemy".Equals(obj?.transform?.parent?.parent?.gameObject?.name);
    }
    public static bool IsNPC(GameObject obj)
    {
        return "NPC".Equals(obj?.transform?.parent?.parent?.gameObject?.name);
    }
    public static bool IsTerrain(GameObject obj)
    {
        return "Terrain".Equals(obj?.transform?.parent?.gameObject?.name);
    }
}

public static class ExtensionMethods
{
    // Clamp a comparable between two values
    // The Internet says that there's a Math.Clamp method, but I didn't find it
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        // swap min and max values if min > max
        if (min.CompareTo(max) > 0) { T temp = min; min = max; max = temp; }
        // compare value with min and max
        if (value.CompareTo(min) < 0) return min;
        else if (value.CompareTo(max) > 0) return max;
        else return value;
    }
    // VECTOR2
    // Should be quite self explanatory
    public static Vector2 DropZ(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }
    public static Vector3 AttachZ(this Vector2 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }
    public static Vector2 Clone(this Vector2 vector)
    {
        return new Vector2(vector.x, vector.y);
    }
    public static Vector2 Add(this Vector2 vec1, Vector2 vec2)
    {
        return new Vector2(vec1.x + vec2.x, vec1.y + vec2.y);
    }
    public static Vector2 Subtract(this Vector2 vec1, Vector2 vec2)
    {
        return vec1.Add(vec2.Negative());
    }
    public static Vector2 MultiplyScalar(this Vector2 vector, float scale)
    {
        return new Vector2(vector.x * scale, vector.y * scale);
    }
    public static Vector2 Negative(this Vector2 vector)
    {
        return vector.MultiplyScalar(-1);
    }
    // RECTANGLES
    // Helper method
    private static Vector2 GetPointByAnchor(this RectTransform transform, Vector2 anchor)
    {
        return new Vector2(
            (transform.rect.xMin * (1 - anchor.x) + transform.rect.xMax * anchor.x) * transform.localScale.x,
            (transform.rect.yMin * (1 - anchor.y) + transform.rect.yMax * anchor.y) * transform.localScale.y);
    }
    // The world coordinates is the same as the map coordinates
    // Getting the world point of certain points (indicate by the anchor) on the rectangle
    public static Vector2 GetWorldPointByAnchor(this RectTransform transform, Vector2 anchor)
    {
        return transform.GetPointByAnchor(anchor).Add(transform.position);
    }
    // Getting the world point of the bottom left corner
    public static Vector2 GetWorldPointAtBottomLeft(this RectTransform transform)
    {
        return transform.GetWorldPointByAnchor(new Vector2(0, 0));
    }
    // Getting the world point of the center
    public static Vector2 GetWorldPointAtCenter(this RectTransform transform)
    {
        return transform.GetWorldPointByAnchor(new Vector2(0.5f, 0.5f));
    }
    // Same methods but gets the local point instead of the world point
    // Less useful than the above methods
    public static Vector2 GetLocalPointByAnchor(this RectTransform transform, Vector2 anchor)
    {
        return transform.GetPointByAnchor(anchor).Add(transform.localPosition);
    }
    public static Vector2 GetLocalPointAtBottomLeft(this RectTransform transform)
    {
        return transform.GetLocalPointByAnchor(new Vector2(0, 0));
    }
    public static Vector2 GetLocalPointAtCenter(this RectTransform transform)
    {
        return transform.GetLocalPointByAnchor(new Vector2(0.5f, 0.5f));
    }
}

public class Timer
{
    public float curValue { get; set; }
    public float maxValue { get; set; }
    public float speed { get; set; }
    public bool stopped { get { return this.curValue == 0; } }
    public float percentage { get { return this.curValue / this.maxValue; } }

    public Timer(float maxValue, float speed = 1)
    {
        this.maxValue = maxValue;
        this.speed = speed;
    }

    public void Reset()
    {
        this.curValue = this.maxValue;
    }

    public bool Update()
    {
        if (this.curValue > 0)
        {
            this.curValue = Math.Max(this.curValue - Time.deltaTime * this.speed, 0);
        }
        else if (this.curValue < 0)
        {
            this.curValue = Math.Min(this.curValue + Time.deltaTime * this.speed, 0);
        }
        else
        {
            return false;
        }
        return true;
    }
}

public class DiscreteTimer
{
    public int curValue { get; set; }
    public int maxValue { get; set; }
    public int step { get; set; }
    public bool stopped { get { return this.curValue == 0; } }

    public DiscreteTimer(int maxValue, int step = 1)
    {
        this.curValue = 0;
        this.maxValue = maxValue;
        this.step = step;
    }

    public void Reset()
    {
        this.curValue = this.maxValue;
    }

    public bool Update()
    {
        if (this.curValue > 0)
        {
            this.curValue = Math.Max(this.curValue - this.step, 0);
        }
        else if (this.curValue < 0)
        {
            this.curValue = Math.Min(this.curValue + this.step, 0);
        }
        else
        {
            return false;
        }
        return true;
    }
}
