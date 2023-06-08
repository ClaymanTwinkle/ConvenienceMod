using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ConvenienceFrontend
{
    internal abstract class BaseFrontPatch
    {
        public abstract void OnModSettingUpdate(string modIdStr);

        public virtual void Initialize(Harmony harmony, string modIdStr) { }

        public virtual void Dispose() { }

        public virtual void OnEnterNewWorld() { }

        public virtual void OnLoadedArchiveData() { }
    }
}
