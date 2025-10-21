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
    //���� �� ���� �ִ� ���ڰ� ����
    public void OnSyncDownloadButtonClicked()
    {
        UpdateSection1Text("OnSyncDownloadButtonClicked: Start");

        //�ǵ������� �����带 ���� �������
        Thread.Sleep(3000);

        UpdateSection1Text("OnSyncDownloadButtonClicked: End");
    }

    //async void �� ���� ���� �ȵǴ� �� << ����Ƽ ��ưŬ�� �Լ� ������ �� Task�� ��ȯ�� ���� �񵿱�� ����������
    //���ڴ� ���ư��µ� 
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
        UpdateSection2Text($"�����: {seconds} ��...");

        for (int i = seconds; i > 0; i--)
        {
            delayCts.Token.ThrowIfCancellationRequested();
            UpdateSection2Text($"���� �ð�: {i} ��...");
            await Task.Delay(1000);
        }

        UpdateSection2Text($"���Ϸ�");
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
                //�ܺ� ĵ�� ��û �� ����� �Ѿ�´�.
                delayCts.Token.ThrowIfCancellationRequested();

                UpdateSection2Text($"���� �ð�: {i} ��...");
                await Task.Delay(1000, delayCts.Token);
            }
            UpdateSection2Text($"���Ϸ�");
        }
        catch (OperationCanceledException)
        {
            UpdateSection2Text($"10�� ��� ���");
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

    //�������� ��ư
    public async void OnSequencialDownloadClicked()
    {
        ResetProgressBars();
        UpdateSection3Text("���� �ٿ�ε� ����");

        float startTime = Time.time;

        await FakeDownloadAsync(progressBar1, 1, 2000);
        await FakeDownloadAsync(progressBar2, 2, 3000);
        await FakeDownloadAsync(progressBar3, 3, 4000);

        float elapsed = Time.time - startTime;
        UpdateSection3Text($"���� �ٿ�ε� ��: {elapsed} ��");
    }

    //���Ľ��� ��ư
    public async void OnParalleDownloadClicked()
    {
        ResetProgressBars();
        UpdateSection3Text("���� �ٿ�ε� ����");

        float startTime = Time.time;

        Task task1 = FakeDownloadAsync(progressBar1, 1, 2000);
        Task task2 = FakeDownloadAsync(progressBar2, 1, 3000);
        Task task3 = FakeDownloadAsync(progressBar3, 1, 4000);

        await Task.WhenAll(task1, task2, task3);

        float elapsed = Time.time - startTime;
        UpdateSection3Text($"���� �ٿ�ε� ��: {elapsed} ��");
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

        Debug.Log($"[Section3] ���� {index} �ٿ�ε� �Ϸ�");
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
        timeOutValue.text = $"{value} ��";
    }

    public async void OnTimeOutDownloadClicked()
    {
        //Cancel�� �ʿ��ϴ�
        Task downloadTask = Task.Delay(4000);
        Task timeOutTask = Task.Delay((int)timeOutSlider.value * 1000);

        Task completedTask = await Task.WhenAny(downloadTask, timeOutTask);

        if (completedTask != downloadTask)
        {
            UpdateSection4Text("�ٿ�ε� �Ϸ�");
        }
        else
        {
            UpdateSection4Text("Ÿ�� �ƿ�");
        }
    }

    private void UpdateSection4Text(string msg)
    {
        section4StatusText.text = msg;
        Debug.Log($"[Section4 {msg}]");
    }

    //Section 5 ����ó�� transform����
    public async void OnSafeCodeClicked()
    {
        UpdateSection5Text("OnSafeCodeClicked");

        await Task.Delay(1000);

        cubeObjTr.position += Vector3.up * 0.5f;

        UpdateSection5Text("ť�� �̵� �Ϸ�");
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
            UpdateSection6Text("�ٿ�ε� �Ϸ�");
        }
        catch (OperationCanceledException)
        {
            UpdateSection6Text("�ٿ�ε� ���");
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
            UpdateSection6Text($"�ٿ�ε� ��: {progressbar.value * 100}%");
            TaskCts.Token.ThrowIfCancellationRequested();
            await Task.Delay(delayPerStep, TaskCts.Token);
        }
        progressbar.value = 1f;

        Debug.Log("���� ��ġ �Ϸ�");
    }

    private void UpdateSection6Text(string msg)
    {
        section6StatusText.text = msg;
        Debug.Log($"[Section6 {msg}]");
    }
}
