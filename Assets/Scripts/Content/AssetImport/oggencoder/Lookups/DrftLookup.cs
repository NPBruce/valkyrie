using System;
using System.Collections.Generic;

namespace OggVorbisEncoder.Lookups
{
    public class DrftLookup
    {
        private readonly IList<int> _splitCache;
        private readonly IList<float> _trigCache;

        public DrftLookup(int n)
        {
            N = n;

            _trigCache = new float[3*n];
            _splitCache = new int[32];

            fdrffti(n);
        }

        public int N { get; }

        private void fdrffti(int n)
        {
            if (n == 1)
                return;

            drfti1(n);
        }

        private void drfti1(int n)
        {
            int[] ntryh = {4, 2, 3, 5};
            const float tpi = 6.28318530717958648f;
            int ntry = 0, i, j = -1;
            var nl = n;
            var nf = 0;

            int nr;
            do
            {
                j++;
                if (j < 4)
                    ntry = ntryh[j];
                else
                    ntry += 2;

                do
                {
                    var nq = nl/ntry;

                    nr = nl - ntry*nq;
                    if (nr != 0)
                        break;

                    nf++;
                    _splitCache[nf + 1] = ntry;
                    nl = nq;
                    if ((ntry != 2) || (nf == 1))
                        continue;

                    for (i = 1; i < nf; i++)
                    {
                        var ib = nf - i + 1;
                        _splitCache[ib + 1] = _splitCache[ib];
                    }

                    _splitCache[2] = 2;
                } while (nl != 1);
            } while (nr != 1);

            _splitCache[0] = n;
            _splitCache[1] = nf;
            var argh = tpi/n;
            var setting = n;
            var nfm1 = nf - 1;
            var l1 = 1;

            if (nfm1 == 0)
                return;

            for (var k1 = 0; k1 < nfm1; k1++)
            {
                var ip = _splitCache[k1 + 2];
                var ld = 0;
                var l2 = l1*ip;
                var ido = n/l2;
                var ipm = ip - 1;

                for (j = 0; j < ipm; j++)
                {
                    ld += l1;
                    i = setting;
                    var argld = ld*argh;
                    var fi = 0f;
                    int ii;
                    for (ii = 2; ii < ido; ii += 2)
                    {
                        fi += 1f;
                        var arg = fi*argld;
                        _trigCache[i++] = (float) Math.Cos(arg);
                        _trigCache[i++] = (float) Math.Sin(arg);
                    }
                    setting += ido;
                }
                l1 = l2;
            }
        }

        public void Forward(IList<float> data)
        {
            if (N == 1)
                return;

            var nf = _splitCache[1];
            var na = 1;
            var l2 = N;
            var iw = N;

            for (var k1 = 0; k1 < nf; k1++)
            {
                var kh = nf - k1;
                var ip = _splitCache[kh + 1];
                var l1 = l2/ip;
                var ido = N/l2;
                var idl1 = ido*l1;
                iw -= (ip - 1)*ido;
                na = 1 - na;

                if (ip != 4)
                {
                    if (ip != 2)
                    {
                        if (ido == 1)
                            na = 1 - na;

                        if (na != 0)
                        {
                            dradfg(ido, ip, l1, idl1, _trigCache, _trigCache, _trigCache, data, data, N + iw - 1);
                            na = 0;
                        }
                        else
                        {
                            dradfg(ido, ip, l1, idl1, data, data, data, _trigCache, _trigCache, N + iw - 1);
                            na = 1;
                        }
                    }
                    else
                    {
                        if (na != 0)
                            dradf2(ido, l1, _trigCache, data, N + iw - 1);
                        else
                            dradf2(ido, l1, data, _trigCache, N + iw - 1);
                    }
                }
                else
                {
                    var ix2 = iw + ido;
                    var ix3 = ix2 + ido;

                    if (na != 0)
                        dradf4(ido, l1, _trigCache, data, N + iw - 1, N + ix2 - 1, N + ix3 - 1);
                    else
                        dradf4(ido, l1, data, _trigCache, N + iw - 1, N + ix2 - 1, N + ix3 - 1);
                }

                l2 = l1;
            }

            if (na == 1)
                return;

            for (var i = 0; i < N; i++)
                data[i] = _trigCache[i];
        }

        private void dradf4(int ido, int l1, IList<float> cc, IList<float> ch, int wa1, int wa2, int wa3)
        {
            const float hsqt2 = .70710678118654752f;
            int k, t5, t6;
            float ti1;
            float tr1, tr2;
            var t0 = l1*ido;
            var t1 = t0;
            var t4 = t1 << 1;
            var t2 = t1 + (t1 << 1);
            var t3 = 0;

            for (k = 0; k < l1; k++)
            {
                tr1 = cc[t1] + cc[t2];
                tr2 = cc[t3] + cc[t4];

                ch[t5 = t3 << 2] = tr1 + tr2;
                ch[(ido << 2) + t5 - 1] = tr2 - tr1;
                ch[(t5 += ido << 1) - 1] = cc[t3] - cc[t4];
                ch[t5] = cc[t2] - cc[t1];

                t1 += ido;
                t2 += ido;
                t3 += ido;
                t4 += ido;
            }

            if (ido < 2)
                return;

            if (ido > 2)
            {
                t1 = 0;
                for (k = 0; k < l1; k++)
                {
                    t2 = t1;
                    t4 = t1 << 2;
                    t5 = (t6 = ido << 1) + t4;
                    for (var i = 2; i < ido; i += 2)
                    {
                        t3 = t2 += 2;
                        t4 += 2;
                        t5 -= 2;

                        t3 += t0;
                        var cr2 = _trigCache[wa1 + i - 2]*cc[t3 - 1] + _trigCache[wa1 + i - 1]*cc[t3];
                        var ci2 = _trigCache[wa1 + i - 2]*cc[t3] - _trigCache[wa1 + i - 1]*cc[t3 - 1];
                        t3 += t0;
                        var cr3 = _trigCache[wa2 + i - 2]*cc[t3 - 1] + _trigCache[wa2 + i - 1]*cc[t3];
                        var ci3 = _trigCache[wa2 + i - 2]*cc[t3] - _trigCache[wa2 + i - 1]*cc[t3 - 1];
                        t3 += t0;
                        var cr4 = _trigCache[wa3 + i - 2]*cc[t3 - 1] + _trigCache[wa3 + i - 1]*cc[t3];
                        var ci4 = _trigCache[wa3 + i - 2]*cc[t3] - _trigCache[wa3 + i - 1]*cc[t3 - 1];

                        tr1 = cr2 + cr4;
                        var tr4 = cr4 - cr2;
                        ti1 = ci2 + ci4;
                        var ti4 = ci2 - ci4;

                        var ti2 = cc[t2] + ci3;
                        var ti3 = cc[t2] - ci3;
                        tr2 = cc[t2 - 1] + cr3;
                        var tr3 = cc[t2 - 1] - cr3;

                        ch[t4 - 1] = tr1 + tr2;
                        ch[t4] = ti1 + ti2;

                        ch[t5 - 1] = tr3 - ti4;
                        ch[t5] = tr4 - ti3;

                        ch[t4 + t6 - 1] = ti4 + tr3;
                        ch[t4 + t6] = tr4 + ti3;

                        ch[t5 + t6 - 1] = tr2 - tr1;
                        ch[t5 + t6] = ti1 - ti2;
                    }
                    t1 += ido;
                }

                if ((ido & 1) != 0)
                    return;
            }

            t2 = (t1 = t0 + ido - 1) + (t0 << 1);
            t3 = ido << 2;
            t4 = ido;
            t5 = ido << 1;
            t6 = ido;

            for (k = 0; k < l1; k++)
            {
                ti1 = -hsqt2*(cc[t1] + cc[t2]);
                tr1 = hsqt2*(cc[t1] - cc[t2]);

                ch[t4 - 1] = tr1 + cc[t6 - 1];
                ch[t4 + t5 - 1] = cc[t6 - 1] - tr1;

                ch[t4] = ti1 - cc[t1 + t0];
                ch[t4 + t5] = ti1 + cc[t1 + t0];

                t1 += ido;
                t2 += ido;
                t4 += t3;
                t6 += ido;
            }
        }

        private void dradf2(int ido, int l1, IList<float> cc, IList<float> ch, int wa1)
        {
            var t1 = 0;
            var t2 = l1*ido;
            var t0 = t2;
            var t3 = ido << 1;
            for (var k = 0; k < l1; k++)
            {
                ch[t1 << 1] = cc[t1] + cc[t2];
                ch[(t1 << 1) + t3 - 1] = cc[t1] - cc[t2];
                t1 += ido;
                t2 += ido;
            }

            if (ido < 2)
                return;

            if (ido > 2)
            {
                t1 = 0;
                t2 = t0;
                for (var k = 0; k < l1; k++)
                {
                    t3 = t2;
                    var t4 = (t1 << 1) + (ido << 1);
                    var t5 = t1;
                    var t6 = t1 + t1;
                    for (var i = 2; i < ido; i += 2)
                    {
                        t3 += 2;
                        t4 -= 2;
                        t5 += 2;
                        t6 += 2;
                        var tr2 = _trigCache[wa1 + i - 2]*cc[t3 - 1] + _trigCache[wa1 + i - 1]*cc[t3];
                        var ti2 = _trigCache[wa1 + i - 2]*cc[t3] - _trigCache[wa1 + i - 1]*cc[t3 - 1];
                        ch[t6] = cc[t5] + ti2;
                        ch[t4] = ti2 - cc[t5];
                        ch[t6 - 1] = cc[t5 - 1] + tr2;
                        ch[t4 - 1] = cc[t5 - 1] - tr2;
                    }
                    t1 += ido;
                    t2 += ido;
                }

                if (ido%2 == 1)
                    return;
            }

            t3 = t2 = (t1 = ido) - 1;
            t2 += t0;
            for (var k = 0; k < l1; k++)
            {
                ch[t1] = -cc[t2];
                ch[t1 - 1] = cc[t3];
                t1 += ido << 1;
                t2 += ido;
                t3 += ido;
            }
        }

        private void dradfg(int ido, int ip, int l1, int idl1, IList<float> cc, IList<float> c1, IList<float> c2,
            IList<float> ch, IList<float> ch2, int wa)
        {
            const float tpi = 6.283185307179586f;

            var arg = tpi/ip;
            var dcp = (float) Math.Cos(arg);
            var dsp = (float) Math.Sin(arg);
            var ipph = (ip + 1) >> 1;
            var ipp2 = ip;
            var idp2 = ido;
            var nbd = (ido - 1) >> 1;
            var t0 = l1*ido;
            var t10 = ip*ido;

            int i, j, k, l, ik, t1, t2, t3, t4, t5, t6, t7, t8, t9;

            if (ido != 1)
            {
                for (ik = 0; ik < idl1; ik++)
                    ch2[ik] = c2[ik];

                t1 = 0;
                for (j = 1; j < ip; j++)
                {
                    t1 += t0;
                    t2 = t1;
                    for (k = 0; k < l1; k++)
                    {
                        ch[t2] = c1[t2];
                        t2 += ido;
                    }
                }

                var setting = -ido;
                t1 = 0;
                
                if (nbd > l1)
                    for (j = 1; j < ip; j++)
                    {
                        t1 += t0;
                        setting += ido;
                        t2 = -ido + t1;
                        for (k = 0; k < l1; k++)
                        {
                            var idij = setting - 1;
                            t2 += ido;
                            t3 = t2;
                            for (i = 2; i < ido; i += 2)
                            {
                                idij += 2;
                                t3 += 2;
                                ch[t3 - 1] = _trigCache[wa + idij - 1]*c1[t3 - 1] + _trigCache[wa + idij]*c1[t3];
                                ch[t3] = _trigCache[wa + idij - 1]*c1[t3] - _trigCache[wa + idij]*c1[t3 - 1];
                            }
                        }
                    }
                else
                    for (j = 1; j < ip; j++)
                    {
                        setting += ido;
                        var idij = setting - 1;
                        t1 += t0;
                        t2 = t1;
                        for (i = 2; i < ido; i += 2)
                        {
                            idij += 2;
                            t2 += 2;
                            t3 = t2;
                            for (k = 0; k < l1; k++)
                            {
                                ch[t3 - 1] = _trigCache[wa + idij - 1]*c1[t3 - 1] + _trigCache[wa + idij]*c1[t3];
                                ch[t3] = _trigCache[wa + idij - 1]*c1[t3] - _trigCache[wa + idij]*c1[t3 - 1];
                                t3 += ido;
                            }
                        }
                    }

                t1 = 0;
                t2 = ipp2*t0;
                if (nbd < l1)
                    for (j = 1; j < ipph; j++)
                    {
                        t1 += t0;
                        t2 -= t0;
                        t3 = t1;
                        t4 = t2;
                        for (i = 2; i < ido; i += 2)
                        {
                            t3 += 2;
                            t4 += 2;
                            t5 = t3 - ido;
                            t6 = t4 - ido;
                            for (k = 0; k < l1; k++)
                            {
                                t5 += ido;
                                t6 += ido;
                                c1[t5 - 1] = ch[t5 - 1] + ch[t6 - 1];
                                c1[t6 - 1] = ch[t5] - ch[t6];
                                c1[t5] = ch[t5] + ch[t6];
                                c1[t6] = ch[t6 - 1] - ch[t5 - 1];
                            }
                        }
                    }
                else
                    for (j = 1; j < ipph; j++)
                    {
                        t1 += t0;
                        t2 -= t0;
                        t3 = t1;
                        t4 = t2;
                        for (k = 0; k < l1; k++)
                        {
                            t5 = t3;
                            t6 = t4;
                            for (i = 2; i < ido; i += 2)
                            {
                                t5 += 2;
                                t6 += 2;
                                c1[t5 - 1] = ch[t5 - 1] + ch[t6 - 1];
                                c1[t6 - 1] = ch[t5] - ch[t6];
                                c1[t5] = ch[t5] + ch[t6];
                                c1[t6] = ch[t6 - 1] - ch[t5 - 1];
                            }
                            t3 += ido;
                            t4 += ido;
                        }
                    }
            }

            for (ik = 0; ik < idl1; ik++)
                c2[ik] = ch2[ik];

            t1 = 0;
            t2 = ipp2*idl1;
            for (j = 1; j < ipph; j++)
            {
                t1 += t0;
                t2 -= t0;
                t3 = t1 - ido;
                t4 = t2 - ido;
                for (k = 0; k < l1; k++)
                {
                    t3 += ido;
                    t4 += ido;
                    c1[t3] = ch[t3] + ch[t4];
                    c1[t4] = ch[t4] - ch[t3];
                }
            }

            var ar1 = 1f;
            var ai1 = 0f;
            t1 = 0;
            t2 = ipp2*idl1;
            t3 = (ip - 1)*idl1;
            for (l = 1; l < ipph; l++)
            {
                t1 += idl1;
                t2 -= idl1;
                var ar1h = dcp*ar1 - dsp*ai1;
                ai1 = dcp*ai1 + dsp*ar1;
                ar1 = ar1h;
                t4 = t1;
                t5 = t2;
                t6 = t3;
                t7 = idl1;

                for (ik = 0; ik < idl1; ik++)
                {
                    ch2[t4++] = c2[ik] + ar1*c2[t7++];
                    ch2[t5++] = ai1*c2[t6++];
                }

                var dc2 = ar1;
                var ds2 = ai1;
                var ar2 = ar1;
                var ai2 = ai1;

                t4 = idl1;
                t5 = (ipp2 - 1)*idl1;
                for (j = 2; j < ipph; j++)
                {
                    t4 += idl1;
                    t5 -= idl1;

                    var ar2h = dc2*ar2 - ds2*ai2;
                    ai2 = dc2*ai2 + ds2*ar2;
                    ar2 = ar2h;

                    t6 = t1;
                    t7 = t2;
                    t8 = t4;
                    t9 = t5;
                    for (ik = 0; ik < idl1; ik++)
                    {
                        ch2[t6++] += ar2*c2[t8++];
                        ch2[t7++] += ai2*c2[t9++];
                    }
                }
            }

            t1 = 0;
            for (j = 1; j < ipph; j++)
            {
                t1 += idl1;
                t2 = t1;
                for (ik = 0; ik < idl1; ik++)
                    ch2[ik] += c2[t2++];
            }

            if (ido < l1)
            {
                for (i = 0; i < ido; i++)
                {
                    t1 = i;
                    t2 = i;
                    for (k = 0; k < l1; k++)
                    {
                        cc[t2] = ch[t1];
                        t1 += ido;
                        t2 += t10;
                    }
                }
            }
            else
            {
                t1 = 0;
                t2 = 0;
                for (k = 0; k < l1; k++)
                {
                    t3 = t1;
                    t4 = t2;

                    for (i = 0; i < ido; i++)
                        cc[t4++] = ch[t3++];

                    t1 += ido;
                    t2 += t10;
                }
            }

            t1 = 0;
            t2 = ido << 1;
            t3 = 0;
            t4 = ipp2*t0;
            for (j = 1; j < ipph; j++)
            {
                t1 += t2;
                t3 += t0;
                t4 -= t0;

                t5 = t1;
                t6 = t3;
                t7 = t4;

                for (k = 0; k < l1; k++)
                {
                    cc[t5 - 1] = ch[t6];
                    cc[t5] = ch[t7];
                    t5 += t10;
                    t6 += ido;
                    t7 += ido;
                }
            }

            if (ido == 1)
                return;

            if (nbd >= l1)
            {
                t1 = -ido;
                t3 = 0;
                t4 = 0;
                t5 = ipp2*t0;
                for (j = 1; j < ipph; j++)
                {
                    t1 += t2;
                    t3 += t2;
                    t4 += t0;
                    t5 -= t0;
                    t6 = t1;
                    t7 = t3;
                    t8 = t4;
                    t9 = t5;
                    for (k = 0; k < l1; k++)
                    {
                        for (i = 2; i < ido; i += 2)
                        {
                            var ic = idp2 - i;
                            cc[i + t7 - 1] = ch[i + t8 - 1] + ch[i + t9 - 1];
                            cc[ic + t6 - 1] = ch[i + t8 - 1] - ch[i + t9 - 1];
                            cc[i + t7] = ch[i + t8] + ch[i + t9];
                            cc[ic + t6] = ch[i + t9] - ch[i + t8];
                        }
                        t6 += t10;
                        t7 += t10;
                        t8 += ido;
                        t9 += ido;
                    }
                }

                return;
            }

            t1 = -ido;
            t3 = 0;
            t4 = 0;
            t5 = ipp2*t0;
            for (j = 1; j < ipph; j++)
            {
                t1 += t2;
                t3 += t2;
                t4 += t0;
                t5 -= t0;
                for (i = 2; i < ido; i += 2)
                {
                    t6 = idp2 + t1 - i;
                    t7 = i + t3;
                    t8 = i + t4;
                    t9 = i + t5;
                    for (k = 0; k < l1; k++)
                    {
                        cc[t7 - 1] = ch[t8 - 1] + ch[t9 - 1];
                        cc[t6 - 1] = ch[t8 - 1] - ch[t9 - 1];
                        cc[t7] = ch[t8] + ch[t9];
                        cc[t6] = ch[t9] - ch[t8];
                        t6 += t10;
                        t7 += t10;
                        t8 += ido;
                        t9 += ido;
                    }
                }
            }
        }
    }
}