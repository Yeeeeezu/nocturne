# nocturne

music visualizer. captures whatever is playing through your speakers (loopback), runs an FFT, and draws frequency bars in a frameless dark window.

---

## what it looks like

64 bars, log-frequency scale (bass on the left, treble on the right). gradient from purple → blue → green → yellow → red. smoothed so it doesn't flicker.

peak level in dB shown in the status bar. no install, no config, just run.

---

## structure

```
Audio/
  LoopbackCapture.cs    WASAPI loopback capture (NAudio)
Dsp/
  Fft.cs                Cooley-Tukey FFT + Hann window + magnitude
Ui/
  VisualizerControl.cs  WPF FrameworkElement — draws bars directly with DrawingContext
MainWindow.xaml         frameless window XAML
MainWindow.xaml.cs      toggle start/stop, sample routing
App.xaml / App.xaml.cs
```

## build

```
dotnet build -c Release
```

.NET 8+, windows only. requires NAudio (pulled automatically by NuGet).

needs default audio output device to be set — uses WASAPI loopback, which captures whatever goes to your speakers/headphones.

## how it works

1. `WasapiLoopbackCapture` (NAudio) captures the system audio output stream
2. samples are accumulated in a ring buffer until one FFT frame (2048 samples) is ready
3. `Fft.Magnitudes` applies a Hann window and runs an in-place Cooley-Tukey FFT
4. 64 bins are mapped on a log scale to emphasize the audible range
5. each bin is smoothed (0.75 factor) to prevent flickering
6. `VisualizerControl.OnRender` draws colored rectangles directly via `DrawingContext`

## testing

built clean. FFT math verified against known input (DC signal produces bin 0 spike). bar rendering logic reviewed. WASAPI loopback capture code pattern follows NAudio documentation.

**not live-tested:** didn't run the visualizer while music was actually playing (no display in this environment). WASAPI loopback requires an active audio device and may fail silently on some configurations.

## license

MIT
