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

public sealed class GossipOverhearModifier : BaseModifier
{
        public GossipOverhearModifier(List<RoleBehaviour> rolesList)
    {
        GossipRoles = rolesList;
    }
    public GossipOverhearModifier(RoleBehaviour[] rolesList)
    {
        GossipRoles = rolesList.ToList();
    }
    public GossipOverhearModifier(string rolesList)
    {
        string[] roleNames = rolesList.Split("|");

        GossipRoles = roleNames.Select(name => DestroyableSingleton<RoleManager>.Instance.AllRoles.ToArray().First(r => r.GetRoleName() == name)).ToList();
    }

        public List<RoleBehaviour> GossipRoles = [];

        public override string ModifierName => "Gossip Target";
        public override bool HideOnUi => true;

        public override void OnActivate()
    {
        base.OnActivate();

        foreach(GossipOverhearModifier? gossipOverhearModifier in PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<GossipOverhearModifier>()).Select(p => p.GetModifier<GossipOverhearModifier>()))
        {
            if(gossipOverhearModifier != this)
            {
                gossipOverhearModifier?.Player.RemoveModifier<GossipOverhearModifier>();
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
        if(!Helpers.GetAlivePlayers().Any(p => p.GetRoleWhenAlive() is GossipRole)) return;

        if(Player == null)
        {
            Error("Player Is Null For Gossip");
            return;
        }        
        
        GossipRole.GenerateGossip(Player, GossipRoles);
        Player.RemoveModifier<GossipOverhearModifier>();

        GossipRoles = new List<RoleBehaviour>();
    }

        public static List<RoleBehaviour> GenerateGossipRoles(PlayerControl player)
    {
        int randRolesCount = (int)OptionGroupSingleton<GossipOptions>.Instance.GossipRoles;
        List<RoleBehaviour> possibleRolesList = new List<RoleBehaviour>();

        List<RoleBehaviour> allRoles = DestroyableSingleton<RoleManager>.Instance.AllRoles.ToArray().Where(delegate (RoleBehaviour r)
        {
            RoleManager.RoleAssignmentData roleData = CustomRoleUtils.GetAssignData(r.Role);

            if (roleData.Count == 0 || roleData.Chance == 0) return false; //Only If It Can Currenlty Be In The Game
            if (!CustomRoleUtils.CanSpawnOnCurrentMode(r)) return false; //Only If It Can Spawn On The Current Mode
            if (r is DeceiverRole) return false; //Can't Be A Role That Logicall Doesn't Make Sense
            if (r is IGhostRole) return false; //No Ghost Roles
            if (r is GossipRole && roleData.Count < 2) return false; //No Gossip Unless Enough Gossips

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

            if(UnityEngine.Random.Range(0, 101) <= OptionGroupSingleton<GossipOptions>.Instance.CrewWeight)
            {
                getableRoles = allRoles.Where(r => r.IsCrewmate()).ToList();

                if (getableRoles.Count == 0)
                {
                    Error("No Roles To Get For Gossip! (Crewmate)");

                    getableRoles = allRoles;
                }
            }
            else
            {
                getableRoles = allRoles.Where(r => !r.IsCrewmate()).ToList();

                if (getableRoles.Count == 0)
                {
                    Error("No Roles To Get For Gossip! (Non-Crew)");

                    getableRoles = allRoles;
                }
            }

            if(getableRoles.Count == 0)
            {
                Error("No Roles To Get For Gossip! (Mid-Picks)");
                break;
            }

            getableRoles.Shuffle();
            RoleBehaviour randomRole = getableRoles[0];

            allRoles.RemoveAll(r => r.GetRoleName() == randomRole.GetRoleName());

            possibleRolesList.Add(randomRole);
        }

        if (possibleRolesList.Count == 0)
        {
            Error("No Roles To Get For Gossip!");
        }

        return possibleRolesList;
    }
}
