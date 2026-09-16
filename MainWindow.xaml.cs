using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Nocturne.Audio;
using Nocturne.Dsp;

namespace Nocturne;

public partial class MainWindow : Window
{
    private LoopbackCapture? _capture;
    private bool             _running;

    // ring buffer: accumulate samples for one FFT frame
    private readonly List<float> _sampleBuf = new();
    private const int FftSize = 2048;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Toggle_Click(object sender, RoutedEventArgs e)
    {
        if (_running) Stop(); else Start();
    }

    private void Start()
    {
        try
        {
            _capture = new LoopbackCapture();
            _capture.SamplesReady += OnSamples;
            _capture.Start();
            _running = true;
            ToggleBtn.Content = "■  stop";
            StatusText.Text   = "  ·  listening";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"  ·  error: {ex.Message}";
        }
    }

    private void Stop()
    {
        _capture?.Stop();
        _capture?.Dispose();
        _capture = null;
        _running = false;
        ToggleBtn.Content = "▶  start";
        StatusText.Text   = "  ·  idle";
        PeakText.Text     = "";
    }

    private void OnSamples(float[] samples)
    {
        _sampleBuf.AddRange(samples);
        if (_sampleBuf.Count < FftSize) return;

        var frame = _sampleBuf.GetRange(0, FftSize).ToArray();
        _sampleBuf.RemoveRange(0, FftSize / 2); // 50% overlap

        var mag = Fft.Magnitudes(frame, FftSize);
        Viz.Update(mag);

        float peak = 0;
        foreach (var s in samples) { float a = MathF.Abs(s); if (a > peak) peak = a; }
        float db = peak > 0 ? 20 * MathF.Log10(peak) : float.NegativeInfinity;

        Dispatcher.InvokeAsync(() =>
            PeakText.Text = float.IsNegativeInfinity(db) ? "peak  -∞ dB" : $"peak  {db:+0.0;-0.0} dB");
    }

    protected override void OnClosed(EventArgs e)
    {
        Stop();
        base.OnClosed(e);
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e)    => Close();
}
