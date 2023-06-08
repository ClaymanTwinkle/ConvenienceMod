using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains.Map;
using GameData.Domains;
using BehTree;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class ResidenceAutoLiveHelper
    {
        // 居所自动入住
        private static bool _enableResidenceAutoLive = false;
        // 厢房自动入住
        private static bool _enableWingRoomAutoLive = false;

        public static void UpdateConfig(Dictionary<string, System.Object> config)
        {
            _enableResidenceAutoLive = (bool)config.GetValueOrDefault("Toggle_EnableResidenceAutoLive", false);
            _enableWingRoomAutoLive = (bool)config.GetValueOrDefault("Toggle_EnableWingRoomAutoLive", false);
        }

        public static void AutoLive(DataContext context)
        {
            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);

            if (_enableWingRoomAutoLive)
            {
                DomainManager.Building.FindAllBuildingsWithSameTemplate(taiwuVillageLocation, buildingAreaData, Config.BuildingBlock.DefKey.ComfortableHouse).ForEach(x => {
                    DomainManager.Building.RemoveAllFromComfortableHouse(context, x);
                    DomainManager.Building.QuickFillComfortableHouse(context, x);
                });
            }

            if (_enableResidenceAutoLive)
            {
                DomainManager.Building.FindAllBuildingsWithSameTemplate(taiwuVillageLocation, buildingAreaData, Config.BuildingBlock.DefKey.Residence).ForEach(x => {
                    DomainManager.Building.QuickFillResidence(context, x);
                });
            }
        }
    }
}
