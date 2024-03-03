using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Character;
using GameData.Domains.Character.Ai;
using HarmonyLib;

namespace ConvenienceBackend.FixRedWords
{
    internal class FixRedWordsBackendPatch : BaseBackendPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FullName), "GetName")]
        public static void PostfixGetStartRelationSuccessRate_BoyOrGirlFriend(FullName __instance, sbyte gender, IReadOnlyDictionary<int, string> customTexts)
        {

        }
    }
}
