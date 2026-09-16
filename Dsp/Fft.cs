using System;

namespace Nocturne.Dsp;

public static class Fft
{
    public static void Transform(float[] re, float[] im)
    {
        int n = re.Length;
        if (n <= 1) return;

        // bit-reversal permutation
        int j = 0;
        for (int i = 1; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) { (re[i], re[j]) = (re[j], re[i]); (im[i], im[j]) = (im[j], im[i]); }
        }

        // Cooley-Tukey
        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = -2 * Math.PI / len;
            double wr = Math.Cos(ang), wi = Math.Sin(ang);
            for (int i = 0; i < n; i += len)
            {
                double cur_r = 1, cur_i = 0;
                for (int k = 0; k < len / 2; k++)
                {
                    float ur = re[i+k], ui = im[i+k];
                    float vr = (float)(re[i+k+len/2] * cur_r - im[i+k+len/2] * cur_i);
                    float vi = (float)(re[i+k+len/2] * cur_i + im[i+k+len/2] * cur_r);
                    re[i+k]         = ur + vr;
                    im[i+k]         = ui + vi;
                    re[i+k+len/2]   = ur - vr;
                    im[i+k+len/2]   = ui - vi;
                    double tmp_r = cur_r * wr - cur_i * wi;
                    cur_i = cur_r * wi + cur_i * wr;
                    cur_r = tmp_r;
                }
            }
        }
    }

    public static float[] Magnitudes(float[] samples, int fftSize = 2048)
    {
        int n = Math.Min(fftSize, samples.Length);
        var re = new float[fftSize];
        var im = new float[fftSize];

        // Hann window
        for (int i = 0; i < n; i++)
        {
            double w = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (n - 1));
            re[i] = (float)(samples[i] * w);
        }

        Transform(re, im);

        var mag = new float[fftSize / 2];
        for (int i = 0; i < mag.Length; i++)
            mag[i] = (float)Math.Sqrt(re[i] * re[i] + im[i] * im[i]);

        return mag;
    }
}
