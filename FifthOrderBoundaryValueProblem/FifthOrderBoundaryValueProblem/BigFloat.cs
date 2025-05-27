using System.Runtime.InteropServices;
using System.Text;

namespace FifthOrderBoundaryValueProblem;
public class BigFloat : IDisposable, IComparable
{
    private const string MPFR_LIB = "libmpfr-6"; // Windows: "mpfr.dll", Linux/macOS: "libmpfr.so"
    private const int MPFR_RNDN = 0; // Round to nearest

    private nint value; // Pointer to MPFR variable
    private int precision;

    // MPFR Interop Methods
    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_init2(nint x, int precision);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_set_str(nint rop, string op, int baseValue, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_add(nint rop, nint op1, nint op2, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_sub(nint rop, nint op1, nint op2, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_mul(nint rop, nint op1, nint op2, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_div(nint rop, nint op1, nint op2, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_exp(nint rop, nint op, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_pow(nint rop, nint baseVal, nint exponent, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_abs(nint rop, nint op, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mpfr_cmp(nint op1, nint op2);

    [DllImport("libmpfr-6.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpfr_sprintf(StringBuilder str, string format, nint r, int roundingMode);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern string mpfr_get_str(nint op, ref int exp, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern double mpfr_get_d(nint op, int rounding);

    [DllImport(MPFR_LIB, CallingConvention = CallingConvention.Cdecl)]
    private static extern void mpfr_clear(nint x);

    // Constructor accepting decimal value as string
    public BigFloat(decimal initialValue, int precisionBits = 128)
    {
        precision = precisionBits;
        value = Marshal.AllocHGlobal(16); // Allocate MPFR variable memory
        mpfr_init2(value, precisionBits);

        // Convert decimal to string and set precision
        string decimalStr = initialValue.ToString("G28"); // Ensures at least 28 digits of precision
        mpfr_set_str(value, decimalStr, 10, MPFR_RNDN); // base 10 for decimal
    }

    public double ToDouble()
    {
        return mpfr_get_d(value, MPFR_RNDN);
    }

    // Get the string representation of the value with full precision
    public string ToStringWithPrecision(int precision)
    {
        // Initialize a StringBuilder with enough space for the result
        StringBuilder str = new StringBuilder(256);

        // Call the mpfr_sprintf function to format the number
        int roundingMode = 0;  // 0 typically means rounding to nearest, change as needed
        mpfr_sprintf(str, "%." + precision + "Rf", value, roundingMode);

        return str.ToString();
    }

    public static BigFloat operator +(BigFloat a, BigFloat b)
    {
        BigFloat result = new BigFloat(0, a.precision);
        mpfr_add(result.value, a.value, b.value, MPFR_RNDN);
        return result;
    }

    public static BigFloat operator -(BigFloat a, BigFloat b)
    {
        BigFloat result = new BigFloat(0, a.precision);
        mpfr_sub(result.value, a.value, b.value, MPFR_RNDN);
        return result;
    }

    public static BigFloat operator *(BigFloat a, BigFloat b)
    {
        BigFloat result = new BigFloat(0, a.precision);
        mpfr_mul(result.value, a.value, b.value, MPFR_RNDN);
        return result;
    }

    public static BigFloat operator /(BigFloat a, BigFloat b)
    {
        BigFloat result = new BigFloat(0, a.precision);
        mpfr_div(result.value, a.value, b.value, MPFR_RNDN);
        return result;
    }

    public BigFloat Exp()
    {
        BigFloat result = new BigFloat(0, precision);
        mpfr_exp(result.value, value, MPFR_RNDN);
        return result;
    }

    public BigFloat Pow(BigFloat exponent, int precisionBits = 128)
    {
        BigFloat result = new BigFloat(0, precisionBits);
        mpfr_pow(result.value, exponent.value, value, MPFR_RNDN);

        exponent.Dispose();

        return result;
    }

    // Absolute value
    public BigFloat Abs()
    {
        BigFloat result = new BigFloat(0, precision);
        mpfr_abs(result.value, value, MPFR_RNDN);
        return result;
    }

    // Comparison operations
    public static bool operator ==(BigFloat a, BigFloat b)
    {
        return mpfr_cmp(a.value, b.value) == 0;
    }

    public static bool operator !=(BigFloat a, BigFloat b)
    {
        return mpfr_cmp(a.value, b.value) != 0;
    }

    public static bool operator <(BigFloat a, BigFloat b)
    {
        return mpfr_cmp(a.value, b.value) < 0;
    }

    public static bool operator >(BigFloat a, BigFloat b)
    {
        return mpfr_cmp(a.value, b.value) > 0;
    }

    public static bool operator <=(BigFloat a, BigFloat b)
    {
        return mpfr_cmp(a.value, b.value) <= 0;
    }

    public static bool operator >=(BigFloat a, BigFloat b)
    {
        return mpfr_cmp(a.value, b.value) >= 0;
    }

    public override bool Equals(object obj)
    {
        if (obj is BigFloat bigFloat)
        {
            return this == bigFloat;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public int CompareTo(object obj)
    {
        if (obj is BigFloat bigFloat)
        {
            return this < bigFloat ? -1 : Equals(bigFloat) ? 0 : 1;
        }
        return -2;
    }

    public void Dispose()
    {
        mpfr_clear(value);
        Marshal.FreeHGlobal(value);
    }

    public override string ToString()
    {
        return ToDouble().ToString();
    }
}
