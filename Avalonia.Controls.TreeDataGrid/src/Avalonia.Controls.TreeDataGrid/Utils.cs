using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls;

public static class MathUtils
{
    internal const double DoubleEpsilon = 2.2204460492503131e-016;
    internal const float FloatEpsilon = 1.192092896e-07F;

    public static bool IsZero(double value)
    {
        return Math.Abs(value) < 2.2204460492503131E-15;
    }

    public static bool AreClose(double value1, double value2)
    {
        //in case they are Infinities (then epsilon check does not work)
        if (value1 == value2)
            return true;
        double eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DoubleEpsilon;
        double delta = value1 - value2;
        return (-eps < delta) && (eps > delta);
    }

    public static bool GreaterThan(double value1, double value2)
    {
        return (value1 > value2) && !AreClose(value1, value2);
    }
}
