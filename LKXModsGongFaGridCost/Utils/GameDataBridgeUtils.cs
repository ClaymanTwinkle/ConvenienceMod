using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CombatStrategy.config;
using GameData.GameDataBridge;
using GameData.Serializer;
using UnityEngine;

namespace ConvenienceFrontend.Utils
{
    internal class GameDataBridgeUtils
    {
        public static void SendData<T, S>(ushort domainId, ushort methodId, ushort flagId, T data, Action<S> callback = null)
        {
            var listenerId = -1;

            if (callback != null)
            {
                void OnNotifyGameData(List<NotificationWrapper> notifications)
                {
                    bool hasHandle = false;
                    foreach (NotificationWrapper notification2 in notifications)
                    {
                        Notification notification = notification2.Notification;

                        if (notification.Type == 1)
                        {
                            if (notification.DomainId == domainId && notification.MethodId == methodId)
                            {
                                System.Object keyValuePair = default;
                                Serializer.DeserializeDefault(notification2.DataPool, notification.ValueOffset, ref keyValuePair);

                                if (keyValuePair != null)
                                {
                                    if (keyValuePair is KeyValuePair<int, S>)
                                    {
                                        Debug.Log(((KeyValuePair<int, S>)keyValuePair).Key);
                                        if (((KeyValuePair<int, S>)keyValuePair).Key == flagId)
                                        {
                                            callback(((KeyValuePair<int, S>)keyValuePair).Value);
                                            hasHandle = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (hasHandle)
                    {
                        GameDataBridge.UnregisterListener(listenerId);
                    }
                }

                listenerId = GameDataBridge.RegisterListener(OnNotifyGameData);
            }
            GameDataBridge.AddMethodCall<ushort, T>(listenerId, domainId, methodId, flagId, data);
        }
    }
}
