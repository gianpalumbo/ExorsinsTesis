using UnityEngine;

public class SmoothInput
{
    private float currentValue = 0f;
    private float currentVelocity = 0f;

    public float Update(float target, float smoothTime)
    {
        currentValue = Mathf.SmoothDamp(currentValue, target, ref currentVelocity, smoothTime);
        return currentValue;
    }
}