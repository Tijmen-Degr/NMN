using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 3f;

    [Header("Camera Targets")]
    public Transform originalCameraTarget;

    public enum CameraView
    {
        Original,
        Area
    }

    public CameraView CurrentView { get; private set; } = CameraView.Original;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;

    private void Awake()
    {
        if (originalCameraTarget != null)
        {
            targetPosition = originalCameraTarget.position;
            targetRotation = originalCameraTarget.rotation;
        }
        else
        {
            targetPosition = transform.position;
            targetRotation = transform.rotation;
        }
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * moveSpeed
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * moveSpeed
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            isMoving = false;
        }
    }

    public void MoveToArea(Transform cameraTarget)
    {
        if (cameraTarget == null) return;

        targetPosition = cameraTarget.position;
        targetRotation = cameraTarget.rotation;
        isMoving = true;
        CurrentView = CameraView.Area;
    }

    public void ReturnToOriginal()
    {
        if (originalCameraTarget == null) return;

        targetPosition = originalCameraTarget.position;
        targetRotation = originalCameraTarget.rotation;
        isMoving = true;
        CurrentView = CameraView.Original;
    }
}
