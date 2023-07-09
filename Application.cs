using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OriginalCircuit.AltiumSharp;
using OriginalCircuit.AltiumSharp.Records;

namespace AltiumSharpTest {
    public class Application {
        public Application(string pathToSchDoc) {
            PathToSchDoc = pathToSchDoc;
        }

        public void run() {
            SchDocReader schDocReader = new SchDocReader();

            var schDoc = schDocReader.Read(PathToSchDoc);

            SchDocAnalyzeUtils utils = new SchDocAnalyzeUtils(schDoc);

            var mcu = utils.findMcu();

            var pins = utils.getConnectedPinToComponent(mcu);

            var mapPinToWire = utils.getMapPinToWire(pins);
        }

        public readonly string PathToSchDoc;        
    }
}
