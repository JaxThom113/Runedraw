using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectGA : GameAction
{
    public AudioClip sound { get; set; }

    public SoundEffectGA(AudioClip sound)
    {
        this.sound = sound;
    }
}
