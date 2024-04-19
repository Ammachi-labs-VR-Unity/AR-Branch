using SolarModule;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    private Vector2 fingerDownPosition;
    private Vector2 fingerUpPosition;

    [SerializeField]
    private float minSwipeDistance = 5f;

    private void Update()
    {
        // Check for user input
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // Check for the beginning of a touch
            if (touch.phase == TouchPhase.Began)
            {
                fingerDownPosition = touch.position;
                fingerUpPosition = touch.position;
            }

            // Check for the end of a touch
            if (touch.phase == TouchPhase.Ended)
            {
                fingerUpPosition = touch.position;
                DetectSwipe();
            }
        }
    }

    private void DetectSwipe()
    {
        // Calculate swipe direction based on finger positions
        Vector2 swipeDirection = fingerUpPosition - fingerDownPosition;

        // Check if swipe distance is greater than the minimum threshold
        if (swipeDirection.magnitude > minSwipeDistance)
        {
            // Check if swipe is horizontal
            if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
            {
                // Swipe right
                if (swipeDirection.x > 0)
                {
                    Debug.Log("Swipe Right Detected!");
                    ModelManager.Instance.RotateRight();
                }
                // Swipe left
                else
                {
                    Debug.Log("Swipe Left Detected!");
                    ModelManager.Instance.RotateLeft();
                }
            }
        }
    }
}
