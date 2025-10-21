using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UniTaskManager : MonoBehaviour
{
    [Header("Section1")]
    public Button delayButton;
    public Button delayFrameButton;
    public Button yieldButton;
    public Button nextFrameButton;

    [Header("Section2")]
    public Button sequentialButton;
    public Button whenAllButton;
    public Button whenAnyButton;
    public Slider progressBar1;
    public Slider progressBar2;
    public Slider progressBar3;

    [Header("Section3")]
    public Button loadResourceButton;
    public Button loadingWithProgressButton;
    public Button cancelLoadButton;
    private CancellationTokenSource loadCts;
    public Slider loadingProgressBar;

    [Header("Texts")]
    public TextMeshProUGUI[] sectionTexts;

    private void Start()
    {
        //1
        delayButton.onClick.AddListener(() => OnDelayClicked().Forget());
        delayFrameButton.onClick.AddListener(() => OnDelayFrameClicked().Forget());
        yieldButton.onClick.AddListener(() => OnYieldClicked().Forget());
        nextFrameButton.onClick.AddListener(() => OnNextFrameClicked().Forget());
        //2
        ResetProgressBars();
        sequentialButton.onClick.AddListener(() => OnSequencialClicked().Forget());
        whenAllButton.onClick.AddListener(() => OnWhenAllClicked().Forget());
        whenAnyButton.onClick.AddListener(() => OnWhenAnyClicked().Forget());
        //3
        loadResourceButton.onClick.AddListener(() => OnResourceClicked().Forget());
        loadingWithProgressButton.onClick.AddListener(() => OnLoadWithProgressBarClicked().Forget());
        cancelLoadButton.onClick.AddListener(() => cancelLoadClick());


    }

    private void OnDestroy()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();
    }

    private void ResetProgressBars()
    {
        progressBar1.value = 0f;
        progressBar2.value = 0f;
        progressBar3.value = 0f;
    }
    private void UpdateSectionText(int section, string msg)
    {
        var log = $"[Section {section}] {msg}";
        sectionTexts[section].text = log;
        Debug.Log(log);
    }

    //1
    private async UniTaskVoid OnDelayClicked()
    {
        UpdateSectionText(1, "OnDelayClicked");
        await UniTask.Delay(2000);
        UpdateSectionText(1, "2초 대기 완료");
    }
    private async UniTaskVoid OnDelayFrameClicked()
    {
        UpdateSectionText(1, "OnDelayFrameClicked");
        int startFrame = Time.frameCount;
        await UniTask.DelayFrame(60);
        int endFrame = Time.frameCount;
        UpdateSectionText(1, $"{endFrame - startFrame}프레임 대기 완료");
    }
    private async UniTaskVoid OnYieldClicked()
    {
        UpdateSectionText(1, "OnDelayClicked");
        int startFrame = Time.frameCount;
        await UniTask.Yield();
        int endFrame = Time.frameCount;
        UpdateSectionText(1, $"Yield 완료: 시작{startFrame} ~ {endFrame}");
    }
    private async UniTaskVoid OnNextFrameClicked()
    {
        UpdateSectionText(1, "OnDelayFrameClicked");
        int startFrame = Time.frameCount;
        await UniTask.NextFrame();
        int endFrame = Time.frameCount;
        UpdateSectionText(1, $"NextFrame 완료: 시작{startFrame} ~ {endFrame}");
    }


    //2
    private async UniTask FakeLoadAsync(Slider progressBar, int ms)
    {
        int steps = 20;
        int delayPerSetep = ms / steps;

        for (int i = 0; i < steps; i++)
        {
            progressBar.value = (float)i / steps;
            await UniTask.Delay(delayPerSetep);
        }
        progressBar.value = 1f;
    }

    public async UniTaskVoid OnSequencialClicked()
    {
        ResetProgressBars();
        UpdateSectionText(2, "OnSequencialClicked");

        float startTime = Time.time;

        await FakeLoadAsync(progressBar1, 2000);
        await FakeLoadAsync(progressBar2, 2500);
        await FakeLoadAsync(progressBar3, 3000);

        float elapsed = Time.time - startTime;

        UpdateSectionText(2, $"순차 실행 완료: {elapsed} 초");
    }
    public async UniTaskVoid OnWhenAllClicked()
    {
        ResetProgressBars();
        UpdateSectionText(2, "OnWhenAllClicked");

        float startTime = Time.time;

        await UniTask.WhenAll(
            FakeLoadAsync(progressBar1, 2000),
            FakeLoadAsync(progressBar2, 2500),
            FakeLoadAsync(progressBar3, 3000)
            );

        float elapsed = Time.time - startTime;

        UpdateSectionText(2, $"WhenAll 실행 완료: {elapsed} 초");
    }
    public async UniTaskVoid OnWhenAnyClicked()
    {
        ResetProgressBars();
        UpdateSectionText(2, "OnWhenAnyClicked");

        float startTime = Time.time;

        int index = await UniTask.WhenAny(
            FakeLoadAsync(progressBar1, 2000),
            FakeLoadAsync(progressBar2, 2500),
            FakeLoadAsync(progressBar3, 3000)
            );

        float elapsed = Time.time - startTime;

        UpdateSectionText(2, $"WhenAny 실행 완료{index}: {elapsed} 초");
    }

    //3
    private async UniTaskVoid OnResourceClicked()
    {
        UpdateSectionText(3, "OnResourceClicked");

        await Resources.LoadAsync<GameObject>("RotatingCube").ToUniTask();

        UpdateSectionText(3, "리소스 로딩 완료");
    }

    private async UniTaskVoid OnLoadWithProgressBarClicked()
    { 
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = new CancellationTokenSource();

        try
        {
            UpdateSectionText(3, "로딩 시작");
            loadingProgressBar.value = 0;
            for (int i = 0; i < 100; i++)
            { 
                loadCts.Token.ThrowIfCancellationRequested();

                loadingProgressBar.value = i / 100f;

                UpdateSectionText(3, $"로딩: {i}%");

                await UniTask.Delay(50, cancellationToken: loadCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            UpdateSectionText(3, "로딩 취소!");
        }
    }

    private void cancelLoadClick()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = new CancellationTokenSource();
    }
}
