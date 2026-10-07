using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JAM.Assets;

public static class JamAudio
{
    private const string ShortPath = "JAM.Resources.Audio";

    public static LoadableAsset<AudioClip> JackOfAllIntro => new LoadableAudioResourceAsset($"{ShortPath}.JackOfAllIntro.wav");
    public static LoadableAsset<AudioClip> TimeKeeperIntro => new LoadableAudioResourceAsset($"{ShortPath}.TimeKeeperIntro.wav");
}
