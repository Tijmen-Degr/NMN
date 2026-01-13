using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine.InputSystem;

public class BeatFlashPanel : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference musicEvent;

    [Header("Beat Flash")]
    public GameObject beatPanel;
    public float flashDuration = 0.12f;

    [Header("Hit UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public float resultDisplayTime = 0.5f;

    private EventInstance musicInstance;
    private bool isRunning = false;

    private float flashTimer;
    private float resultTimer;

    private PlayerControls controls;
    private EVENT_CALLBACK beatCallback;

    private volatile bool beatTriggered = false;
    private bool beatWindowOpen = false;

    [StructLayout(LayoutKind.Sequential)]
    struct TimelineBeatProperties
    {
        public int bar;
        public int beat;
        public float tempo;
        public int position;
    }

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Gameplay.Mouse1.performed += OnHit;
    }

    private void OnDisable()
    {
        controls.Gameplay.Mouse1.performed -= OnHit;
        controls.Disable();
    }

    public void StartBeatSystem()
    {
        if (isRunning) return;
        isRunning = true;

        beatPanel.SetActive(false);
        resultPanel.SetActive(false);

        musicInstance = RuntimeManager.CreateInstance(musicEvent);

        beatCallback = OnFmodEventCallback;
        musicInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        musicInstance.start();
    }

    private void Update()
    {
        if (beatTriggered)
        {
            beatTriggered = false;

            beatPanel.SetActive(true);
            beatWindowOpen = true;
            flashTimer = flashDuration;
        }

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                beatPanel.SetActive(false);
                beatWindowOpen = false;
            }
        }

        if (resultTimer > 0f)
        {
            resultTimer -= Time.deltaTime;
            if (resultTimer <= 0f)
                resultPanel.SetActive(false);
        }
    }

    private void OnHit(InputAction.CallbackContext ctx)
    {
        if (beatWindowOpen)
            ShowResult("CORRECT", Color.green);
        else
            ShowResult("MISS", Color.red);
    }

    private void ShowResult(string text, Color color)
    {
        resultPanel.SetActive(true);
        resultText.text = text;
        resultText.color = color;
        resultTimer = resultDisplayTime;
    }

    private void OnDestroy()
    {
        isRunning = false;

        if (musicInstance.isValid())
        {
            musicInstance.setCallback(null);
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }

    // ============================
    // FMOD AUDIO THREAD
    // ============================
    private FMOD.RESULT OnFmodEventCallback(
        EVENT_CALLBACK_TYPE type,
        IntPtr eventInstance,
        IntPtr parameters)
    {
        if (!isRunning)
            return FMOD.RESULT.OK;

        if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            beatTriggered = true;
        }

        return FMOD.RESULT.OK;
    }
}
