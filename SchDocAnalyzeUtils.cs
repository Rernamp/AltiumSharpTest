using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OriginalCircuit.AltiumSharp;
using OriginalCircuit.AltiumSharp.BasicTypes;
using OriginalCircuit.AltiumSharp.Records;

namespace AltiumSharpTest {
    public class SchDocAnalyzeUtils {
        public SchDocAnalyzeUtils(SchDoc SchDoc) {
            this.schDoc = SchDoc;

            foreach (var element in schDoc.Items) {
                addElementToList(element, components);
                addElementToList(element, wires);
                addElementToList(element, netLabels);
            }

        }

        public SchComponent findMcu(string designItemId = "nRF52833_QDAA") {
            SchComponent result = null;
            foreach (var schComponent in components) {
                    if (schComponent.DesignItemId == "nRF52833_QDAA") {
                        result = schComponent;
                        break;
                    }
            }

            return result;
        }

        public List<SchPin> getConnectedPinToComponent(SchComponent component) {
            List<SchPin> result = new();

            foreach (var childPrimitive in component.Primitives) {
                if (childPrimitive is SchPin pin) {
                    result.Add(pin);
                }
            }

            return result;
        }

        public Dictionary<SchPin, SchWire> getMapPinToWire(List<SchPin> pins) {
            Dictionary<SchPin, SchWire> result = new();

            foreach (var pin in pins) {
                var wire = findWireConnectedToPin(pin);
                if (wire != null) {
                    result.Add(pin, wire);
                }
            }


            return result;
        }

        public SchWire findWireConnectedToPin(SchPin pin) {
            SchWire result = null;

            var endOfPin = pin.Location;
            int addedValue = (pin.Orientation.HasFlag(TextOrientations.Flipped) ? -pin.PinLength : pin.PinLength);
            if (pin.Orientation.HasFlag(TextOrientations.Rotated)) {
                endOfPin = new CoordPoint(endOfPin.X, endOfPin.Y + addedValue);
            } else {
                endOfPin = new CoordPoint(endOfPin.X + addedValue, endOfPin.Y);
            }

            foreach (var wire in wires) {
                for (int i = 0; i < wire.Vertices.Count; i++) {
                    
                    if ((endOfPin == wire.Vertices[i])) {
                        result = wire;
                        break;
                    }
                }
                if (result != null) {
                    break;
                }
            }

            return result;
        }

        public Dictionary<SchWire, SchNetLabel> getMapWireToNet(List<SchWire> wires) {
            Dictionary<SchWire, SchNetLabel> result = new();

            foreach (var wire in wires) {
                for (int i = 0; i < wire.Vertices.Count - 1; i++) {
                    var netLabel = findNetLabelConnectedToWire(wire.Vertices[i], wire.Vertices[i + 1]);
                    if (netLabel != null) {
                        result.Add(wire, netLabel);
                        break;
                    }
                }
            }

            return result;
        }

        public SchNetLabel? findNetLabelConnectedToWire(CoordPoint point1, CoordPoint point2) {
            SchNetLabel result = null;

            if (point1.X > point2.X) {
                //(point1.X, point2.X) = (point2.X, point1.X);
            }
            
            foreach (var netLabel in netLabels) {

            }

            return result;
        }



        private void addElementToList<T>(SchPrimitive element, List<T> container) {
            if (element is T TElement) {
                container.Add(TElement);
            }
        }

        private SchDoc schDoc;

        public List<SchComponent> components = new();
        public List<SchWire> wires = new();
        public List<SchNetLabel>  netLabels = new();
    }
}
