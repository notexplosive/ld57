using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame.Data;
using LD57.CartridgeManagement;
using Microsoft.Xna.Framework.Audio;

namespace LD57.Gameplay;

public class AmbientSoundPlayer
{
    private readonly Dictionary<string, SoundEffectInstance> _playingAmbientSounds = new();

    public void HandleAmbientSoundRequest(AmbientPlayMode mode, string soundName,
        SoundEffectSettings soundEffectSettings)
    {
        var sound = GetAmbientSound(soundName);
        if (sound != null)
        {
            if (mode == AmbientPlayMode.Play)
            {
                PlayAmbientSound(soundName, soundEffectSettings);
            }

            if (mode == AmbientPlayMode.Stop)
            {
                StopAmbientSound(soundName);
            }
        }
    }

    private void StopAmbientSound(string soundName)
    {
        var sound = GetAmbientSound(soundName);

        if (sound == null)
        {
            return;
        }

        _playingAmbientSounds.Remove(soundName);
        sound.Stop();
    }

    private void PlayAmbientSound(string soundName, SoundEffectSettings soundEffectSettings)
    {
        var sound = GetAmbientSound(soundName);

        if (sound == null)
        {
            return;
        }
        
        _playingAmbientSounds[soundName] = sound;

        sound.Play(soundEffectSettings with
        {
            Loop = true,
            Cached = true
        });
    }

    private SoundEffectInstance? GetAmbientSound(string soundName)
    {
        if (_playingAmbientSounds.TryGetValue(soundName, out var resultFromAmbient))
        {
            return resultFromAmbient;
        }

        return LdResourceAssets.Instance.SoundInstances.GetValueOrDefault($"Sound/{soundName}");
    }

    public void StopAll()
    {
        foreach (var soundName in _playingAmbientSounds.Keys)
        {
            StopAmbientSound(soundName);
        }
    }
}
