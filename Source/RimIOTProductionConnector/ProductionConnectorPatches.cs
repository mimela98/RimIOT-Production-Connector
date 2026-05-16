using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimIOT;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimIOTProductionConnector
{
    [HarmonyPatch(typeof(Building), nameof(Building.SpawnSetup))]
    public static class Patch_Building_SpawnSetup
    {
        public static void Postfix(Building __instance, Map map)
        {
            map?.GetComponent<MapComponent_ProductionConnectorCache>()?.NotifyRelevantBuildingChanged(__instance);
        }
    }

    [HarmonyPatch(typeof(Building), nameof(Building.DeSpawn))]
    public static class Patch_Building_DeSpawn
    {
        public static void Prefix(Building __instance)
        {
            __instance.Map?.GetComponent<MapComponent_ProductionConnectorCache>()?.NotifyRelevantBuildingDespawning(__instance);
        }
    }

    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class Patch_GenRecipe_MakeRecipeProducts
    {
        public static void Postfix(ref IEnumerable<Thing> __result, IBillGiver billGiver)
        {
            if (__result == null || !(billGiver is Building producer))
            {
                return;
            }

            __result = FilterProducts(__result, producer);
        }

        private static IEnumerable<Thing> FilterProducts(IEnumerable<Thing> products, Building producer)
        {
            if (!ProductionConnectorUtility.TryGetActiveNetworkForProducer(producer, out StorageNetwork network, out Map map))
            {
                foreach (var product in products)
                {
                    yield return product;
                }

                yield break;
            }

            foreach (var product in products)
            {
                if (!TryDepositProduct(product, network, map))
                {
                    yield return product;
                }
                else if (product != null && !product.Destroyed && !product.Spawned && product.stackCount > 0)
                {
                    yield return product;
                }
            }
        }

        private static bool TryDepositProduct(Thing product, StorageNetwork network, Map map)
        {
            if (product == null || product.Destroyed)
            {
                return false;
            }

            return ProductionConnectorDeposit.TryPlaceInNetwork(product, network, map);
        }
    }

    [HarmonyPatch(typeof(Toils_Recipe), "CalculateIngredients")]
    public static class Patch_ToilsRecipe_CalculateIngredients
    {
        public static void Prefix(Job job)
        {
            RecipeDropDepositContext.Begin(job);
        }

        public static void Postfix()
        {
            RecipeDropDepositContext.End();
        }

        public static void Finalizer()
        {
            RecipeDropDepositContext.End();
        }
    }

    [HarmonyPatch(typeof(GenPlace), nameof(GenPlace.TryPlaceThing), new[]
    {
        typeof(Thing),
        typeof(IntVec3),
        typeof(Map),
        typeof(ThingPlaceMode),
        typeof(System.Action<Thing, int>),
        typeof(System.Predicate<IntVec3>),
        typeof(System.Nullable<Rot4>),
        typeof(int)
    })]
    public static class Patch_GenPlace_TryPlaceThing_NoResult
    {
        public static bool Prefix(Thing thing, Map map, ref bool __result)
        {
            if (!RecipeDropDepositContext.Active)
            {
                return true;
            }

            if (RecipeDropDepositContext.TryDeposit(thing, map))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    public static class Patch_GenPlace_TryPlaceThing_WithResult
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(GenPlace), nameof(GenPlace.TryPlaceThing), new[]
            {
                typeof(Thing),
                typeof(IntVec3),
                typeof(Map),
                typeof(ThingPlaceMode),
                typeof(Thing).MakeByRefType(),
                typeof(System.Action<Thing, int>),
                typeof(System.Predicate<IntVec3>),
                typeof(System.Nullable<Rot4>),
                typeof(int)
            });
        }

        public static bool Prefix(Thing thing, Map map, ref Thing lastResultingThing, ref bool __result)
        {
            if (!RecipeDropDepositContext.Active)
            {
                return true;
            }

            if (RecipeDropDepositContext.TryDeposit(thing, map))
            {
                lastResultingThing = thing;
                __result = true;
                return false;
            }

            return true;
        }
    }
}
