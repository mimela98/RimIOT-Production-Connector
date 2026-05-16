using UnityEngine;
using Verse;

namespace RimIOTProductionConnector
{
    public class RimIOTProductionConnectorSettings : ModSettings
    {
        public bool enableRecipeDropIntercept = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableRecipeDropIntercept, "enableRecipeDropIntercept", true);
        }
    }

    public class RimIOTProductionConnectorModSettings : Mod
    {
        public static RimIOTProductionConnectorSettings Settings;

        public RimIOTProductionConnectorModSettings(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RimIOTProductionConnectorSettings>();
        }

        public override string SettingsCategory()
        {
            return "RimIOT - Production Connector";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                "RimIOT_PC_EnableRecipeDropIntercept".Translate(),
                ref Settings.enableRecipeDropIntercept,
                "RimIOT_PC_EnableRecipeDropInterceptDesc".Translate());
            listing.GapLine();
            listing.Label("RimIOT_PC_RestartRequired".Translate());
            listing.End();
        }
    }
}
