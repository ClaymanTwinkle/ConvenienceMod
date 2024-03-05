using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace ConvenienceFrontend.MergeBookPanel
{
    public class PageView : Refers
    {
        // Token: 0x06000023 RID: 35 RVA: 0x000046B4 File Offset: 0x000028B4
        public void SetClickEvent(UnityAction onClick)
        {
            CButton component = base.GetComponent<CButton>();
            component.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                component.onClick.AddListener(onClick);
            }
        }

        // Token: 0x06000024 RID: 36 RVA: 0x000046E2 File Offset: 0x000028E2
        public void SetInteractable(bool interactable)
        {
            if (base.GetComponent<CButton>() != null)
            {
                base.GetComponent<CButton>().interactable = interactable;
            }
        }
    }
}
