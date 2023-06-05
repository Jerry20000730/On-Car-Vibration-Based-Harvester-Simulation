using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PanelAnim : MonoBehaviour
{
    public AnimationCurve showCurve;
    public AnimationCurve hideCurve;
    public float AnimationSpeed;
    public GameObject loadingPanel;

    public GameObject straight_arrow;
    public GameObject left_arrow;
    public GameObject right_arrow;

    IEnumerator ShowPanel(GameObject gameObject)
    {
        float timer = 0;
        while (timer <= 1)
        {
            gameObject.transform.localScale = Vector3.one * showCurve.Evaluate(timer);
            timer += Time.deltaTime * AnimationSpeed;
            yield return null;
        }
        timer = 0;
        // do something here
        while (timer <= 5)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(HidePanel(loadingPanel));
    }

    IEnumerator HidePanel(GameObject gameObject)
    {
        float timer = 0;
        while (timer <= 1)
        {
            gameObject.transform.localScale = Vector3.one * hideCurve.Evaluate(timer);
            timer += Time.deltaTime * AnimationSpeed;
            yield return null;
        }
        left_arrow.GetComponent<CanvasGroup>().alpha = 1f;
        left_arrow.GetComponent<RectTransform>().localPosition = new Vector3(left_arrow.GetComponent<RectTransform>().localPosition.x, left_arrow.GetComponent<RectTransform>().localPosition.y+4f, left_arrow.GetComponent<RectTransform>().localPosition.z);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            StartCoroutine(ShowPanel(loadingPanel));
        }
    }
}
