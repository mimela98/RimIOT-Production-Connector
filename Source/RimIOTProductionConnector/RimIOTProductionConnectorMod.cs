using System.Reflection;
using HarmonyLib;
using Verse;

namespace RimIOTProductionConnector
{
    [StaticConstructorOnStartup]
    public static class RimIOTProductionConnectorMod
    {
        static RimIOTProductionConnectorMod()
        {
            new Harmony("local.rimiot.productionconnector").PatchAll(Assembly.GetExecutingAssembly());
            ProductionConnectorDeposit.Initialize();
            Log.Message("[RimIOT Production Connector] Harmony patches applied.");
        }
    }
}
