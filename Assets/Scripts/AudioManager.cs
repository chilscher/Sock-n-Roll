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
    public Sound[] sounds;
    private List<Sound> fadingOutSounds = new List<Sound>();
    private List<Sound> fadingInSounds = new List<Sound>();

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

    public void Play(string sound) {
        Sound s = getSound(sound);
        s.source.time = s.startDelay;
        s.source.Play();
    }

    public void PlayConsecutively(string sound1, string sound2) {
        //plays sound after delayedSound ends
        Sound s1 = getSound(sound1);
        Sound s2 = getSound(sound2);
        s1.source.time = s1.startDelay;
        s1.source.Play();
        s2.source.time = s2.startDelay;
        s2.source.PlayDelayed(s1.clip.length);
         
    }

    public void Stop(string sound) {
        Sound s = getSound(sound);
        s.source.Stop();
    }

    public void StopAll() {
        foreach (Sound s in sounds) {
            if (s.source.isPlaying) {
                s.source.Stop();
            }
        }
    }
    
    public void FadeOut(Sound s) {
        fadingOutSounds.Add(s);
        s.fadeTimeLeft = globalFadeTime;
    }

    private void countDownFadeOutSounds() {
        List<Sound> toRemove = new List<Sound>();
        foreach (Sound s in fadingOutSounds) {
            s.source.volume = s.volume * s.fadeTimeLeft / globalFadeTime;
            s.fadeTimeLeft -= Time.deltaTime;
            if (s.fadeTimeLeft < 0) { s.fadeTimeLeft = 0; }
            if (s.fadeTimeLeft == 0) {
                s.source.Stop();
                toRemove.Add(s);
                s.source.volume = s.volume;
            }
        }
        foreach (Sound s in toRemove) {
            fadingOutSounds.Remove(s);
        }
    }

    public void FadeOutAll() {
        foreach (Sound s in sounds) {
            if (s.source.isPlaying) {
                FadeOut(s);
            }
        }
    }

    public void FadeIn(string sound) {
        Sound s = getSound(sound);
        fadingInSounds.Add(s);
        s.fadeTimeLeft = globalFadeTime;
        s.source.volume = 0;
        s.source.time = s.startDelay;
        s.source.Play();
    }

    private Sound getSound(string name) {
        Sound s = Array.Find(sounds, item => item.name == name);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            return null;
        }
        return s;
    }

    private void countDownFadeInSounds() {
        List<Sound> toRemove = new List<Sound>();
        foreach (Sound s in fadingInSounds) {
            s.source.volume = s.volume * (1 - (s.fadeTimeLeft / globalFadeTime));
            s.fadeTimeLeft -= Time.deltaTime;
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
}
