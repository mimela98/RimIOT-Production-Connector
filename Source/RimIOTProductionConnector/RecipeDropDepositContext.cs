using RimWorld;
using Verse;
using Verse.AI;

namespace RimIOTProductionConnector
{
    public static class RecipeDropDepositContext
    {
        [System.ThreadStatic]
        private static RimIOT.StorageNetwork activeNetwork;

        [System.ThreadStatic]
        private static Map activeMap;

        [System.ThreadStatic]
        private static bool depositing;

        public static bool Active => activeNetwork != null && activeMap != null;

        public static bool IsDepositing => depositing;

        public static void Begin(Job job)
        {
            activeNetwork = null;
            activeMap = null;

            if (job?.targetA.Thing is Building producer)
            {
                ProductionConnectorUtility.TryGetActiveNetworkForProducer(producer, out activeNetwork, out activeMap);
            }
        }

        public static void End()
        {
            activeNetwork = null;
            activeMap = null;
        }

        public static bool TryDeposit(Thing thing, Map map)
        {
            if (depositing || !Active || thing == null || thing.Destroyed || map == null || activeMap != map)
            {
                return false;
            }

            depositing = true;
            try
            {
                return ProductionConnectorDeposit.TryPlaceInNetwork(thing, activeNetwork, activeMap);
            }
            finally
            {
                depositing = false;
            }
        }
    }
}
