using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Manager : MonoBehaviour
{
    [Header("StopWatch")]
    public Button startButton;
    public Button stopButton;
    public Button reStartButton;
    public Button resetButton;
    public TextMeshProUGUI stopWatchText;

    private int watchTimer = 10;
    private bool _isPlaying = false;
    private CancellationTokenSource swCts;

    [Header("SceneLoad")]
    public Button sceneLoadButton;
    public CanvasGroup sceneLoadPanel;
    public Slider sceneLoadSlider;
    public TextMeshProUGUI sceneLoadText;
    private bool _isClick;

    [Header("Resource Loading")]
    public Button resourceLoadButton;
    public Button resourceLoadCancelButton;

    public Slider resourceLoadSlider1;
    public Slider resourceLoadSlider2;
    public Slider resourceLoadSlider3;
    public Slider allResourceLoadSlider;

    public TextMeshProUGUI resourceLoadSlider1Text;
    public TextMeshProUGUI resourceLoadSlider2Text;
    public TextMeshProUGUI resourceLoadSlider3Text;
    public TextMeshProUGUI allResourceLoadSliderText;
    public TextMeshProUGUI resourceLoadText;

    private bool _isRLClick;

    [Header("UI Animation Sequence")]
    public Button animationStartButton;
    public Button animationResetButton;
    public Button animationStopButton;
    public CanvasGroup fadeCube;
    public Transform cube;
    private CancellationTokenSource aniCts;


    private void Start()
    {
        //StopWatch 
        startButton.onClick.AddListener(() => OnStartClicked().Forget());
        stopButton.onClick.AddListener(() => OnStopClicked());
        reStartButton.onClick.AddListener(() => OnStartClicked().Forget());
        resetButton.onClick.AddListener(() => OnReSetClicked());
        //SceneLoad
        sceneLoadPanel.gameObject.SetActive(false);
        sceneLoadSlider.gameObject.SetActive(false);
        sceneLoadButton.onClick.AddListener(() => OnSceneLoadClicked().Forget());
        _isClick = false;
        //Resource Loading
        resourceLoadButton.onClick.AddListener(() => OnResourceLoadClicked().Forget());
        resourceLoadCancelButton.onClick.AddListener(() => OnLoadCancelClicked());
        allResourceLoadSlider.maxValue = 1f;
        allResourceLoadSlider.minValue = 0f;
        _isRLClick = false;
        //UI Animation Sequence
        fadeCube.gameObject.SetActive(false);
        animationStartButton.onClick.AddListener(() => OnAnimationStartClicked().Forget());

        animationStopButton.onClick.AddListener(() => OnAnimationStopClicked());
    }

    //StopWatch 
    private void StopWatchText(int time)
    {
        var ts = TimeSpan.FromSeconds(time);
        stopWatchText.text = $"{ts:mm\\:ss}";
        Debug.Log(stopWatchText.text);
        if (watchTimer == 0)
            stopWatchText.text = "Time's Up!";
    }
    private async UniTaskVoid OnStartClicked()
    {
        if (_isPlaying)
            return;

        _isPlaying = true;
        swCts?.Cancel();
        swCts?.Dispose();
        swCts = new CancellationTokenSource();

        try
        {
            for (int i = watchTimer; i >= 0; i--)
            {
                swCts.Token.ThrowIfCancellationRequested();
                StopWatchText(watchTimer);
                watchTimer--;
                await UniTask.Delay(1000, cancellationToken: swCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            StopWatchText(watchTimer);
        }
        finally
        {
            _isPlaying = false;
        }


    }
    private void OnStopClicked()
    {
        swCts?.Cancel();
        swCts?.Dispose();
        swCts = new CancellationTokenSource();
    }
    private void OnReSetClicked()
    {
        swCts?.Cancel();
        swCts?.Dispose();
        swCts = new CancellationTokenSource();
        watchTimer = 10;
        StopWatchText(watchTimer);
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
        await SceneManager.LoadSceneAsync("LoadScene").ToUniTask(progress);
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

    //Resource Loading
    private void ResourceLoadText(string msg)
    {
        resourceLoadText.text = msg;
    }

    private void LoadText(TextMeshProUGUI textmeseh, string msg)
    {
        textmeseh.text = msg;
    }

    private async UniTask FakeResourceLoadAsync(Slider progressBar, TextMeshProUGUI textmeseh, int ms)
    {
        var Cts = new CancellationTokenSource();

        var timeOutCts = new CancellationTokenSource();
        timeOutCts.CancelAfterSlim(TimeSpan.FromSeconds(10));

        var allCts = CancellationTokenSource.CreateLinkedTokenSource(
            Cts.Token,
            timeOutCts.Token,
            this.GetCancellationTokenOnDestroy()
            );

        int steps = 100;
        int delayPerSetep = ms / steps;
        progressBar.value = 0f;
        try
        {
            for (int i = 0; i <= steps; i++)
            {
                progressBar.value = (float)i / steps;
                LoadText(textmeseh, $"Loading...{i}");
                if (!_isRLClick)
                {
                    Cts.Cancel();
                    Cts.Dispose();
                }
                await UniTask.Delay(delayPerSetep, cancellationToken: allCts.Token);
            }
            progressBar.value = 1f;
        }
        catch (OperationCanceledException)
        {
            if (Cts.IsCancellationRequested)
                ResourceLoadText("Loading cancelled");
            if (timeOutCts.IsCancellationRequested)
                ResourceLoadText("Loading timeout!");
            throw;
        }
    }

    //private async UniTask FakeResourceAllLoadAsync(Slider progressBar, TextMeshProUGUI textmeseh)
    //{
    //    var Cts = new CancellationTokenSource();

    //    var timeOutCts = new CancellationTokenSource();
    //    timeOutCts.CancelAfterSlim(TimeSpan.FromSeconds(10));

    //    var allCts = CancellationTokenSource.CreateLinkedTokenSource(
    //        Cts.Token,
    //        timeOutCts.Token,
    //        this.GetCancellationTokenOnDestroy()
    //        );

    //    progressBar.value = 0f;
    //    try
    //    {
    //        while(progressBar.value >= 1.0)
    //        {
    //            progressBar.value = (resourceLoadSlider1.value + resourceLoadSlider2.value + resourceLoadSlider3.value) / 3;
    //            LoadText(textmeseh, $"Loading...{progressBar.value * 100}");
    //            if (!_isRLClick)
    //            {
    //                Cts.Cancel();
    //                Cts.Dispose();
    //            }
    //            await UniTask.NextFrame();
    //        }
    //        progressBar.value = 1f;
    //    }
    //    catch (OperationCanceledException)
    //    {
    //        if (Cts.IsCancellationRequested)
    //            ResourceLoadText("Loading cancelled");
    //        if (timeOutCts.IsCancellationRequested)
    //            ResourceLoadText("Loading timeout!");
    //        throw;
    //    }
    //}

    private async UniTask OnResourceLoadClicked()
    {
        if (_isRLClick)
            return;

        _isRLClick = true;

        ResourceLoadText("OnResourceLoadClicked");

        await UniTask.WhenAll(
            FakeResourceLoadAsync(resourceLoadSlider1, resourceLoadSlider1Text, 1500),
         FakeResourceLoadAsync(resourceLoadSlider2, resourceLoadSlider2Text, 2300),
         FakeResourceLoadAsync(resourceLoadSlider3, resourceLoadSlider3Text, 3300)
         //FakeResourceAllLoadAsync(allResourceLoadSlider, allResourceLoadSliderText)
         );


        ResourceLoadText("All resources loaded!");
        _isRLClick = false;
    }
    private void OnLoadCancelClicked()
    {
        _isRLClick = false;
    }

    //UI Animation Sequence
    private async UniTask MoveToAsync(RectTransform target, Vector2 to, float duration)
    {
        Vector2 from = target.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.anchoredPosition = Vector2.Lerp(from, to, t);
            await UniTask.Yield();
        }
        target.anchoredPosition = to;
    }
    private async UniTask MoveToAsync(RectTransform target, Vector3 to, float duration)
    {
        Vector3 from = target.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.localScale = Vector3.Lerp(from, to, t);
            await UniTask.Yield();
        }
        target.anchoredPosition = to;
    }

    private async UniTaskVoid OnAnimationStartClicked()
    {
        aniCts?.Cancel();
        aniCts?.Dispose();
        aniCts = new CancellationTokenSource();
        fadeCube.gameObject.SetActive(true);
        try
        {
            await FadeAsync(fadeCube, 0f, 1f, 2f);

            var rectTr = cube as RectTransform;
            var originalPos = rectTr.anchoredPosition;
            var pos = new Vector2(+20, 0);

            await MoveToAsync(rectTr, pos, 0.5f);
        }
        catch (OperationCanceledException)
        {
            aniCts = null;
        }



    }
    private void OnAnimationStopClicked()
    {
        aniCts.Cancel();
        aniCts.Dispose();
        aniCts = new CancellationTokenSource();
    }
}
