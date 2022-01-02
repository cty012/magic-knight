using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A custom timer that replaces the curXxxTime, maxXxxTime, curXxxCD, and maxXxxCD in the previous scripts
[Serializable]
public class Timer
{
    public float curValue { get; set; }
    public float maxValue;
    public float speed = 1;

    // Stopped and percentage can be used to check the current progress
    public bool stopped { get { return this.curValue == 0; } }
    public float percentage { get { return this.curValue / this.maxValue; } }

    public Timer() { }

    // Higher speed will cause timer to run faster
    public Timer(float maxValue, float speed = 1)
    {
        this.maxValue = maxValue;
        this.speed = speed;
    }

    // Reset the timer to the highest value
    public void Reset()
    {
        this.curValue = this.maxValue;
    }

    // Decreases the value by deltaTime * speed
    // If value is negative will increase the value instead
    public void Update()
    {
        if (GameManager.instance.paused) return;
        else if (this.curValue > 0)
        {
            this.curValue = Math.Max(this.curValue - Time.deltaTime * this.speed, 0);
        }
        else if (this.curValue < 0)
        {
            this.curValue = Math.Min(this.curValue + Time.deltaTime * this.speed, 0);
        }
        else
        {
            return;
        }
        return;
    }
}

// Similar to above but uses int values
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

    // Update will increase/decrease the value by 1
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
