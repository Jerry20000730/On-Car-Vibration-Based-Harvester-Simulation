using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// gif显示测试
/// </summary>
public class UniGifTest : MonoBehaviour
{
    public InputField gifUrlInput;
    public UniGifImage gifImage;
    public Button loadBtn;
    public Button playBtn;
    public Button stopBtn;
    public Button pauseBtn;
    public Button resumeBtn;

    /// <summary>
    /// 锁
    /// </summary>
    private bool m_mutex;

    private void Start()
    {
        loadBtn.onClick.AddListener(() => 
        {
            if (m_mutex || gifImage == null || string.IsNullOrEmpty(gifUrlInput.text))
            {
                return;
            }

            m_mutex = true;
            StartCoroutine(ViewGifCoroutine());
        });

        playBtn.onClick.AddListener(() => { gifImage.Play(); });
        stopBtn.onClick.AddListener(() => { gifImage.Stop(); });
        pauseBtn.onClick.AddListener(() => { gifImage.Pause(); });
        resumeBtn.onClick.AddListener(() => { gifImage.Resume(); });
    }

    private IEnumerator ViewGifCoroutine()
    {
        yield return StartCoroutine(gifImage.SetGifFromUrlCoroutine(gifUrlInput.text));
        m_mutex = false;
    }
}