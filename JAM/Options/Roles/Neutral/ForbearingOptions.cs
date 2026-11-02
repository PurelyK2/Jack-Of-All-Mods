using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Roles.Neutral;

namespace JAM.Options.Roles.Neutral;

public sealed class ForbearingOptions : AbstractOptionGroup<ForbearingRole>
{
    public override string GroupName => "Time Keeper Options";
    
    [ModdedNumberOption("Kill Cooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ForbearingCooldown { get; set; } = 20f;

    [ModdedToggleOption("Cooldown Decreases Each Tie")]
    public bool AlwaysDecreaseCd { get; set; } = true;
    public ModdedNumberOption CooldownDecrease { get; set; } = new("Cooldown Decrease", 5f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<ForbearingOptions>.Instance.AlwaysDecreaseCd
    };

    [ModdedToggleOption("Time Keeper Notifies")]
    public bool TimeNotif { get; set; } = true;

    [ModdedToggleOption("Forbearing Starts With Kill Button")]
    public bool StartWithKillButton { get; set; } = false;
}
