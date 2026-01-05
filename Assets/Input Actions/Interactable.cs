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

    private static bool musicStarted = false;

    [Header("Minigame Settings")]
    public Transform minigameCameraPosition; // assign in inspector
    public RhythmHitPanel rhythmMinigame;    // assign in inspector

    private void Awake()
    {
        manager = FindFirstObjectByType<InteractablesManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && manager != null)
            manager.EnterInteractable(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && manager != null)
            manager.ExitInteractable(this);
    }

    public virtual void OnInteract()
    {
        // Area 1 logic
        if (areaID == 1)
        {
            // Mark correct in Minigame1Gate
            if (Minigame1Gate.Instance != null)
                Minigame1Gate.Instance.Area1Interacted();

            // Move camera to minigame position
            if (minigameCameraPosition != null)
            {
                Camera.main.transform.position = minigameCameraPosition.position;
                Camera.main.transform.rotation = minigameCameraPosition.rotation;
            }

            // Start rhythm minigame
            if (rhythmMinigame != null)
                rhythmMinigame.gameObject.SetActive(true);

            if (manager != null)
                manager.UpdateText("Minigame Started!");
        }

        // Area 2 starts music once
        if (areaID == 2 && !musicStarted && FMODMusicController.Instance != null)
        {
            FMODMusicController.Instance.StartMusic();
            musicStarted = true;

            if (manager != null)
                manager.UpdateText("Music Started!");
        }
    }
}
