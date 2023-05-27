using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Mod;
using GameData.Domains;
using GameData.Domains.Global;
using HarmonyLib;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains.World;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;

namespace ConvenienceBackend.ManualArchive
{
    internal class ManualArchiveBackendPatch: BaseBackendPatch
    {
        /// <summary>
        /// 修改存档次数开关
        /// </summary>
        private static bool EnabledChangedBackupCount = true;

        /// <summary>
        /// 自动保存开关
        /// </summary>
        private static bool EnabledAutoSave = true;

        /// <summary>
        /// 存档个数
        /// </summary>
        private static sbyte BackupCount = 10;

        /// <summary>
        /// 自动保存时间间隔，单位：月
        /// </summary>
        private static int AutoSaveInterval = 1;

        /// <summary>
        /// 上次自动保存的时间
        /// </summary>
        public static int LastAutoSaveTime = 0;

        public override void OnModSettingUpdate(string ModIdStr)
        {
            DomainManager.Mod.GetSetting(ModIdStr, "Bool_EnabledChangedBackupCount", ref EnabledChangedBackupCount);
            int num = (int)BackupCount;
            DomainManager.Mod.GetSetting(ModIdStr, "Num_BackupCount", ref num);
            BackupCount = (sbyte)Math.Clamp(num, 1, 127);
            DomainManager.Mod.GetSetting(ModIdStr, "Bool_EnabledAutoSave", ref EnabledAutoSave);
            DomainManager.Mod.GetSetting(ModIdStr, "Num_AutoSaveInterval", ref AutoSaveInterval);
        }

        [HarmonyPatch(typeof(GlobalDomain), "ShouldMakeBackup")]
        [HarmonyPrefix]
        public static bool Prefix(ref sbyte __result)
        {
            bool enabledChangedBackupCount = EnabledChangedBackupCount;
            bool result;
            if (enabledChangedBackupCount)
            {
                __result = BackupCount;
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        [HarmonyPatch(typeof(WorldDomain), "AdvanceMonth_DisplayedMonthlyNotifications")]
        [HarmonyPrefix]
        public static void AdvanceMonth_DisplayedMonthlyNotifications_Prefix(DataContext context, ref bool saveWorld)
        {
            bool flag = saveWorld;
            if (flag)
            {
                bool flag2 = false;
                bool flag3 = !EnabledAutoSave;
                if (flag3)
                {
                    flag2 = true;
                }
                else
                {
                    bool flag4 = AutoSaveInterval > 1;
                    if (flag4)
                    {
                        flag2 = true;
                        LastAutoSaveTime++;
                        bool flag5 = LastAutoSaveTime >= AutoSaveInterval;
                        if (flag5)
                        {
                            LastAutoSaveTime = 0;
                            flag2 = false;
                        }
                    }
                }
                bool flag6 = flag2;
                if (flag6)
                {
                    WorldDomain.CheckSanity();
                    Events.RaiseBeforeSaveWorld(context);
                    AddSaveMonitoredData();
                    saveWorld = false;
                }
            }
        }

        // Token: 0x06000002 RID: 2 RVA: 0x000020D0 File Offset: 0x000002D0
        private static void AddSaveMonitoredData()
        {
            NotificationCollection pendingNotifications = GameDataBridge.GetPendingNotifications();
            RawDataPool dataPool = pendingNotifications.DataPool;
            List<Notification> notifications = pendingNotifications.Notifications;
            DataMonitorManager dataMonitorManager = AccessTools.Field(typeof(GameDataBridge), "DataMonitorManager").GetValue(null) as DataMonitorManager;
            HashSet<DataUid> source = (HashSet<DataUid>)AccessTools.Field(typeof(DataMonitorManager), "_monitoredData").GetValue(dataMonitorManager);
            DataUid dataUid = source.FirstOrDefault((DataUid x) => x.DomainId == 0 && x.DataId == 2);
            int num = Serializer.Serialize(true, dataPool);
            notifications.Add(Notification.CreateDataModification(dataUid, num));
            num = Serializer.Serialize(false, dataPool);
            notifications.Add(Notification.CreateDataModification(dataUid, num));
        }
    }
}
