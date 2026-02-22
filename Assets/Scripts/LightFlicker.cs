using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    private Light2D _light2D;

    [Header("Intensity Config")]
    public float baseIntensity = 1.25f;
    public float variationRange = 0.2f;

    [Header("Speed")]
    public float flickerSpeed = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _light2D = GetComponent<Light2D>();

        // Is it really a light?
        if (_light2D == null)
        {
            Debug.LogError("There's no Light2D in this object: " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_light2D != null)
        {
            // Readjustable on the Inspector
            float torchFlicker = Random.Range(-variationRange, variationRange);

            // Intensity applied
            _light2D.intensity = baseIntensity + torchFlicker;

        }

    }
        

}
