using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float totalTurnTime = 10;
    public float timeRemaining = 10;
    
    public bool isTurnOver;
    public GameObject TurnBar;
    private Slider slider;

    public bool timerIsRunning = false;
    public Text timeText;

    private void Start()
    {
        //timerIsRunning = true;
        slider = TurnBar.GetComponent<Slider>();
        if (slider)
            slider.maxValue = timeRemaining;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0 && slider)
            {
                isTurnOver = false;
                slider.SetValueWithoutNotify(timeRemaining);
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                isTurnOver = true;
                timeRemaining = totalTurnTime;
                slider.maxValue = timeRemaining;
                //timerIsRunning = false;
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        //Debug.Log(string.Format("{0:00}:{1:00}", minutes, seconds));
        if(timeText)
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void Time2X()
    {
        if (timerIsRunning && !isTurnOver)
            totalTurnTime *= 2;
    }
    public void Time_1_2()
    {
        if (timerIsRunning &&!isTurnOver)
            totalTurnTime *= .5f;
    }
}