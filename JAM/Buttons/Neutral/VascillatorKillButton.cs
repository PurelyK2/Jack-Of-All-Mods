using AmongUs.GameOptions;
using Il2CppSystem.Web.Util;
using JAM.Options.Roles.Neutral;
using JAM.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Networking;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Patches.Options;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Buttons.Neutral;

public sealed class VascillatorKillButton : TownOfUsKillRoleButton<VacillatorRole, PlayerControl>, IKillButton
{
    public override float Cooldown => GameManager.Instance.LogicOptions.GetKillCooldown();
    public override string Name => "Kill";
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Indecisive Kill: Target is null");
            return;
        }


        if(Role.SafeShotsLeft > 0)
        {
            Role.winsWithCrew = true;

            if (Target.IsCrewmate() && !Target.HasModifier<AllianceGameModifier>())
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Killed A Crewmate. You Now Win With Non-Crew.", Color.white, null, Role.GetRoleIcon());
                
                Role.winsWithCrew = false;
            }
            else
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Killed A Non-Crew. You Now Win With Crewmates.", Color.white, null, Role.GetRoleIcon());
            }
            Role.SafeShotsLeft--;

            PlayerControl.LocalPlayer.RpcCustomMurder(Target);
            return;
        }

        if(Target.IsCrewmate() && !Target.HasModifier<AllianceGameModifier>() && Role.winsWithCrew)
        {
            //Killed Crewmate on crew team, misfire
            IndecisiveMisfire();
            return;
        }
        else if(!Role.winsWithCrew && (!Target.IsCrewmate() || Target.HasModifier<AllianceGameModifier>()))
        {
            //Killed Non-Crew On Non-Crew Team, misfire
            IndecisiveMisfire();
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }

    void IndecisiveMisfire()
    {
        MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Tried To Kill Someone On Your Team, So You Died.", Color.white, null, Role.GetRoleIcon());
        
        PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer, MeetingCheck.OutsideMeeting, true, true, true, true, true, true);
        Coroutines.Start(MiscUtils.CoFlash(Color.red, 1f, 0.3f));
    }
}
