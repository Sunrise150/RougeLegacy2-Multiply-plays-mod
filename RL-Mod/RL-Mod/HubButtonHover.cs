using UnityEngine;
using UnityEngine.EventSystems;
using HarmonyLib;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.9.4：联机中心主菜单按钮的悬停高亮控制器。
    /// 游戏原生 MainMenuButton 只实现了 OnPointerEnter(点亮) 却没有 OnPointerExit(熄灭)，
    /// 且它的 OnSelect 已被 MainMenuHubButton 屏蔽（避免打乱键盘导航）。
    /// 这里给克隆按钮挂这个组件，自己管理 m_selectedIndicator：进入点亮、退出熄灭，
    /// 从而实现"悬停有变化、移开恢复"的正常反馈。
    /// </summary>
    public class HubButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private GameObject _indicator;
        private MainMenuButton _btn;

        private void Awake()
        {
            _btn = GetComponent<MainMenuButton>();
            if (_btn != null)
            {
                _indicator = Traverse.Create(_btn).Field("m_selectedIndicator").GetValue<GameObject>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_indicator != null) _indicator.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_indicator != null) _indicator.SetActive(false);
        }

        private void OnDisable()
        {
            if (_indicator != null) _indicator.SetActive(false);
        }
    }
}
