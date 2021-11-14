using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    float turnTime = 10; //la durata standard del turno, non si tocca
    public float totalTurnTime = 10; //la durata del turno del personaggio
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
        {
            slider.maxValue = timeRemaining;
        }

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
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float fraction = Mathf.FloorToInt(timeToDisplay * 100) % 100;
        if (timeText)
        {
            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, fraction);
        }

    }
    public void SetSliderToMax()
    {
        if(!slider) { return; }
        slider.maxValue = totalTurnTime;
    }
    public void Time2X()
    {
        if (timerIsRunning && !isTurnOver)
        {
            totalTurnTime *= 2;
            timeRemaining = totalTurnTime;
            slider.maxValue = timeRemaining;
        }

    }
    public void Time_1_2()
    {
        if (timerIsRunning && !isTurnOver)
        {
            totalTurnTime *= .5f;
            timeRemaining = totalTurnTime;
            slider.maxValue = timeRemaining;
        }

    }
    public void Time_Zero()
    {
        timeRemaining = 0;
    }
    public float GetStandardTurnTime()
    {
        return turnTime;
    }
}