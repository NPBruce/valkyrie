using System;
using System.Collections.Generic;

namespace OggVorbisEncoder.Lookups
{
    public class MdctLookup
    {
        private const float Pi3Eighths = .38268343236508977175f;
        private const float Pi2Eighths = .70710678118654752441f;
        private const float Pi1Eighth = .92387953251128675613f;
        private readonly int[] _bitReverse;
        private readonly int _log2N;
        private readonly int _n;

        private readonly float _scale;
        private readonly float[] _trig;

        public MdctLookup(int n)
        {
            _n = n;
            var n2 = n >> 1;
            _log2N = (int) Math.Round(Math.Log(n)/Math.Log(2f));
            _trig = new float[n + n/4];
            _bitReverse = new int[n/4];

            // trig lookups
            for (var i = 0; i < n/4; i++)
            {
                _trig[i*2] = (float) Math.Cos(Math.PI/n*(4*i));
                _trig[i*2 + 1] = (float) -Math.Sin(Math.PI/n*(4*i));
                _trig[n2 + i*2] = (float) Math.Cos(Math.PI/(2*n)*(2*i + 1));
                _trig[n2 + i*2 + 1] = (float) Math.Sin(Math.PI/(2*n)*(2*i + 1));
            }

            for (var i = 0; i < n/8; i++)
            {
                _trig[n + i*2] = (float) (Math.Cos(Math.PI/n*(4*i + 2))*.5);
                _trig[n + i*2 + 1] = (float) (-Math.Sin(Math.PI/n*(4*i + 2))*.5);
            }

            // bitreverse lookup
            var mask = (1 << (_log2N - 1)) - 1;
            var msb = 1 << (_log2N - 2);
            for (var i = 0; i < n/8; i++)
            {
                var acc = 0;
                for (var j = 0; msb >> j != 0; j++)
                    if (((msb >> j) & i) != 0)
                        acc |= 1 << j;

                _bitReverse[i*2] = (~acc & mask) - 1;
                _bitReverse[i*2 + 1] = acc;
            }

            _scale = 4f/n;
        }

        public void Forward(IList<float> input, float[] output)
        {
            var n = _n;
            var n2 = n >> 1;
            var n4 = n >> 2;
            var n8 = n >> 3;
            var work = new float[n];
            var w2 = new OffsetArray<float>(work, n2);

            // rotate 
            // window + rotate + step 1 
            var x0 = n2 + n4;
            var x1 = x0 + 1;
            var t = n2;

            var i = 0;
            for (; i < n8; i += 2)
            {
                x0 -= 4;
                t -= 2;
                var r0 = input[x0 + 2] + input[x1 + 0];
                var r1 = input[x0 + 0] + input[x1 + 2];
                w2[i] = r1*_trig[t + 1] + r0*_trig[t + 0];
                w2[i + 1] = r1*_trig[t + 0] - r0*_trig[t + 1];
                x1 += 4;
            }

            x1 = 1;

            for (; i < n2 - n8; i += 2)
            {
                t -= 2;
                x0 -= 4;
                var r0 = input[x0 + 2] - input[x1 + 0];
                var r1 = input[x0 + 0] - input[x1 + 2];
                w2[i] = r1*_trig[t + 1] + r0*_trig[t + 0];
                w2[i + 1] = r1*_trig[t + 0] - r0*_trig[t + 1];
                x1 += 4;
            }

            x0 = n;

            for (; i < n2; i += 2)
            {
                t -= 2;
                x0 -= 4;
                var r0 = -input[x0 + 2] - input[x1 + 0];
                var r1 = -input[x0 + 0] - input[x1 + 2];
                w2[i] = r1*_trig[t + 1] + r0*_trig[t + 0];
                w2[i + 1] = r1*_trig[t + 0] - r0*_trig[t + 1];
                x1 += 4;
            }


            Butterflies(w2, n2);
            ReverseBits(work);

            // rotate + window 
            t = n2;
            x0 = n2;
            var offset = 0;
            for (i = 0; i < n4; i++)
            {
                x0--;
                output[i] = (work[offset + 0]*_trig[t + 0]
                             + work[offset + 1]*_trig[t + 1])*_scale;

                output[x0 + 0] = (work[offset + 0]*_trig[t + 1]
                                  - work[offset + 1]*_trig[t + 0])*_scale;
                offset += 2;
                t += 2;
            }
        }

        private void Butterflies(IList<float> data, int points)
        {
            var stages = _log2N - 5;

            if (--stages > 0)
                ButterflyFirst(data, points);

            for (var i = 1; --stages > 0; i++)
                for (var j = 0; j < 1 << i; j++)
                    ButterflyGeneric(data, (points >> i)*j, points >> i, 4 << i);

            for (var j = 0; j < points; j += 32)
                Butterfly32(data, j);
        }

        private static void Butterfly32(IList<float> data, int offset)
        {
            var r0 = data[offset + 30] - data[offset + 14];
            var r1 = data[offset + 31] - data[offset + 15];

            data[offset + 30] += data[offset + 14];
            data[offset + 31] += data[offset + 15];
            data[offset + 14] = r0;
            data[offset + 15] = r1;

            r0 = data[offset + 28] - data[offset + 12];
            r1 = data[offset + 29] - data[offset + 13];
            data[offset + 28] += data[offset + 12];
            data[offset + 29] += data[offset + 13];
            data[offset + 12] = r0*Pi1Eighth - r1*Pi3Eighths;
            data[offset + 13] = r0*Pi3Eighths + r1*Pi1Eighth;

            r0 = data[offset + 26] - data[offset + 10];
            r1 = data[offset + 27] - data[offset + 11];
            data[offset + 26] += data[offset + 10];
            data[offset + 27] += data[offset + 11];
            data[offset + 10] = (r0 - r1)*Pi2Eighths;
            data[offset + 11] = (r0 + r1)*Pi2Eighths;

            r0 = data[offset + 24] - data[offset + 8];
            r1 = data[offset + 25] - data[offset + 9];
            data[offset + 24] += data[offset + 8];
            data[offset + 25] += data[offset + 9];
            data[offset + 8] = r0*Pi3Eighths - r1*Pi1Eighth;
            data[offset + 9] = r1*Pi3Eighths + r0*Pi1Eighth;

            r0 = data[offset + 22] - data[offset + 6];
            r1 = data[offset + 7] - data[offset + 23];
            data[offset + 22] += data[offset + 6];
            data[offset + 23] += data[offset + 7];
            data[offset + 6] = r1;
            data[offset + 7] = r0;

            r0 = data[offset + 4] - data[offset + 20];
            r1 = data[offset + 5] - data[offset + 21];
            data[offset + 20] += data[offset + 4];
            data[offset + 21] += data[offset + 5];
            data[offset + 4] = r1*Pi1Eighth + r0*Pi3Eighths;
            data[offset + 5] = r1*Pi3Eighths - r0*Pi1Eighth;

            r0 = data[offset + 2] - data[offset + 18];
            r1 = data[offset + 3] - data[offset + 19];
            data[offset + 18] += data[offset + 2];
            data[offset + 19] += data[offset + 3];
            data[offset + 2] = (r1 + r0)*Pi2Eighths;
            data[offset + 3] = (r1 - r0)*Pi2Eighths;

            r0 = data[offset + 0] - data[offset + 16];
            r1 = data[offset + 1] - data[offset + 17];
            data[offset + 16] += data[offset + 0];
            data[offset + 17] += data[offset + 1];
            data[offset + 0] = r1*Pi3Eighths + r0*Pi1Eighth;
            data[offset + 1] = r1*Pi1Eighth - r0*Pi3Eighths;

            Butterfly16(data, offset);
            Butterfly16(data, offset + 16);
        }

        private static void Butterfly16(IList<float> data, int offset)
        {
            var r0 = data[offset + 1] - data[offset + 9];
            var r1 = data[offset + 0] - data[offset + 8];

            data[offset + 8] += data[offset + 0];
            data[offset + 9] += data[offset + 1];
            data[offset + 0] = (r0 + r1)*Pi2Eighths;
            data[offset + 1] = (r0 - r1)*Pi2Eighths;

            r0 = data[offset + 3] - data[offset + 11];
            r1 = data[offset + 10] - data[offset + 2];
            data[offset + 10] += data[offset + 2];
            data[offset + 11] += data[offset + 3];
            data[offset + 2] = r0;
            data[offset + 3] = r1;

            r0 = data[offset + 12] - data[offset + 4];
            r1 = data[offset + 13] - data[offset + 5];
            data[offset + 12] += data[offset + 4];
            data[offset + 13] += data[offset + 5];
            data[offset + 4] = (r0 - r1)*Pi2Eighths;
            data[offset + 5] = (r0 + r1)*Pi2Eighths;

            r0 = data[offset + 14] - data[offset + 6];
            r1 = data[offset + 15] - data[offset + 7];
            data[offset + 14] += data[offset + 6];
            data[offset + 15] += data[offset + 7];
            data[offset + 6] = r0;
            data[offset + 7] = r1;

            Butterfly8(data, offset);
            Butterfly8(data, offset + 8);
        }

        private static void Butterfly8(IList<float> data, int offset)
        {
            var r0 = data[offset + 6] + data[offset + 2];
            var r1 = data[offset + 6] - data[offset + 2];
            var r2 = data[offset + 4] + data[offset + 0];
            var r3 = data[offset + 4] - data[offset + 0];

            data[offset + 6] = r0 + r2;
            data[offset + 4] = r0 - r2;

            r0 = data[offset + 5] - data[offset + 1];
            r2 = data[offset + 7] - data[offset + 3];
            data[offset + 0] = r1 + r0;
            data[offset + 2] = r1 - r0;

            r0 = data[offset + 5] + data[offset + 1];
            r1 = data[offset + 7] + data[offset + 3];
            data[offset + 3] = r2 + r3;
            data[offset + 1] = r2 - r3;
            data[offset + 7] = r1 + r0;
            data[offset + 5] = r1 - r0;
        }

        private void ButterflyGeneric(IList<float> data, int offset, int points, int trigIncrement)
        {
            var t = 0;
            var x1 = offset + points - 8;
            var x2 = offset + (points >> 1) - 8;

            do
            {
                var r0 = data[x1 + 6] - data[x2 + 6];
                var r1 = data[x1 + 7] - data[x2 + 7];
                data[x1 + 6] += data[x2 + 6];
                data[x1 + 7] += data[x2 + 7];
                data[x2 + 6] = r1*_trig[t + 1] + r0*_trig[t + 0];
                data[x2 + 7] = r1*_trig[t + 0] - r0*_trig[t + 1];

                t += trigIncrement;

                r0 = data[x1 + 4] - data[x2 + 4];
                r1 = data[x1 + 5] - data[x2 + 5];
                data[x1 + 4] += data[x2 + 4];
                data[x1 + 5] += data[x2 + 5];
                data[x2 + 4] = r1*_trig[t + 1] + r0*_trig[t + 0];
                data[x2 + 5] = r1*_trig[t + 0] - r0*_trig[t + 1];

                t += trigIncrement;

                r0 = data[x1 + 2] - data[x2 + 2];
                r1 = data[x1 + 3] - data[x2 + 3];
                data[x1 + 2] += data[x2 + 2];
                data[x1 + 3] += data[x2 + 3];
                data[x2 + 2] = r1*_trig[t + 1] + r0*_trig[t + 0];
                data[x2 + 3] = r1*_trig[t + 0] - r0*_trig[t + 1];

                t += trigIncrement;

                r0 = data[x1 + 0] - data[x2 + 0];
                r1 = data[x1 + 1] - data[x2 + 1];
                data[x1 + 0] += data[x2 + 0];
                data[x1 + 1] += data[x2 + 1];
                data[x2 + 0] = r1*_trig[t + 1] + r0*_trig[t + 0];
                data[x2 + 1] = r1*_trig[t + 0] - r0*_trig[t + 1];

                t += trigIncrement;
                x1 -= 8;
                x2 -= 8;
            } while (x2 >= offset);
        }

        private void ButterflyFirst(IList<float> data, int points)
        {
            var x1 = points - 8;
            var x2 = (points >> 1) - 8;
            var t = 0;

            do
            {
                var r0 = data[x1 + 6] - data[x2 + 6];
                var r1 = data[x1 + 7] - data[x2 + 7];
                data[x1 + 6] += data[x2 + 6];
                data[x1 + 7] += data[x2 + 7];
                data[x2 + 6] = r1*_trig[t + 1] + r0*_trig[t + 0];
                data[x2 + 7] = r1*_trig[t + 0] - r0*_trig[t + 1];

                r0 = data[x1 + 4] - data[x2 + 4];
                r1 = data[x1 + 5] - data[x2 + 5];
                data[x1 + 4] += data[x2 + 4];
                data[x1 + 5] += data[x2 + 5];
                data[x2 + 4] = r1*_trig[t + 5] + r0*_trig[t + 4];
                data[x2 + 5] = r1*_trig[t + 4] - r0*_trig[t + 5];

                r0 = data[x1 + 2] - data[x2 + 2];
                r1 = data[x1 + 3] - data[x2 + 3];
                data[x1 + 2] += data[x2 + 2];
                data[x1 + 3] += data[x2 + 3];
                data[x2 + 2] = r1*_trig[t + 9] + r0*_trig[t + 8];
                data[x2 + 3] = r1*_trig[t + 8] - r0*_trig[t + 9];

                r0 = data[x1 + 0] - data[x2 + 0];
                r1 = data[x1 + 1] - data[x2 + 1];
                data[x1 + 0] += data[x2 + 0];
                data[x1 + 1] += data[x2 + 1];
                data[x2 + 0] = r1*_trig[t + 13] + r0*_trig[t + 12];
                data[x2 + 1] = r1*_trig[t + 12] - r0*_trig[t + 13];

                x1 -= 8;
                x2 -= 8;
                t += 16;
            } while (x2 >= 0);
        }

        private void ReverseBits(IList<float> data)
        {
            var n = _n;
            var bit = 0;
            var w0 = 0;
            var w1 = w0 + (n >> 1);
            var x = w1;
            var t = n;

            do
            {
                var x0 = x + _bitReverse[bit + 0];
                var x1 = x + _bitReverse[bit + 1];

                var r0 = data[x0 + 1] - data[x1 + 1];
                var r1 = data[x0 + 0] + data[x1 + 0];
                var r2 = r1*_trig[t + 0] + r0*_trig[t + 1];
                var r3 = r1*_trig[t + 1] - r0*_trig[t + 0];

                w1 -= 4;

                r0 = 0.5f*(data[x0 + 1] + data[x1 + 1]);
                r1 = 0.5f*(data[x0 + 0] - data[x1 + 0]);

                data[w0 + 0] = r0 + r2;
                data[w1 + 2] = r0 - r2;
                data[w0 + 1] = r1 + r3;
                data[w1 + 3] = r3 - r1;

                x0 = x + _bitReverse[bit + 2];
                x1 = x + _bitReverse[bit + 3];

                r0 = data[x0 + 1] - data[x1 + 1];
                r1 = data[x0 + 0] + data[x1 + 0];
                r2 = r1*_trig[t + 2] + r0*_trig[t + 3];
                r3 = r1*_trig[t + 3] - r0*_trig[t + 2];

                r0 = 0.5f*(data[x0 + 1] + data[x1 + 1]);
                r1 = 0.5f*(data[x0 + 0] - data[x1 + 0]);

                data[w0 + 2] = r0 + r2;
                data[w1 + 0] = r0 - r2;
                data[w0 + 3] = r1 + r3;
                data[w1 + 1] = r3 - r1;

                t += 4;
                bit += 4;
                w0 += 4;
            } while (w0 < w1);
        }
    }
}