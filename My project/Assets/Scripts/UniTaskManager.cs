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

    [Header("Section4")]
    public Button UpdateButton;
    public Button FixedUpdateButton;
    public Button LateUpdateButton;

    [Header("Section5")]
    public Button destroyTokenButton;
    public Button timeOutButton;
    public Button linkedTokenButton;
    public Button cancelSection5Button;
    public Slider section5ProgressBar;
    private CancellationTokenSource section5Cts;

    [Header("Section6")]
    public Button fadeInButton;
    public Button fadeOutButton;
    public Button AnimationButton;
    public Button waitForInputButton;
    public CanvasGroup fadePanel;
    public Transform animatedCube;


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
        //4
        UpdateButton.onClick.AddListener(() => OnUpdateClicked().Forget());
        FixedUpdateButton.onClick.AddListener(() => OnFixedUpdateClicked().Forget());
        LateUpdateButton.onClick.AddListener(() => OnLateUpdateClicked().Forget());
        //5
        destroyTokenButton.onClick.AddListener(() => OnDestroyTokenClicked().Forget());
        timeOutButton.onClick.AddListener(() => OnTimeOutClicked().Forget());
        linkedTokenButton.onClick.AddListener(() => OnLinkedCtsClicked().Forget());
        cancelSection5Button.onClick.AddListener(() => OnCancelSection5Clicked());

        //6
        fadeInButton.onClick.AddListener(() => OnFadeInClicked().Forget());
        fadeOutButton.onClick.AddListener(() => OnFadeOutClicked().Forget());
        AnimationButton.onClick.AddListener(() => OnAnimationClicked().Forget());
        waitForInputButton.onClick.AddListener(() => OnWaitKeyClicked().Forget());
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

        var prefab = await Resources.LoadAsync<GameObject>("RotatingCube").ToUniTask();

        UpdateSectionText(3, "리소스 로딩 완료");
        Instantiate(prefab);
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
        if (loadCts != null && !loadCts.IsCancellationRequested)
        {
            loadCts?.Cancel();
            loadCts?.Dispose();
            loadCts = null;
        }
        else
            UpdateSectionText(3, "테스크 없음");
    }

    //4
    private async UniTaskVoid OnUpdateClicked()
    {
        UpdateSectionText(4, "OnUpdateClicked");

        for (int i = 0; i < 3; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            UpdateSectionText(4, $"업데이트 프레임: {Time.frameCount}");
        }

        UpdateSectionText(4, "업데이트 타이밍 테스트 끝");
    }
    private async UniTaskVoid OnFixedUpdateClicked()
    {
        UpdateSectionText(4, "OnFixedUpdateClicked");

        for (int i = 0; i < 3; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            UpdateSectionText(4, $"픽스드업데이트 프레임: {Time.frameCount}");
        }

        UpdateSectionText(4, "업데이트 타이밍 테스트 끝");
    }
    private async UniTaskVoid OnLateUpdateClicked()
    {
        UpdateSectionText(4, "OnLateUpdateClicked");

        for (int i = 0; i < 3; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.LastTimeUpdate);
            UpdateSectionText(4, $"레이트 업데이트 프레임: {Time.frameCount}");
        }

        UpdateSectionText(4, "레이트 업데이트 타이밍 테스트 끝");
    }

    //5
    public async UniTask LongTaskAsync(CancellationToken ct)
    {
        UpdateSectionText(5, "Long task started... (10s)");
        for (int i = 0; i <= 100; i++)
        {
            ct.ThrowIfCancellationRequested();
            section5ProgressBar.value = i / 100f;
            UpdateSectionText(5, $"Progress: {i}% (Cancellable)");
            await UniTask.Delay(100, cancellationToken: ct);
        }

        UpdateSectionText(5, "Task complete! (100%)");
    }

    private async UniTaskVoid OnDestroyTokenClicked()
    {
        try
        {
            await LongTaskAsync(this.GetCancellationTokenOnDestroy());
            UpdateSectionText(5, "테스크 완료!");
        }
        catch (OperationCanceledException)
        {
            UpdateSectionText(5, "취소!");
        }
    }

    private async UniTaskVoid OnTimeOutClicked()
    {
        UpdateSectionText(5, "OnTimeOutClicked");

        var cts = new CancellationTokenSource();
        cts.CancelAfterSlim(TimeSpan.FromSeconds(3));

        try
        {
            await LongTaskAsync(cts.Token);
            UpdateSectionText(5, "테스트 완료!");
        }
        catch (OperationCanceledException)
        {
            UpdateSectionText(5, "타임 아웃 취소!");
        }
    }

    private async UniTaskVoid OnLinkedCtsClicked()
    {
        UpdateSectionText(5, "OnLinkedCtsClicked");

        section5Cts?.Cancel();
        section5Cts?.Dispose();
        section5Cts = new CancellationTokenSource();

        var timeOutCts = new CancellationTokenSource();
        timeOutCts.CancelAfterSlim(TimeSpan.FromSeconds(101));

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            section5Cts.Token,
            timeOutCts.Token,
            this.GetCancellationTokenOnDestroy()
            );

        try
        {
            await LongTaskAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            if (section5Cts.IsCancellationRequested)
            {
                UpdateSectionText(5, "수동 취소");
            }
            else if (timeOutCts.IsCancellationRequested)
            {
                UpdateSectionText(5, "타임 아웃 취소");
            }
            else
            {
                UpdateSectionText(5, "OnDestroy 취소");
            }
        }
        finally
        {
            timeOutCts.Cancel();
            timeOutCts.Dispose();

            linkedCts.Dispose();

            section5Cts?.Cancel();
            section5Cts?.Dispose();
            section5Cts = null;
        }
    }

    private void OnCancelSection5Clicked()
    {
        if (section5Cts != null && !section5Cts.IsCancellationRequested)
        {
            section5Cts.Cancel();
        }
        else
        {
            UpdateSectionText(5, "테스크 없음");
        }
    }

    //6
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

    private async UniTask RetateToAsync(Transform target, float speed, float duration)
    {
        float from = target.eulerAngles.z;
        float elapsed = 0f;
        float currentAngle = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentAngle += speed * Time.deltaTime;
            target.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            await UniTask.Yield();
            target.rotation = Quaternion.Euler(0f, 0f, from);
        }
    }

    private async UniTaskVoid OnFadeInClicked()
    {
        UpdateSectionText(6, "OnFadeInClicked");
        await FadeAsync(fadePanel, 0f, 1f, 0.5f);
        UpdateSectionText(6, "페이드 인 완료");
    }
    private async UniTaskVoid OnFadeOutClicked()
    {
        UpdateSectionText(6, "OnFadeOutClicked");
        await FadeAsync(fadePanel, 1f, 0f, 0.5f);
        UpdateSectionText(6, "페이드 아웃 완료");
    }

    private async UniTaskVoid OnAnimationClicked()
    {
        var rectTr = animatedCube as RectTransform;
        var originalPos = rectTr.anchoredPosition;

        await MoveToAsync(rectTr, originalPos + Vector2.up * 50f, 0.5f);
        UpdateSectionText(6, "1. 위로 이동 완료");

        await RetateToAsync(animatedCube, 360f, 0.5f);
        UpdateSectionText(6, "2. 회전 완료");

        await MoveToAsync(rectTr, originalPos, 0.5f);
        UpdateSectionText(6, "3. 원위치 완료");
    }

    private async UniTaskVoid OnWaitKeyClicked()
    {
        UpdateSectionText(6, "OnWaitKeyClicked");
        await UniTask.WaitUntil(() => Input.anyKey);
        UpdateSectionText(6, "키 입력");
    }
}
