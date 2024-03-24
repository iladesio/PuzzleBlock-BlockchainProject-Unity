using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animation script for moving up and down and object.
/// </summary>
public class CurrentLevelAnimation : MonoBehaviour
{
    public float moveDistance = 0.2f; // The distance to move up and down
    public float moveSpeed = 0.35f; // The speed of movement

    private Vector3 initialPosition;
    private bool movingUp = true;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        // Calculate target position based on whether moving up or down
        Vector3 targetPosition = movingUp ? initialPosition + Vector3.up * moveDistance : initialPosition;

        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // If reached the target position, switch direction
        if (transform.position == targetPosition)
        {
            movingUp = !movingUp;
        }
    }
}
