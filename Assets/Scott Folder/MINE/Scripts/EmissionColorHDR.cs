using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionColorHDR : MonoBehaviour
{
    [ColorUsage(true,true)]
    public Color HDREmissionColor=Color.white;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Light>().color=HDREmissionColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
