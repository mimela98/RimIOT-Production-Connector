using System;
using System.Reflection;
using HarmonyLib;
using RimIOT;
using Verse;

namespace RimIOTProductionConnector
{
    public static class ProductionConnectorDeposit
    {
        private delegate bool TryPlaceInNetworkDelegate(Thing thing, StorageNetwork network, Map map, out IntVec3 placedCell);

        private static MethodInfo tryPlaceInNetwork;
        private static TryPlaceInNetworkDelegate tryPlaceInNetworkDelegate;
        private static bool warnedMissing;
        private static bool warnedException;

        public static void Initialize()
        {
            var helperType = AccessTools.TypeByName("RimIOT.DepositHelper");
            if (helperType == null)
            {
                WarnMissing();
                return;
            }

            tryPlaceInNetwork = AccessTools.Method(
                helperType,
                "TryPlaceInNetwork",
                new[] { typeof(Thing), typeof(StorageNetwork), typeof(Map), typeof(IntVec3).MakeByRefType() });

            if (tryPlaceInNetwork == null)
            {
                WarnMissing();
                return;
            }

            try
            {
                tryPlaceInNetworkDelegate = AccessTools.MethodDelegate<TryPlaceInNetworkDelegate>(tryPlaceInNetwork);
            }
            catch
            {
                tryPlaceInNetworkDelegate = null;
            }
        }

        public static bool TryPlaceInNetwork(Thing thing, StorageNetwork network, Map map)
        {
            if (thing == null || thing.Destroyed || network == null || map == null)
            {
                return false;
            }

            if (tryPlaceInNetwork == null)
            {
                WarnMissing();
                return false;
            }

            try
            {
                if (tryPlaceInNetworkDelegate != null)
                {
                    return tryPlaceInNetworkDelegate(thing, network, map, out _);
                }

                object[] args = { thing, network, map, IntVec3.Invalid };
                return (bool)tryPlaceInNetwork.Invoke(null, args);
            }
            catch (Exception ex)
            {
                if (!warnedException)
                {
                    warnedException = true;
                    Log.Warning("[RimIOT Production Connector] TryPlaceInNetwork reflection call failed. Products will fall back to vanilla placement. " + ex);
                }

                return false;
            }
        }

        private static void WarnMissing()
        {
            if (warnedMissing)
            {
                return;
            }

            warnedMissing = true;
            Log.Warning("[RimIOT Production Connector] RimIOT.DepositHelper.TryPlaceInNetwork was not found. Products will fall back to vanilla placement.");
        }
    }
}
