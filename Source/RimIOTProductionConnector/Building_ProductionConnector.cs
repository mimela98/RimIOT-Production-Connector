using System.Text;
using RimIOT;
using RimWorld;
using Verse;

namespace RimIOTProductionConnector
{
    public class Building_ProductionConnector : Building
    {
        public override string GetInspectString()
        {
            var baseText = base.GetInspectString();
            var status = ProductionConnectorUtility.GetStatus(this);
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(baseText))
            {
                builder.AppendLine(baseText);
            }

            builder.Append(status.Key.Translate());

            if (status.Producer != null)
            {
                builder.AppendLine();
                builder.Append("Connected building: ");
                builder.Append(status.Producer.LabelShortCap);
            }

            if (status.MultipleConnectors)
            {
                builder.AppendLine();
                builder.Append("RimIOT_PC_StatusMultiple".Translate());
            }

            return builder.ToString();
        }
    }
}
