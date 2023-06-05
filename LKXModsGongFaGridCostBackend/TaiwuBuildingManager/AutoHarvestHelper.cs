using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using GameData.Common;
using GameData.Domains.Building;
using GameData.Domains.Item;
using GameData.Domains;
using GameData.Domains.Map;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class AutoHarvestHelper
    {
        // 过月自动收获
        private static bool _enableAutoHarvest = false;
        // 过月自动购买物品
        private static bool _enableAutoBuy = false;
        // 过月自动招人才
        private static bool _enableAutoRecruit = false;

        public static void UpdateConfig(Dictionary<string, System.Object> config)
        {
            _enableAutoHarvest = (bool)config.GetValueOrDefault("Toggle_EnableAutoHarvest", false);
            _enableAutoBuy = (bool)config.GetValueOrDefault("Toggle_EnableAutoBuy", false);
            _enableAutoRecruit = (bool)config.GetValueOrDefault("Toggle_EnableAutoRecruit", false);
        }

        /// <summary>
        /// 过月自动收获
        /// </summary>
        /// <param name="context"></param>
        public static void HandleAutoHarvest(DataContext context)
        {
            if (!_enableAutoHarvest) return;
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            BuildingAreaData element_BuildingAreas = DomainManager.Building.GetElement_BuildingAreas(taiwuVillageLocation);
            for (short num = 0; num < (short)(element_BuildingAreas.Width * element_BuildingAreas.Width); num += 1)
            {
                BuildingBlockKey buildingBlockKey = new BuildingBlockKey(taiwuVillageLocation.AreaId, taiwuVillageLocation.BlockId, num);
                BuildingBlockData element_BuildingBlocks = DomainManager.Building.GetElement_BuildingBlocks(buildingBlockKey);
                if (element_BuildingBlocks.RootBlockIndex <= 0)
                {
                    BuildingBlockItem item = BuildingBlock.Instance.GetItem(element_BuildingBlocks.TemplateId);
                    if (item != null)
                    {
                        QuickCollectShopSoldItem(context, buildingBlockKey, item);

                        QuickCollectShopItem(context, taiwu, buildingBlockKey, element_BuildingBlocks, item);

                        QuickRecruitPeople(context, taiwu, buildingBlockKey, element_BuildingBlocks, item);
                    }
                }
            }
        }

        private static void QuickRecruitPeople(DataContext context, GameData.Domains.Character.Character taiwu, BuildingBlockKey blockKey, BuildingBlockData blockData, BuildingBlockItem configData)
        {
            List<int> charIdList = new List<int>();
            if (configData.SuccesEvent.Count != 0 && ShopEvent.Instance.GetItem(configData.SuccesEvent[0]).RecruitPeopleProb.Count > 0)
            {
                if (blockData.TemplateId == BuildingBlock.DefKey.ExcellentPersonShop)
                {
                    if (_enableAutoRecruit && taiwu.GetResource(7) >= 3000)
                    {
                        DomainManager.Building.RecruitPeopleQuick(context, blockKey, charIdList);
                        return;
                    }
                }
                else
                {
                    DomainManager.Building.RecruitPeopleQuick(context, blockKey, charIdList);
                }
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x0000225C File Offset: 0x0000045C
        private static void QuickCollectShopSoldItem(DataContext context, BuildingBlockKey blockKey, BuildingBlockItem configData)
        {
            if (configData.SuccesEvent.Count != 0 && ShopEvent.Instance.GetItem(configData.SuccesEvent[0]).exchangeResourceGoods != -1)
            {
                DomainManager.Building.ShopBuildingSoldItemReceiveQuick(context, blockKey);
            }
            if (configData.SuccesEvent.Count != 0 && ShopEvent.Instance.GetItem(configData.SuccesEvent[0]).ResourceGoods != -1)
            {
                DomainManager.Building.AcceptBuildingBlockCollectEarningQuick(context, blockKey, false);
            }
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000022D8 File Offset: 0x000004D8
        private static void QuickCollectShopItem(DataContext context, GameData.Domains.Character.Character taiwu, BuildingBlockKey blockKey, BuildingBlockData blockData, BuildingBlockItem configData)
        {
            List<ItemKey> itemKeyList = new List<ItemKey>();
            if (configData.SuccesEvent.Count != 0 && ShopEvent.Instance.GetItem(configData.SuccesEvent[0]).ItemList.Count > 0)
            {
                if (blockData.TemplateId == BuildingBlock.DefKey.Pawnshop)
                {
                    if (_enableAutoBuy && taiwu.GetResource(6) >= 3000)
                    {
                        DomainManager.Building.CollectItemQuick(context, blockKey, itemKeyList);
                        return;
                    }
                }
                else
                {
                    DomainManager.Building.CollectItemQuick(context, blockKey, itemKeyList);
                }
            }
        }
    }
}
