using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Header("UI")]
    [TextArea]
    public string message = "Press E to interact";

    [Header("Area Settings")]
    public int areaID = 1;

    private InteractablesManager manager;

    private void Awake()
    {
        manager = FindFirstObjectByType<InteractablesManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (manager != null)
            manager.EnterInteractable(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (manager != null)
            manager.ExitInteractable(this);
    }

    // Called when player presses Interact
    public virtual void OnInteract()
    {
        if (manager != null)
        {
            manager.UpdateText(message);
        }

        Debug.Log($"[Interactable] Interacted with Area {areaID}");
    }
}
