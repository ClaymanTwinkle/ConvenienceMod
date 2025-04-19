using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Utilities;

namespace ConvenienceBackend.Utils
{
    public class CollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
    {
        public static TCollection Get()
        {
            return CollectionPool<TCollection, TItem>.s_Pool.Get();
        }

        public static void Release(TCollection toRelease)
        {
            toRelease.Clear();
            CollectionPool<TCollection, TItem>.s_Pool.Return(toRelease);
        }

        internal static readonly ObjectPool<TCollection> s_Pool = new(10, 10000);
    }
}
