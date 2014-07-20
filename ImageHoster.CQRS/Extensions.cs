using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.CQRS.Domain
{
    public static class Extensions
    {
        public static bool IsSubsetOf<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            return !a.Except(b).Any();
        }
    }
}