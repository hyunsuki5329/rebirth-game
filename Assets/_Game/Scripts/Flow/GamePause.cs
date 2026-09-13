using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class GamePause : MonoBehaviour
    {
        private GameBootstrap session;
        private float previousScale;
        private bool previousAudioPause;
        public bool IsPaused { get; private set; }
        public void Initialize(GameBootstrap owner) => session = owner;
        public bool Pause()
        {
            if (IsPaused || session == null || session.IsLoading || session.Flow.Phase != RunFlowPhase.Dungeon ||
                session.Dungeon == null || session.Dungeon.Player.IsDead) return false;
            previousScale = Time.timeScale; previousAudioPause = AudioListener.pause;
            IsPaused = true; session.Dungeon.Player.GetComponent<PlayerInputReader>().SetBlocked(true);
            Time.timeScale = 0; AudioListener.pause = true; return true;
        }
        public bool Resume()
        {
            if (!IsPaused) return false;
            Time.timeScale = previousScale; AudioListener.pause = previousAudioPause; IsPaused = false;
            if (session != null && session.Flow.Phase == RunFlowPhase.Dungeon && session.Dungeon != null && !session.Dungeon.Player.IsDead)
                session.Dungeon.Player.GetComponent<PlayerInputReader>().SetBlocked(false);
            return true;
        }
        private void Update()
        {
            if (IsPaused && (session == null || session.IsLoading || session.Flow.Phase != RunFlowPhase.Dungeon || session.Dungeon.Player.IsDead)) Resume();
        }
        private void OnDisable() => Resume();
    }
}
