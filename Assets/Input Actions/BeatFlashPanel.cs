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

    // Reference to your slider/progress controller
    [Header("Progress")]
    public SliderController sliderController;

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

    // track whether we've attached input handlers so we don't double-subscribe
    private bool inputsAttached = false;

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
        if (controls == null)
            controls = new PlayerControls();

        // quick developer hint if UI references are missing
        if (resultPanel == null || resultText == null)
            Debug.LogWarning("[BeatFlashPanel] resultPanel or resultText is not assigned in the Inspector. Assign them to avoid exceptions.");
            cameraController = FindFirstObjectByType<CameraController>();
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new PlayerControls();

        controls.Enable();

        if (!inputsAttached)
        {
            controls.Gameplay.Mouse1.performed += OnMouse1;
            controls.Gameplay.Mouse2.performed += OnMouse2;
            inputsAttached = true;
        }
    }

    private void OnDisable()
    {
        if (controls != null && inputsAttached)
        {
            controls.Gameplay.Mouse1.performed -= OnMouse1;
            controls.Gameplay.Mouse2.performed -= OnMouse2;
            inputsAttached = false;
        }

        if (controls != null)
            controls.Disable();
    }

    // Named handlers forward to OnHit so removal works
    private void OnMouse1(InputAction.CallbackContext ctx) => OnHit(BeatInput.Left);
    private void OnMouse2(InputAction.CallbackContext ctx) => OnHit(BeatInput.Right);

    public void StartBeatSystem()
    {
        if (isRunning) return;
        isRunning = true;

        if (beatPanel != null) beatPanel.SetActive(false);
        if (leftPanel != null) leftPanel.SetActive(false);
        if (rightPanel != null) rightPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

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

                if (leftPanel != null) leftPanel.SetActive(currentRequiredInput == BeatInput.Left);
                if (rightPanel != null) rightPanel.SetActive(currentRequiredInput == BeatInput.Right);
            }
        }

        // Anticipation timing before beat flash
        if (anticipationTimer > 0f)
        {
            anticipationTimer -= Time.deltaTime;
            if (anticipationTimer <= 0f)
            {
                if (beatPanel != null) beatPanel.SetActive(true);
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
                if (beatPanel != null) beatPanel.SetActive(false);
                beatWindowOpen = false;
                if (leftPanel != null) leftPanel.SetActive(false);
                if (rightPanel != null) rightPanel.SetActive(false);

                // No input = MISS -> remove progress
                if (!inputReceivedThisBeat)
                {
                    Debug.Log("[BeatFlashPanel] No input on beat -> MISS, removing progress.");
                    sliderController?.RemoveProgress();
                    ShowResult("MISS", Color.red);
                }
            }
        }

        // Result timer
        if (resultTimer > 0f)
        {
            resultTimer -= Time.deltaTime;
            if (resultTimer <= 0f)
            {
                if (resultPanel != null)
                    resultPanel.SetActive(false);
            }
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
            Debug.Log("[BeatFlashPanel] Hit outside window -> MISS, removing progress.");
            sliderController?.RemoveProgress();
            ShowResult("MISS", Color.red);
            return;
        }

        inputReceivedThisBeat = true;

        if (input == currentRequiredInput)
        {
            Debug.Log("[BeatFlashPanel] CORRECT hit -> updating progress.");
            sliderController?.UpdateProgress();
            ShowResult("CORRECT", Color.green);
        }
        else
        {
            Debug.Log("[BeatFlashPanel] WRONG hit (within window).");
            // optionally penalize here:
            // sliderController?.RemoveProgress();
            ShowResult("WRONG", Color.yellow);
        }
    }

    private void ShowResult(string text, Color color)
    {
        // defensive: avoid throwing in input callbacks if designer forgot to assign UI
        if (resultPanel != null)
            resultPanel.SetActive(true);
        else
            Debug.LogWarning("[BeatFlashPanel] ShowResult called but resultPanel is not assigned.");

        if (resultText != null)
        {
            resultText.text = text;
            resultText.color = color;
        }
        else
        {
            Debug.LogWarning($"[BeatFlashPanel] ShowResult called but resultText is not assigned. text='{text}'");
        }

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

        if (controls != null && inputsAttached)
        {
            controls.Gameplay.Mouse1.performed -= OnMouse1;
            controls.Gameplay.Mouse2.performed -= OnMouse2;
            inputsAttached = false;
        }

        if (controls != null)
            controls.Disable();
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
