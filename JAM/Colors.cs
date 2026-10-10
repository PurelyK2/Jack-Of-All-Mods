using MiraAPI.Utilities;
using TownOfUs;
using UnityEngine;

namespace JAM;

public static class Colors
{
    // Crew Colors
    public static Color Gossip => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(255, 237, 162, byte.MaxValue);
    public static Color JackOfAll => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : Color.white;
    public static Color Snoop => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(97, 147, 212, byte.MaxValue);

    //Neutral Colors
    public static Color Scrubber => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(97, 147, 212, byte.MaxValue);
    public static Color BountyHunter => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(24, 102, 31, byte.MaxValue);
    public static Color TimeKeeper => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(230, 242, 200, byte.MaxValue);
    public static Color Zombie => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(84, 192, 113, byte.MaxValue);
    public static Color Mimic => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(200, 200, 0, byte.MaxValue);

    //Modifiers
    public static Color Blind => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : Color.grey;
    public static Color Hyperfocus => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(0, 60, 95, byte.MaxValue);
    public static Color Unstable => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(57, 255, 20, byte.MaxValue);
    public static Color Ventable => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(88, 90, 204, byte.MaxValue);
    public static Color Concealed => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(100, 100, 100, byte.MaxValue);

    //Null
    public static Color Micromanager => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(38, 104, 148, 255);
    public static Color Mortician => TownOfUsColors.UseBasic ? Palette.CrewmateBlue : new Color32(0, 68, 218, 255);
    public static Color Shackled => new Color32(145, 155, 155, 255);
    public static Color Exposed => new Color32(220, 175, 51, 255);
    public static Color Workaholic => new Color32(124, 142, 158, 255);
    public static Color Battery => new Color32(174, 218, 27, 255);
}
