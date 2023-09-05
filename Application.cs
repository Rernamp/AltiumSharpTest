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

        //public SchComponent findMcu(string designItemId = "nRF52833_QDAA") {
        //    SchComponent result = null;
        //    foreach (var schComponent in components) {
        //        if (schComponent.DesignItemId == "nRF52833_QDAA") {
        //            result = schComponent;
        //            break;
        //        }
        //    }

        //    return result;
        //}

        public void run() {
            SchDocReader schDocReader = new SchDocReader();

            var schDoc = schDocReader.Read(PathToSchDoc);

        }

        public readonly string PathToSchDoc;        
    }
}
