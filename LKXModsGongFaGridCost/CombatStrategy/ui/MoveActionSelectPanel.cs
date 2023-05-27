using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CombatStrategy.config.data;
using UnityEngine;

namespace ConvenienceFrontend.CombatStrategy.ui
{
    public class MoveActionSelectPanel
    {
        private RectTransform _moveActionSelectPanel;

        public static MoveActionSelectPanel Create(Transform parent) { return new MoveActionSelectPanel(parent); }

        private MoveActionSelectPanel(Transform parent)
        {
            _moveActionSelectPanel = UIUtils.CreateOneValueOptionsPanel(parent).GetComponent<RectTransform>();
        }

        public void Show(Refers parentRefers, Strategy strategy, Action onConfirm)
        {
            var parent = parentRefers.transform;
            Vector3 vector = UIManager.Instance.UiCamera.WorldToScreenPoint(parent.position);
            this._moveActionSelectPanel.position = UIManager.Instance.UiCamera.ScreenToWorldPoint(vector);
            this._moveActionSelectPanel.anchoredPosition += new Vector2(40f, -50f);
            this._moveActionSelectPanel.gameObject.SetActive(true);
            this._moveActionSelectPanel.parent.gameObject.SetActive(true);
            Refers refers = this._moveActionSelectPanel.gameObject.GetComponent<Refers>();

            var valueOptions = refers.CGet<CDropdown>("ValueOptions");
            valueOptions.ClearOptions();
            valueOptions.AddOptions(StrategyConst.MoveActionOptions.ToList());

            valueOptions.value = strategy.teammateCommandAction?.id ?? valueOptions.value;

            refers.CGet<CButton>("Confirm").ClearAndAddListener(delegate ()
            {
                var value = valueOptions.value;

                strategy.type = (short)StrategyConst.StrategyType.AutoMove;
                strategy.SetAction(new AutoMoveAction(value));

                onConfirm();

                this._moveActionSelectPanel.gameObject.SetActive(false);
                this._moveActionSelectPanel.parent.gameObject.SetActive(false);
            });
            refers.CGet<CButton>("Cancel").ClearAndAddListener(delegate ()
            {
                this._moveActionSelectPanel.gameObject.SetActive(false);
                this._moveActionSelectPanel.parent.gameObject.SetActive(false);
            });
        }
    }
}
