using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OriginalCircuit.AltiumSharp;

namespace AltiumSharpTest {
    public class Application {
        public Application(string pathToSchDoc) {
            PathToSchDoc = pathToSchDoc;
        }

        public void run() {
            SchDocReader schDocReader = new SchDocReader();

            var schDoc = schDocReader.Read(PathToSchDoc);
        }

        public readonly string PathToSchDoc;

    }
}
