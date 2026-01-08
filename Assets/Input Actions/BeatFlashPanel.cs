using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Runtime.InteropServices;

public class BeatFlashPanel : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference musicEvent;

    [Header("UI")]
    public GameObject panel;
    public float flashDuration = 0.1f;

    private EventInstance musicInstance;

    // Flag set from FMOD audio thread
    private volatile bool beatTriggered = false;

    private bool isRunning = false;
    private float flashTimer = 0f;

    // IMPORTANT: keep delegate alive
    private EVENT_CALLBACK beatCallback;

    public void StartBeatSystem()
    {
        if (isRunning) return;
        isRunning = true;

        if (panel != null)
            panel.SetActive(false);

        if (!musicInstance.isValid())
            musicInstance = RuntimeManager.CreateInstance(musicEvent);

        // Assign named callback
        beatCallback = OnFmodEventCallback;
        musicInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        musicInstance.start();
    }

    private void Update()
    {
        if (beatTriggered)
        {
            beatTriggered = false;
            panel.SetActive(true);
            flashTimer = flashDuration;
        }

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && panel.activeSelf)
                panel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }

    // ==================================
    // FMOD CALLBACK (AUDIO THREAD)
    // ==================================
    private FMOD.RESULT OnFmodEventCallback(
        EVENT_CALLBACK_TYPE type,
        IntPtr eventInstance,
        IntPtr parameters)
    {
        if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            // DO NOT call Unity here
            beatTriggered = true;
        }

        return FMOD.RESULT.OK;
    }
}
