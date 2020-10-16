using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject heath;

    public void SetUpHP (float hpNormalized)
    {
        heath.transform.localScale = new Vector3(hpNormalized, 1.0f);
    }

    public IEnumerator SetHpSmooth(float newHpNormalized)
    {
        float curHPNormalized = heath.transform.localScale.x;
        float changeAmt = curHPNormalized - newHpNormalized;

        while (curHPNormalized - newHpNormalized > Mathf.Epsilon)
        {
            curHPNormalized -= changeAmt * Time.deltaTime;
            SetUpHP(curHPNormalized);
            yield return null;
        }

        SetUpHP(newHpNormalized);
    }
}
