using System.Linq;
using System.Text;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using JAM.Options.Roles.Crewmate;
using JAM.Patches;

namespace JAM.Modifiers.Hidden;

// public sealed class MorticianRevealModifier(RoleBehaviour role) : BaseRevealModifier
public sealed class MorticianRevealModifier : BaseRevealModifier
{
    public override string ModifierName => "Mortician Revealed";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override bool RevealRole { get; set; } = true;

    // private RoleBehaviour? RevealedRole
    // {
    //     get
    //     {
    //         if (Player == null)
    //         {
    //             return role;
    //         }

    //         var fromDict = MementoModifier.ResolveRoleBeforeDeath(Player.PlayerId);
    //         if (fromDict != null)
    //         {
    //             return fromDict;
    //         }

    //         var fromHistory = Player.GetRoleWhenAlive();
    //         if (fromHistory != null)
    //         {
    //             return fromHistory;
    //         }

    //         return role;
    //     }
    // }
    
    private RoleBehaviour? RevealedRole
    {
        get
        {
            if (Player == null)
            {
                return null;
            }

            var fromDict = MorticianPatch.ResolveRoleBeforeDeath(Player.PlayerId);

            if (fromDict != null)
            {
                return fromDict;
            }

            return Player.GetRoleWhenAlive();
        }
    }

    public override RoleBehaviour? ShownRole
    {
        get => RevealedRole;
        set { }
    }

    // Only living local viewers see the Memento reveal; the dead (incl. the holder
    // themselves) get the unmodified vanilla / TouMira display.
    public override bool Visible
    {
        get => PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.HasDied();
        set { }
    }

    public override string ExtraNameText
    {
        get
        {
            if (!OptionGroupSingleton<MorticianOptions>.Instance.ShowModifiers || Player == null)
            {
                return string.Empty;
            }

            return HeldModifiersText(Player);
        }
        set { }
    }

    private static string HeldModifiersText(PlayerControl pc)
    {
        var modifiers = pc.GetModifiers<GameModifier>()
            .Where(x => x is not ExcludedGameModifier)
            .OrderBy(x => x.ModifierName)
            .ToList();

        var builder = new StringBuilder();
        var first = true;
        foreach (var modifier in modifiers)
        {
            builder.Append(first ? "\n<size=55%>(" : ", ");
            first = false;
            var color = MiscUtils.GetModifierColour(modifier);
            builder.Append($"{color.ToTextColor()}{modifier.ModifierName}</color>");
        }

        if (first)
        {
            return string.Empty;
        }

        builder.Append(")</size>");
        return builder.ToString();
    }
}
