using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Building;
using GameData.Domains.Map;
using GameData.Domains;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class BuildingFinder
    {
        public static List<BuildingBlockKey> FindBuildingsByType(Location location, BuildingAreaData buildingArea, EBuildingBlockType type)
        {
            List<BuildingBlockKey> list = new List<BuildingBlockKey>();
            for (short num = 0; num < buildingArea.Width * buildingArea.Width; num = (short)(num + 1))
            {
                BuildingBlockKey buildingBlockKey = new(location.AreaId, location.BlockId, num);
                BuildingBlockData element_BuildingBlocks = DomainManager.Building.GetElement_BuildingBlocks(buildingBlockKey);
                var buildingBlockItem = Config.BuildingBlock.Instance[element_BuildingBlocks.TemplateId];
                if (buildingBlockItem != null && buildingBlockItem.Type == type && element_BuildingBlocks.CanUse())
                {
                    list.Add(buildingBlockKey);
                }
            }

            return list;
        }
    }
}
