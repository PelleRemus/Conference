namespace FifthOrderBoundaryValueProblem;

public class FifthOrderBVP : IDisposable
{
    /// <summary>
    /// <b>[a, b]</b><br/>
    /// The lower bound of the interval on which the solution is calculated.
    /// </summary>
    public BigFloat a;

    /// <summary>
    /// <b>[a, b]</b><br/>
    /// The upper bound of the interval on which the solution is calculated.
    /// </summary>
    public BigFloat b;

    /// <summary>
    /// <b>n</b><br/>
    /// The number of subintervals created for the approximation.
    /// </summary>
    public int n;

    /// <summary>
    /// <b>k</b><br/>
    /// The number of executed iterations until we reached the final solution.
    /// </summary>
    public int k;
    public int maxSteps;

    /// <summary>
    /// <b>ε</b><br/>
    /// The maximum error of the final solution.
    /// </summary>
    public BigFloat epsilon;

    /// <summary>
    /// <b>c<sub>1</sub> = x(a)</b><br/>
    /// Necessary parameter for the 5th order.
    /// </summary>
    public BigFloat c1;

    /// <summary>
    /// <b>c<sub>2</sub> = x'(a)</b><br/>
    /// Necessary parameter for the 5th order.
    /// </summary>
    public BigFloat c2;

    /// <summary>
    /// <b>c<sub>3</sub> = x"(a)</b><br/>
    /// Necessary parameter for the 5th order.
    /// </summary>
    public BigFloat c3;

    /// <summary>
    /// <b>c<sub>4</sub> = x(b)</b><br/>
    /// Necessary parameter for the 5th order.
    /// </summary>
    public BigFloat c4;

    /// <summary>
    /// <b>c<sub>5</sub> = x'(b)</b><br/>
    /// Necessary parameter for the 5th order.
    /// </summary>
    public BigFloat c5;

    /// <summary>
    /// <b>ti</b><br/>
    /// The bounds of the n subintervals of the [a, b] interval.
    /// </summary>
    public BigFloat[] t;

    /// <summary>
    /// <b>x(k, i)</b><br/>
    /// The calculated solutions.
    /// </summary>
    public BigFloat[] xk;

    /// <summary>
    /// <b>x(k-1, i)</b><br/>
    /// The solutions of the previous iteration, used for calculations.
    /// </summary>
    public BigFloat[] xk_1;

    /// <summary>
    /// <b>f(s,v,w)</b><br/>
    /// The function which approximate the exact solution x*(t).
    /// </summary>
    public Func<BigFloat, BigFloat, BigFloat, BigFloat> f;

    /// <summary>
    /// <b>fi(t)</b><br/>
    /// The function which "slows down" the parameter t.
    /// </summary>
    public Func<BigFloat, BigFloat> fi;

    public FifthOrderBVP(decimal a, decimal b, int n, decimal epsilon,
        decimal c1, decimal c2, decimal c3, decimal c4, decimal c5,
        Func<BigFloat, BigFloat, BigFloat, BigFloat> f, Func<BigFloat, BigFloat> fi, int maxSteps)
    {
        this.a = new BigFloat(a);
        this.b = new BigFloat(b);
        this.n = n;
        this.epsilon = new BigFloat(epsilon);
        this.c1 = new BigFloat(c1);
        this.c2 = new BigFloat(c2);
        this.c3 = new BigFloat(c3);
        this.c4 = new BigFloat(c4);
        this.c5 = new BigFloat(c5);
        this.f = f;
        this.fi = fi;
        this.maxSteps = maxSteps;
        xk = new BigFloat[n + 1];
        xk_1 = new BigFloat[n + 1];
        t = new BigFloat[n + 1];
    }

    public async Task Calculate()
    {
        await Task.Run(() =>
        {
            // ---------------------------------------- Step 2 ----------------------------------------
            // h represents the step, the subintervals size
            // ti represent the subintervals bounds
            BigFloat h = (b - a) / new BigFloat(n);
            for (int i = 0; i <= n; i++)
            {
                t[i] = new BigFloat(i) * h;
            }

            // ---------------------------------------- Step 4 ----------------------------------------
            // The 0th iteration of the solution is represented by g(ti)
            xk_1[0] = c1;
            for (int i = 1; i < n; i++)
            {
                xk_1[i] = g(t[i]);
            }
            xk_1[n] = c4;

            // ---------------------------------------- Step 5 ----------------------------------------
            // u[i] represents the middle of the subintervals, meaning u[1] would represent t[0.5]
            BigFloat[] u = new BigFloat[n + 1];
            for (int i = 1; i <= n; i++)
            {
                u[i] = (t[i - 1] + t[i]) / _._2;
            }

            // We calculate the 1st iteration of the solution
            BigFloat h_30 = h / _._30;
            xk[0] = c1;
            for (int i = 1; i < n; i++)
            {
                BigFloat suma = _._0;
                for (int j = 1; j <= n; j++)
                {
                    suma += _._7 * F(t[i], t[j - 1]) + _._16 * F(t[i], u[j]) + _._7 * F(t[i], t[j]);
                }
                xk[i] = g(t[i]) + h_30 * suma;
            }
            xk[n] = c4;

            // ---------------------------------------- Step 5' ----------------------------------------
            // z[i] represents the solution on the middles of the subintervals, meaning z[1] would represent x[0.5]
            BigFloat[] zk = new BigFloat[n + 1];
            zk[0] = _._0;
            for (int i = 1; i <= n; i++)
            {
                BigFloat suma = _._0;
                for (int j = 1; j <= n; j++)
                {
                    suma += _._7 * F(u[i], t[j - 1]) + _._16 * F(u[i], u[j]) + _._7 * F(u[i], t[j]);
                }
                zk[i] = g(u[i]) + h_30 * suma;
            }

            // ---------------------------------------- Step 6 ----------------------------------------
            // A, B, C, D and E will help for the Spline function
            Func<BigFloat, int, BigFloat> A = (T, i) =>
            {
                BigFloat T_ti_h = (T - t[i]) / h;
                return (_._1 - _._2 * T_ti_h) * (_._1 - T_ti_h) * (_._1 - T_ti_h) * (_._1 + _._4 * T_ti_h);
            };
            Func<BigFloat, int, BigFloat> B = (T, i) =>
            {
                BigFloat T_ti_h = (T - t[i]) / h;
                return _._16 * T_ti_h * T_ti_h * (_._1 - T_ti_h) * (_._1 - T_ti_h);
            };
            Func<BigFloat, int, BigFloat> C = (T, i) =>
            {
                BigFloat T_ti_h = (T - t[i]) / h;
                return T_ti_h * T_ti_h * (_._2 * T_ti_h - _._1) * (_._1 + _._4 * (_._1 - T_ti_h));
            };
            Func<BigFloat, int, BigFloat> D = (T, i) =>
            {
                BigFloat T_ti_h = (T - t[i]) / h;
                return T_ti_h * (_._1 - _._2 * T_ti_h) * (_._1 - T_ti_h) * (_._1 - T_ti_h);
            };
            Func<BigFloat, int, BigFloat> E = (T, i) =>
            {
                BigFloat T_ti_h = (T - t[i]) / h;
                return T_ti_h * T_ti_h * (_._1 - _._2 * T_ti_h) * (_._1 - T_ti_h);
            };

            // ---------------------------------------- Step 6' ----------------------------------------
            // M represents the Spline function derivatives on the bounds
            BigFloat h6 = h * _._6;
            BigFloat[] M = new BigFloat[n + 1];
            M[0] = c2;
            for (int i = 1; i < n; i++)
            {
                M[i] = (xk[i - 1] - _._8 * zk[i] + _._8 * zk[i + 1] - xk[i + 1]) / h6;
            }
            M[n] = c5;

            BigFloat[] zk_1 = new BigFloat[n + 1];
            Func<BigFloat, BigFloat> S = T =>
            {
                int i = 1;
                while (T > t[i] && i < n)
                    i++;
                return A(T, i - 1) * xk_1[i - 1] + B(T, i - 1) * zk_1[i] + C(T, i - 1) * xk_1[i]
                    + D(T, i - 1) * h * M[i - 1] + E(T, i - 1) * h * M[i];
            };

            // ---------------------------------------- Step 7 ----------------------------------------
            k = 1;
            while (!SolutionIsGoodEnough(k))
            {
                Array.Copy(xk, xk_1, n + 1);
                Array.Copy(zk, zk_1, n + 1);
                xk[0] = c1;
                for (int i = 1; i < n; i++)
                {
                    BigFloat suma = _._0;
                    for (int j = 1; j <= n; j++)
                    {
                        suma += _._7 * F(t[i], t[j - 1], xk_1[j - 1], S) + _._16 * F(t[i], u[j], zk[j], S) + _._7 * F(t[i], t[j], xk_1[j], S);
                    }
                    xk[i] = g(t[i]) + h_30 * suma;
                }
                xk[n] = c4;

                zk[0] = _._0;
                for (int i = 1; i <= n; i++)
                {
                    BigFloat suma = _._0;
                    for (int j = 1; j <= n; j++)
                    {
                        suma += _._7 * F(u[i], t[j - 1], xk_1[j - 1], S) + _._16 * F(u[i], u[j], zk_1[j], S) + _._7 * F(u[i], t[j], xk_1[j], S);
                    }
                    zk[i] = g(u[i]) + h_30 * suma;
                }

                // ---------------------------------------- Step 7' ----------------------------------------
                M[0] = c2;
                for (int i = 1; i < n; i++)
                {
                    M[i] = (xk[i - 1] - _._8 * zk[i] + _._8 * zk[i + 1] - xk[i + 1]) / h6;
                }
                M[n] = c5;
                k++;
            }
            Array.ForEach(u, t => t?.Dispose());
            Array.ForEach(zk, Z => Z?.Dispose());
            //Array.ForEach(zk_1, z => z?.Dispose());
            Array.ForEach(M, m => m?.Dispose());
        });
    }

    public bool SolutionIsGoodEnough(int k)
    {
        if (maxSteps != 0)
        {
            return maxSteps <= k;
        }

        BigFloat maxM = xk.Skip(1).Take(n - 2).Max(), maxM_1 = xk_1.Skip(1).Take(n - 2).Max();
        return (maxM - maxM_1).Abs() < epsilon;
    }

    public BigFloat F(BigFloat ti, BigFloat t)
    {
        return Green(ti, t) * f(t, g(t), g(fi(t)));
    }

    public BigFloat F(BigFloat ti, BigFloat t, BigFloat x, Func<BigFloat, BigFloat> spline)
    {
        return Green(ti, t) * f(t, x, spline(fi(t)));
    }

    public BigFloat g(BigFloat T)
    {
        return (c5 - _._3 * c4 + c3 / _._2 + _._2 * c2 + _._3 * c1) * T * T * T * T
            + (_._4 * c4 - c5 - c3 - _._3 * c2 - _._4 * c1) * T * T * T + c3 / _._2 * T * T + c2 * T + c1;
    }

    public BigFloat Green(BigFloat t, BigFloat s)
    {
        if (s <= t)
        {
            return H(t, s);
        }
        else
        {
            return K(t, s);
        }
    }

    public BigFloat H(BigFloat t, BigFloat s)
    {
        var _1 = new BigFloat(1);
        var _2 = new BigFloat(2);
        var _3 = new BigFloat(3);
        var _4 = new BigFloat(4);
        var _6 = new BigFloat(6);
        var _8 = new BigFloat(8);
        var _24 = new BigFloat(24);
        return s * s * (t - _1) * (t - _1) / _24
            * (_3 * s * s * t * t + _2 * s * s * t + s * s
                - _8 * s * t * t - _4 * s * t + _6 * t * t);
    }

    public BigFloat K(BigFloat t, BigFloat s)
    {
        var _1 = new BigFloat(1);
        var _3 = new BigFloat(3);
        var _4 = new BigFloat(4);
        var _24 = new BigFloat(24);
        return (s - _1) * (s - _1) * (s - _1) * t * t * t / _24
            * (t - _4 * s + _3 * t * s);
    }

    public void Dispose()
    {
        Array.ForEach(xk, x => x?.Dispose());
        //Array.ForEach(xk_1, x => x?.Dispose());
        Array.ForEach(t, t => t?.Dispose());
    }
}
