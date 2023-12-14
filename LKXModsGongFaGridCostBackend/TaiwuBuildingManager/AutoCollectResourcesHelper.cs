using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains.Map;
using GameData.Domains;
using GameData.Utilities;
using Newtonsoft.Json.Linq;
using GameData.Domains.TaiwuEvent;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class AutoCollectResourcesHelper
    {
        private static List<Location> _collectResourceLocations = new List<Location>();

        // 过月自动采资源
        private static bool _enableCollectResource = false;
        private static bool[] _IntCollectResourceType = new bool[6] { false, false, false, false, false, false };

        public static void UpdateConfig(Dictionary<string, System.Object> config)
        {
            _enableCollectResource = (bool)config.GetValueOrDefault("Toggle_EnableCollectResource", false);
            _IntCollectResourceType = ((JArray)config.GetValueOrDefault("Toggle_IntCollectResourceType", new JArray(false, false, false, false, false, false))).ToList().ConvertAll(x => (bool)x).ToArray();
        }

        public static void AssignIdlePeopleToCollectResources(DataContext context)
        {
            if (!_enableCollectResource) return;

            _collectResourceLocations.Clear();
            // 0 Food
            // 1 Wood
            // 2 Stone
            // 3 Jade
            // 4 Silk
            // 5 Herbal
            var resourceNames = new string[6] { "食材", "木材", "金铁", "玉石", "织物", "药材" };
            // 查找最少资源项
            sbyte collectResourceType = 0;
            for (sbyte i = 0; i < 6; i++)
            {
                if (DomainManager.Taiwu.GetTaiwu().GetResource(i) < DomainManager.Taiwu.GetTaiwu().GetResource(collectResourceType))
                {
                    collectResourceType = i;
                }
            }
            // 如果不是最小项，则选用户选择的
            for (sbyte i = 0; i < _IntCollectResourceType.Length; i++)
            {
                if (_IntCollectResourceType[i])
                {
                    collectResourceType = i;
                    break;
                }
            }

            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var allAreaBlocks = DomainManager.Map.GetAreaBlocks(taiwuVillageLocation.AreaId).ToArray().FindAll(x => x.CurrResources.Get(collectResourceType) > 0 && !DomainManager.Taiwu.TryGetElement_VillagerWorkLocations(x.GetLocation(), out var xx));
            allAreaBlocks.Sort((a, b) => b.CurrResources.Get(collectResourceType) - a.CurrResources.Get(collectResourceType));

            foreach (var mapBlockData in allAreaBlocks)
            {
                var charId = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true).FirstOrDefault(-1);
                AdaptableLog.Info("总共闲人数量" + DomainManager.Taiwu.GetAllVillagersAvailableForWork(true).Count);

                if (charId > -1)
                {
                    _collectResourceLocations.Add(mapBlockData.GetLocation());

                    var hasMark = DomainManager.Extra.TryGetElement_LocationMarkHashSet(mapBlockData.GetLocation(), out var ignore);
                    // 派遣工作
                    DomainManager.Taiwu.SetVillagerCollectResourceWork(context, charId, mapBlockData.AreaId, mapBlockData.BlockId, collectResourceType);
                    // 移动到目的地
                    GameData.Domains.Character.Character character = DomainManager.Character.GetElement_Objects(charId);
                    Location srcLocation = character.GetLocation();
                    if (srcLocation.AreaId != mapBlockData.AreaId || srcLocation.BlockId != mapBlockData.BlockId)
                    {
                        DomainManager.Character.LeaveGroup(context, character, true);
                        DomainManager.Character.GroupMove(context, character, mapBlockData.GetLocation());
                    }
                    // 不要标记
                    if (!hasMark)
                    {
                        DomainManager.Extra.RemoveLocationMark(context, mapBlockData.GetLocation());
                    }
                }
                else
                {
                    break;
                }
            }
            AdaptableLog.Info("派遣闲人采资源团" + _collectResourceLocations.Count + "人，去采集" + resourceNames[collectResourceType] + "，预计能采集到" + DomainManager.Taiwu.CalcResourceChangeByVillageWork(context)[collectResourceType]);
        }

        public static void DemobilizePeopleToCollectResources(DataContext context)
        {
            if (!_enableCollectResource) return;

            foreach (var location in _collectResourceLocations)
            {
                DomainManager.Taiwu.StopVillagerWorkOptional(context, location.AreaId, location.BlockId, 10, true);
            }

            AdaptableLog.Info("遣散闲人采资源团" + _collectResourceLocations.Count + "人");
            _collectResourceLocations.Clear();
        }
    }
}
