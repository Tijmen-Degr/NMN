using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InteractablesManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TMP_Text panelText;

    private Interactable currentInteractable;
    private InputAction interactAction;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        PlayerInput playerInput = GameObject.FindWithTag("Player")?.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interact"];
            interactAction.performed += OnInteractPerformed;
            interactAction.Enable();
        }
    }

    private void OnDestroy()
    {
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
    }

    public void EnterInteractable(Interactable interactable)
    {
        currentInteractable = interactable;
        if (panel != null && panelText != null)
        {
            panel.SetActive(true);
            panelText.text = interactable.message;
        }
    }

    public void ExitInteractable(Interactable interactable)
    {
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
            if (panel != null)
                panel.SetActive(false);
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnInteract();
        }
    }

    // Called by Interactable to update the panel text
    public void UpdateText(string newText)
    {
        if (panelText != null)
            panelText.text = newText;
    }
}
