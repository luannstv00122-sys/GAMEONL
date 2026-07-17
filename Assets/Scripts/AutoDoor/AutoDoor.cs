using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Open Settings")]
    public float openDistance = 1.5f;
    public float openSpeed = 3f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;

    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen;

    void Start()
    {
        if (leftDoor != null)
        {
            leftClosedPos = leftDoor.localPosition;
            leftOpenPos = leftClosedPos + Vector3.left * openDistance;
        }

        if (rightDoor != null)
        {
            rightClosedPos = rightDoor.localPosition;
            rightOpenPos = rightClosedPos + Vector3.right * openDistance;
        }
    }

    void Update()
    {
        if (leftDoor != null)
        {
            Vector3 targetPos = isOpen ? leftOpenPos : leftClosedPos;

            leftDoor.localPosition = Vector3.Lerp(
                leftDoor.localPosition,
                targetPos,
                Time.deltaTime * openSpeed
            );
        }

        if (rightDoor != null)
        {
            Vector3 targetPos = isOpen ? rightOpenPos : rightClosedPos;

            rightDoor.localPosition = Vector3.Lerp(
                rightDoor.localPosition,
                targetPos,
                Time.deltaTime * openSpeed
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = false;
        }
    }
}