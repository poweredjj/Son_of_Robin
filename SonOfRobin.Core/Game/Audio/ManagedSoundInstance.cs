using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class ManagedSoundInstance
    {
        private static readonly Dictionary<SoundData.Name, List<ManagedSoundInstance>> instancesByName = new Dictionary<SoundData.Name, List<ManagedSoundInstance>>();
        private static readonly Dictionary<string, ManagedSoundInstance> activeInstancesByPlayID = new Dictionary<string, ManagedSoundInstance>();
        public static int CreatedInstancesCount { get; private set; } = 0;

        private readonly SoundData.Name soundName;
        private readonly SoundEffectInstance instance;
        private DateTime lastPlayed;

        private string currentSoundID;

        public ManagedSoundInstance(SoundData.Name soundName)
        {
            this.soundName = soundName;
            this.instance = SoundData.soundsDict[soundName].CreateInstance();
            this.currentSoundID = null;

            CreatedInstancesCount++;
        }

        public void AssignSoundID(string id)
        {
            this.currentSoundID = id;
            activeInstancesByPlayID[id] = this;
        }

        public bool Play()
        {
            if (this.currentSoundID == null) throw new ArgumentException($"Sound ID for {this.soundName} hasn't been set.");

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
            var activeInstancesByLastPlayed = activeInstancesByPlayID.OrderBy(x => x.Value.lastPlayed).Select(x => x.Value);
            foreach (ManagedSoundInstance managedSoundInstance in activeInstancesByLastPlayed)
            {
                if (managedSoundInstance.instance.State == SoundState.Playing) return managedSoundInstance;
            }

            return null;
        }

        public static void CleanUpActiveInstances()
        {
            var stoppedInstances = activeInstancesByPlayID.Where(kvp => kvp.Value.instance.State == SoundState.Stopped).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var kvp in stoppedInstances)
            {
                string id = kvp.Key;
                ManagedSoundInstance instance = kvp.Value;
                activeInstancesByPlayID.Remove(id);
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

        public static void Stop(string id)
        {
            if (activeInstancesByPlayID.ContainsKey(id)) activeInstancesByPlayID[id].Stop();
        }

        public void Stop()
        {
            if (this.instance.State != SoundState.Stopped) this.instance.Stop();
            activeInstancesByPlayID.Remove(this.currentSoundID);
            this.currentSoundID = null;
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

        public bool IsPlaying
        { get { return this.instance.State == SoundState.Playing; } }

        public static ManagedSoundInstance GetNewOrStoppedInstance(SoundData.Name soundName)
        {
            if (!instancesByName.ContainsKey(soundName)) instancesByName[soundName] = new List<ManagedSoundInstance>();

            foreach (ManagedSoundInstance managedSoundInstance in instancesByName[soundName])
            {
                if (!managedSoundInstance.IsPlaying) return managedSoundInstance;
            }

            ManagedSoundInstance newManagedInstance = new ManagedSoundInstance(soundName);
            instancesByName[soundName].Add(newManagedInstance);

            return newManagedInstance;
        }

        public static ManagedSoundInstance GetPlayingInstance(string id)
        {
            if (!activeInstancesByPlayID.ContainsKey(id)) return null;

            ManagedSoundInstance managedSoundInstance = activeInstancesByPlayID[id];

            if (managedSoundInstance.instance.State == SoundState.Stopped)
            {
                managedSoundInstance.Stop(); // to clear id
                return null;
            }
            else return managedSoundInstance;
        }

        public static void PauseAll()
        {
            foreach (ManagedSoundInstance managedSoundInstance in activeInstancesByPlayID.Values.ToList()) managedSoundInstance.Pause();
        }

        public static void ResumeAll()
        {
            foreach (ManagedSoundInstance managedSoundInstance in activeInstancesByPlayID.Values.ToList()) managedSoundInstance.Resume();
        }

        public static void StopAll()
        {
            foreach (ManagedSoundInstance managedSoundInstance in activeInstancesByPlayID.Values.ToList()) managedSoundInstance.Stop();
        }

        public static int ActiveInstancesCount
        { get { return activeInstancesByPlayID.Count; } }

        public static int InactiveInstancesCount
        {
            get
            {
                int inactiveCount = 0;
                foreach (var instancesList in instancesByName.Values)
                {
                    inactiveCount += instancesList.Count;
                }

                return inactiveCount;
            }
        }
    }

}
