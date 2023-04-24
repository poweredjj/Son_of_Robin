using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class ManagedSoundInstance
    {
        private static readonly Dictionary<SoundData.Name, List<ManagedSoundInstance>> instancesByName = new Dictionary<SoundData.Name, List<ManagedSoundInstance>>();
        private static readonly Dictionary<string, ManagedSoundInstance> instancesByPlayID = new Dictionary<string, ManagedSoundInstance>();
        public static int CreatedInstancesCount { get; private set; } = 0;

        private readonly SoundData.Name soundName;
        private readonly SoundEffectInstance instance;
        private readonly int hash;
        private DateTime lastPlayed;

        private string currentSoundID;

        public ManagedSoundInstance(SoundData.Name soundName)
        {
            this.soundName = soundName;
            this.instance = SoundData.soundsDict[soundName].CreateInstance();
            this.hash = this.instance.GetHashCode();
            this.currentSoundID = null;

            CreatedInstancesCount++;
        }

        public void AssignSoundID(string id)
        {
            this.currentSoundID = id;
            instancesByPlayID[id] = this;
        }

        public void Play()
        {
            if (this.currentSoundID == null) throw new ArgumentException($"Sound ID for {this.soundName} hasn't been set.");

            try
            {
                this.instance.Play();
                this.lastPlayed = DateTime.Now;
            }
            catch (InstancePlayLimitException)
            {
                // TODO add code for stopping other, stopped instance
            }
        }

        public static bool StopOldestActiveInstance()
        {
            CleanUpActiveInstances();
            ManagedSoundInstance oldestPlayingInstance = GetOldestPlayingInstance();
            if (oldestPlayingInstance != null) oldestPlayingInstance.Stop();

            return oldestID != null;
        }

        public static ManagedSoundInstance GetOldestPlayingInstance()
        {
            ManagedSoundInstance oldestPlayingInstance = null;
            if (instancesByPlayID.Any()) oldestPlayingInstance = instancesByPlayID.OrderBy(x => x.Value).Select(x => x.Value).FirstOrDefault();

            return oldestPlayingInstance;
        }

        public static void CleanUpActiveInstances()
        {
            var stoppedInstances = instancesByPlayID.Where(kvp => kvp.Value.instance.State == SoundState.Stopped).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var kvp in stoppedInstances)
            {
                string id = kvp.Key;
                ManagedSoundInstance instance = kvp.Value;
                instancesByPlayID.Remove(id);
            }
        }


        public void Pause()
        {
            if (this.instance.State != SoundState.Stopped) this.instance.Pause();
        }

        public void Stop()
        {
            if (this.instance.State != SoundState.Stopped) this.instance.Stop();
            instancesByPlayID.Remove(this.currentSoundID);
            this.currentSoundID = null;
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
            if (!instancesByPlayID.ContainsKey(id)) return null;

            ManagedSoundInstance managedSoundInstance = instancesByPlayID[id];

            if (managedSoundInstance.instance.State == SoundState.Stopped)
            {
                managedSoundInstance.Stop(); // to clear id
                return null;
            }
            else return managedSoundInstance;
        }
    }

    public class SoundInstanceManager
    {
        private static readonly Dictionary<string, SoundEffectInstance> activeSoundInstancesByID = new Dictionary<string, SoundEffectInstance>();
        private static readonly Dictionary<string, DateTime> instanceCreationDateByID = new Dictionary<string, DateTime>();

        private static readonly Dictionary<SoundData.Name, List<SoundEffectInstance>> inactiveInstancesByName = new Dictionary<SoundData.Name, List<SoundEffectInstance>>();
        private static readonly Dictionary<int, SoundData.Name> soundNamesByInstanceHash = new Dictionary<int, SoundData.Name>(); // to keep track what instance plays what sound

        public static int CreatedInstancesCount { get; private set; } = 0;

        public static int ActiveInstancesCount
        { get { return activeSoundInstancesByID.Count; } }

        public static int InactiveInstancesCount
        {
            get
            {
                int inactiveCount = 0;

                foreach (var instancesList in inactiveInstancesByName.Values)
                {
                    inactiveCount += instancesList.Count;
                }

                return inactiveCount;
            }
        }


        public static SoundEffectInstance GetPlayingInstance(string id)
        {
            if (!activeSoundInstancesByID.ContainsKey(id)) return null;
            SoundEffectInstance previousInstance = activeSoundInstancesByID[id];

            return previousInstance.State == SoundState.Stopped ? null : previousInstance;
        }

        public static SoundEffectInstance GetNewOrStoppedInstance(SoundData.Name soundName, string id)
        {
            if (inactiveInstancesByName.ContainsKey(soundName) && inactiveInstancesByName[soundName].Any())
            {
                SoundEffectInstance previousInstance = inactiveInstancesByName[soundName][0];
                inactiveInstancesByName[soundName].RemoveAt(0);

                if (activeSoundInstancesByID.ContainsKey(id))
                {
                    SoundEffectInstance previousActiveInstance = activeSoundInstancesByID[id];
                    AddToInactiveInstances(previousActiveInstance);
                }

                activeSoundInstancesByID[id] = previousInstance;
                return previousInstance;
            }

            SoundEffectInstance newInstance = SoundData.soundsDict[soundName].CreateInstance();
            CreatedInstancesCount++;

            if (activeSoundInstancesByID.ContainsKey(id))
            {
                SoundEffectInstance previousActiveInstance = activeSoundInstancesByID[id];
                AddToInactiveInstances(previousActiveInstance);
            }

            activeSoundInstancesByID[id] = newInstance;
            instanceCreationDateByID[id] = DateTime.Now;
            soundNamesByInstanceHash[newInstance.GetHashCode()] = soundName;
            return newInstance;
        }

        public static bool StopOldestActiveInstance()
        {
            CleanUpActiveInstances();
            string oldestID = GetOldestActiveInstanceId();
            if (oldestID != null) StopInstance(oldestID);

            return oldestID != null;
        }

        public static string GetOldestActiveInstanceId()
        {
            string oldestID = null;
            if (instanceCreationDateByID.Any()) oldestID = instanceCreationDateByID.OrderBy(x => x.Value).Select(x => x.Key).FirstOrDefault();

            return oldestID;
        }

        public static void StopInstance(string id)
        {
            if (!activeSoundInstancesByID.ContainsKey(id)) return;

            SoundEffectInstance instance = activeSoundInstancesByID[id];
            if (instance.State != SoundState.Stopped) instance.Stop();
        }

        public static void PauseInstance(string id)
        {
            if (!activeSoundInstancesByID.ContainsKey(id)) return;

            SoundEffectInstance instance = activeSoundInstancesByID[id];
            if (instance.State != SoundState.Stopped) instance.Pause();
        }

        public static void ResumeInstance(string id)
        {
            if (!activeSoundInstancesByID.ContainsKey(id)) return;

            SoundEffectInstance instance = activeSoundInstancesByID[id];
            if (instance.State == SoundState.Paused) instance.Resume();
        }

        public static void PauseAll()
        {
            foreach (string id in activeSoundInstancesByID.Keys.ToList()) PauseInstance(id);
        }

        public static void ResumeAll()
        {
            foreach (string id in activeSoundInstancesByID.Keys.ToList()) ResumeInstance(id);
        }

        public static void StopAll()
        {
            foreach (string id in activeSoundInstancesByID.Keys.ToList()) StopInstance(id);
        }

        public static void CleanUpActiveInstances()
        {
            var stoppedInstances = activeSoundInstancesByID.Where(kvp => kvp.Value.State == SoundState.Stopped).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var kvp in stoppedInstances)
            {
                string id = kvp.Key;
                SoundEffectInstance instance = kvp.Value;

                activeSoundInstancesByID.Remove(id);
                instanceCreationDateByID.Remove(id);
                AddToInactiveInstances(instance);
            }
        }

        private static void AddToInactiveInstances(SoundEffectInstance instance)
        {
            instance.Stop();
            SoundData.Name soundName = soundNamesByInstanceHash[instance.GetHashCode()];
            if (!inactiveInstancesByName.ContainsKey(soundName)) inactiveInstancesByName[soundName] = new List<SoundEffectInstance>();
            inactiveInstancesByName[soundName].Add(instance);
        }
    }
}