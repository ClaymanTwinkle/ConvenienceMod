using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ConvenienceBackend
{
    internal abstract class BaseBackendPatch
    {
        public abstract void OnModSettingUpdate(string modIdStr);

        public virtual void Initialize(Harmony harmony, string modIdStr) { }

        public virtual void Dispose() { }
    }
}
