using Pooling;
using UnityEngine;
using UnityEngine.UI;

public class ContentDisplayElement : MonoBehaviour
{
    public Image logoImage;
    
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
