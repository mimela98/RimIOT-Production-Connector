using System;
using System.Collections.Generic;
using System.Linq;
using RimIOT;
using RimWorld;
using Verse;

namespace RimIOTProductionConnector
{
    public enum ProductionConnectorState
    {
        Active,
        NoProducer,
        NoNetwork,
        NoPower,
        FlickedOff,
        NetworkUnpowered
    }

    public readonly struct ProductionConnectorStatus
    {
        public ProductionConnectorStatus(ProductionConnectorState state, Building producer, bool multipleConnectors)
        {
            State = state;
            Producer = producer;
            MultipleConnectors = multipleConnectors;
        }

        public ProductionConnectorState State { get; }
        public Building Producer { get; }
        public bool MultipleConnectors { get; }

        public string Key
        {
            get
            {
                switch (State)
                {
                    case ProductionConnectorState.Active:
                        return "RimIOT_PC_StatusActive";
                    case ProductionConnectorState.NoNetwork:
                        return "RimIOT_PC_StatusNoNetwork";
                    case ProductionConnectorState.NoPower:
                        return "RimIOT_PC_StatusNoPower";
                    case ProductionConnectorState.FlickedOff:
                        return "RimIOT_PC_StatusFlickedOff";
                    case ProductionConnectorState.NetworkUnpowered:
                        return "RimIOT_PC_StatusNetworkUnpowered";
                    default:
                        return "RimIOT_PC_StatusNoProducer";
                }
            }
        }
    }

    public static class ProductionConnectorUtility
    {
        public static bool IsProductionBuilding(Building building)
        {
            if (building == null || building is Building_ProductionConnector)
            {
                return false;
            }

            return building is IBillGiver giver && giver.BillStack != null;
        }

        public static bool IsConnector(Building building)
        {
            return building is Building_ProductionConnector;
        }

        public static ProductionConnectorStatus GetStatus(Building_ProductionConnector connector)
        {
            var producer = connector?.Map?.GetComponent<MapComponent_ProductionConnectorCache>()?.GetProducer(connector);
            var multiple = producer != null && connector.Map.GetComponent<MapComponent_ProductionConnectorCache>().GetConnectors(producer).Count > 1;

            if (producer == null)
            {
                return new ProductionConnectorStatus(ProductionConnectorState.NoProducer, null, false);
            }

            if (!IsFlickedOn(connector))
            {
                return new ProductionConnectorStatus(ProductionConnectorState.FlickedOff, producer, multiple);
            }

            if (!IsPowered(connector))
            {
                return new ProductionConnectorStatus(ProductionConnectorState.NoPower, producer, multiple);
            }

            var mapComp = connector.Map.GetComponent<MapComponent_NetworkManager>();
            var net = mapComp?.GetNetworkFor(connector);
            if (net == null)
            {
                return new ProductionConnectorStatus(ProductionConnectorState.NoNetwork, producer, multiple);
            }

            if (!mapComp.IsNetworkPowered(net))
            {
                return new ProductionConnectorStatus(ProductionConnectorState.NetworkUnpowered, producer, multiple);
            }

            return new ProductionConnectorStatus(ProductionConnectorState.Active, producer, multiple);
        }

        public static bool TryGetActiveNetwork(Building_ProductionConnector connector, out StorageNetwork network, out Map map)
        {
            network = null;
            map = connector?.Map;
            if (connector == null || map == null || !IsFlickedOn(connector) || !IsPowered(connector))
            {
                return false;
            }

            var mapComp = map.GetComponent<MapComponent_NetworkManager>();
            if (mapComp == null)
            {
                return false;
            }

            network = mapComp.GetNetworkFor(connector);
            return network != null && mapComp.IsNetworkPowered(network);
        }

        public static Building_ProductionConnector FirstActiveConnectorFor(Building producer)
        {
            var cache = producer?.Map?.GetComponent<MapComponent_ProductionConnectorCache>();
            if (cache == null)
            {
                return null;
            }

            foreach (var connector in cache.GetConnectors(producer))
            {
                if (TryGetActiveNetwork(connector, out _, out _))
                {
                    return connector;
                }
            }

            return null;
        }

        public static bool TryGetActiveNetworkForProducer(Building producer, out StorageNetwork network, out Map map)
        {
            network = null;
            map = null;

            var cache = producer?.Map?.GetComponent<MapComponent_ProductionConnectorCache>();
            if (cache == null)
            {
                return false;
            }

            foreach (var connector in cache.GetConnectors(producer))
            {
                if (TryGetActiveNetwork(connector, out network, out map))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFlickedOn(Thing thing)
        {
            var flickable = thing?.TryGetComp<CompFlickable>();
            return flickable == null || flickable.SwitchIsOn;
        }

        public static bool IsPowered(Thing thing)
        {
            var power = thing?.TryGetComp<CompPowerTrader>();
            return power == null || power.PowerOn;
        }

        public static List<Building_ProductionConnector> ConnectorsUnder(Building producer, IEnumerable<Building_ProductionConnector> connectors)
        {
            var rect = producer.OccupiedRect();
            return connectors.Where(c => c.Spawned && c.Map == producer.Map && rect.Contains(c.Position)).ToList();
        }

        public static bool ProducerContainsConnector(Building producer, Building_ProductionConnector connector)
        {
            return producer != null && connector != null && producer.Map == connector.Map && producer.OccupiedRect().Contains(connector.Position);
        }
    }
}
