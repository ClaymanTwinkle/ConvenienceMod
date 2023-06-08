using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    internal static class UITool
    {
        // Token: 0x06000053 RID: 83 RVA: 0x0000698C File Offset: 0x00004B8C
        public static List<GameObject> GetUIElementPrefabs(List<UIElement> uiElementList)
        {
            string path = (string)AccessTools.Field(typeof(UIElement), "rootPrefabPath").GetValue(null);
            GameObject[] returnGoArray = new GameObject[uiElementList.Count];
            Dictionary<int, UIElement> dictionary = new Dictionary<int, UIElement>();
            for (int i = 0; i < uiElementList.Count; i++)
            {
                UIElement uielement = uiElementList[i];
                bool flag = UITool._uiElementPrefabDict.ContainsKey(uielement);
                if (flag)
                {
                    returnGoArray[i] = UITool._uiElementPrefabDict[uielement];
                }
                else
                {
                    dictionary.Add(i, uielement);
                }
            }
            bool flag2 = dictionary.Count > 0;
            if (flag2)
            {
                CountdownEvent countdownEvent = new CountdownEvent(dictionary.Count);
                using (Dictionary<int, UIElement>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        int index = enumerator.Current;
                        try
                        {
                            UIElement uiElement = dictionary[index];
                            string path2 = (string)AccessTools.Field(typeof(UIElement), "_path").GetValue(uiElement);
                            ResLoader.Load<GameObject>(Path.Combine(path, path2), delegate (GameObject newGameObject)
                            {
                                UITool._uiElementPrefabDict.Add(uiElement, newGameObject);
                                returnGoArray[index] = newGameObject;
                                countdownEvent.Signal();
                            }, null);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                            countdownEvent.Signal();
                        }
                    }
                }
                countdownEvent.Wait();
            }
            return new List<GameObject>(returnGoArray);
        }

        // Token: 0x06000054 RID: 84 RVA: 0x00006B84 File Offset: 0x00004D84
        public static Canvas GetRootCanvas(RectTransform rectTransform)
        {
            Canvas componentInParent = rectTransform.GetComponentInParent<Canvas>();
            bool flag = componentInParent != null;
            Canvas result;
            if (flag)
            {
                result = (componentInParent.isRootCanvas ? componentInParent : componentInParent.rootCanvas);
            }
            else
            {
                result = null;
            }
            return result;
        }

        // Token: 0x04000051 RID: 81
        private static Dictionary<UIElement, GameObject> _uiElementPrefabDict = new Dictionary<UIElement, GameObject>();
    }
}
