using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
public class SoundManager : MonoBehaviour
{
    [SerializeField]
    List<SoundData> soundDataList;
    AudioSource _audioSource;
    bool isMute = false;

    public void PlaySound(int index)
    {
        if (isMute) return;

        if (index < soundDataList.Count)
        {
            MMSoundManagerPlayOptions options;
            options = MMSoundManagerPlayOptions.Default;
            options.ID = soundDataList[index].index;
            options.Volume = soundDataList[index].volume;
            options.Priority = soundDataList[index].priority;
            options.Loop = soundDataList[index].loop;
            options.Pitch = soundDataList[index].pitch;
            options.PlaybackTime = soundDataList[index].startTime;
            _audioSource = MMSoundManagerSoundPlayEvent.Trigger(soundDataList[index].clip, options);
        }
    }

    public void StopSound(int index)
    {
        if (isMute) return;
        MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, index, _audioSource);
    }

    public void Mute(bool state)
    {
        isMute = state;
    }
}


[Serializable]
struct SoundData
{
    public string name;
    public int index;
    public AudioClip clip;
    public bool loop;
    public int priority;
    public float startTime;
    [Range(0, 1)] public float volume;
    [Range(0, 3)] public float pitch;
}
