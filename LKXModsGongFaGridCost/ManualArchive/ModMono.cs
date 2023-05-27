using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ConvenienceFrontend.ManualArchive
{
    internal class ModMono : MonoBehaviour
    {
        private void Awake()
        {
            Object.DontDestroyOnLoad(base.gameObject);
            Debug.Log("手动存档插件载入成功");
        }
        public void DelayInvoke(float f, Action action)
        {
            base.StartCoroutine(this.DelayInvokeCoroutine(f, action));
        }

        // Token: 0x06000020 RID: 32 RVA: 0x0000256D File Offset: 0x0000076D
        private IEnumerator DelayInvokeCoroutine(float f, Action action)
        {
            yield return new WaitForSeconds(f);
            action();
            yield break;
        }
    }
}
