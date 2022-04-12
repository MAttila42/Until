using System;
using System.Collections.Generic;

namespace Until.Services
{
    static class RandomExtensions
    {
        public static void Shuffle<T>(this Random r, ref List<T> l)
        {
            int n = l.Count;
            while (n > 1)
            {
                int k = r.Next(n--);
                T temp = l[n];
                l[n] = l[k];
                l[k] = temp;
            }
        }
    }
}
