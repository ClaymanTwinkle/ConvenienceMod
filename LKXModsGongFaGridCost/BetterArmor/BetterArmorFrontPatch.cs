using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Utilities;
using HarmonyLib;
using TMPro;

namespace ConvenienceFrontend.BetterArmor
{
    internal class BetterArmorFrontPatch : BaseFrontPatch
    {
        private static bool _enableMod = false;

        public override void Dispose()
        {
            AdaptableLog.Info("更平衡的装备 前端 卸载");
        }

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "Toggle_EnableBetterArmor", ref _enableMod);
            ModManager.GetSetting(modIdStr, "ShowModification", ref this.showModification);
        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
            OnModSettingUpdate(modIdStr);
            if (!_enableMod) return;
            AdaptableLog.Info("更平衡的装备 前端 初始化开始");

            BetterArmor betterArmor = new BetterArmor(this.showModification);
            betterArmor.Modify();
            AdaptableLog.Info("更平衡的装备 前端 初始化结束");
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000020E8 File Offset: 0x000002E8
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TipsRefiningEffect), "SetData")]
        public static bool SetData(TipsRefiningEffect __instance, sbyte equipType, sbyte propertyType, int value, bool percent = true)
        {
            if (!_enableMod) return true;

            bool flag = false;
            string text = (value >= 0) ? "+" : "";
            string text2 = value.ToString();
            bool flag2 = equipType == 0 || equipType == 1;
            if (flag2)
            {
                bool flag3 = propertyType >= 0 && propertyType <= 5;
                if (flag3)
                {
                    flag = true;
                    text2 = (value / 10).ToString() + "." + (value % 10).ToString();
                }
                bool flag4 = propertyType == 4 || propertyType == 5;
                if (flag4)
                {
                    percent = false;
                }
            }
            string text3 = percent ? "%" : "";
            string text4 = flag ? " （更平衡的精制）" : "";
            __instance.CGet<CImage>("EffectIcon").SetSprite(BetterArmorFrontPatch.RefiningIconName[(int)equipType][(int)propertyType], false, null);
            __instance.CGet<TextMeshProUGUI>("Name").text = BetterArmorFrontPatch.RefiningPropertyName[(int)equipType][(int)propertyType];
            __instance.CGet<TextMeshProUGUI>("Value").text = string.Format("{0}{1}{2}{3}", new object[]
            {
                text,
                text2,
                text3,
                text4
            });
            return false;
        }

        // Token: 0x04000002 RID: 2
        private bool showModification;

        // Token: 0x04000003 RID: 3
        private static readonly string[][] RefiningPropertyName = new string[][]
        {
            new string[]
            {
                "力道",
                "精妙",
                "迅疾",
                "动心",
                "破甲",
                "坚韧",
                "攻击",
                "重量"
            },
            new string[]
            {
                "卸力",
                "拆招",
                "闪避",
                "守心",
                "破刃",
                "坚韧",
                "防御",
                "重量"
            },
            new string[]
            {
                "力道",
                "精妙",
                "迅疾",
                "动心",
                "卸力",
                "拆招",
                "闪避",
                "守心"
            }
        };

        // Token: 0x04000004 RID: 4
        private static readonly string[][] RefiningIconName = new string[][]
        {
            new string[]
            {
                "mousetip_mingzhong_1_0",
                "mousetip_mingzhong_1_1",
                "mousetip_mingzhong_1_2",
                "mousetip_mingzhong_1_3",
                "mousetip_jingzhi_3",
                "mousetip_jingzhi_2",
                "mousetip_jingzhi_0",
                "mousetip_jingzhi_5"
            },
            new string[]
            {
                "mousetip_huajie_0",
                "mousetip_huajie_1",
                "mousetip_huajie_2",
                "mousetip_huajie_3",
                "mousetip_jingzhi_4",
                "mousetip_jingzhi_2",
                "mousetip_jingzhi_1",
                "mousetip_jingzhi_5"
            },
            new string[]
            {
                "mousetip_mingzhong_1_2",
                "mousetip_mingzhong_1_0",
                "mousetip_mingzhong_1_3",
                "mousetip_mingzhong_1_1",
                "mousetip_huajie_0",
                "mousetip_huajie_1",
                "mousetip_huajie_2",
                "mousetip_huajie_3"
            }
        };
    }
}
