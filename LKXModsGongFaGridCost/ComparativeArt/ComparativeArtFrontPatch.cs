using System;
using System.Collections.Generic;
using ConvenienceFrontend.Utils;
using FrameWork;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using TaiwuModdingLib.Core.Utils;
using UnityEngine;

namespace ConvenienceFrontend
{
    internal class ComparativeArtFrontPatch : BaseFrontPatch
    {
        private static bool _artAlwaysWin = false;
        private static bool _artDirectvictory = false;

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "Toggle_ArtAlwaysWin", ref _artAlwaysWin);
            ModManager.GetSetting(modIdStr, "Toggle_ArtDirectvictory", ref _artDirectvictory);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(DisplayEventHandler), "HandleDisplayEvents")]
        public static bool DisplayEventHandler_HandleDisplayEvents_Prefix(List<NotificationWrapper> notifications)
        {
            if (!_artDirectvictory) return true;

            int i = 0;
            int count = notifications.Count;
            while (i < count)
            {
                NotificationWrapper wrapper = notifications[i++];
                Notification notification = wrapper.Notification;
                if (notification.DisplayEventType == ((ushort)DisplayEventType.OpenLifeSkillCombat)) 
                {
                    int argsOffset = notification.ValueOffset;
                    RawDataPool dataPool = wrapper.DataPool;
                    int enemyId = 0;
                    argsOffset += Serializer.Deserialize(dataPool, argsOffset, ref enemyId);
                    sbyte lifeSkillType = -1;
                    Serializer.Deserialize(dataPool, argsOffset, ref lifeSkillType);

                    // SingletonObject.getInstance<WorldMapModel>().ChangeTaiwuMoveState(WorldMapModel.MoveState.WaitEventShow);
                    GameDataBridgeUtils.SendData<int, int>(12, 1970, 0, enemyId, null);
                    return false;
                }
            }

            return true;
        }
    }
}
