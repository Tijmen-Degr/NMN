using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Header("UI")]
    [TextArea]
    public string message = "Press E to interact";

    [Header("Area Settings")]
    public int areaID = 1;

    [Header("Camera")]
    public Transform cameraTarget;

    private InteractablesManager manager;
    private CameraController cameraController;

    private void Awake()
    {
        manager = FindFirstObjectByType<InteractablesManager>();
        cameraController = FindFirstObjectByType<CameraController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        manager?.EnterInteractable(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        manager?.ExitInteractable(this);
    }

    public virtual void OnInteract()
    {
        manager?.UpdateText(message);

        if (cameraController != null && cameraTarget != null)
            cameraController.MoveToArea(cameraTarget);

        Debug.Log($"[Interactable] Interacted with Area {areaID}");
    }
}
