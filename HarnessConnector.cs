using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OriginalCircuit.AltiumSharp;
using OriginalCircuit.AltiumSharp.BasicTypes;
using OriginalCircuit.AltiumSharp.Records;
using System.Diagnostics;
using System.Drawing;

namespace AltiumSharpTest {
    public class HarnessConnector {
        public HarnessConnector(SchHarnessConnector connector) { 
            this.connector = connector;
        }
        private const int scaleFactorForTop_Frac1 = 100000;
        public void addEntry(SchHarnessEntry entry) {
            Point startPoint = new Point(connector.Location.X.ToInt32(), connector.Location.Y.ToInt32());
            var addedValue = OriginalCircuit.AltiumSharp.BasicTypes.Utils.MilsToCoord((entry.DistanceFromTop * 10 + (entry.DistanceFromTop_Frac1 / scaleFactorForTop_Frac1)) * 10).ToInt32();
            if (entry.SideOfEntry == SchHarnessEntry.Side.Right) {
                startPoint.X += OriginalCircuit.AltiumSharp.BasicTypes.Utils.MilsToCoord(connector.Size.Width * 10).ToInt32();
                startPoint.Y -= addedValue;
            }

            if (entry.SideOfEntry == SchHarnessEntry.Side.Left) {
                startPoint.Y -= addedValue;
            }

            if (entry.SideOfEntry == SchHarnessEntry.Side.Top) {
                startPoint.X += addedValue;
            }

            if (entry.SideOfEntry == SchHarnessEntry.Side.Bottom) {
                startPoint.Y -= OriginalCircuit.AltiumSharp.BasicTypes.Utils.MilsToCoord(connector.Size.Height * 10).ToInt32();
                startPoint.X += addedValue;
            }
            
            CoordPoint pointOfEntry = new CoordPoint(startPoint.X, startPoint.Y);

            mapEntryToCoordinate.Add(entry, pointOfEntry);
        }

        public SchHarnessConnector connector;
        public SchHarnessType? type = null;
        public Dictionary<SchHarnessEntry, CoordPoint> mapEntryToCoordinate = new();
    }
}
