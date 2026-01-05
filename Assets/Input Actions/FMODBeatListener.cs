using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

public class FMODBeatListener : MonoBehaviour
{
    [Header("FMOD Music Event")]
    public EventReference musicEvent;

    private EventInstance musicInstance;
    private FMOD.Studio.EVENT_CALLBACK beatCallback;

    // Public beat event for the rest of the game (every beat)
    public static event Action OnBeat;

    // Thread-safe queue to move callbacks to main thread
    private static readonly ConcurrentQueue<Action> mainThreadActions
        = new ConcurrentQueue<Action>();

    void Start()
    {
        // Get the music instance created by the controller
        musicInstance = FMODMusicController.Instance.GetMusicInstance();

        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
        musicInstance.setCallback(
            beatCallback,
            EVENT_CALLBACK_TYPE.TIMELINE_BEAT
        );
    }


    void Update()
    {
        // Execute queued beat events on Unity main thread
        while (mainThreadActions.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT BeatEventCallback(
        EVENT_CALLBACK_TYPE type,
        IntPtr eventInstance,
        IntPtr parameters)
    {
        if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            // Always trigger on every beat
            mainThreadActions.Enqueue(() =>
            {
                OnBeat?.Invoke();
            });
        }

        return FMOD.RESULT.OK;
    }
}
