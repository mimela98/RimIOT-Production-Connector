using RimWorld;
using Verse;

namespace RimIOTProductionConnector
{
    [DefOf]
    public static class RimIOTPCDefOf
    {
        public static ThingDef RimIOT_ProductionConnector;

        static RimIOTPCDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RimIOTPCDefOf));
        }
    }
}
