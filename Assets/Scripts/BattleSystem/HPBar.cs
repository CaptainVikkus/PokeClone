using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject heath;
    // Start is called before the first frame update
    void Start()
    {
        heath.transform.localScale = new Vector3(0.5f, 1.0f);
        
    }

    public void SetUpHP (float hpNormalized)
    {
        heath.transform.localScale = new Vector3(hpNormalized, 1.0f);
    }
}
