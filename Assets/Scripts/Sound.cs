using UnityEngine.Audio;
using UnityEngine;

//taken from https://www.youtube.com/watch?v=6OT43pvUyfY, a youtube tutorial on adding sounds to games by Brackeys
//code can be downloaded from downloads.http://brackeys.com/wp-content/FilesForDownload/AudioManager.zip
//some code has been cut - references to an AudioMixerGroup object, as well as  references to a sound's volume variance and pitch variance

[System.Serializable]
public class Sound {

    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = .75f;
    //[Range(0f, 1f)]
    //public float originalVolume = 0.75f;
    [Range(.1f, 3f)]
    public float pitch = 1f;
    public float startDelay = 0f;
    public bool loop = false;
    public bool pausable = false;

    [HideInInspector]
    public AudioSource source;
    [HideInInspector]
    public float fadeTimeLeft = 0f;
    [HideInInspector]
    public bool fadeOutWithReducedSound = false;

}