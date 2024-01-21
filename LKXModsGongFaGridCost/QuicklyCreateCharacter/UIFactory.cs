using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Events;
using UnityEngine;
using FrameWork.ModSystem;
using ConvenienceFrontend.CombatStrategy;
using System.Runtime.Remoting.Contexts;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    internal class UIFactory
    {
        public static GameObject GetRollButtonGo(string buttonText, UnityAction onClick)
        {
            //GameObject gameObject = Object.Instantiate<GameObject>(UIFactory.GetButtonPrefab());
            //GameObject gameObject2 = Object.Instantiate<GameObject>(UIFactory.GetRollIcoPrefab());
            //gameObject.transform.Find("LabelBack/Label").GetComponent<TextMeshProUGUI>().text = buttonText;
            //UIFactory.ButtonAddOnClick(gameObject, onClick);
            //gameObject2.transform.SetParent(gameObject.transform, false);
            //gameObject2.transform.localPosition = gameObject.transform.Find("Icon").localPosition;
            //gameObject2.transform.localScale = Vector3.one * 0.5f;
            //Object.Destroy(gameObject.transform.Find("Icon").gameObject);
            var button = GameObjectCreationUtils.UGUICreateCButton(null, new Vector2(0, 0), new Vector2(200, 80), 25, buttonText);
            button.ClearAndAddListener(delegate () {
                onClick.Invoke();
            });
            return button.gameObject;
        }

        // Token: 0x06000041 RID: 65 RVA: 0x00005D00 File Offset: 0x00003F00
        private static GameObject GetRollIcoPrefab()
        {
            List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
            {
                UIElement.NewGame
            });
            GameObject gameObject = uielementPrefabs[0];
            Transform child = gameObject.transform.GetChild(2).GetChild(1).GetChild(1).GetChild(2).GetChild(2).GetChild(1).GetChild(0);
            UIFactory._rollIconPrefab = child.gameObject;
            return UIFactory._rollIconPrefab;
        }

        // Token: 0x06000042 RID: 66 RVA: 0x00005D74 File Offset: 0x00003F74
        public static GameObject GetBlankLabelGo()
        {
            bool flag = UIFactory._blankLabelPrefab == null;
            if (flag)
            {
                List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
                {
                    UIElement.CharacterMenuInfo
                });
                GameObject gameObject = uielementPrefabs[0];
                Transform child = gameObject.transform.GetChild(0).GetChild(4).GetChild(0).GetChild(0); // Camera_UIRoot/Canvas/LayerPopUp/UI_CharacterMenuInfo/ElementsRoot/AreaFeature/FeatureTtitle/Feature
                UIFactory._blankLabelPrefab = child.gameObject;
            }
            GameObject gameObject2 = Object.Instantiate<GameObject>(UIFactory._blankLabelPrefab);
            gameObject2.SetActive(true);
            return gameObject2;
        }

        // Token: 0x06000043 RID: 67 RVA: 0x00005DF8 File Offset: 0x00003FF8
        public static GameObject GetSubTitleGo(string titleName)
        {
            bool flag = UIFactory._subTitlePrefab == null;
            if (flag)
            {
                List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
                {
                    UIElement.CharacterMenuInfo
                });
                GameObject gameObject = uielementPrefabs[0];
                Transform child = gameObject.transform.GetChild(0).GetChild(4).GetChild(0); // Camera_UIRoot/Canvas/LayerPopUp/UI_CharacterMenuInfo/ElementsRoot/AreaFeature/FeatureTtitle
                UIFactory._subTitlePrefab = child.gameObject;
            }
            GameObject gameObject2 = Object.Instantiate<GameObject>(UIFactory._subTitlePrefab);
            gameObject2.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = titleName;
            gameObject2.name = titleName;
            gameObject2.SetActive(true);
            return gameObject2;
        }

        // Token: 0x06000044 RID: 68 RVA: 0x00005E98 File Offset: 0x00004098
        public static GameObject GetLifeQulificationGo(int type)
        {
            var text = GameObjectCreationUtils.UGUICreateTMPText(null, new Vector2(0, 0), new Vector2(200, 40), 18, "<b>" + CharacterDataTool.LifeSkillNameArray[type].ToString() + "</b>");
            text.gameObject.SetActive(true);
            text.gameObject.name = "lifeSkill" + type.ToString();

            return text.gameObject;

            bool flag = UIFactory._skillQulificationPrefab == null;
            if (flag)
            {
                List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
                {
                    UIElement.CharacterMenuLifeSkill
                });
                GameObject gameObject = uielementPrefabs[0];
                Transform child = gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2).GetChild(0).GetChild(3);
                UIFactory._skillQulificationPrefab = child.gameObject;
            }
            GameObject gameObject2 = Object.Instantiate<GameObject>(UIFactory._skillQulificationPrefab.gameObject);
            gameObject2.transform.GetChild(0).localPosition = new Vector2(-15f, 0f);
            gameObject2.transform.GetChild(2).localPosition = new Vector2(35f, 0f);
            gameObject2.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "<b>" + CharacterDataTool.LifeSkillNameArray[type].ToString() + "</b>";
            gameObject2.transform.GetChild(1).gameObject.SetActive(false);
            gameObject2.transform.GetChild(3).gameObject.SetActive(false);
            gameObject2.transform.GetChild(4).gameObject.SetActive(false);
            gameObject2.transform.GetChild(5).gameObject.SetActive(false);
            gameObject2.SetActive(true);
            gameObject2.name = "lifeSkill" + type.ToString();
            return gameObject2;
        }

        // Token: 0x06000045 RID: 69 RVA: 0x00006024 File Offset: 0x00004224
        public static GameObject GetCombatQulificationGo(int type)
        {
            var text = GameObjectCreationUtils.UGUICreateTMPText(null, new Vector2(0, 0), new Vector2(200, 40), 18, "<b>" + CharacterDataTool.CombatSkillNameArray[type].ToString() + "</b>");
            text.gameObject.SetActive(true);
            text.gameObject.name = "lifeSkill" + type.ToString();

            return text.gameObject;

            bool flag = UIFactory._skillQulificationPrefab == null;
            if (flag)
            {
                List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
                {
                    UIElement.CharacterMenuLifeSkill
                });
                GameObject gameObject = uielementPrefabs[0];
                Transform child = gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2).GetChild(0).GetChild(3);
                UIFactory._skillQulificationPrefab = child.gameObject;
            }
            GameObject gameObject2 = Object.Instantiate<GameObject>(UIFactory._skillQulificationPrefab);
            gameObject2.transform.GetChild(0).localPosition = new Vector2(-15f, 0f);
            gameObject2.transform.GetChild(2).localPosition = new Vector2(35f, 0f);
            gameObject2.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "<b>" + CharacterDataTool.CombatSkillNameArray[type].ToString() + "</b>";
            gameObject2.transform.GetChild(1).gameObject.SetActive(false);
            gameObject2.transform.GetChild(3).gameObject.SetActive(false);
            gameObject2.transform.GetChild(4).gameObject.SetActive(false);
            gameObject2.transform.GetChild(5).gameObject.SetActive(false);
            gameObject2.SetActive(true);
            gameObject2.name = "lifeSkill" + type.ToString();
            return gameObject2;
        }

        // Token: 0x06000046 RID: 70 RVA: 0x000061AC File Offset: 0x000043AC
        public static GameObject GetSkillBookGo()
        {
            var text = GameObjectCreationUtils.UGUICreateTMPText(null, new Vector2(0, 0), new Vector2(200, 40), 23, "技能课本");
            text.gameObject.name = "SkillBook";
            return text.gameObject;

            bool flag = UIFactory._skillBookPrefab == null;
            if (flag)
            {
                List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
                {
                    UIElement.CharacterMenuLifeSkill
                });
                GameObject gameObject = uielementPrefabs[0];
                Transform child = gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2).GetChild(0).GetChild(3);
                UIFactory._skillBookPrefab = child.gameObject;
            }
            GameObject gameObject2 = Object.Instantiate<GameObject>(UIFactory._skillBookPrefab.gameObject);
            gameObject2.transform.GetChild(0).localPosition = new Vector2(0f, 10f);
            gameObject2.transform.GetChild(2).localPosition = new Vector2(0f, -10f);
            gameObject2.transform.GetChild(0).localScale = Vector3.one * 0.6f;
            gameObject2.transform.GetChild(2).localScale = Vector3.one * 0.6f;
            gameObject2.transform.GetChild(0).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Midline;
            gameObject2.transform.GetChild(2).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Midline;
            gameObject2.transform.GetChild(1).gameObject.SetActive(false);
            gameObject2.transform.GetChild(3).gameObject.SetActive(false);
            gameObject2.transform.GetChild(4).gameObject.SetActive(false);
            gameObject2.transform.GetChild(5).gameObject.SetActive(false);
            gameObject2.transform.localScale = Vector3.one * 2f;
            gameObject2.SetActive(true);
            gameObject2.name = "SkillBook";
            return gameObject2;
        }

        // Token: 0x06000047 RID: 71 RVA: 0x00006390 File Offset: 0x00004590
        public static GameObject GetTotalTitleGo(string titleText = "标题")
        {
            List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
            {
                UIElement.Dialog
            });
            GameObject gameObject = uielementPrefabs[0];
            GameObject gameObject2 = gameObject.transform.Find("AnimRoot/ImgTitle36").gameObject;
            GameObject gameObject3 = Object.Instantiate<GameObject>(gameObject2);
            gameObject3.SetActive(true);
            gameObject3.name = "TotalTitle";
            gameObject3.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = titleText;
            return gameObject3;
        }

        // Token: 0x06000048 RID: 72 RVA: 0x00006414 File Offset: 0x00004614
        public static GameObject GetMainAttributeGo()
        {
            List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
            {
                UIElement.CharacterMenu
            });
            GameObject gameObject = uielementPrefabs[0];
            Transform child = gameObject.transform.GetChild(2).GetChild(0).GetChild(0);
            GameObject gameObject2 = Object.Instantiate<GameObject>(child.gameObject);
            gameObject2.SetActive(true);
            gameObject2.name = "MainAttribute";
            gameObject2.transform.Find("TabToggleGroup").gameObject.SetActive(false);
            gameObject2.transform.Find("TabAttribute").Find("AttackAttributeTitleBack").Find("DifficultyLayout_Penetrations").gameObject.SetActive(false);
            gameObject2.transform.Find("TabAttribute").Find("DefendAttributeTitleBack").Find("DifficultyLayout_PenetrationResists").gameObject.SetActive(false);
            gameObject2.transform.Find("TabAttribute").Find("HitAttributeTitleBack").Find("DifficultyLayout_HitValues").gameObject.SetActive(false);
            gameObject2.transform.Find("TabAttribute").Find("AvoidAttributeTitleBack").Find("DifficultyLayout_AvoidValues").gameObject.SetActive(false);
            gameObject2.transform.Find("TabAttribute").Find("MinorAttributeTitleBack").Find("DifficultyLayout_SecondAttribute").gameObject.SetActive(false);
            return gameObject2;
        }

        // Token: 0x06000049 RID: 73 RVA: 0x00006590 File Offset: 0x00004790
        public static GameObject GetFeatureScrollGo()
        {
            
            List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
            {
                UIElement.CharacterMenuInfo
            });

            GameObject gameObject = uielementPrefabs[0];
            Transform child = gameObject.transform.GetChild(0).GetChild(4).GetChild(3);
            return Object.Instantiate<GameObject>(gameObject.GetComponentInChildren<InfinityScroll>().gameObject);
        }

        // Token: 0x0600004A RID: 74 RVA: 0x000065EC File Offset: 0x000047EC
        public static GameObject GetTotalMedalGo()
        {
            List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
            {
                UIElement.CharacterMenuInfo
            });
            GameObject gameObject = uielementPrefabs[0];
            Transform child = gameObject.transform.GetChild(0).GetChild(4).GetChild(2);
            return Object.Instantiate<GameObject>(child.gameObject);
        }

        // Token: 0x0600004B RID: 75 RVA: 0x00006648 File Offset: 0x00004848
        public static GameObject GetCommonButtonGo(string buttonText, UnityAction onClick, bool hasIcon)
        {
            GameObject buttonPrefab = UIFactory.GetButtonPrefab();
            GameObject gameObject = Object.Instantiate<GameObject>(buttonPrefab);
            gameObject.transform.Find("LabelBack/Label").GetComponent<TextMeshProUGUI>().text = buttonText;
            bool flag = !hasIcon;
            if (flag)
            {
                Object.Destroy(gameObject.transform.Find("Icon").gameObject);
                gameObject.transform.Find("LabelBack").localPosition = Vector2.zero;
            }
            UIFactory.ButtonAddOnClick(gameObject, onClick);
            return gameObject;
        }

        // Token: 0x0600004C RID: 76 RVA: 0x000066D4 File Offset: 0x000048D4
        public static GameObject GetNewXCloseButtonGo(UnityAction onClick = null)
        {
            List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
            {
                UIElement.Dialog
            });
            GameObject gameObject = uielementPrefabs[0];
            GameObject gameObject2 = gameObject.transform.Find("AnimRoot/BtnNo").gameObject;
            GameObject gameObject3 = Object.Instantiate<GameObject>(gameObject2);
            gameObject3.SetActive(true);
            gameObject3.name = "xCloseButton";
            UIFactory.ButtonAddOnClick(gameObject3, onClick);
            return gameObject3;
        }

        // Token: 0x0600004D RID: 77 RVA: 0x00006744 File Offset: 0x00004944
        public static GameObject GetNewUIMaskGo(Color color)
        {
            List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
            {
                UIElement.SystemOption
            });
            GameObject gameObject = uielementPrefabs[0];
            Transform transform = gameObject.transform.Find("UIMask");
            GameObject gameObject2 = Object.Instantiate<GameObject>(transform.gameObject);
            gameObject2.SetActive(true);
            gameObject2.name = "UIMask";
            gameObject2.GetComponent<CRawImage>().color = color;
            return gameObject2;
        }

        // Token: 0x0600004E RID: 78 RVA: 0x000067B8 File Offset: 0x000049B8
        public static GameObject GetNewBackgroundContainerGoA()
        {
            bool flag = UIFactory._backgroundContainerGameObjectAPrefab == null;
            if (flag)
            {
                List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
                {
                    UIElement.NewGame
                });
                GameObject gameObject = uielementPrefabs[0];
                GameObject gameObject2 = gameObject.transform.Find("WindowRoot/NewGameBack/ScrollTabs/SettingView").gameObject;
                UIFactory._backgroundContainerGameObjectAPrefab = gameObject2;
            }
            GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(UIFactory._backgroundContainerGameObjectAPrefab);
            gameObject3.transform.Find("SettingBack").gameObject.SetActive(false);
            gameObject3.SetActive(true);
            return gameObject3;
        }

        // Token: 0x0600004F RID: 79 RVA: 0x0000684C File Offset: 0x00004A4C
        public static GameObject GetNewBackgroundContainerGoB()
        {
            bool flag = UIFactory._backgroundContainerGameObjectBPrefab == null;
            if (flag)
            {
                List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
                {
                    UIElement.CharacterMenu
                });
                GameObject gameObject = uielementPrefabs[0];
                GameObject gameObject2 = gameObject.transform.Find("SubPageBack").gameObject;
                UIFactory._backgroundContainerGameObjectBPrefab = gameObject2;
            }
            return UnityEngine.Object.Instantiate<GameObject>(UIFactory._backgroundContainerGameObjectBPrefab);
        }

        // Token: 0x06000050 RID: 80 RVA: 0x000068BC File Offset: 0x00004ABC
        private static GameObject GetButtonPrefab()
        {
            bool flag = UIFactory._buttonPrefab == null;
            if (flag)
            {
                List<GameObject> uielementPrefabs = UITool.GetUIElementPrefabs(new List<UIElement>
                {
                    UIElement.SystemOption
                });
                GameObject gameObject = uielementPrefabs[0];
                Transform transform = gameObject.transform.Find("MainWindow/ButtonHolder/ReturnToGame");
                bool flag2 = transform == null;
                if (flag2)
                {
                    transform = gameObject.transform.Find("MainWindow/ReturnToGame");
                }
                GameObject gameObject2 = transform.gameObject;
                UIFactory._buttonPrefab = gameObject2;
            }
            return UIFactory._buttonPrefab;
        }

        // Token: 0x06000051 RID: 81 RVA: 0x00006948 File Offset: 0x00004B48
        public static void ButtonAddOnClick(GameObject button, UnityAction onClick)
        {
            button.GetComponent<CButton>().onClick.RemoveAllListeners();
            bool flag = onClick != null;
            if (flag)
            {
                button.GetComponent<CButton>().onClick.AddListener(onClick);
            }
        }

        // Token: 0x04000049 RID: 73
        private static GameObject _backgroundContainerGameObjectAPrefab;

        // Token: 0x0400004A RID: 74
        private static GameObject _backgroundContainerGameObjectBPrefab;

        // Token: 0x0400004B RID: 75
        private static GameObject _skillBookPrefab;

        // Token: 0x0400004C RID: 76
        private static GameObject _buttonPrefab;

        // Token: 0x0400004D RID: 77
        private static GameObject _blankLabelPrefab;

        // Token: 0x0400004E RID: 78
        private static GameObject _subTitlePrefab;

        // Token: 0x0400004F RID: 79
        private static GameObject _skillQulificationPrefab;

        // Token: 0x04000050 RID: 80
        private static GameObject _rollIconPrefab;
    }
}
