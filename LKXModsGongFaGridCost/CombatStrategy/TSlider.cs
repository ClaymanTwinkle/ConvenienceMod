using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ConvenienceFrontend.CombatStrategy
{
    public class TSlider : Slider
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            AudioManager.Instance.PlaySound(this.ClickAudioKey, false);
        }

        // Token: 0x06000002 RID: 2 RVA: 0x0000206D File Offset: 0x0000026D
        public void SetWithoutNotify(float input)
        {
            this.Set(input, false);
        }

        // Token: 0x06000003 RID: 3 RVA: 0x0000207C File Offset: 0x0000027C
        protected override void Set(float input, bool sendCallback)
        {
            base.Set(input, sendCallback);
            bool flag = this.label != null;
            if (flag)
            {
                this.label.text = (input / (float)this.Divider).ToString(this.format);
            }
        }

        // Token: 0x04000001 RID: 1
        public string ClickAudioKey;

        // Token: 0x04000002 RID: 2
        public TextMeshProUGUI label;

        // Token: 0x04000003 RID: 3
        public int Divider;

        // Token: 0x04000004 RID: 4
        public string format;
    }
}
