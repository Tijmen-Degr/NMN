using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

public class FMODMarkerListener : MonoBehaviour
{
    private FMOD.Studio.EVENT_CALLBACK markerCallback;

    private static readonly ConcurrentQueue<Action> mainThreadActions
        = new ConcurrentQueue<Action>();

    void Start()
    {
        var controller = FMODMusicController.Instance;
        if (controller == null) return;

        EventInstance musicInstance = controller.GetMusicInstance(); // non-nullable
        if (!musicInstance.isValid())
        {
            UnityEngine.Debug.LogWarning("Music instance invalid for marker listener!");
            return;
        }

        markerCallback = new FMOD.Studio.EVENT_CALLBACK(MarkerCallback);
        musicInstance.setCallback(markerCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
    }

    void Update()
    {
        while (mainThreadActions.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT MarkerCallback(
        EVENT_CALLBACK_TYPE type,
        IntPtr eventInstance,
        IntPtr parameters)
    {
        if (type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            var markerProps = (TIMELINE_MARKER_PROPERTIES)
                Marshal.PtrToStructure(parameters, typeof(TIMELINE_MARKER_PROPERTIES));

            string markerName = markerProps.name;

            if (markerName == "DetectionCorrection")
            {
                mainThreadActions.Enqueue(() =>
                {
                    EventInstance musicInstance = FMODMusicController.Instance.GetMusicInstance(); // non-nullable
                    if (!musicInstance.isValid())
                    {
                        UnityEngine.Debug.LogWarning("Music instance invalid at marker!");
                        return;
                    }

                    if (Minigame1Gate.Instance != null)
                        Minigame1Gate.Instance.Resolve(musicInstance);
                    else
                        UnityEngine.Debug.LogWarning("Minigame1Gate not found in scene.");
                });
            }
        }

        return FMOD.RESULT.OK;
    }
}
