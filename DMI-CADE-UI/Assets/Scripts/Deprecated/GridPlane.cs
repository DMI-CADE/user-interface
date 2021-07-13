using System;
using Pooling;
using UnityEngine;
using UnityEngine.UI;

public class GridPlane : MonoBehaviour
{
    public int gridGroup;

    public void Move(Vector3 direction, LeanTweenType ease, float time, Action callback)
    {
        LeanTween.move(gameObject, transform.position + direction, time).setEase(ease).setOnComplete(callback);
    }
    
    public void Move(Vector3 direction, LeanTweenType ease, float time)
    {
        LeanTween.move(gameObject, transform.position + direction, time).setEase(ease);
    }

    public void ReturnToPool()
    {
        GridPlanePool.Instance.ReturnToPool(this);
    }
}