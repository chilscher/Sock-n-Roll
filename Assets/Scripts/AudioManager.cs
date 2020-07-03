//for Sock 'n Roll, copyright Cole Hilscher 2020

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
    //this script controls the audio for the entire game. it is loaded in the main menu, and not destroyed during transition between scenes

    public float globalVolumeMultiplier = 1f; //was initially used for having a volume scale, but now is just used to turn music on or off. There is no scale
    public float globalFadeTime = 1.5f; //the fading time between audio tracks
    public Sound[] sounds; //all of the sound objects used by the game. sounds are added and modified in the inspector

    //lists of sounds that are fading. used to have a smooth transition between scenes
    private List<Sound> fadingOutSounds = new List<Sound>();
    private List<Sound> fadingInSounds = new List<Sound>();

    //the sounds that are paused during the pause menu in a level
    private List<Sound> pausedSounds = new List<Sound>();
    //when the player opens the pause menu, some sounds continue playing at a lower volume, some % of the normal volume
    [Range(0f, 100f)]
    public float reducedSoundPercent = 50;

    void Awake() {
        DontDestroyOnLoad(gameObject); //retain the audiomanager in between scenes

        //set up all sounds at the start of the game
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;
            s.volume *= globalVolumeMultiplier * StaticVariables.globalAudioScale;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    private void Update() {
        //handle all fading sounds
        countDownFadeOutSounds();
        countDownFadeInSounds();
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT PLAY, PAUSE, AND STOP SOUNDS
    // ---------------------------------------------------

    public void play(string name) {
        //start playing a sound. the delay is how far into the sound byte we want to start listening
        Sound s = getSound(name);
        s.source.time = s.startDelay;
        s.source.Play();
    }

    public void playConsecutively(string sound1, string sound2) {
        //plays sound1 and sound2 consecutively. sound2 is set to start right after sound1 ends
        Sound s1 = getSound(sound1);
        Sound s2 = getSound(sound2);
        s1.source.time = s1.startDelay;
        s1.source.Play();
        s2.source.time = s2.startDelay;
        s2.source.PlayDelayed(s1.clip.length);
    }

    public void stop(string name) {
        //stop a specific sound
        Sound s = getSound(name);
        s.source.Stop();
    }

    public void stopAll() {
        //stop all sounds
        foreach (Sound s in sounds) {
            if (s.source.isPlaying) {
                s.source.Stop();
            }
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
            s.fadeOutWithReducedSound = true;
        }
        fadeOutAll();

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

    // ---------------------------------------------------
    //FUNCTIONS THAT HANDLE THE FADE-IN AND FADE-OUT PROCESS FOR SOUNDS
    // ---------------------------------------------------

    public void fadeOut(Sound s) {
        //starts the fade-out process for a sound
        if (!fadingOutSounds.Contains(s)) {
            fadingOutSounds.Add(s);
            s.fadeTimeLeft = globalFadeTime;
        }
    }

    private void countDownFadeOutSounds() {
        //handles the fade-out of all the currently fading-out sounds
        //sounds that are fading out have their volumes scaled down
        //this is called each frame, until the sound has hit a volume of 0
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
        //start fading out all sounds
        foreach (Sound s in sounds) {
            if (s.source.isPlaying) {
                fadeOut(s);
            }
        }
    }

    public void fadeIn(string name) {
        //start fading in a specific sound
        Sound s = getSound(name);
        if (fadingOutSounds.Contains(s)) {fadingOutSounds.Remove(s);}
        fadingInSounds.Add(s);
        s.fadeTimeLeft = globalFadeTime;
        s.source.volume = 0;
        s.source.time = s.startDelay;
        s.source.Play();
    }

    private void countDownFadeInSounds() {
        //handles the fade-in of all the currently fading-in sounds
        //sounds that are fading in have their volumes scaled up
        //this is called each frame, until the sound has hit a volume of 100%
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

    // ---------------------------------------------------
    //OTHER FUNCTIONS
    // ---------------------------------------------------
    
    private Sound getSound(string name) {
        //gets a Sound object from the sound's name
        Sound s = Array.Find(sounds, item => item.name == name);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            return null;
        }
        return s;
    }
    
    public bool isPlaying(string name) {
        //returns true if the specified sound is currently playing
        Sound s = getSound(name);
        return s.source.isPlaying;
    }

    public void applyGlobalVolume() {
        //if the global volume is set to 0, stop all sounds.
        AudioListener.volume = StaticVariables.globalAudioScale;
        if (AudioListener.volume == 0f) {
            stopAll();
        }
    }
}
