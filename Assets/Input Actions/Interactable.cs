using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [TextArea]
    public string message = "Press E to interact";

    public int areaID = 1; // Unique number for each area

    private InteractablesManager manager;

    private void Awake()
    {
        Interactable interactable = Object.FindFirstObjectByType<Interactable>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && manager != null)
        {
            manager.EnterInteractable(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && manager != null)
        {
            manager.ExitInteractable(this);
        }
    }

    // Called when player presses E while in this area
    public virtual void OnInteract()
    {
        if (manager != null)
        {
            manager.UpdateText("Area " + areaID + " here!");
        }
    }
}
