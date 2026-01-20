using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SliderController : MonoBehaviour
{
    public Text ScoreText;
    int progress = 0;
    public Slider slider;

    public void OnSliderChanged(float value) { 
        ScoreText.text = value.ToString();
    }

    public void UpdateProgress() {
        progress ++ ;
        slider.value = progress;
    }

    public void RemoveProgress()
    {
        progress--;
        slider.value = progress;
    }
}
