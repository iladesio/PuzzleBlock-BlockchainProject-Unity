using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelAnimation : MonoBehaviour
{
    // Set the speed of scaling
    private float scalingSpeed = 8f; 

    private bool isScaling;

    /// <summary>
    /// Trigger when the GameObject gets enabled
    /// </summary>
    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        isScaling = true;
    }

    /// <summary>
    /// Trigger when the GameObject is disabled
    /// </summary>
    private void OnDisable()
    {
        transform.localScale = Vector3.one;
        isScaling = false;

    }

    /// <summary>
    /// Execute every frame, modify local position of the GameObject according to its current state
    /// </summary>
    private void Update()
    {
        if (isScaling)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * scalingSpeed);

        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * scalingSpeed);

        }
    }
}
