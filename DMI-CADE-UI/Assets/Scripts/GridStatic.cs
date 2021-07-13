using System;
using UnityEngine;

public class GridStatic : MonoBehaviour
{
    public void Move(Vector3 direction, LeanTweenType ease, float time, Action callback)
    {
        LeanTween.move(gameObject, transform.position + direction, time).setEase(ease).setOnComplete(callback);
    }
    
    public void Move(Vector3 direction, LeanTweenType ease, float time)
    {
        LeanTween.move(gameObject, transform.position + direction, time).setEase(ease);
    }
}
