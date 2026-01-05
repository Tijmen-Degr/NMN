using UnityEngine;
using FMOD.Studio;

public class Minigame1Gate : MonoBehaviour
{
    public static Minigame1Gate Instance;

    private bool area1Interacted = false;

    private void Awake()
    {
        Instance = this;
    }

    // Called when player interacts with Area 1
    public void Area1Interacted()
    {
        area1Interacted = true;
        UnityEngine.Debug.Log("Area 1 interacted: marking Correct");

        // Immediately update FMOD parameter if music is playing
        var controller = FMODMusicController.Instance;
        if (controller != null)
        {
            EventInstance musicInstance = controller.GetMusicInstance(); // non-nullable
            if (musicInstance.isValid())
            {
                musicInstance.setParameterByName("Correction", 0f); // Correct
                UnityEngine.Debug.Log("FMOD parameter 'Correction' set to 0 (Correct) immediately");
            }
        }
    }

    // Called when FMOD marker is reached
    public void Resolve(EventInstance musicInstance)
    {
        if (!musicInstance.isValid())
        {
            UnityEngine.Debug.LogWarning("Music instance invalid in Resolve()");
            return;
        }

        string paramName = "Correction";
        float value = area1Interacted ? 0f : 1f;

        musicInstance.setParameterByName(paramName, value);
        UnityEngine.Debug.Log($"[Marker] Setting parameter '{paramName}' = {value} ({(area1Interacted ? "Correct" : "Incorrect")})");
    }
}
