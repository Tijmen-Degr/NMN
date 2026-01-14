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
    public string cameraResetMarkerName = "Start Player Walk 2";

    [Header("Minigame UI Root")]
    public GameObject minigameUIRoot;

    [Header("Countdown UI")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public float countdownStart = 3f;

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
    private EVENT_CALLBACK fmodCallback;
    private bool isRunning = false;

    private float flashTimer;
    private float resultTimer;
    private float anticipationTimer = -1f;

    private PlayerControls controls;
    private CameraController cameraController;

    private bool beatWindowOpen = false;
    private bool inputReceivedThisBeat = false;

    // ===== Countdown state =====
    private bool countdownActive = false;
    private float countdownTimer = 0f;
    private bool minigameEnabled = false;

    private enum RequiredInput { Left, Right }
    private RequiredInput currentInput;

    private volatile bool beatQueued = false;
    private volatile int queuedBeatNumber = 0;
    private volatile bool cameraResetQueued = false;

    [StructLayout(LayoutKind.Sequential)]
    struct TimelineBeatProperties
    {
        public int bar;
        public int beat;
        public float tempo;
        public int position;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct TimelineMarkerProperties
    {
        public IntPtr name;
        public int position;
    }

    private void Awake()
    {
        controls = new PlayerControls();
        cameraController = FindFirstObjectByType<CameraController>();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Gameplay.Mouse1.performed += _ => OnInput(RequiredInput.Left);
        controls.Gameplay.Mouse2.performed += _ => OnInput(RequiredInput.Right);
    }

    private void OnDisable()
    {
        controls.Gameplay.Mouse1.performed -= _ => OnInput(RequiredInput.Left);
        controls.Gameplay.Mouse2.performed -= _ => OnInput(RequiredInput.Right);
        controls.Disable();
    }

    public void StartBeatSystem()
    {
        if (isRunning) return;
        isRunning = true;

        // Initial state
        beatPanel.SetActive(false);
        leftPanel.SetActive(false);
        rightPanel.SetActive(false);
        resultPanel.SetActive(false);
        minigameUIRoot.SetActive(false);
        countdownPanel.SetActive(false);

        musicInstance = RuntimeManager.CreateInstance(musicEvent);

        fmodCallback = OnFmodEventCallback;
        musicInstance.setCallback(
            fmodCallback,
            EVENT_CALLBACK_TYPE.TIMELINE_BEAT | EVENT_CALLBACK_TYPE.TIMELINE_MARKER
        );

        musicInstance.start();
    }

    private void Update()
    {
        // ===============================
        // CAMERA STATE → COUNTDOWN / UI
        // ===============================
        if (cameraController != null)
        {
            if (cameraController.CurrentView == CameraController.CameraView.Area)
            {
                if (!countdownActive && !minigameEnabled)
                    StartCountdown();
            }
            else
            {
                ResetMinigameUI();
            }
        }

        // ===============================
        // COUNTDOWN
        // ===============================
        if (countdownActive)
        {
            countdownTimer -= Time.deltaTime;
            int displayNumber = Mathf.CeilToInt(countdownTimer);

            countdownText.text = displayNumber.ToString();

            if (countdownTimer <= 0f)
            {
                countdownActive = false;
                countdownPanel.SetActive(false);

                minigameEnabled = true;
                minigameUIRoot.SetActive(true);
            }
        }

        // ===============================
        // CAMERA RESET FROM FMOD
        // ===============================
        if (cameraResetQueued)
        {
            cameraResetQueued = false;
            cameraController?.ReturnToOriginal();
        }

        // ===============================
        // BEAT LOGIC (only if minigame enabled)
        // ===============================
        if (!minigameEnabled)
            return;

        if (beatQueued)
        {
            beatQueued = false;

            // Only flash beats 1 & 3
            if (queuedBeatNumber == 1 || queuedBeatNumber == 3)
            {
                anticipationTimer = anticipationTime;

                currentInput = UnityEngine.Random.value < 0.5f
                    ? RequiredInput.Left
                    : RequiredInput.Right;

                leftPanel.SetActive(currentInput == RequiredInput.Left);
                rightPanel.SetActive(currentInput == RequiredInput.Right);
            }
        }

        // Anticipation timing before beat flash
        if (anticipationTimer > 0f)
        {
            anticipationTimer -= Time.deltaTime;
            if (anticipationTimer <= 0f)
            {
                beatPanel.SetActive(true);
                beatWindowOpen = true;
                inputReceivedThisBeat = false;
                flashTimer = flashDuration;
            }
        }

        // Flash panel duration
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                beatPanel.SetActive(false);
                beatWindowOpen = false;
                leftPanel.SetActive(false);
                rightPanel.SetActive(false);

                if (!inputReceivedThisBeat)
                    ShowResult("MISS", Color.red);
            }
        }

        // Result timer
        if (resultTimer > 0f)
        {
            resultTimer -= Time.deltaTime;
            if (resultTimer <= 0f)
                resultPanel.SetActive(false);
        }
    }

    private void StartCountdown()
    {
        countdownActive = true;
        minigameEnabled = false;
        countdownTimer = countdownStart;

        countdownPanel.SetActive(true);
        minigameUIRoot.SetActive(false);
    }

    private void ResetMinigameUI()
    {
        countdownActive = false;
        minigameEnabled = false;

        countdownPanel.SetActive(false);
        minigameUIRoot.SetActive(false);
        beatPanel.SetActive(false);
        leftPanel.SetActive(false);
        rightPanel.SetActive(false);
    }

    private void OnInput(RequiredInput input)
    {
        if (!beatWindowOpen || !minigameEnabled)
        {
            ShowResult("MISS", Color.red);
            return;
        }

        inputReceivedThisBeat = true;

        if (input == currentInput)
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
        else if (type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            var marker = Marshal.PtrToStructure<TimelineMarkerProperties>(parameters);
            string markerName = Marshal.PtrToStringAnsi(marker.name);

            if (markerName == cameraResetMarkerName)
                cameraResetQueued = true;
        }

        return FMOD.RESULT.OK;
    }
}
