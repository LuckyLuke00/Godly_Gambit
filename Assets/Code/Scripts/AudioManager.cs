using UnityEngine;

namespace GodlyGambit
{
    public class AudioManager : Singleton<AudioManager>
    {
        private static readonly AudioSource[] _effectSources = new AudioSource[5];
        private static AudioSource _musicSource = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                go.AddComponent<AudioManager>();
            }

            CreateEffectSources();
            CreateMusicSource();
        }

        // Play a sound effect
        public static void PlaySound(Audio audio, bool forcePlay = false, float fadeTimeSec = 0f)
        {
            // Return if there is no audio clip
            if (!audio.Clip) return;

            // Get a free audio source
            AudioSource source = GetFreeAudioSource(forcePlay);

            // Check if the sound is already playing
            if (!forcePlay && IsAlreadyPlaying(audio)) return;

            // Return if there are no free audio sources
            if (!source) return;

            // Set the source's clip to the audio clip
            source.clip = audio.Clip;
            source.loop = audio.Loop;
            source.maxDistance = audio.MaxDistance;
            source.pitch = audio.Pitch + Random.Range(-audio.PitchVariation, audio.PitchVariation);
            source.spatialBlend = audio.SpatialBlend;
            source.volume = audio.Volume;

            // If the fade in speed is 0, play the sound immediately
            if (fadeTimeSec <= 0f)
            {
                source.Play();
                return;
            }

            // Fade in the sound using a coroutine, set the volume to 0
            source.volume = 0f;
            source.Play();
            Instance.StartCoroutine(Instance.FadeInAudio(source, fadeTimeSec, audio.Volume));
        }

        public static void PlaySound(Audio audio, float fadeTimeSec = 0f)
        {
            PlaySound(audio, false, fadeTimeSec);
        }

        public static void PlaySound(Audio[] audio, float fadeTimeSec = 0f)
        {
            foreach (Audio a in audio)
            {
                PlaySound(a, false, fadeTimeSec);
            }
        }

        public static void PlaySound(Audio[] audio, bool forcePlay = false, float fadeTimeSec = 0f)
        {
            foreach (Audio a in audio)
            {
                PlaySound(a, forcePlay, fadeTimeSec);
            }
        }

        public static void PlayMusic(Audio audio, float fadeTimeSec = 0f)
        {
            PlayMusic(audio, false, fadeTimeSec);
        }

        public static void PlayMusic(Audio audio, bool forcePlay = false, float fadeTimeSec = 0f)
        {
            if (!forcePlay && IsAlreadyPlaying(audio)) return;

            // Return if there is no audio clip
            if (!audio.Clip) return;

            // Check if the audio clip the same
            if (!_musicSource || _musicSource.clip == audio.Clip) return;

            // Set the source's clip to the audio clip
            _musicSource.clip = audio.Clip;
            _musicSource.loop = audio.Loop;
            _musicSource.maxDistance = audio.MaxDistance;
            _musicSource.pitch = audio.Pitch + Random.Range(-audio.PitchVariation, audio.PitchVariation);
            _musicSource.spatialBlend = audio.SpatialBlend;
            _musicSource.volume = audio.Volume;

            // If the fade in speed is 0, play the music immediately
            if (fadeTimeSec <= 0f)
            {
                _musicSource.Play();
                return;
            }

            // Fade in the music using a coroutine, set the volume to 0
            _musicSource.volume = 0f;
            _musicSource.Play();
            Instance.StartCoroutine(Instance.FadeInAudio(_musicSource, fadeTimeSec, audio.Volume));
        }

        public static void StopMusic(float fadeOutSpeed = 0f)
        {
            // Return if there is no music source
            if (!_musicSource) return;

            // If the fade out speed is 0, stop the music immediately
            if (fadeOutSpeed <= 0f)
            {
                _musicSource.Stop();
                return;
            }

            // Start the fade out coroutine
            Instance.StartCoroutine(Instance.FadeOutAudio(_musicSource, fadeOutSpeed));
        }

        private System.Collections.IEnumerator FadeOutAudio(AudioSource source, float fadeTimeSec)
        {
            // Calculate the speed
            float speed = source.volume / fadeTimeSec;
            // Loop until the music is faded out
            while (source.volume > 0f)
            {
                // Decrease the volume of the music
                source.volume -= Time.deltaTime * speed;
                // Wait until the next frame
                yield return null;
            }
            // Stop the music
            source.Stop();
        }

        private System.Collections.IEnumerator FadeInAudio(AudioSource source, float fadeTimeSec, float targetVolume)
        {
            // Calculate the speed
            float speed = (targetVolume - source.volume) / fadeTimeSec;

            // Loop until the music is faded in
            while (source.volume < targetVolume)
            {
                // Increase the volume of the music
                source.volume += Time.deltaTime * speed;

                // Wait until the next frame
                yield return null;
            }

            // Reset the volume
            source.volume = targetVolume;
        }

        public static bool IsAlreadyPlaying(Audio audio)
        {
            // Check if the Music is already playing
            if (_musicSource.isPlaying && _musicSource.clip == audio.Clip) return true;

            // Check if the Sound effect is already playing
            foreach (AudioSource source in _effectSources)
            {
                if (source.isPlaying && source.clip == audio.Clip) return true;
            }

            return false;
        }

        public static void StopAllSounds(float fadeTimeSec = 0f)
        {
            // Loop through all audio sources
            foreach (AudioSource source in _effectSources)
            {
                // If the source is playing, stop it
                if (source.isPlaying)
                {
                    // If the fade out speed is 0, stop the sound immediately
                    if (fadeTimeSec <= 0f)
                    {
                        source.Stop();
                        continue;
                    }

                    // Start the fade out coroutine
                    Instance.StartCoroutine(Instance.FadeOutAudio(source, fadeTimeSec));
                }
            }
        }

        public static void StopSound(Audio audio, float fadeOutSpeed = 0f)
        {
            foreach (AudioSource source in _effectSources)
            {
                if (source.isPlaying && source.clip == audio.Clip)
                {
                    // If the fade out speed is 0, stop the sound immediately
                    if (fadeOutSpeed <= 0f)
                    {
                        source.Stop();
                        return;
                    }

                    // Start the fade out coroutine
                    Instance.StartCoroutine(Instance.FadeOutAudio(source, fadeOutSpeed));

                    return;
                }
            }
        }

        private static AudioSource GetFreeAudioSource(bool returnMostComplete = false)
        {
            AudioSource mostComplete = _effectSources[0];
            // Loop through all audio sources
            foreach (AudioSource source in _effectSources)
            {
                // If the source is not playing, return it
                if (!source.isPlaying) return source;

                // If the source is playing, check if it is more complete than the current one
                if (returnMostComplete && !mostComplete || source.timeSamples > mostComplete.timeSamples)
                {
                    mostComplete = source;
                }
            }

            // If all sources are playing return the first one
            return returnMostComplete ? mostComplete : null;
        }

        private static void CreateEffectSources()
        {
            GameObject effectParent = new GameObject("Effect Sources");
            effectParent.transform.parent = Instance.transform;
            for (int i = 0; i < _effectSources.Length; i++)
            {
                // Create a new audio source
                AudioSource source = effectParent.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.Stop();

                // Add the audio source to the array
                _effectSources[i] = source;
            }
        }

        private static void CreateMusicSource()
        {
            GameObject musicParent = new GameObject("Music Source");
            musicParent.transform.parent = Instance.transform;
            _musicSource = musicParent.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.Stop();
        }
    }
}
