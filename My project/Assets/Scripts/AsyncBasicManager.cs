using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using System;

public class AsyncBasicManager : MonoBehaviour
{
    [Header("Section1")]
    public TextMeshProUGUI section1StatusText;

    [Header("Section2")]
    public TextMeshProUGUI section2StatusText;
    private CancellationTokenSource delayCts = new CancellationTokenSource();

    [Header("Section3")]
    public Slider progressBar1;
    public Slider progressBar2;
    public Slider progressBar3;
    public TextMeshProUGUI section3StatusText;

    [Header("Section4")]
    public Slider timeOutSlider;
    public TextMeshProUGUI timeOutValue;
    public TextMeshProUGUI section4StatusText;

    [Header("Section5")]
    public TextMeshProUGUI section5StatusText;
    public Transform cubeObjTr;

    [Header("Section6")]
    public Slider progressBar4;
    public TextMeshProUGUI section6StatusText;
    private CancellationTokenSource TaskCts = new CancellationTokenSource();

    private void Start()
    {
        InitTimeOutSlide();
        InitPrograssBarValue();
    }

    private void OnDestroy()
    {
        delayCts?.Cancel();
        TaskCts?.Cancel();
    }

    //Section1
    //실행 시 돌고 있는 상자가 멈춤
    public void OnSyncDownloadButtonClicked()
    {
        UpdateSection1Text("OnSyncDownloadButtonClicked: Start");

        //의도적으로 스레드를 쉬게 해줘야함
        Thread.Sleep(3000);

        UpdateSection1Text("OnSyncDownloadButtonClicked: End");
    }

    //async void 는 원래 쓰면 안되는 것 << 유니티 버튼클릭 함수 연결할 때 Task로 반환이 없는 비동기로 만들어줘야함
    //상자는 돌아가는데 
    public async void OnAsyncDownloadButtonClicked()
    {
        UpdateSection1Text("OnAsyncDownloadButtonClicked: Start");

        await Task.Delay(3000);

        UpdateSection1Text("OnAsyncDownloadButtonClicked: End");
    }
    private void UpdateSection1Text(string msg)
    {
        section1StatusText.text = msg;
        Debug.Log($"[Section1 {msg}]");
    }

    //Section2
    public async void OnDelayClicked(int seconds)
    {
        UpdateSection2Text($"대기중: {seconds} 초...");

        for (int i = seconds; i > 0; i--)
        {
            delayCts.Token.ThrowIfCancellationRequested();
            UpdateSection2Text($"남은 시간: {i} 초...");
            await Task.Delay(1000);
        }

        UpdateSection2Text($"대기완료");
    }

    public async void OnCancelableClicked()
    {
        delayCts?.Cancel();
        delayCts?.Dispose();
        delayCts = new CancellationTokenSource();

        try
        {
            for (int i = 10; i > 0; i--)
            {
                //외부 캔슬 요청 시 여기로 넘어온다.
                delayCts.Token.ThrowIfCancellationRequested();

                UpdateSection2Text($"남은 시간: {i} 초...");
                await Task.Delay(1000, delayCts.Token);
            }
            UpdateSection2Text($"대기완료");
        }
        catch (OperationCanceledException)
        {
            UpdateSection2Text($"10초 대기 취소");
        }
    }

    public void OnCancelClicked()
    {
        delayCts?.Cancel();
        delayCts?.Dispose();
        delayCts = new CancellationTokenSource();
    }

    private void UpdateSection2Text(string msg)
    {
        section2StatusText.text = msg;
        Debug.Log($"[Section2 {msg}]");
    }

    //Section3
    private void ResetProgressBars()
    {
        progressBar1.value = 0;
        progressBar2.value = 0;
        progressBar3.value = 0;
    }

    //순차실행 버튼
    public async void OnSequencialDownloadClicked()
    {
        ResetProgressBars();
        UpdateSection3Text("순차 다운로드 시작");

        float startTime = Time.time;

        await FakeDownloadAsync(progressBar1, 1, 2000);
        await FakeDownloadAsync(progressBar2, 2, 3000);
        await FakeDownloadAsync(progressBar3, 3, 4000);

        float elapsed = Time.time - startTime;
        UpdateSection3Text($"순차 다운로드 끝: {elapsed} 초");
    }

    //병렬실행 버튼
    public async void OnParalleDownloadClicked()
    {
        ResetProgressBars();
        UpdateSection3Text("병렬 다운로드 시작");

        float startTime = Time.time;

        Task task1 = FakeDownloadAsync(progressBar1, 1, 2000);
        Task task2 = FakeDownloadAsync(progressBar2, 1, 3000);
        Task task3 = FakeDownloadAsync(progressBar3, 1, 4000);

        await Task.WhenAll(task1, task2, task3);

        float elapsed = Time.time - startTime;
        UpdateSection3Text($"순차 다운로드 끝: {elapsed} 초");
    }

    private async Task FakeDownloadAsync(Slider progressbar, int index, int durationMs)
    {
        int steps = 20;
        int delayPerStep = durationMs / steps;

        for (int i = 0; i < steps; i++)
        {
            progressbar.value = (float)i / steps;
            await Task.Delay(delayPerStep);
        }
        progressbar.value = 1;

        Debug.Log($"[Section3] 파일 {index} 다운로드 완료");
    }


    private void UpdateSection3Text(string msg)
    {
        section3StatusText.text = msg;
        Debug.Log($"[Section3 {msg}]");
    }

    //Section4
    private void InitTimeOutSlide()
    {
        timeOutSlider.minValue = 1f;
        timeOutSlider.maxValue = 5f;
        timeOutSlider.value = 3f;
        OnTimeOutSliderChanged(timeOutSlider.value);

        timeOutSlider.onValueChanged.AddListener(OnTimeOutSliderChanged);
    }

    private void OnTimeOutSliderChanged(float value)
    {
        timeOutValue.text = $"{value} 초";
    }

    public async void OnTimeOutDownloadClicked()
    {
        //Cancel이 필요하다
        Task downloadTask = Task.Delay(4000);
        Task timeOutTask = Task.Delay((int)timeOutSlider.value * 1000);

        Task completedTask = await Task.WhenAny(downloadTask, timeOutTask);

        if (completedTask != downloadTask)
        {
            UpdateSection4Text("다운로드 완료");
        }
        else
        {
            UpdateSection4Text("타임 아웃");
        }
    }

    private void UpdateSection4Text(string msg)
    {
        section4StatusText.text = msg;
        Debug.Log($"[Section4 {msg}]");
    }

    //Section 5 예외처리 transform접근
    public async void OnSafeCodeClicked()
    {
        UpdateSection5Text("OnSafeCodeClicked");

        await Task.Delay(1000);

        cubeObjTr.position += Vector3.up * 0.5f;

        UpdateSection5Text("큐브 이동 완료");
    }

    public async void OnUnsafeCodeClicked()
    {
        UpdateSection5Text("OnUnsafeCodeClicked");

        await Task.Run(() =>
        {
            Thread.Sleep(1000);
            cubeObjTr.position += Vector3.up * 0.5f;
        }
            );
    }

    private void UpdateSection5Text(string msg)
    {
        section5StatusText.text = msg;
        Debug.Log($"[Section5 {msg}]");
    }


    //Section6
    private void InitPrograssBarValue()
    {
        progressBar4.minValue = 0f;
        progressBar4.maxValue = 1f;
        progressBar4.value = 0f;
    }
    public async void OnStartLongTaskClicked()
    {
        TaskCts?.Cancel();
        TaskCts?.Dispose();
        TaskCts = new CancellationTokenSource();
        try
        {
            await FakeBarDownloadAsync(progressBar4, 3000);
            UpdateSection6Text("다운로드 완료");
        }
        catch (OperationCanceledException)
        {
            UpdateSection6Text("다운로드 취소");
        }
    }

    public void OnCancelLongTaskClicked()
    { 
        TaskCts?.Cancel();
        TaskCts?.Dispose();
        TaskCts = new CancellationTokenSource();
    }

    private async Task FakeBarDownloadAsync(Slider progressbar, int durationMs)
    {
        int steps = 100;
        int delayPerStep = durationMs / steps;

        for (int i = 0; i < steps; i++)
        {
            progressbar.value = (float)i / steps;
            UpdateSection6Text($"다운로드 중: {progressbar.value * 100}%");
            TaskCts.Token.ThrowIfCancellationRequested();
            await Task.Delay(delayPerStep, TaskCts.Token);
        }
        progressbar.value = 1f;

        Debug.Log("파일 설치 완료");
    }

    private void UpdateSection6Text(string msg)
    {
        section6StatusText.text = msg;
        Debug.Log($"[Section6 {msg}]");
    }
}
