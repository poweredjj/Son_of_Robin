using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class ActiveSoundPack
    {
        private readonly Dictionary<PieceSoundPackTemplate.Action, Sound> activeSoundDict;
        private readonly BoardPiece boardPiece;
        private readonly PieceSoundPackTemplate template;

        public ActiveSoundPack(BoardPiece boardPiece)
        {
            this.boardPiece = boardPiece;
            this.activeSoundDict = new Dictionary<PieceSoundPackTemplate.Action, Sound>();
            this.template = boardPiece.pieceInfo.pieceSoundPackTemplate;
        }

        public object Serialize()
        {
            var playingLoopsList = new List<object>();

            foreach (var kvp in this.activeSoundDict)
            {
                PieceSoundPackTemplate.Action action = kvp.Key;
                Sound sound = kvp.Value;

                if (sound.isLooped && sound.IsPlaying) playingLoopsList.Add(action);
            }

            if (playingLoopsList.Count == 0) return null;
            return playingLoopsList;
        }

        public void Deserialize(Object soundPackData)
        {
            var playingLoopsList = (List<object>)soundPackData;

            foreach (object actionObj in playingLoopsList)
            {
                PieceSoundPackTemplate.Action action = (PieceSoundPackTemplate.Action)(Int64)actionObj;
                this.Play(action);
            }
        }

        public void Play(PieceSoundPackTemplate.Action action, bool ignore3D = false, bool ignoreCooldown = false)
        {
            if (!this.activeSoundDict.ContainsKey(action))
            {
                if (!this.template.staticSoundDict.ContainsKey(action)) return;
                this.activeSoundDict[action] = this.template.staticSoundDict[action].MakeCopyForPiece(this.boardPiece);
            }

            this.activeSoundDict[action].Play(ignore3DThisPlay: ignore3D, ignoreCooldown: ignoreCooldown);
        }

        public void Stop(PieceSoundPackTemplate.Action action)
        {
            if (!this.activeSoundDict.ContainsKey(action)) return;
            this.activeSoundDict[action].Stop();
        }

        public Sound GetPlayingSound(PieceSoundPackTemplate.Action action)
        {
            return this.activeSoundDict.ContainsKey(action) ? this.activeSoundDict[action] : null;
        }

        public bool IsPlaying(PieceSoundPackTemplate.Action action)
        {
            return this.activeSoundDict.ContainsKey(action) && this.activeSoundDict[action].IsPlaying;
        }

        public void StopAll(PieceSoundPackTemplate.Action ignoredAction)
        {
            foreach (var kvp in this.activeSoundDict)
            {
                if (kvp.Key != ignoredAction) kvp.Value.Stop();
            }
        }
    }
}