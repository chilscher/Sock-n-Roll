using UnityEngine.Audio;
using System.Collections.Generic;
using System;
using UnityEngine;

//taken from https://www.youtube.com/watch?v=6OT43pvUyfY, a youtube tutorial on adding sounds to games by Brackeys
//code can be downloaded from downloads.http://brackeys.com/wp-content/FilesForDownload/AudioManager.zip
//some code has been cut - references to an AudioMixerGroup object, as well as code that guarantees only one instance of the class exists
//also references to a sound's volume variance and pitch variance have been removed

//to play audio from another script, use FindObjectOfType<AudioManager>().Play("Audio Name");
//to stop audio from another script, use FindObjectOfType<AudioManager>().Stop("Audio Name");

public class AudioManager : MonoBehaviour {
    public float globalVolumeMultiplier = 1f;
    public float globalFadeTime = 1.5f;
    [Range(0f, 100f)]
    public float reducedSoundPercent = 50;
    public Sound[] sounds;
    private List<Sound> fadingOutSounds = new List<Sound>();
    private List<Sound> fadingInSounds = new List<Sound>();
    private List<Sound> pausedSounds = new List<Sound>();

    void Awake() {
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;
            s.volume *= globalVolumeMultiplier;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    private void Update() {
        countDownFadeOutSounds();
        countDownFadeInSounds();
    }

    public void play(string name) {
        Sound s = getSound(name);
        s.source.time = s.startDelay;
        s.source.Play();
    }

    public void playConsecutively(string sound1, string sound2) {
        //plays sound after delayedSound ends
        Sound s1 = getSound(sound1);
        Sound s2 = getSound(sound2);
        s1.source.time = s1.startDelay;
        s1.source.Play();
        s2.source.time = s2.startDelay;
        s2.source.PlayDelayed(s1.clip.length);
         
    }

    public void stop(string name) {
        Sound s = getSound(name);
        s.source.Stop();
    }

    public void stopAll() {
        foreach (Sound s in sounds) {
            if (s.source.isPlaying) {
                s.source.Stop();
            }
        }
    }
    
    public void fadeOut(Sound s) {
        if (!fadingOutSounds.Contains(s)) {
            fadingOutSounds.Add(s);
            s.fadeTimeLeft = globalFadeTime;
        }
    }

    private void countDownFadeOutSounds() {
        //sounds that are fading out have their volumes scaled down
        List<Sound> toRemove = new List<Sound>();
        foreach (Sound s in fadingOutSounds) {
            s.source.volume = s.volume * s.fadeTimeLeft / globalFadeTime;
            if (s.fadeOutWithReducedSound) { s.source.volume *= (reducedSoundPercent / 100); }
            s.fadeTimeLeft -= Time.unscaledDeltaTime; //scales volume down even if game is paused (time.deltaTime = 0 but unscaledDeltaTime is not 0)
            if (s.fadeTimeLeft < 0) { s.fadeTimeLeft = 0; }
            if (s.fadeTimeLeft == 0) {
                s.source.Stop();
                toRemove.Add(s);
                s.source.volume = s.volume;
                s.fadeOutWithReducedSound = false;
            }
        }
        foreach (Sound s in toRemove) {
            fadingOutSounds.Remove(s);
        }
    }

    public void fadeOutAll() {
        foreach (Sound s in sounds) {
            if (s.source.isPlaying) {
                fadeOut(s);
            }
        }
    }

    public void fadeIn(string name) {
        Sound s = getSound(name);
        if (fadingOutSounds.Contains(s)) {fadingOutSounds.Remove(s);}
        fadingInSounds.Add(s);
        s.fadeTimeLeft = globalFadeTime;
        s.source.volume = 0;
        s.source.time = s.startDelay;
        s.source.Play();
    }

    private Sound getSound(string name) {
        //gets a Sound object from the sound's name
        Sound s = Array.Find(sounds, item => item.name == name);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            return null;
        }
        return s;
    }

    private void countDownFadeInSounds() {
        //sounds that are fading in have their volumes scaled up
        List<Sound> toRemove = new List<Sound>();
        foreach (Sound s in fadingInSounds) {
            s.source.volume = s.volume * (1 - (s.fadeTimeLeft / globalFadeTime));
            s.fadeTimeLeft -= Time.unscaledDeltaTime; //scales volume up even if game is paused (time.deltaTime = 0 but unscaledDeltaTime is not 0)
            if (s.fadeTimeLeft < 0) { s.fadeTimeLeft = 0; }
            if (s.fadeTimeLeft == 0) {
                toRemove.Add(s);
                s.source.volume = s.volume;
            }
        }
        foreach (Sound s in toRemove) {
            fadingInSounds.Remove(s);
        }
    }

    public void pause() {
        //pausable sounds are paused, and the global volume is reduced
        AudioListener.volume = reducedSoundPercent / 100;
        foreach (Sound s in sounds) {
            if (s.source.isPlaying) {
                if (s.pausable) {
                    s.source.Pause();
                    pausedSounds.Add(s);
                }
            }
        }
    }

    public void resume() {
        //pausable sounds are resumed, and the global volume is restored
        AudioListener.volume = 1;
        foreach (Sound s in pausedSounds) {
            s.source.UnPause();
        }
        pausedSounds = new List<Sound>();
    }

    public void resumeWithMusicFadeout() {
        //pausable sounds are resumed and the global volume is restored, then non-pausable sounds are faded out
        List<Sound> playingMusic = new List<Sound>();
        foreach (Sound s in sounds) {
            if (!s.pausable && s.source.isPlaying) {
                playingMusic.Add(s);
            }
        }
        resume();
        foreach (Sound s in playingMusic) {
            //s.source.volume *= (reducedSoundPercent / 100);
            s.fadeOutWithReducedSound = true;
        }
        fadeOutAll();

    }

    public bool isPlaying(string name) {
        Sound s = getSound(name);
        return s.source.isPlaying;
    }

    public void stopPausableSounds() {
        //all pausable sounds are stopped
        foreach (Sound s in sounds) {
            if (s.source.isPlaying || pausedSounds.Contains(s)) {
                if (s.pausable) {
                    s.source.Stop();
                }
            }
        }
        pausedSounds = new List<Sound>();
    }
}
