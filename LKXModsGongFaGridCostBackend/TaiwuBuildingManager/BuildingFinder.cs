using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Building;
using GameData.Domains.Map;
using GameData.Domains;
using System.Reflection;
using TaiwuModdingLib.Core.Utils;
using GameData.Utilities;
using Config.Common;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class BuildingFinder
    {
        /// <summary>
        /// 通过类型查找建筑
        /// </summary>
        /// <param name="location"></param>
        /// <param name="buildingArea"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取拆除建筑后可以获取到多少资源
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public static int[] GetRemoveOperationResReturn(BuildingBlockData blockData)
        {
            int[] returnRes = new int[8];

            var buildingBlockItem = Config.BuildingBlock.Instance[blockData.TemplateId];

            int addCostPerLevel = buildingBlockItem.AddBuildCostPerLevel;
            int resReturnPercent = buildingBlockItem.RemoveGetResourcePercent;
            sbyte buildingLevel = blockData.Level;


            for (sbyte type = 0; type < 8; type += 1)
            {
                ushort baseCost = buildingBlockItem.BaseBuildCost[(int)type];
                if (baseCost > 0)
                {
                    int returnCount2 = (int)DomainManager.Building.CallMethod("GetRemoveOperationResReturn", BindingFlags.Instance | BindingFlags.NonPublic, baseCost, addCostPerLevel, resReturnPercent, buildingLevel);
                    returnRes[type] = returnCount2;
                }
            }

            return returnRes;
        }

        /// <summary>
        /// 检查要添加的资源数量是否会导致超重
        /// </summary>
        /// <param name="resoureType"></param>
        /// <param name="addNum"></param>
        /// <returns></returns>
        public static bool CheckAddResourceIsOverload(sbyte resoureType, int addNum)
        {
            return DomainManager.Taiwu.GetTaiwu().GetResource(resoureType) + addNum >= DomainManager.Taiwu.GetMaterialResourceMaxCount();
        }
    }
}
