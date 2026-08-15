using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Open Settings")]
    public float openDistance = 1.5f;
    public float openSpeed = 3f;

    [Header("Door Sounds")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;

    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen;

    void Start()
    {
        // Lưu vị trí WORLD ban đầu
        if (leftDoor != null)
            leftClosedPos = leftDoor.position;

        if (rightDoor != null)
            rightClosedPos = rightDoor.position;

        // Hướng ngang của toàn bộ bộ cửa
        Vector3 horizontalDirection = transform.right;

        if (leftDoor != null)
        {
            leftOpenPos =
                leftClosedPos - horizontalDirection * openDistance;
        }

        if (rightDoor != null)
        {
            rightOpenPos =
                rightClosedPos + horizontalDirection * openDistance;
        }
    }

    void Update()
    {
        if (leftDoor != null)
        {
            Vector3 target =
                isOpen ? leftOpenPos : leftClosedPos;

            leftDoor.position = Vector3.Lerp(
                leftDoor.position,
                target,
                Time.deltaTime * openSpeed
            );
        }

        if (rightDoor != null)
        {
            Vector3 target =
                isOpen ? rightOpenPos : rightClosedPos;

            rightDoor.position = Vector3.Lerp(
                rightDoor.position,
                target,
                Time.deltaTime * openSpeed
            );
        }
    }

    private void PlayDoorSound(AudioClip clip)
    {
        if (doorAudioSource != null && clip != null)
        {
            doorAudioSource.PlayOneShot(clip);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            isOpen = true;
            PlayDoorSound(openSound);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isOpen)
        {
            isOpen = false;
            PlayDoorSound(closeSound);
        }
    }
}