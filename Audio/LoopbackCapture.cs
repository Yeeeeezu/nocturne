using System;
using NAudio.Wave;

namespace Nocturne.Audio;

public sealed class LoopbackCapture : IDisposable
{
    private readonly WasapiLoopbackCapture _capture;
    private readonly BufferedWaveProvider  _buffer;

    public event Action<float[]>? SamplesReady;

    public LoopbackCapture()
    {
        _capture = new WasapiLoopbackCapture();
        _buffer  = new BufferedWaveProvider(_capture.WaveFormat) { DiscardOnBufferOverflow = true };
        _capture.DataAvailable += OnData;
    }

    private void OnData(object? sender, WaveInEventArgs e)
    {
        _buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);

        int floatCount = e.BytesRecorded / sizeof(float);
        var samples    = new float[floatCount];
        Buffer.BlockCopy(e.Buffer, 0, samples, 0, e.BytesRecorded);
        SamplesReady?.Invoke(samples);
    }

    public void Start() => _capture.StartRecording();
    public void Stop()  => _capture.StopRecording();

    public void Dispose()
    {
        _capture.DataAvailable -= OnData;
        _capture.Dispose();
        _buffer.ClearBuffer();
    }
}
