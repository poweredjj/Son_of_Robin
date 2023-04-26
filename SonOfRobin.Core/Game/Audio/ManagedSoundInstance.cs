using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class ManagedSoundInstance
    {
        private static readonly Dictionary<SoundData.Name, List<ManagedSoundInstance>> instanceListsByName = new Dictionary<SoundData.Name, List<ManagedSoundInstance>>();
        private static readonly Dictionary<string, ManagedSoundInstance> activeInstancesBySoundID = new Dictionary<string, ManagedSoundInstance>();
        public static int CreatedInstancesCount { get; private set; } = 0;

        private readonly SoundData.Name soundName;
        private readonly SoundEffectInstance instance; // should not be publicly exposed, to avoid uncontrolled Play()
        private DateTime lastPlayed;

        private string currentSoundID;

        public ManagedSoundInstance(SoundData.Name soundName)
        {
            this.soundName = soundName;
            this.instance = SoundData.soundsDict[soundName].CreateInstance();
            this.lastPlayed = DateTime.Now; // to have a proper value
            this.currentSoundID = null;

            if (!instanceListsByName.ContainsKey(soundName)) instanceListsByName[soundName] = new List<ManagedSoundInstance>();
            instanceListsByName[soundName].Add(this);

            CreatedInstancesCount++;
        }

        public bool IsObsolete
        { get { return this.instance.State == SoundState.Stopped && (DateTime.Now - this.lastPlayed > TimeSpan.FromMinutes(2)); } }

        private void Delete()
        {
            this.instance.Dispose();
            if (this.currentSoundID != null && activeInstancesBySoundID.ContainsKey(this.currentSoundID)) activeInstancesBySoundID.Remove(this.currentSoundID);
            instanceListsByName[this.soundName].Remove(this);
            CreatedInstancesCount--;
        }

        private void AssignSoundID(string soundID)
        {
            if (activeInstancesBySoundID.ContainsKey(soundID) && activeInstancesBySoundID[soundID] != this) activeInstancesBySoundID[soundID].Stop();

            activeInstancesBySoundID[soundID] = this;
            this.currentSoundID = soundID;
        }

        private void ClearSoundID()
        {
            if (this.currentSoundID == null) return;
            activeInstancesBySoundID.Remove(this.currentSoundID);
            this.currentSoundID = null;
        }

        public bool Play(string soundID)
        {
            if (this.currentSoundID != soundID) this.AssignSoundID(soundID);

            try
            {
                this.instance.Play();
                this.lastPlayed = DateTime.Now;
                return true;
            }
            catch (InstancePlayLimitException)
            {
                bool instanceStopped = StopOldestPlayingInstance();
                if (instanceStopped)
                {
                    try
                    {
                        instance.Play();
                        MessageLog.AddMessage(msgType: MsgType.Debug, message: "Instance stopped correctly.");
                        return true;
                    }
                    catch (InstancePlayLimitException)
                    { }
                }
            }

            return false;
        }

        public static bool StopOldestPlayingInstance()
        {
            CleanUpActiveInstances();
            ManagedSoundInstance oldestPlayingInstance = GetOldestPlayingInstance();
            if (oldestPlayingInstance != null) oldestPlayingInstance.Stop();

            return oldestPlayingInstance != null;
        }

        public static ManagedSoundInstance GetOldestPlayingInstance()
        {
            var activeInstancesByLastPlayed = activeInstancesBySoundID.OrderBy(x => x.Value.lastPlayed).Select(x => x.Value);
            foreach (ManagedSoundInstance managedSoundInstance in activeInstancesByLastPlayed)
            {
                if (managedSoundInstance.instance.State == SoundState.Playing) return managedSoundInstance;
            }

            return null;
        }

        public static void Update()
        {
            CleanUpActiveInstances();
            DeleteInactiveInstances();
        }

        private static void CleanUpActiveInstances()
        {
            foreach (var kvp in activeInstancesBySoundID.ToList())
            {
                ManagedSoundInstance managedSoundInstance = kvp.Value;
                if (managedSoundInstance.instance.State == SoundState.Stopped)
                {
                    managedSoundInstance.ClearSoundID();
                    activeInstancesBySoundID.Remove(kvp.Key);
                }
            }
        }

        private static void DeleteInactiveInstances()
        {
            if (SonOfRobinGame.CurrentUpdate % 124 != 0) return;

            foreach (SoundData.Name soundName in instanceListsByName.Keys)
            {
                foreach (ManagedSoundInstance managedSoundInstance in instanceListsByName[soundName].ToList())
                {
                    if (managedSoundInstance.IsObsolete) managedSoundInstance.Delete();
                }
            }
        }

        public void Pause()
        {
            if (this.instance.State != SoundState.Stopped) this.instance.Pause();
        }

        public void Resume()
        {
            if (this.instance.State == SoundState.Paused) this.instance.Resume();
        }

        public static void Stop(string soundID)
        {
            if (activeInstancesBySoundID.ContainsKey(soundID)) activeInstancesBySoundID[soundID].Stop();
        }

        public void Stop()
        {
            if (this.instance.State != SoundState.Stopped) this.instance.Stop();
            this.ClearSoundID();
        }

        public float Volume
        {
            get { return this.instance.Volume; }
            set { this.instance.Volume = value; }
        }

        public float Pitch
        {
            get { return this.instance.Pitch; }
            set { this.instance.Pitch = value; }
        }

        public bool IsLooped
        {
            get { return this.instance.IsLooped; }
            set { this.instance.IsLooped = value; }
        }

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            this.instance.Apply3D(listener: listener, emitter: emitter);
        }

        public static ManagedSoundInstance GetNewOrStoppedInstance(SoundData.Name soundName)
        {
            if (!instanceListsByName.ContainsKey(soundName)) instanceListsByName[soundName] = new List<ManagedSoundInstance>();

            foreach (ManagedSoundInstance managedSoundInstance in instanceListsByName[soundName])
            {
                if (managedSoundInstance.instance.State != SoundState.Playing) return managedSoundInstance;
            }

            return new ManagedSoundInstance(soundName);
        }

        public static ManagedSoundInstance GetPlayingInstance(string soundID)
        {
            if (!activeInstancesBySoundID.ContainsKey(soundID)) return null;

            ManagedSoundInstance managedSoundInstance = activeInstancesBySoundID[soundID];

            if (managedSoundInstance.instance.State == SoundState.Stopped)
            {
                managedSoundInstance.ClearSoundID();
                return null;
            }
            else return managedSoundInstance;
        }

        public static void PauseAll()
        {
            foreach (ManagedSoundInstance managedSoundInstance in activeInstancesBySoundID.Values.ToList()) managedSoundInstance.Pause();
        }

        public static void ResumeAll()
        {
            foreach (ManagedSoundInstance managedSoundInstance in activeInstancesBySoundID.Values.ToList()) managedSoundInstance.Resume();
        }

        public static void StopAll()
        {
            foreach (ManagedSoundInstance managedSoundInstance in activeInstancesBySoundID.Values.ToList()) managedSoundInstance.Stop();
        }

        public static int ActiveInstancesCount
        { get { return activeInstancesBySoundID.Count; } }

        public static int InactiveInstancesCount
        {
            get
            {
                int inactiveCount = 0;
                foreach (var instancesList in instanceListsByName.Values)
                {
                    foreach (ManagedSoundInstance managedSoundInstance in instancesList)
                    {
                        if (managedSoundInstance.currentSoundID != null && !activeInstancesBySoundID.ContainsKey(managedSoundInstance.currentSoundID)) inactiveCount++;
                    }
                }

                return inactiveCount;
            }
        }
    }
}