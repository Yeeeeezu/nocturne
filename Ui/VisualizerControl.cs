using System;
using System.Windows;
using System.Windows.Media;

namespace Nocturne.Ui;

public sealed class VisualizerControl : FrameworkElement
{
    private float[] _bars = Array.Empty<float>();
    private float[] _smoothed = Array.Empty<float>();

    private static readonly Color[] Palette =
    [
        Color.FromRgb(0x7c, 0x6a, 0xf7),
        Color.FromRgb(0x5b, 0x9b, 0xff),
        Color.FromRgb(0x3e, 0xcf, 0x8e),
        Color.FromRgb(0xf0, 0xc0, 0x40),
        Color.FromRgb(0xf0, 0x70, 0x70),
    ];

    private const double Smoothing = 0.75;
    private const int    BinCount  = 64;

    public void Update(float[] fftMags)
    {
        var bins = ResampleToBins(fftMags, BinCount);

        if (_smoothed.Length != BinCount)
            _smoothed = new float[BinCount];

        for (int i = 0; i < BinCount; i++)
            _smoothed[i] = (float)(_smoothed[i] * Smoothing + bins[i] * (1 - Smoothing));

        _bars = _smoothed;
        Dispatcher.InvokeAsync(InvalidateVisual);
    }

    private static float[] ResampleToBins(float[] mag, int n)
    {
        var bins = new float[n];
        if (mag.Length == 0) return bins;

        // log-scale mapping: concentrate lower frequencies
        int half = mag.Length;
        for (int i = 0; i < n; i++)
        {
            double t0 = Math.Pow((double)i / n, 2.0) * half;
            double t1 = Math.Pow((double)(i + 1) / n, 2.0) * half;
            int lo = (int)t0, hi = Math.Min((int)t1 + 1, half - 1);
            float sum = 0;
            for (int j = lo; j <= hi; j++) sum += mag[j];
            bins[i] = sum / (hi - lo + 1);
        }
        return bins;
    }

    protected override void OnRender(DrawingContext dc)
    {
        var w = ActualWidth;
        var h = ActualHeight;
        dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x0d, 0x0d, 0x0f)), null, new Rect(0, 0, w, h));

        if (_bars.Length == 0) return;

        float max = 0;
        foreach (var b in _bars) if (b > max) max = b;
        if (max < 1e-6f) max = 1;

        double gap   = 2;
        double barW  = (w - gap * (_bars.Length - 1)) / _bars.Length;
        if (barW < 1) barW = 1;

        for (int i = 0; i < _bars.Length; i++)
        {
            double norm   = Math.Pow(_bars[i] / max, 0.5);
            double barH   = norm * (h * 0.90);
            double x      = i * (barW + gap);
            double y      = h - barH;

            double t = (double)i / (_bars.Length - 1);
            int ci = (int)(t * (Palette.Length - 1));
            double blend = t * (Palette.Length - 1) - ci;
            Color c = ci + 1 < Palette.Length
                ? LerpColor(Palette[ci], Palette[ci + 1], blend)
                : Palette[ci];

            var brush = new SolidColorBrush(c);
            dc.DrawRectangle(brush, null, new Rect(x, y, barW, barH));

            // glow reflection
            var reflect = new LinearGradientBrush(
                Color.FromArgb(60, c.R, c.G, c.B),
                Color.FromArgb(0,  c.R, c.G, c.B),
                90);
            dc.DrawRectangle(reflect, null, new Rect(x, h, barW, Math.Min(barH * 0.25, 20)));
        }
    }

    private static Color LerpColor(Color a, Color b, double t) => Color.FromRgb(
        (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t),
        (byte)(a.B + (b.B - a.B) * t));
}
