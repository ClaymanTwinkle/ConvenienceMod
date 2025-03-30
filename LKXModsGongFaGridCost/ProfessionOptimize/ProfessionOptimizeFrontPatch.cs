using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CustomWeapon;
using FrameWork;
using GameData.Domains.Item.Display;
using GameData.Domains.Item;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using UnityEngine;
using GameData.Domains.Extra;
using GameData.Domains.Taiwu.Profession;
using UnityEngine.UI;
using GameData.Domains.World;
using ConvenienceFrontend.CombatStrategy;
using System.Reflection;

namespace ConvenienceFrontend.ProfessionOptimize
{
    internal class ProfessionOptimizeFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }
    }
}
