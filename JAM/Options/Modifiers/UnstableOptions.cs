using JAM.Modifiers.Game.Universal;
using System;
using System.Runtime.CompilerServices;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules.Localization;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace JAM.Options.Modifiers.UniversalModifierOptions;

public sealed class UnstableOptions : AbstractOptionGroup<UnstableModifier>
{
        public override string GroupName => "Unstable Options";

        [ModdedNumberOption("Unstable Count", 0f, 5f, 1f)]
    public float UnstableCount { get; set; } = 1f;

        [ModdedNumberOption("Unstable Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float UnstableChance { get; set; } = 50f;

        [ModdedNumberOption("Minimum TP Cooldown", 0f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float UnstableMinCooldown { get; set; } = 30f;

        [ModdedNumberOption("Maximum TP Cooldown", 5f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float UnstableMaxCooldown { get; set; } = 100f;
}
