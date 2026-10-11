using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HOOD
{
    // Values saved to the mod settings file
    public class HOODSettingsData : ModSettings
    {
        public bool checkPower = true;
        public bool disableOnDoors = false;
        public bool enableLogging = false;

        // Door defName => whether the door is excluded from instant opening/closing.
        // A door with no entry uses its default (see HOODSettings.DefaultDisallowed).
        public Dictionary<string, bool> doors = new Dictionary<string, bool>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref checkPower, "checkPower", true);
            Scribe_Values.Look(ref disableOnDoors, "disableOnDoors", false);
            Scribe_Values.Look(ref enableLogging, "enableLogging", false);
            Scribe_Collections.Look(ref doors, "doors", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && doors == null)
                doors = new Dictionary<string, bool>();
        }
    }

    // Runtime state read by the door patches
    public static class HOODSettings
    {
        public static List<string> disallowedDoors = new List<string>();
        public static bool disableOnDoors = false;
        public static bool checkPower = true;

        // Door defs found at startup; filled by DoorScanner
        public static List<ThingDef> doorDefs = new List<ThingDef>();

        // Powered doors are excluded by default
        public static bool DefaultDisallowed(ThingDef def) => def.HasComp(typeof(CompPower));

        public static bool IsDisallowed(HOODSettingsData data, ThingDef def)
        {
            return data.doors.TryGetValue(def.defName, out bool value) ? value : DefaultDisallowed(def);
        }

        // Copy the saved values into the fields the patches read
        public static void Apply()
        {
            var data = HOODMod.Data;
            if (data == null) return;

            checkPower = data.checkPower;
            disableOnDoors = data.disableOnDoors;
            L.loggingEnabled = data.enableLogging;
            disallowedDoors = doorDefs.Where(def => IsDisallowed(data, def)).Select(def => def.defName).ToList();

            L.Log("Settings applied: checkPower (" + checkPower + ") - disableOnDoors (" + disableOnDoors
                + ") - disallowedDoors (" + string.Join(",", disallowedDoors) + ")");
        }
    }

    // Finds the doors once all defs are loaded (replaces HugsLib's DefsLoaded)
    [StaticConstructorOnStartup]
    static class DoorScanner
    {
        static DoorScanner()
        {
            HOODSettings.doorDefs = FindDoors();
            HOODSettings.Apply();
        }

        // A door is any building whose class has a holdOpenInt field (vanilla and modded doors)
        private static List<ThingDef> FindDoors()
        {
            L.Log("Finding doors...");

            var doors = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.building == null || def.thingClass == null) continue;
                if (AccessTools.Field(def.thingClass, "holdOpenInt") == null) continue;

                // Jecrell's invisible door is actioned like any other door, but it has no useful entry in the settings
                if (def.label != null && def.label.ToLower().Equals("invisible door")) continue;

                L.Log("Found a door - " + def.label + " in the assembly " + def.thingClass);
                doors.Add(def);
            }

            L.Log("Found " + doors.Count + " doors");
            return doors;
        }
    }

    // Mod entry point and settings window (replaces HugsLib's ModBase)
    public class HOODMod : Mod
    {
        public static HOODSettingsData Data;

        private Vector2 scrollPosition;
        private float viewHeight = 1000f;

        public HOODMod(ModContentPack content) : base(content)
        {
            Data = GetSettings<HOODSettingsData>();
        }

        public override string SettingsCategory() => "Hold Open Opens Doors";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var doors = HOODSettings.doorDefs;
            var viewRect = new Rect(0f, 0f, inRect.width - 20f, viewHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            listing.CheckboxLabeled("Enable power check", ref Data.checkPower,
                "When enabled this checks that the door has power to open/close (for added realism)");
            listing.CheckboxLabeled("Disable on selected doors", ref Data.disableOnDoors,
                "When enabled this stops immediate opening/closing on the selected doors below (for added realism)");
            listing.CheckboxLabeled("Enable logging", ref Data.enableLogging, "Write debug messages to the log");

            listing.GapLine();
            listing.Label("Selected doors (only used when \"Disable on selected doors\" is on):");
            if (doors.Count == 0)
                listing.Label("No doors found.");

            foreach (ThingDef def in doors.OrderBy(d => d.label))
            {
                bool disallowed = HOODSettings.IsDisallowed(Data, def);
                bool before = disallowed;
                listing.CheckboxLabeled(def.LabelCap, ref disallowed, def.description);
                if (disallowed != before)
                    Data.doors[def.defName] = disallowed;
            }

            viewHeight = listing.CurHeight;
            listing.End();
            Widgets.EndScrollView();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            HOODSettings.Apply();
        }
    }
}
