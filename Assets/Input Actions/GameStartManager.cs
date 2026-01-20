using UnityEngine;
using UnityEngine.InputSystem;

public class GameStartManager : MonoBehaviour
{
    private PlayerControls controls;
    private bool gameStarted = false;

    [Header("Beat System")]
    public BeatFlashPanel beatFlashPanel; // Assign in Inspector

    private void Awake()
    {
        if (controls == null)
            controls = new PlayerControls();

        controls.Gameplay.StartGameF.performed += OnStartGame;
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new PlayerControls();

        controls.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            // unsubscribe to avoid leaks and NREs
            controls.Gameplay.StartGameF.performed -= OnStartGame;
            controls.Disable();
        }
    }

    private void OnStartGame(InputAction.CallbackContext context)
    {
        if (gameStarted) return;
        gameStarted = true;

        // Start music and beat system
        if (beatFlashPanel != null)
            beatFlashPanel.StartBeatSystem();

        Debug.Log("Game Started!");
    }
}
