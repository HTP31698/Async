using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LoadSceneManager : MonoBehaviour
{
    [Header("SceneLoad")]
    public Button sceneLoadButton;
    public CanvasGroup sceneLoadPanel;
    public Slider sceneLoadSlider;
    public TextMeshProUGUI sceneLoadText;
    public static AsyncOperation LoadSceneAsync;
    private bool _isClick;

    private void Start()
    {
        //SceneLoad
        sceneLoadPanel.gameObject.SetActive(false);
        sceneLoadSlider.gameObject.SetActive(false);
        sceneLoadButton.onClick.AddListener(() => OnSceneLoadClicked().Forget());
        _isClick = false;
    }

    //SceneLoad
    private void SceneLoadText(string msg)
    {
        sceneLoadText.text = msg;
    }
    private async UniTask FadeAsync(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        canvasGroup.alpha = from;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            await UniTask.Yield();
        }
        canvasGroup.alpha = to;
    }
    private void FakeSceneLoadAsync(Slider progressBar, float ms)
    {
        SceneLoadText($"Loading... {ms * 100}%");
        progressBar.value = ms;
        if (ms >= 0.89)
        {
            SceneLoadText($"Loading... {100}%");
            progressBar.value = 1;
        }
    }
    async UniTask LoadSceneUniTask(Slider progressBar)
    {
        var progress = Progress.Create<float>(x => FakeSceneLoadAsync(progressBar, x));
        await SceneManager.LoadSceneAsync("47UniTask").ToUniTask(progress);
    }
    private async UniTask OnSceneLoadClicked()
    {
        if (_isClick)
            return;

        DontDestroyOnLoad(sceneLoadPanel.transform.root.gameObject);

        _isClick = true;
        sceneLoadPanel.gameObject.SetActive(true);
        await FadeAsync(sceneLoadPanel, 0f, 1f, 1f);


        sceneLoadSlider.gameObject.SetActive(true);
        await LoadSceneUniTask(sceneLoadSlider);
        sceneLoadSlider.gameObject.SetActive(false);

        await FadeAsync(sceneLoadPanel, 1f, 0f, 1f);
        sceneLoadPanel.gameObject.SetActive(false);

        Destroy(sceneLoadPanel.transform.root.gameObject);
    }

}
