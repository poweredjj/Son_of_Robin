﻿using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class SoundInstanceManager_
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