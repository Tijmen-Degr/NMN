using UnityEngine;
using System.Collections;

public class BeatPanelPopup : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;

    [Header("Timing")]
    public float visibleTime = 0.1f;

    void OnEnable()
    {
        FMODBeatListener.OnBeat += HandleBeat;
    }

    void OnDisable()
    {
        FMODBeatListener.OnBeat -= HandleBeat;
    }

    private void HandleBeat()
    {
        StopAllCoroutines();
        StartCoroutine(FlashPanel());
    }

    private IEnumerator FlashPanel()
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(visibleTime);
        panel.SetActive(false);
    }
}
