using JAM.Assets;
using JAM.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers;

public sealed class FoolishModifier : UniversalGameModifier, IContinuesGame
{
    public override void Update()
    {
        base.Update();

        if(MeetingHud.Instance?.discussionTimer > 0 && MeetingHud.Instance?.discussionTimer <= OptionGroupSingleton<FoolishOptions>.Instance.FoolishTime)
        {
            Info(MeetingHud.Instance.discussionTimer);
        }
    }

    public override string ModifierName => "Foolish";

    public override string GetDescription()
    {
        return "Vote Quickly To Get Double Votes";
    }

    public string GetAdvancedDescription()
    {
        return GetDescription() + MiscUtils.AppendOptionsText(base.GetType());
    }

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<FoolishOptions>.Instance.FoolishChance;
    }

    public override ModifierUiConfiguration Configuration
    {
        get
        {
            return new ModifierUiConfiguration(Colors.Foolish, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Foolish.LoadAsset(), "Foolish", 1.45f));
        }
    }

    public override bool HideOnUi => false;

    public override LoadableAsset<Sprite> ModifierIcon => JamModifierIcons.Foolish;

    public override int GetAmountPerGame()
    {
        return CustomAmount;
    }

    public override int CustomAmount => (int)OptionGroupSingleton<FoolishOptions>.Instance.FoolishAmount;

    public override int CustomChance => (int)OptionGroupSingleton<FoolishOptions>.Instance.FoolishChance;

    public bool ContinuesGame //Same As Tiebreaker
    {
        get
        {
            if (Player.HasDied() || Player.IsImpostorAligned())
            {
                return false;
            }
            if ((!Player.IsCrewmate() || Helpers.GetAlivePlayers().Count(x => x.IsCrewmate()) == 1) &&
                Player.Data.Role is ITownOfUsRole touRole &&
                touRole.RoleAlignment is not RoleAlignment.NeutralKilling && Helpers.GetAlivePlayers().Count < 4 &&
                Helpers.GetAlivePlayers().Count > 1)
            {
                return touRole.CanModifierContinueGame(this);
            }

            return false;
        }
    }
}
