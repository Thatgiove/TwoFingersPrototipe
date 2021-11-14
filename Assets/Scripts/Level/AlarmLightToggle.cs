using UnityEngine;

[RequireComponent(typeof(Light))]
public class AlarmLightToggle : MonoBehaviour
{
    float interval = 0.5f;
    float nextTime = 0;

    void Update()
    {
        if (Time.time >= nextTime)
        {
            nextTime += interval;
            GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
        }
    }
}
