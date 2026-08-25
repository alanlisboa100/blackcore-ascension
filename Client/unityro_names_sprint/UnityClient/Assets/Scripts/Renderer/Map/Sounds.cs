using ROIO;
using ROIO.Models.FileTypes;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Sounds
{
    private List<Playing> playing;
    private GameObject _parent = null;
    
    private class Playing
    {
        public AudioClip clip;
        public AudioSource source;
        public float playAt;
        public RSW.Sound info;
    }
        
    public Sounds() {
        playing = new List<Playing>();
    }

    public void Clear() {
        if (_parent != null) {
            GameObject.Destroy(_parent);
            _parent = null;
        }

        foreach (Playing p in playing) {
            FileCache.Remove(p.info.file);
        }
        playing.Clear();
    }

    public int Count() {
        return playing.Count;
    }

    public async void Add(RSW.Sound sound, GameObject parent) {
        if (_parent == null) {
            _parent = new GameObject("_sounds");
            _parent.transform.parent = MapRenderer.mapParent.transform;
        }

        Playing p = new Playing();
        p.playAt = 0;
        p.info = sound;

        if (parent == null) { // static sound
            var obj = new GameObject(sound.file + "[" + sound.name + "]" + sound.cycle);
            obj.transform.parent = _parent.transform;
            p.source = obj.AddComponent<AudioSource>();
            p.source.transform.position = new Vector3(sound.pos[0], sound.pos[1], sound.pos[2]);
        } else { // sound is attached to an entity
            p.source = parent.GetComponent<AudioSource>();
        }

        if (p.source == null) {
            return;
        }

        p.source.loop = false;
        p.source.playOnAwake = false;
        p.source.volume = Mathf.Clamp(sound.vol, 0, 1);
        p.source.spatialBlend = 1;
        p.source.rolloffMode = AudioRolloffMode.Linear;
        p.source.spatialize = true;
        p.source.outputAudioMixerGroup = MapRenderer.SoundsMixerGroup;
        p.source.dopplerLevel = 0;
        p.source.minDistance = p.info.width;
        p.source.maxDistance = sound.range + sound.height;

        try {
            p.clip = await AddressableAssetCache<AudioClip>.LoadAsync(sound.file.SanitizeForAddressables());
            if (p.clip != null && p.source != null) {
                playing.Add(p);
            }
        } catch (System.Exception ex) {
            Debug.LogWarning($"Unable to load map sound '{sound.file}': {ex.Message}");
            if (parent == null && p.source != null) {
                GameObject.Destroy(p.source.gameObject);
            }
        }
    }

    public void Update() {
        float now = Time.realtimeSinceStartup;

        foreach(Playing p in playing) {
            if(p.playAt <= now && p.source != null) {
                p.source.PlayOneShot(p.clip);
                p.playAt = now + p.info.cycle;
            }
        }
    }
}
