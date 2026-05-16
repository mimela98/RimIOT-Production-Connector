using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimIOTProductionConnector
{
    public class MapComponent_ProductionConnectorCache : MapComponent
    {
        private readonly Dictionary<Building, List<Building_ProductionConnector>> connectorsByProducer = new Dictionary<Building, List<Building_ProductionConnector>>();
        private readonly Dictionary<Building_ProductionConnector, Building> producerByConnector = new Dictionary<Building_ProductionConnector, Building>();
        private readonly List<Building_ProductionConnector> emptyConnectors = new List<Building_ProductionConnector>();

        public MapComponent_ProductionConnectorCache(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            RebuildAll();
        }

        public void RebuildAll()
        {
            connectorsByProducer.Clear();
            producerByConnector.Clear();

            var connectors = map.listerBuildings.AllBuildingsColonistOfDef(RimIOTPCDefOf.RimIOT_ProductionConnector)
                .OfType<Building_ProductionConnector>()
                .ToList();

            foreach (var producer in map.listerBuildings.allBuildingsColonist.Where(ProductionConnectorUtility.IsProductionBuilding))
            {
                var linked = ProductionConnectorUtility.ConnectorsUnder(producer, connectors);
                if (linked.Count == 0)
                {
                    continue;
                }

                connectorsByProducer[producer] = linked;
                foreach (var connector in linked)
                {
                    if (!producerByConnector.ContainsKey(connector))
                    {
                        producerByConnector.Add(connector, producer);
                    }
                }
            }
        }

        public void NotifyRelevantBuildingChanged(Building building)
        {
            if (building is Building_ProductionConnector connector)
            {
                RegisterConnector(connector);
                return;
            }

            if (ProductionConnectorUtility.IsProductionBuilding(building))
            {
                RefreshProducer(building);
            }
        }

        public void NotifyRelevantBuildingDespawning(Building building)
        {
            if (building is Building_ProductionConnector connector)
            {
                if (producerByConnector.TryGetValue(connector, out var producer) && connectorsByProducer.TryGetValue(producer, out var connectors))
                {
                    connectors.Remove(connector);
                    if (connectors.Count == 0)
                    {
                        connectorsByProducer.Remove(producer);
                    }
                }

                producerByConnector.Remove(connector);
                return;
            }

            if (ProductionConnectorUtility.IsProductionBuilding(building) && connectorsByProducer.TryGetValue(building, out var linked))
            {
                foreach (var linkedConnector in linked)
                {
                    producerByConnector.Remove(linkedConnector);
                }

                connectorsByProducer.Remove(building);
            }
        }

        public List<Building_ProductionConnector> GetConnectors(Building producer)
        {
            if (producer != null && connectorsByProducer.TryGetValue(producer, out var connectors))
            {
                return connectors;
            }

            return emptyConnectors;
        }

        public Building GetProducer(Building_ProductionConnector connector)
        {
            return connector != null && producerByConnector.TryGetValue(connector, out var producer) ? producer : null;
        }

        private void RegisterConnector(Building_ProductionConnector connector)
        {
            if (connector == null || !connector.Spawned)
            {
                return;
            }

            RemoveConnector(connector);

            foreach (var producer in map.listerBuildings.allBuildingsColonist.Where(ProductionConnectorUtility.IsProductionBuilding))
            {
                if (!ProductionConnectorUtility.ProducerContainsConnector(producer, connector))
                {
                    continue;
                }

                if (!connectorsByProducer.TryGetValue(producer, out var connectors))
                {
                    connectors = new List<Building_ProductionConnector>();
                    connectorsByProducer.Add(producer, connectors);
                }

                connectors.Add(connector);
                producerByConnector[connector] = producer;
                return;
            }
        }

        private void RefreshProducer(Building producer)
        {
            RemoveProducer(producer);

            var linked = ProductionConnectorUtility.ConnectorsUnder(
                producer,
                map.listerBuildings.AllBuildingsColonistOfDef(RimIOTPCDefOf.RimIOT_ProductionConnector).OfType<Building_ProductionConnector>());

            if (linked.Count == 0)
            {
                return;
            }

            connectorsByProducer[producer] = linked;

            foreach (var connector in linked)
            {
                RemoveConnector(connector);
                producerByConnector[connector] = producer;
            }
        }

        private void RemoveConnector(Building_ProductionConnector connector)
        {
            if (connector == null || !producerByConnector.TryGetValue(connector, out var producer))
            {
                return;
            }

            if (connectorsByProducer.TryGetValue(producer, out var connectors))
            {
                connectors.Remove(connector);
                if (connectors.Count == 0)
                {
                    connectorsByProducer.Remove(producer);
                }
            }

            producerByConnector.Remove(connector);
        }

        private void RemoveProducer(Building producer)
        {
            if (producer == null || !connectorsByProducer.TryGetValue(producer, out var linked))
            {
                return;
            }

            foreach (var connector in linked)
            {
                producerByConnector.Remove(connector);
            }

            connectorsByProducer.Remove(producer);
        }
    }
}
