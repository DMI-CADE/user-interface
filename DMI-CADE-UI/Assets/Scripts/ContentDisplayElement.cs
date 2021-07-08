using System.Collections;
using System.Collections.Generic;
using Pooling;
using UnityEngine;

public class ContentDisplayElement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToPool()
    {
        ContentDisplayElementPool.Instance.ReturnToPool(this);
    }
}
