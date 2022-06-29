using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class SoundInstanceManager
    {
        private readonly static Dictionary<string, SoundEffectInstance> activeSoundInstancesByID = new Dictionary<string, SoundEffectInstance>();
        private readonly static Dictionary<SoundData.Name, List<SoundEffectInstance>> inactiveInstancesByName = new Dictionary<SoundData.Name, List<SoundEffectInstance>>();
        private readonly static Dictionary<int, SoundData.Name> soundNamesByInstanceHash = new Dictionary<int, SoundData.Name>(); // to keep track what instance plays what sound

        private static int createdInstancesCount = 0;
        public static int CreatedInstancesCount { get { return createdInstancesCount; } }
        public static int ActiveInstancesCount { get { return activeSoundInstancesByID.Count; } }
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

        public static int InactiveNamesCount { get { return inactiveInstancesByName.Keys.Count; } }

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
            createdInstancesCount++;

            if (activeSoundInstancesByID.ContainsKey(id))
            {
                SoundEffectInstance previousActiveInstance = activeSoundInstancesByID[id];
                AddToInactiveInstances(previousActiveInstance);
            }

            activeSoundInstancesByID[id] = newInstance;
            soundNamesByInstanceHash[newInstance.GetHashCode()] = soundName;
            return newInstance;
        }

        public static void StopInstance(string id)
        {
            if (!activeSoundInstancesByID.ContainsKey(id)) return;

            SoundEffectInstance instance = activeSoundInstancesByID[id];
            if (instance.State != SoundState.Stopped) instance.Stop();
        }

        public static void StopAll()
        {
            foreach (string id in activeSoundInstancesByID.Keys.ToList())
            {
                StopInstance(id);
            }
        }

        public static void CleanUpActiveInstances()
        {
            var notPlayingInstances = activeSoundInstancesByID.Where(kvp => kvp.Value.State != SoundState.Playing).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var kvp in notPlayingInstances)
            {
                string id = kvp.Key;
                SoundEffectInstance instance = kvp.Value;

                activeSoundInstancesByID.Remove(id);
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
