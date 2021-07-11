using System;
using Pooling;
using UnityEngine;
using UnityEngine.UI;

public class ContentDisplayElement : MonoBehaviour
{
    public Image logoImage;
    public RectTransform rectTransform;

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
        ContentDisplayElementPool.Instance.ReturnToPool(this);
    }
}
