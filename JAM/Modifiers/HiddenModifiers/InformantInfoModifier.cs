using MiraAPI.Modifiers;
using JAM.Roles.Crewmate;
using TownOfUs.Modules;
using MiraAPI.GameOptions;
using JAM.Options.Roles.Crewmate;
using MiraAPI.Roles;
using TownOfUs.Utilities;
using MiraAPI.Utilities;
using AmongUs.GameOptions;
using TMPro;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Extensions;

namespace JAM.Modifiers.Crewmate;

public sealed class InformantInfoModifier : BaseModifier
{
    public InformantInfoModifier(List<RoleBehaviour> rolesList)
    {
        InformantRoles = rolesList;
    }
    public InformantInfoModifier(RoleBehaviour[] rolesList)
    {
        InformantRoles = rolesList.ToList();
    }
    public InformantInfoModifier(string rolesList)
    {
        string[] roleNames = rolesList.Split("|");

        InformantRoles = roleNames.Select(name => DestroyableSingleton<RoleManager>.Instance.AllRoles.ToArray().First(r => r.GetRoleName() == name)).ToList();
    }

    public List<RoleBehaviour> InformantRoles = [];

    public override string ModifierName => "Informant Target";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();

        foreach(InformantInfoModifier? InformantInfoModifier in PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<InformantInfoModifier>()).Select(p => p.GetModifier<InformantInfoModifier>()))
        {
            if(InformantInfoModifier != this)
            {
                InformantInfoModifier?.Player.RemoveModifier<InformantInfoModifier>();
            }
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        Player.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        if(!Helpers.GetAlivePlayers().Any(p => p.GetRoleWhenAlive() is InformantRole)) return;

        if(Player == null)
        {
            Error("Player Is Null For Informant");
            return;
        }        
        
        InformantRole.GenerateInfo(Player, InformantRoles);
        Player.RemoveModifier<InformantInfoModifier>();

        InformantRoles = new List<RoleBehaviour>();
    }

    public static List<RoleBehaviour> GenerateInfoRoles(PlayerControl player)
    {
        int randRolesCount = (int)OptionGroupSingleton<InformantOptions>.Instance.InformantRoles;
        List<RoleBehaviour> possibleRolesList = new List<RoleBehaviour>();

        List<RoleBehaviour> allRoles = DestroyableSingleton<RoleManager>.Instance.AllRoles.ToArray().Where(delegate (RoleBehaviour r)
        {
            RoleManager.RoleAssignmentData roleData = CustomRoleUtils.GetAssignData(r.Role);

            if (roleData.Count == 0 || roleData.Chance == 0) return false; //Only If It Can Currenlty Be In The Game
            if (!CustomRoleUtils.CanSpawnOnCurrentMode(r)) return false; //Only If It Can Spawn On The Current Mode
            if (r is DeceiverRole) return false; //Can't Be A Role That Logicall Doesn't Make Sense
            if (r is IGhostRole) return false; //No Ghost Roles
            if (r is InformantRole && roleData.Count < 2) return false; //No Informant Unless Enough Informants

            return true; //Will Be Ok Here
        }).ToList();

        if (player.HasModifier<ImitatorCacheModifier>())
        {
            possibleRolesList.Add(allRoles.First(r => r is ImitatorRole));
        }
        else
        {
            possibleRolesList.Add(player.Data.Role);
        }

        allRoles.RemoveAll(r => possibleRolesList.Any(role => role.GetRoleName() == r.GetRoleName()));

        for (int i = 0; i < randRolesCount; i++)
        {
            var getableRoles = new List<RoleBehaviour>();

            if(UnityEngine.Random.Range(0, 101) <= OptionGroupSingleton<InformantOptions>.Instance.CrewWeight)
            {
                getableRoles = allRoles.Where(r => r.IsCrewmate()).ToList();

                if (getableRoles.Count == 0)
                {
                    Error("No Roles To Get For Informant! (Crewmate)");

                    getableRoles = allRoles;
                }
            }
            else
            {
                getableRoles = allRoles.Where(r => !r.IsCrewmate()).ToList();

                if (getableRoles.Count == 0)
                {
                    Error("No Roles To Get For Informant! (Non-Crew)");

                    getableRoles = allRoles;
                }
            }

            if(getableRoles.Count == 0)
            {
                Error("No Roles To Get For Informant! (Mid-Picks)");
                break;
            }

            getableRoles.Shuffle();
            RoleBehaviour randomRole = getableRoles[0];

            allRoles.RemoveAll(r => r.GetRoleName() == randomRole.GetRoleName());

            possibleRolesList.Add(randomRole);
        }

        if (possibleRolesList.Count == 0)
        {
            Error("No Roles To Get For Informant!");
        }

        return possibleRolesList;
    }
}
