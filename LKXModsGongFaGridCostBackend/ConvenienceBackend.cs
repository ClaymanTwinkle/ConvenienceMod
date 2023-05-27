using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ConvenienceBackend.CombatStrategy;
using ConvenienceBackend.CustomSteal;
using ConvenienceBackend.CustomWeapon;
using ConvenienceBackend.ManualArchive;
using ConvenienceBackend.ModifyCombatSkill;
using ConvenienceBackend.NotNTR;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Global.Inscription;
using GameData.Domains.Map;
using GameData.Domains.Organization;
using GameData.Domains.SpecialEffect;
using GameData.Domains.World;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace ConvenienceBackend
{
    // Token: 0x02000002 RID: 2
    [PluginConfig("ConvenienceBackend", "kesar", "1.0.0")]
    public class ConvenienceBackend : TaiwuRemakePlugin
    {
        private List<BaseBackendPatch> allPatch = new List<BaseBackendPatch>()
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
            new ModifyCombatSkillBackendPatch()
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
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000021C8 File Offset: 0x000003C8
        public override void OnLoadedArchiveData()
        {

        }

        // Token: 0x04000001 RID: 1
        private Harmony harmony;

        // Token: 0x04000003 RID: 3
        public static bool bool_Toggle_Total;
    }

}
