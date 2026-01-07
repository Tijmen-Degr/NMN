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
    private volatile bool beatThisFrame = false;
    private bool isRunning = false;

    private float flashTimer = 0f;

    // Call from GameStartManager
    public void StartBeatSystem()
    {
        if (isRunning) return;
        isRunning = true;

        if (panel != null)
            panel.SetActive(false);

        // Create only one instance
        if (!musicInstance.isValid())
            musicInstance = RuntimeManager.CreateInstance(musicEvent);

        // Set FMOD callback for tempo marker beats
        musicInstance.setCallback((type, instance, ptr) =>
        {
            if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
            {
                beatThisFrame = true;
            }
            return FMOD.RESULT.OK;
        }, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        // Start music
        musicInstance.start();
    }

    private void Update()
    {
        // Trigger panel flash when a beat happens
        if (beatThisFrame)
        {
            beatThisFrame = false;
            panel.SetActive(true);
            flashTimer = flashDuration;
        }

        // Turn off panel after flashDuration
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && panel.activeSelf)
            {
                panel.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release(); // Release memory safely
        }
    }
}
