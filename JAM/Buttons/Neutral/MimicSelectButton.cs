using AmongUs.GameOptions;
using HarmonyLib;
using JAM.Modifiers.Role;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Buttons.Neutral;

public sealed class MimicSelectButton : TownOfUsRoleButton<MimicRole>
{
    public override float Cooldown => 0;
    static List<RoleBehaviour> ChosenRoles { get; } = [];
    RoleBehaviour? RandomRole;

    public override LoadableAsset<Sprite> Sprite => TouRoleIcons.Agent;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => Colors.Mimic;

    public override void ClickHandler()
    {
        if (!CanClick() || Minigame.Instance || PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        if (ChosenRoles.Count == 0)
        {
            List<RoleBehaviour> roleList = DestroyableSingleton<RoleManager>.Instance.AllRoles.ToArray().Where(delegate (RoleBehaviour r)
            {
                RoleManager.RoleAssignmentData roleData = CustomRoleUtils.GetAssignData(r.Role);

                if (roleData.Count == 0 || roleData.Chance == 0) return false; // Only If It Can Currenlty Be In The Game
                if (!CustomRoleUtils.CanSpawnOnCurrentMode(r)) return false; // Only If It Can Spawn On The Current Mode
                if (r is IGhostRole) return false; // No Ghost Roles
                if (r is MimicRole) return false; // Not Self
                if (r is IAssignableTargets) return false; // Not A Targeting One (eg. Exe or Fairy)
                if (r is MercenaryRole) return false; // Technically Not A Targeting One
                if (r is IDoubleDraftRole) return false; // For Things Like Recruiter And Vampire
                if (r.GetRoleAlignment() == RoleAlignment.NeutralOutlier) return false; // Removing Chef, Inquiz, etc.

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
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{Colors.Mimic.ToTextColor()}No roles are available for the taking.</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Colorblind.LoadAsset());

                notif1.AdjustNotification();
                return;
            }

            var mimicMenu = MimicSelectMinigame.Create();
            mimicMenu.Open(
                ChosenRoles,
                role =>
                {
                    if (!Role.Player.HasModifier<MimicCacheModifier>())
                        Role.Player.RpcAddModifier<MimicCacheModifier>();
                    Role.Player.RpcChangeRole((ushort) role.Role, false);
                    mimicMenu.Close();
                },
                RandomRole?.Role
            );
        }
    }

    [RegisterEvent(0)]
    public static void OnRoundStartEvent(RoundStartEvent @event)
    {
        ChosenRoles.RemoveAll(x => true);
    }
}
