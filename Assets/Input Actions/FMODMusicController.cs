using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class FMODMusicController : MonoBehaviour
{
    [Header("FMOD Music Event")]
    public EventReference musicEvent;

    private EventInstance musicInstance;
    private bool hasStarted = false;

    public static FMODMusicController Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartMusic()
    {
        if (hasStarted)
            return;

        musicInstance = RuntimeManager.CreateInstance(musicEvent);

        // Set default parameter to Incorrect = 1
        if (musicInstance.isValid())
        {
            musicInstance.setParameterByName("Correction", 1f);
            UnityEngine.Debug.Log("Music started: parameter 'Correction' set to 1 (Incorrect by default)");
        }

        musicInstance.start();
        hasStarted = true;
    }

    public EventInstance GetMusicInstance()
    {
        return musicInstance;
    }

    private void OnDestroy()
    {
        if (hasStarted)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}
