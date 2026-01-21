using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject controlsPanel;

    void Awake()
    {
        // Ensure the controls page starts hidden (safe if not assigned)
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    // Loads a scene by name (wired to UI buttons)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Show the controls page (wire to "Controls" button)
    public void OpenControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
        else
            Debug.LogWarning("MainMenu: controlsPanel is not assigned in the Inspector.");
    }

    // Hide the controls page (wire to "Back" button on the controls page)
    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
