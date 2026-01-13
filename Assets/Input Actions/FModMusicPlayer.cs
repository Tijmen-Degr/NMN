using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayFMODSong : MonoBehaviour
{
    [Header("FMOD Event")]
    public EventReference musicEvent; // Assign your FMOD music event in the inspector

    private EventInstance musicInstance;

    void Start()
    {
        // Create an instance of the FMOD event
        musicInstance = RuntimeManager.CreateInstance(musicEvent);

        // Start playing the music
        musicInstance.start();
    }

    private void OnDestroy()
    {
        // Stop and release the FMOD instance when the object is destroyed
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}
