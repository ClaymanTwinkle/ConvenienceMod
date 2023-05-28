using System.Collections.Generic;
using System.Runtime.InteropServices;
using ConvenienceFrontend.CombatStrategy;
using ConvenienceFrontend.CustomSteal;
using ConvenienceFrontend.CustomWeapon;
using ConvenienceFrontend.ManualArchive;
using ConvenienceFrontend.ModifyCombatSkill;
using ConvenienceFrontend.RollCreateRole;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace ConvenienceFrontend
{
    [PluginConfig("ConvenienceFrontend", "kesar", "1.0.0")]
    public class ConvenienceFrontend : TaiwuRemakePlugin
    {
        private List<BaseFrontPatch> allPatch = new List<BaseFrontPatch>()
        {
            // 较艺必胜
            new ComparativeArtFrontPatch(),
            // 自动战斗
            new CombatStrategyMod(),
            // 自定义偷窃
            new CustomStealFrontPatch(),
            // 修改武器的式
            new CustomWeaponFrontPatch(),
            // 手动存档
            // new ManualArchiveFrontendPatch(),
            // 修改功法
            new ModifyCombatSkillFrontPatch(),
            // roll角色属性
            // new RollCreateRoleFrontPatch()
        };

        public override void OnModSettingUpdate()
        {
            ModManager.GetSetting(base.ModIdStr, "Toggle_Total", ref ConvenienceFrontend.bool_Toggle_Total);

            allPatch.ForEach((BaseFrontPatch patch) => patch.OnModSettingUpdate(base.ModIdStr));
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002069 File Offset: 0x00000269
        public override void Initialize()
        {
            this.harmony = Harmony.CreateAndPatchAll(typeof(ConvenienceFrontend), null);

            allPatch.ForEach((BaseFrontPatch patch) => this.harmony.PatchAll(patch.GetType()));
            allPatch.ForEach((BaseFrontPatch patch) => patch.Initialize(harmony, base.ModIdStr));
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002088 File Offset: 0x00000288
        public override void Dispose()
        {
            allPatch.ForEach((BaseFrontPatch patch) => patch.Dispose());

            if (this.harmony != null)
            {
                this.harmony.UnpatchSelf();
            }

            this.harmony = null;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000020F0 File Offset: 0x000002F0
        public override void OnLoadedArchiveData()
        {
        }

        // Token: 0x04000001 RID: 1
        private Harmony harmony;

        // Token: 0x04000002 RID: 2
        private static bool bool_Toggle_Total;
    }
}
