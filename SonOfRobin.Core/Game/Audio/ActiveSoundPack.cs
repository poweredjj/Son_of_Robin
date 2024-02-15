﻿using System;
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
            if (boardPiece.pieceInfo != null) this.template = boardPiece.pieceInfo.pieceSoundPackTemplate;
        }

        public object Serialize()
        {
            if (this.activeSoundDict.Count == 0) return null;

            var playingLoopsList = new List<object>();

            foreach (var kvp in this.activeSoundDict)
            {
                if (kvp.Value.isLooped && kvp.Value.IsPlaying) playingLoopsList.Add(kvp.Key);
            }

            return playingLoopsList.Count == 0 ? null : playingLoopsList;
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

        public void Play(PieceSoundPackTemplate.Action action, bool ignore3D = false, bool ignoreCooldown = false, int coolDownExtension = 0)
        {
            if (!this.activeSoundDict.ContainsKey(action))
            {
                Sound templateSound = this.template.GetSound(action);
                if (templateSound == null) return;

                this.activeSoundDict[action] = templateSound.MakeCopyForPiece(this.boardPiece);
            }

            this.activeSoundDict[action].Play(ignore3DThisPlay: ignore3D, ignoreCooldown: ignoreCooldown, coolDownExtension: coolDownExtension);
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