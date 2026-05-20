using RimIOT;
using Verse;

namespace RimIOTProductionConnector
{
    public static class ProductionConnectorDeposit
    {
        private static bool warnedException;

        public static void Initialize()
        {
        }

        public static bool TryPlaceInNetwork(Thing thing, StorageNetwork network, Map map)
        {
            if (thing == null || thing.Destroyed || network == null || map == null)
            {
                return false;
            }

            try
            {
                return RimIOTApi.TryPlaceInNetwork(thing, network, map, out _);
            }
            catch (System.Exception ex)
            {
                if (!warnedException)
                {
                    warnedException = true;
                    Log.Warning("[RimIOT Production Connector] RimIOTApi.TryPlaceInNetwork failed. Products will fall back to vanilla placement. " + ex);
                }

                return false;
            }
        }
    }
}
