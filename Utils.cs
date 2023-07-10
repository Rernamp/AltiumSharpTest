using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltiumSharpTest {
    static class Utils {

        public static bool IsWithin<T>(this T value, T minimum, T maximum) where T : IComparable<T> {
            if (minimum.CompareTo(maximum) > 0) {
                (minimum, maximum) = (maximum, minimum);
            }
            if (value.CompareTo(minimum) < 0)
                return false;
            if (value.CompareTo(maximum) > 0)
                return false;
            return true;
        }

    }
}
