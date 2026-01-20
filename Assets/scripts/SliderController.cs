using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [Header("UI")]
    public Text ScoreText;
    public Slider slider;

    [Header("Score amounts (change in Inspector)")]
    public int scoreOnMiss = -10;
    public int scoreOnWrong = -5;
    public int scoreOnCorrect = 15;

    int progress = 0;

    void Start()
    {
        // Try to auto-assign if fields were not set in the Inspector
        if (slider == null) slider = GetComponentInChildren<Slider>();
        if (ScoreText == null) ScoreText = GetComponentInChildren<Text>();

        if (slider != null)
        {
            slider.wholeNumbers = true;
            progress = Mathf.RoundToInt(slider.value);
            // Update slider visually without firing callbacks
            slider.SetValueWithoutNotify(progress);
        }

        UpdateScoreText();
    }

    // Called by the UI when the user drags the slider (if wired)
    public void OnSliderChanged(float value)
    {
        progress = Mathf.RoundToInt(value);
        UpdateScoreText();
    }

    // Convenience methods
    public void AddMiss()    => AddScore(scoreOnMiss);
    public void AddWrong()   => AddScore(scoreOnWrong);
    public void AddCorrect() => AddScore(scoreOnCorrect);

    // Generic API to change score by any delta
    public void AddScore(int delta)
    {
        int newProgress = progress + delta;

        if (slider != null)
        {
            // Respect the slider's configured min/max (Unity handles max)
            int clamped = Mathf.Clamp(newProgress, (int)slider.minValue, (int)slider.maxValue);
            progress = clamped;
            slider.SetValueWithoutNotify(progress); // avoid re-entering OnSliderChanged
        }
        else
        {
            progress = newProgress;
        }

        UpdateScoreText();
    }

    // Backwards-compatible simple increment/decrement
    public void UpdateProgress() => AddScore(1);
    public void RemoveProgress() => AddScore(-1);

    void UpdateScoreText()
    {
        if (ScoreText != null)
            ScoreText.text = progress.ToString();
    }
}
