using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.MergeBookPanel
{
    public static class MyMath
    {
        // Token: 0x0600006C RID: 108 RVA: 0x00006C55 File Offset: 0x00004E55
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
            {
                return min;
            }
            if (val.CompareTo(max) > 0)
            {
                return max;
            }
            return val;
        }
    }
}
