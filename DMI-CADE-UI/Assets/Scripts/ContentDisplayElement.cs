using System;
using Pooling;
using UnityEngine;
using UnityEngine.UI;

public class ContentDisplayElement : MonoBehaviour
{
    private static readonly Color DarkBaseColor = new Color(.5f, .5f, .5f);

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

    public void Darken(float time)
    {
        LeanTween.color(rectTransform, DarkBaseColor, time);
    }

    public void Brighten(float time)
    {
        LeanTween.color(rectTransform, Color.white, time);
    }

    public void ReturnToPool()
    {
        ContentDisplayElementPool.Instance.ReturnToPool(this);
    }
}
