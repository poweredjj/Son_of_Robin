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

        public readonly SoundData.Name soundName;
        private readonly SoundEffectInstance instance; // should not be publicly exposed, to avoid uncontrolled Play()
        private readonly DateTime created;
        private DateTime lastPlayed;

        private string currentSoundID;

        public ManagedSoundInstance(SoundData.Name soundName)
        {
            this.soundName = soundName;
            this.instance = SoundData.soundsDict[soundName].CreateInstance();
            this.created = DateTime.Now;
            this.currentSoundID = null;

            if (!instanceListsByName.ContainsKey(soundName)) instanceListsByName[soundName] = new List<ManagedSoundInstance>();
            instanceListsByName[soundName].Add(this);

            CreatedInstancesCount++;
        }

        private void Delete()
        {
            this.instance.Dispose();
            CreatedInstancesCount--;
        }

        private void AssignSoundID(string soundID)
        {
            activeInstancesBySoundID[soundID] = this;
            this.currentSoundID = soundID;
        }

        private void ClearSoundID()
        {
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
                if (kvp.Value.instance.State == SoundState.Stopped)
                {
                    activeInstancesBySoundID.Remove(kvp.Key);
                }
            }
        }

        private static void DeleteInactiveInstances()
        {
            if (SonOfRobinGame.CurrentUpdate % 124 != 0) return;

            foreach (SoundData.Name soundName in instanceListsByName.Keys)
            {
                var newInstanceList = new List<ManagedSoundInstance>();

                foreach (ManagedSoundInstance managedSoundInstance in instanceListsByName[soundName])
                {
                    if (managedSoundInstance.instance.State != SoundState.Stopped && DateTime.Now - managedSoundInstance.created < TimeSpan.FromSeconds(20))
                    {
                        newInstanceList.Add(managedSoundInstance);
                    }
                    else managedSoundInstance.Delete();

                    instanceListsByName[soundName] = newInstanceList;
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
                managedSoundInstance.Stop(); // to clear soundID
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
                    inactiveCount += instancesList.Count;
                }

                return inactiveCount;
            }
        }
    }
}