//for Sock 'n Roll, copyright Cole Hilscher 2020

using UnityEngine.Audio;
using UnityEngine;

//taken from https://www.youtube.com/watch?v=6OT43pvUyfY, a youtube tutorial on adding sounds to games by Brackeys
//code can be downloaded from downloads.http://brackeys.com/wp-content/FilesForDownload/AudioManager.zip
//some code has been cut - references to an AudioMixerGroup object, as well as  references to a sound's volume variance and pitch variance

[System.Serializable]
public class Sound {
    //an instance of this object represents a single audio clip
    //the sounds are created and edited in the inspector, on the main menu scene, under the AudioManager script
    public string name;
    public AudioClip clip; //the actual audio byte
    [Range(0f, 1f)]
    public float volume = .75f;
    [Range(.1f, 3f)]
    public float pitch = 1f;
    public float startDelay = 0f; //how far into the audio clip do we want to start playing? ex 1.5f starts the audio clip 1.5 seconds in
    public bool loop = false; //does the audio clip loop after finishing?
    public bool pausable = false;

    //variables modified and read by the audiomanager itself
    [HideInInspector]
    public AudioSource source;
    [HideInInspector]
    public float fadeTimeLeft = 0f;
    [HideInInspector]
    public bool fadeOutWithReducedSound = false;

}