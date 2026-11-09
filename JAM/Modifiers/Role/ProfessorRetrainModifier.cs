using AmongUs.GameOptions;
using Il2CppSystem.Web.Util;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers.Role;

internal class ProfessorRetrainModifier : BaseModifier
{
    static List<RoleBehaviour> ChosenRoles = [];
    static RoleBehaviour? RandomRole;

    public override string ModifierName => "Professor Retrain Modifier";
    public override bool HideOnUi => true;

    [RegisterEvent(0)]
    public static void OnRoundStartEvent(RoundStartEvent @event)
    {
        if (!PlayerControl.LocalPlayer.HasModifier<ProfessorRetrainModifier>()) return;

        if(!MiraAPI.Utilities.Helpers.GetAlivePlayers().Any(p => p.Data.Role is ProfessorRole))
        {
            PlayerControl.LocalPlayer.RpcRemoveModifier<ProfessorRetrainModifier>();
        }    

        if (ChosenRoles.Count == 0)
        {
            List<RoleBehaviour> roleList = DestroyableSingleton<RoleManager>.Instance.AllRoles.ToArray().Where(delegate (RoleBehaviour r)
            {
                if (r.GetRoleAlignment() != PlayerControl.LocalPlayer.Data.Role.GetRoleAlignment()) return false; // Only Roles In The Same Faction

                RoleManager.RoleAssignmentData roleData = CustomRoleUtils.GetAssignData(r.Role);

                if (roleData.Count == 0 || roleData.Chance == 0) return false; // Only If It Can Currenlty Be In The Game
                if (!CustomRoleUtils.CanSpawnOnCurrentMode(r)) return false; // Only If It Can Spawn On The Current Mode
                if (r is IDoubleDraftRole) return false; // For Things Like Recruiter And Vampire
                if (r.Role == PlayerControl.LocalPlayer.Data.Role.Role) return false; // Can't Turn Into Your Role
                if (r.Role == (RoleTypes)RoleId.Get<ProfessorRole>()) return false; // Can't Turn Into Professor

                return true; //Will Be Ok Here
            }).ToList();

            roleList.Shuffle();
            var random = roleList[0];

            for (var i = 0; i < 3; i++)
            {
                roleList.Shuffle();
                var selected = roleList[0];
                if (selected == null)
                {
                    continue;
                }

                ChosenRoles.Add(selected);
                roleList.Remove(selected);
            }

            RandomRole = random;
        }

        if (!Minigame.Instance)
        {
            if (ChosenRoles.Count == 0)
            {
                var notif1 = MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b>{Colors.Professor.ToTextColor()}No roles are available for the taking.</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Agent.LoadAsset());

                notif1.AdjustNotification();
                return;
            }

            var professorConfirm = ProfessorConfirmMinigame.Create();

            professorConfirm.Open(
                (confirm) =>
                {
                    professorConfirm.Close();

                    ConfirmProfessor(confirm);
                }
            );
        }

        PlayerControl.LocalPlayer.RpcRemoveModifier<ProfessorRetrainModifier>();
    }
    public override void OnMeetingStart()
    {
        ModifierComponent.RemoveModifier(this);
    }
    static void ConfirmProfessor(bool confirmed)
    {
        if (confirmed)
        {
            var professorMenu = ProfessorSelectMinigame.Create();
            professorMenu.Open(
                ChosenRoles,
                role =>
                {
                    PlayerControl.LocalPlayer.RpcChangeRole((ushort)role.Role);
                    professorMenu.Close();
                    ChosenRoles.RemoveAll(r => true);
                },
                RandomRole?.Role
            );
        }
    }
}
