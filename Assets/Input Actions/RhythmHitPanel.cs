using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class RhythmHitPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject beatPanel;
    public GameObject feedbackPanel;
    public TMP_Text feedbackText;

    [Header("Timing")]
    public float panelVisibleTime = 0.15f;
    public float perfectWindow = 0.35f;

    private float beatTime; // time when the panel appeared
    private bool awaitingInput = false;

    void OnEnable()
    {
        FMODBeatListener.OnBeat += ShowBeatPanel;
    }

    void OnDisable()
    {
        FMODBeatListener.OnBeat -= ShowBeatPanel;
    }

    void Update()
    {
        if (awaitingInput && Mouse.current.leftButton.wasPressedThisFrame)
        {
            float inputTime = Time.time;
            float diff = Mathf.Abs(inputTime - beatTime);

            string result;

            if (diff <= perfectWindow)
                result = "Perfect";
            else
                result = "Miss";

            StartCoroutine(ShowFeedback(result));
            awaitingInput = false;
        }
    }

    private void ShowBeatPanel()
    {
        StopAllCoroutines();
        StartCoroutine(FlashBeatPanel());
    }

    private IEnumerator FlashBeatPanel()
    {
        beatPanel.SetActive(true);
        beatTime = Time.time;
        awaitingInput = true;

        yield return new WaitForSeconds(panelVisibleTime);
        beatPanel.SetActive(false);

        // If no input was pressed during the window, count as Miss
        if (awaitingInput)
        {
            awaitingInput = false;
            StartCoroutine(ShowFeedback("Miss"));
        }
    }

    private IEnumerator ShowFeedback(string result)
    {
        feedbackText.text = result;
        feedbackPanel.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        feedbackPanel.SetActive(false);
    }
}
