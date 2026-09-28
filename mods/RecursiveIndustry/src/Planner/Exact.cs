using System;
using System.Globalization;
using System.Numerics;

namespace RecursiveIndustry.Planner;

internal readonly struct Exact : IComparable<Exact>, IEquatable<Exact>
{
    private readonly BigInteger _numerator;
    private readonly BigInteger _denominator;
    internal BigInteger Numerator => _numerator;
    internal BigInteger Denominator => _denominator.IsZero ? BigInteger.One : _denominator;
    internal static Exact Zero => default;
    internal static Exact One => new(1);

    internal Exact(BigInteger numerator) : this(numerator, BigInteger.One) { }

    internal Exact(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero) throw new DivideByZeroException();
        if (denominator.Sign < 0) { numerator = -numerator; denominator = -denominator; }
        BigInteger divisor = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
        _numerator = numerator / divisor;
        _denominator = denominator / divisor;
    }

    internal static bool TryParse(string text, out Exact value)
    {
        value = Zero;
        if (string.IsNullOrWhiteSpace(text) || text.Length > 64) return false;
        string[] parts = text.Trim().Split('/');
        if (parts.Length == 2)
        {
            if (!BigInteger.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out BigInteger numerator)
                || !BigInteger.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out BigInteger denominator)
                || denominator.IsZero) return false;
            value = new Exact(numerator, denominator);
            return true;
        }
        if (parts.Length != 1 || !decimal.TryParse(parts[0], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out decimal number)) return false;
        int[] bits = decimal.GetBits(number);
        BigInteger magnitude = (new BigInteger((uint)bits[2]) << 64) + (new BigInteger((uint)bits[1]) << 32) + (uint)bits[0];
        value = new Exact(bits[3] < 0 ? -magnitude : magnitude, BigInteger.Pow(10, (bits[3] >> 16) & 255));
        return true;
    }

    internal int Ceiling()
    {
        BigInteger whole = BigInteger.DivRem(Numerator, Denominator, out BigInteger remainder);
        return checked((int)(remainder.Sign > 0 ? whole + 1 : whole));
    }

    internal string Display() => ((double)Numerator / (double)Denominator).ToString("0.###", CultureInfo.InvariantCulture);
    public override string ToString() => Denominator.IsOne ? Numerator.ToString(CultureInfo.InvariantCulture)
        : Numerator.ToString(CultureInfo.InvariantCulture) + "/" + Denominator.ToString(CultureInfo.InvariantCulture);
    public int CompareTo(Exact other) => (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
    public bool Equals(Exact other) => Numerator == other.Numerator && Denominator == other.Denominator;
    public override bool Equals(object other) => other is Exact exact && Equals(exact);
    public override int GetHashCode() => Numerator.GetHashCode() ^ Denominator.GetHashCode();
    internal static Exact Min(Exact left, Exact right) => left < right ? left : right;
    internal static Exact Max(Exact left, Exact right) => left > right ? left : right;
    public static implicit operator Exact(int value) => new(value);
    public static implicit operator Exact(long value) => new(value);
    public static Exact operator +(Exact left, Exact right) => new(left.Numerator * right.Denominator + right.Numerator * left.Denominator, left.Denominator * right.Denominator);
    public static Exact operator -(Exact left, Exact right) => new(left.Numerator * right.Denominator - right.Numerator * left.Denominator, left.Denominator * right.Denominator);
    public static Exact operator *(Exact left, Exact right) => new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);
    public static Exact operator /(Exact left, Exact right) => new(left.Numerator * right.Denominator, left.Denominator * right.Numerator);
    public static bool operator ==(Exact left, Exact right) => left.Equals(right);
    public static bool operator !=(Exact left, Exact right) => !left.Equals(right);
    public static bool operator <(Exact left, Exact right) => left.CompareTo(right) < 0;
    public static bool operator >(Exact left, Exact right) => left.CompareTo(right) > 0;
    public static bool operator <=(Exact left, Exact right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Exact left, Exact right) => left.CompareTo(right) >= 0;
}