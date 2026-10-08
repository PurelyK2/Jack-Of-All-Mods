using JAM.Modifiers;
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

namespace JAM.Options.Modifiers;

public sealed class BlindOptions : AbstractOptionGroup<BlindModifier>
{
    public override string GroupName => "Blind Options";

    [ModdedNumberOption("Blind Count", 0f, 5f, 1f)]
    public float BlindCount { get; set; } = 1f;
    
    [ModdedNumberOption("Blind Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float BlindChance { get; set; } = 50f;

    [ModdedNumberOption("Blind Amount", 5f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float BlindAmount { get; set; } = 30f;
}
