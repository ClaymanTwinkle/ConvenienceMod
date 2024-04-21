using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using GameData.GameDataBridge;
using HarmonyLib;
using UnityEngine;

namespace ConvenienceFrontend
{
    internal class ResetResolutionFrontPatch : BaseFrontPatch
    {
        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
            GEvent.Add(EEvents.OnGameResourceReady, OnGameResourceReady);
        }

        public override void OnModSettingUpdate(string modIdStr)
        {
            
        }

        private void OnGameResourceReady(ArgumentBox argBox)
        {
            GEvent.Remove(EEvents.OnGameResourceReady, OnGameResourceReady);

            SingletonObject.getInstance<GlobalSettings>().Resolution = new Vector2Int(2560, 1440);
        }
    }
}
