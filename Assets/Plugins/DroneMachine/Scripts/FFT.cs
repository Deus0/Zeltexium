using System;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    public class FFT
    {
        public static void Forward(double[] real, double[] imag)
        {
            if (real.Length != imag.Length)
            {
                Debug.LogError("FFT: Input lengths do not match.");
                return;
            }

            int n = real.Length;
            int levels;
            {
                int temp = n;
                levels = 0;

                while (temp > 1)
                {
                    levels++;
                    temp >>= 1;
                }

                if (1 << levels != n)
                {
                    Debug.LogError("FFT: Length is not a power of 2");
                    return;
                }
            }

            // Trignometric tables
            double[] cosTable = new double[n/2];
            double[] sinTable = new double[n/2];
            for (int i = 0; i < n / 2; i++)
            {
                cosTable[i] = Math.Cos(2 * Math.PI * i / n);
                sinTable[i] = Math.Sin(2 * Math.PI * i / n);
            }

            // Bit-reversed addressing permutation
            for (int i = 0; i < n; i++)
            {
                int j = ReverseBits(i, levels);
                if (j > i)
                {
                    double temp = real[i];
                    real[i] = real[j];
                    real[j] = temp;
                    temp = imag[i];
                    imag[i] = imag[j];
                    imag[j] = temp;
                }
            }

            // Cooley-Tukey decimation-in-time radix-2 FFT
            for (int size = 2; size <= n; size *= 2)
            {
                int halfsize = size / 2;
                int tablestep = n / size;
                for (int i = 0; i < n; i += size)
                {
                    for (int j = i, k = 0; j < i + halfsize; j++, k += tablestep)
                    {
                        double tpre = real[j + halfsize] * cosTable[k] + imag[j + halfsize] * sinTable[k];
                        double tpim = -real[j + halfsize] * sinTable[k] + imag[j + halfsize] * cosTable[k];
                        real[j + halfsize] = real[j] - tpre;
                        imag[j + halfsize] = imag[j] - tpim;
                        real[j] += tpre;
                        imag[j] += tpim;
                    }
                }
                if (size == n)  // Prevent overflow in 'size *= 2'
                    break;
            }
        }

        public static void Inverse(double[] real, double[] imag)
        {
            Forward(imag, real);
        }

        static int ReverseBits(int x, int n)
        {
            int result = 0;
            int i;
            for (i = 0; i < n; i++, x >>= 1)
                result = (result << 1) | (x & 1);
            return result;
        } 
    }
}