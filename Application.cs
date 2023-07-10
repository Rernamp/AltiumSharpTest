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

            List<SchPin> gpioPin = new();

            foreach (var pin in pins) {
                if (pin.Name[0] == 'P') {
                    gpioPin.Add(pin);
                }
            }

            var mapPinToWire = utils.getMapPinToWire(gpioPin);
            var mapWireToNet = utils.getMapWireToNet(mapPinToWire.Values.ToList());
        }

        public readonly string PathToSchDoc;        
    }
}
