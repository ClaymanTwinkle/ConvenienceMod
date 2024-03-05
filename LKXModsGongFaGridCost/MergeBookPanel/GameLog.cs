using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ConvenienceFrontend.MergeBookPanel
{
    public static class GameLog
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public static void LogMessage(string message)
        {
            Debug.Log(message);
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
        public static void LogMessage<T>(List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                StringBuilder stringBuilder = sb;
                T t = list[i];
                stringBuilder.Append(t.ToString() + ", ");
            }
            Debug.Log(sb.ToString());
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000020B0 File Offset: 0x000002B0
        public static void LogMessage<T>(T[] list)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                sb.Append(list[i].ToString() + ", ");
            }
            Debug.Log(sb.ToString());
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002101 File Offset: 0x00000301
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002109 File Offset: 0x00000309
        public static void LogError(string message)
        {
            Debug.LogWarning(message);
        }
    }
}
