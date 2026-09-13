using Roguelite.Core;
using UnityEngine;
using UnityEngine.UI;
namespace Roguelite
{
    public sealed class SettingsView : MonoBehaviour
    {
        private GameBootstrap session;
        private Text volume, status;
        private Button quieter, louder, retry;
        public void Initialize(GameBootstrap owner)
        {
            session = owner;
            volume = GameFlowView.TextAt(transform, "", 990, 20, 18); volume.rectTransform.sizeDelta = new Vector2(250, 30);
            quieter = GameFlowView.ButtonAt(transform, "Vol -", 990, 55, () => Adjust(-0.1f));
            louder = GameFlowView.ButtonAt(transform, "Vol +", 1105, 55, () => Adjust(0.1f));
            foreach (var button in new[] { quieter, louder })
            {
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(105, 40);
                button.GetComponentInChildren<Text>().rectTransform.sizeDelta = new Vector2(105, 40);
                button.GetComponentInChildren<Text>().fontSize = 18;
            }
            status = GameFlowView.TextAt(transform, "", 20, 655, 16); status.rectTransform.sizeDelta = new Vector2(980, 60);
            retry = GameFlowView.ButtonAt(transform, "Retry Save", 1030, 655, () => session.RetrySave());
        }
        private void Adjust(float amount) => session.SetMasterVolume(Mathf.Clamp01(session.Profile.settings.masterVolume + amount));
        private void Update()
        {
            if (session == null) return;
            bool settingsVisible = session.Flow.Phase == RunFlowPhase.MainMenu || session.Flow.Phase == RunFlowPhase.Hub || session.Pause.IsPaused;
            volume.gameObject.SetActive(settingsVisible); quieter.gameObject.SetActive(settingsVisible); louder.gameObject.SetActive(settingsVisible);
            volume.text = $"Master volume {session.Profile.settings.masterVolume:P0}";
            quieter.interactable = louder.interactable = !session.IsLoading;
            bool failed = !string.IsNullOrEmpty(session.SaveError);
            retry.gameObject.SetActive(failed); retry.interactable = !session.IsLoading;
            status.color = failed ? new Color(1, 0.65f, 0.3f) : Color.white;
            status.text = failed ? "Save failed; current progress remains in memory. Retry before quitting.\n" + session.SaveError :
                (string.IsNullOrEmpty(session.SaveNotice) ? "Meta progress and settings saved. Runs do not resume after quitting." : session.SaveNotice);
        }
    }
}
