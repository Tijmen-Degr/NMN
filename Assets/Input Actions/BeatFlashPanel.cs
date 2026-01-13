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
    public float flashDuration = 0.25f;
    public float anticipationTime = 0.5f;

    [Header("Direction Panels")]
    public GameObject leftPanel;
    public GameObject rightPanel;

    [Header("Hit UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public float resultDisplayTime = 0.5f;

    private EventInstance musicInstance;
    private bool isRunning = false;

    private float flashTimer;
    private float resultTimer;
    private float scheduledFlashTimer = -1f;

    private PlayerControls controls;
    private EVENT_CALLBACK beatCallback;

    private bool beatWindowOpen = false;
    private bool inputReceivedThisBeat = false;

    private volatile bool beatQueued = false;
    private volatile int queuedBeatNumber = 0;

    private enum BeatInput { Left, Right }
    private BeatInput currentRequiredInput;

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
        controls.Gameplay.Mouse1.performed += ctx => OnHit(BeatInput.Left);
        controls.Gameplay.Mouse2.performed += ctx => OnHit(BeatInput.Right);
    }

    private void OnDisable()
    {
        controls.Gameplay.Mouse1.performed -= ctx => OnHit(BeatInput.Left);
        controls.Gameplay.Mouse2.performed -= ctx => OnHit(BeatInput.Right);
        controls.Disable();
    }

    public void StartBeatSystem()
    {
        if (isRunning) return;
        isRunning = true;

        beatPanel.SetActive(false);
        leftPanel.SetActive(false);
        rightPanel.SetActive(false);
        resultPanel.SetActive(false);

        musicInstance = RuntimeManager.CreateInstance(musicEvent);

        beatCallback = OnFmodEventCallback;
        musicInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        musicInstance.start();
    }

    private void Update()
    {
        if (beatQueued)
        {
            beatQueued = false;

            if (queuedBeatNumber == 1 || queuedBeatNumber == 3)
            {
                scheduledFlashTimer = anticipationTime;

                currentRequiredInput = UnityEngine.Random.value < 0.5f ? BeatInput.Left : BeatInput.Right;

                leftPanel.SetActive(currentRequiredInput == BeatInput.Left);
                rightPanel.SetActive(currentRequiredInput == BeatInput.Right);
            }
        }

        if (scheduledFlashTimer > 0f)
        {
            scheduledFlashTimer -= Time.deltaTime;
            if (scheduledFlashTimer <= 0f)
            {
                beatPanel.SetActive(true);
                beatWindowOpen = true;
                inputReceivedThisBeat = false;
                flashTimer = flashDuration;
            }
        }

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                beatPanel.SetActive(false);
                beatWindowOpen = false;
                leftPanel.SetActive(false);
                rightPanel.SetActive(false);

                // No input = MISS
                if (!inputReceivedThisBeat)
                    ShowResult("MISS", Color.red);
            }
        }

        if (resultTimer > 0f)
        {
            resultTimer -= Time.deltaTime;
            if (resultTimer <= 0f)
                resultPanel.SetActive(false);
        }
    }

    private void OnHit(BeatInput input)
    {
        if (!beatWindowOpen)
        {
            ShowResult("MISS", Color.red);
            return;
        }

        inputReceivedThisBeat = true;

        if (input == currentRequiredInput)
            ShowResult("CORRECT", Color.green);
        else
            ShowResult("WRONG", Color.yellow);
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
            var beat = Marshal.PtrToStructure<TimelineBeatProperties>(parameters);
            queuedBeatNumber = beat.beat;
            beatQueued = true;
        }

        return FMOD.RESULT.OK;
    }
}
