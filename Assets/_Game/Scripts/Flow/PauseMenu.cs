using Roguelite.Core;
using UnityEngine;
using UnityEngine.UI;
namespace Roguelite
{
    public sealed class PauseMenu : MonoBehaviour
    {
        private GameBootstrap session;
        private Button pause;
        private GameObject panel;
        public void Initialize(GameBootstrap owner)
        {
            session = owner;
            pause = GameFlowView.ButtonAt(transform, "Pause", 1040, 120, () => session.Pause.Pause());
            panel = new GameObject("Pause overlay", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(transform, false);
            var rect = panel.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.025f, 0.04f, 0.06f, 0.95f);
            GameFlowView.TextAt(panel.transform, "PAUSED", 40, 50, 36);
            GameFlowView.TextAt(panel.transform, "Combat, movement and cooldowns are frozen.\nChange master volume above, or resume your run.", 40, 130, 22);
            GameFlowView.ButtonAt(panel.transform, "Resume", 40, 240, () => session.Pause.Resume());
        }
        private void Update()
        {
            if (session == null) return;
            pause.gameObject.SetActive(session.Flow.Phase == RunFlowPhase.Dungeon && !session.Pause.IsPaused);
            pause.interactable = !session.IsLoading;
            panel.SetActive(session.Pause.IsPaused);
        }
    }
}
