using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BorderedContainer : MonoBehaviour
{
    public float spritesPixelPerUnit = 1f;
    public int pixelPerIncrement;
    public float tweenTime = 1f;
    public bool useTimePerIncrement = false;
    public float timePerIncrement;
    public GameObject[] containedElements;

    public Action OnOpen;
    public Action OnClose;
    
    private RectTransform _rectTransform;
    private Image _image;
    private Vector2 _maxSize;
    private Vector2 _maxIncrements;
    private float _smallestSize;
    private float _incrementSize;

    private void Awake()
    {
        _rectTransform = gameObject.GetComponent<RectTransform>();
        _image = gameObject.GetComponent<Image>();
        _maxSize = _rectTransform.sizeDelta;
        _maxIncrements = new Vector2((int) ((_maxSize.x - _smallestSize) / _incrementSize),
            (int) ((_maxSize.y - _smallestSize) / _incrementSize));
        
        _incrementSize = spritesPixelPerUnit * pixelPerIncrement;
        _smallestSize = 2 * _incrementSize; // Room for border without increments.
        
        float incrementsToTween = (Math.Max(_maxSize.x, _maxSize.y) - _smallestSize) / _incrementSize;
        tweenTime = useTimePerIncrement ? incrementsToTween * timePerIncrement : tweenTime;
    }

    public void Open()
    {
        LeanTween.cancel(gameObject);
        
        _image.enabled = true;
        
        LeanTween.value(gameObject, UpdateOpen, _smallestSize*Vector2.one, _maxSize, tweenTime)
            .setOnComplete(SetOpen);
    }

    private void UpdateOpen(Vector2 openAmount)
    {
        //_rectTransform.sizeDelta = openAmount;
        _rectTransform.sizeDelta = new Vector2(
            openAmount.x - openAmount.x % _incrementSize, 
            openAmount.y - openAmount.y % _incrementSize);
    }

    public void Close()
    {
        LeanTween.cancel(gameObject);
        
        StartClose();
        
        LeanTween.value(gameObject, UpdateOpen, _maxSize, _smallestSize*Vector2.one, tweenTime)
            .setOnComplete(ClosedDone);
    }

    private void UpdateClosed(Vector2 openAmount)
    {
        //_rectTransform.sizeDelta = openAmount;
        _rectTransform.sizeDelta = new Vector2(
            openAmount.x - openAmount.x % _incrementSize, 
            openAmount.y - openAmount.y % _incrementSize);
    }
    
    public void SetOpen()
    {
        foreach (GameObject innerElement in containedElements)
            innerElement.SetActive(true);

        _rectTransform.sizeDelta = _maxSize;
        _image.enabled = true;
        
        OnOpen?.Invoke();
    }
    
    public void SetClosed()
    {
        StartClose();
        ClosedDone();
    }

    private void StartClose()
    {
        foreach (GameObject innerElement in containedElements)
            innerElement.SetActive(false);
    }

    private void ClosedDone()
    {
        _image.enabled = false;
        _rectTransform.sizeDelta = _smallestSize * Vector2.one;
        OnClose?.Invoke();
    }
}
