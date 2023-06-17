using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ConvenienceBackend.BetterArmor;
using ConvenienceBackend.CombatStrategy;
using ConvenienceBackend.CustomSteal;
using ConvenienceBackend.CustomWeapon;
using ConvenienceBackend.ManualArchive;
using ConvenienceBackend.ModifyCombatSkill;
using ConvenienceBackend.NotNTR;
using ConvenienceBackend.QuicklyCreateCharacter;
using ConvenienceBackend.TaiwuBuildingManager;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Global;
using GameData.Domains.Global.Inscription;
using GameData.Domains.Map;
using GameData.Domains.Organization;
using GameData.Domains.SpecialEffect;
using GameData.Domains.Taiwu;
using GameData.Domains.World;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json;
using TaiwuModdingLib.Core.Plugin;

namespace ConvenienceBackend
{
    // Token: 0x02000002 RID: 2
    [PluginConfig("ConvenienceBackend", "kesar", "1.0.0")]
    public class ConvenienceBackend : TaiwuRemakePlugin
    {
        private static List<BaseBackendPatch> allPatch = new List<BaseBackendPatch>()
        {
            // sl偷窃、哄骗
            new CustomStealBackendPatch(),
            // 自动战斗
            new CombatStrategyBackendPatch(),
            // 拒绝NTR
            new NotNTRBackendPatch(),
            // 修改武器的式
            new CustomWeaponBackendPatch(),
            // 手动存档
            // new ManualArchiveBackendPatch(),
            // 修改功法
            new ModifyCombatSkillBackendPatch(),
            // 太吾村管家
            new TaiwuBuildingManagerBackendPatch(),
            // 
            new QuicklyCreateCharacterBackend(),
            // 平衡装备
            //new BetterArmorBackendPatch(),
        };

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public override void OnModSettingUpdate()
        {            
            DomainManager.Mod.GetSetting(ModIdStr, "Toggle_Total", ref bool_Toggle_Total);

            allPatch.ForEach((BaseBackendPatch patch) => patch.OnModSettingUpdate(ModIdStr));

        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(ConvenienceBackend), null);

            allPatch.ForEach((BaseBackendPatch patch) => this.harmony.PatchAll(patch.GetType()));
            allPatch.ForEach((BaseBackendPatch patch) => patch.Initialize(harmony, ModIdStr));
        }

        public override void Dispose()
        {
            allPatch.ForEach((BaseBackendPatch patch) => patch.Dispose());
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
            harmony = null;
        }

        // Token: 0x06000002 RID: 2 RVA: 0x000021B9 File Offset: 0x000003B9
        public override void OnEnterNewWorld()
        {
            allPatch.ForEach((BaseBackendPatch patch) => patch.OnEnterNewWorld());
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000021C8 File Offset: 0x000003C8
        public override void OnLoadedArchiveData()
        {
            allPatch.ForEach((BaseBackendPatch patch) => patch.OnLoadedArchiveData());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GlobalDomain), "CallMethod")]
        public static bool GlobalDomain_CallMethod_Prefix(TaiwuDomain __instance, Operation operation, RawDataPool argDataPool, DataContext context, ref int __result)
        {
            if (operation.MethodId == LOAD_CONFIG_METHOD_ID)
            {
                AdaptableLog.Info("CallMethod Load Config");
                string json = null;
                int num = operation.ArgsOffset;
                num += Serializer.Deserialize(argDataPool, num, ref json);
                Config = JsonConvert.DeserializeObject<Dictionary<string, System.Object>>(json);
                allPatch.ForEach((BaseBackendPatch patch) => patch.OnConfigUpdate(Config));
                __result = -1;
                return false;
            }

            return true;
        }

        // Token: 0x04000001 RID: 1
        private Harmony harmony;

        // Token: 0x04000003 RID: 3
        public static bool bool_Toggle_Total;

        private const int LOAD_CONFIG_METHOD_ID = 1994;

        public static Dictionary<string, System.Object> Config = new Dictionary<string, object>();
    }

}
