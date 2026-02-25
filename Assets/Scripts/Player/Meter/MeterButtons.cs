using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Added if needed, but not directly used by this script for UI itself

public class MeterButtons : MonoBehaviour
{
    public MeterScript timeMeter; //this allows you to link this script to the meter's script via the inspector
    public int currentTime; //defines the variable you'd like to keep track of
    public int maxTime = 80; //defines the maximum value of your variable (keep it at a multiple of 8 so that it matches the meter's sections)


    void Start()
    {
        currentTime = maxTime; //sets your variable to maximum from the start
        timeMeter.SetMaxTime(maxTime); //sets your meter's fill to maximum from the start

    }


    void FixedUpdate()
    {
        timeMeter.SetTime(currentTime); //links your variable to the meter's fill

    }

    public void Increase()
    {

            currentTime += 10; //increases the variable's value by 10
    }

    public void Decrease()
    {

            currentTime -= 10; //decreases the variable's value by 10
    }
}
