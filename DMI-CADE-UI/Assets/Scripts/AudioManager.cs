using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using Dmicade;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public ContentDisplayManager contentDisplayManager;
    public InfoOverlay infoOverlay;
    
    public Sound[] sounds;
    public Dictionary<string, Sound> soundSourceReferences;

    // Start is called before the first frame update
    void Awake()
    {
        soundSourceReferences = new Dictionary<string, Sound>();

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            
            soundSourceReferences.Add(s.name, s);
        }

        contentDisplayManager.OnScrollStart += (Vector3 v) => Play("ScrollSound");
        
        DmicSceneManager.Instance.OnAppStarting += (string appId) => Play("GameSelected");
        
        infoOverlay.OnEnable += () => Play("ButtonPress");
        infoOverlay.OnEnable += () => Play("MoreInfoOpen");
        infoOverlay.OnDisable += () => Play("ButtonPress");
        infoOverlay.OnDisable += () => Play("MoreInfoClose");
    }

    private void Start()
    {
        Play("MainTheme");
    }

    public void Play(string soundName)
    {
        if (soundSourceReferences.ContainsKey(soundName))
            soundSourceReferences[soundName].source.Play();
        else
            Debug.LogWarning("Sound: " + soundName + " not found.");
    }
}
