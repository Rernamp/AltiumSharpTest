using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OriginalCircuit.AltiumSharp;
using OriginalCircuit.AltiumSharp.BasicTypes;
using OriginalCircuit.AltiumSharp.Records;
using System.Diagnostics;

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
            var endOfPin = pin.Location;
            int addedValue = (pin.Orientation.HasFlag(TextOrientations.Flipped) ? -pin.PinLength : pin.PinLength);
            if (pin.Orientation.HasFlag(TextOrientations.Rotated)) {
                endOfPin = new CoordPoint(endOfPin.X, endOfPin.Y + addedValue);
            } else {
                endOfPin = new CoordPoint(endOfPin.X + addedValue, endOfPin.Y);
            }

            return findWireContainingPoint(endOfPin);
        }

        public List<SchWire> findWiresContainingPoint(CoordPoint point) { 
            List<SchWire>? result = new();

            foreach (var wire in wires) {
                for (int i = 0; i < wire.Vertices.Count; i++) {

                    if ((point == wire.Vertices[i])) {
                        result.Add(wire);
                    }
                }
            }

            return result;
        }

        public SchWire? findWireContainingPoint(CoordPoint point) {
            return findWiresContainingPoint(point).FirstOrDefault();
        }


        public Dictionary<SchWire, SchNetLabel> getMapWireToNet(List<SchWire> wires) {
            Dictionary<SchWire, SchNetLabel> result = new();

            foreach (var wire in wires) {
                var netLabel = findNetLabelConnectedToWire(wire);
                if (netLabel != null) {
                    result.Add(wire, netLabel);
                    continue;
                } else {
                    var listOfConnectedWireToCurrentWire = findWiresConnectedToWire(wire);
                    foreach (var connectedWire in listOfConnectedWireToCurrentWire) {
                        netLabel = findNetLabelConnectedToWire(connectedWire);
                        if (netLabel != null) {
                            result.Add(wire, netLabel);
                            break;
                        }
                    }
                }

            }

            return result;
        }

        public List<SchWire> findWiresConnectedToWire(SchWire wire) {
            var result = new List<SchWire>();

            foreach (var pointOfWire in wire.Vertices) {
                var wireConnectedToPoint = findWiresContainingPoint(pointOfWire);
                if ((wireConnectedToPoint.Count != 0)) {
                    wireConnectedToPoint.RemoveAll(value => value == wire);
                    result.AddRange(wireConnectedToPoint);
                }
            }

            return result;
        }

        public SchNetLabel? findNetLabelConnectedToWire(SchWire wire) {
            SchNetLabel result = null;
            for (int i = 0; i < wire.Vertices.Count - 1; i++) {
                var netLabel = findNetOnLine(wire.Vertices[i], wire.Vertices[i + 1]);
                if (netLabel != null) {
                    result = netLabel;
                    break;
                }
            }

            return result;
        }


        public SchNetLabel? findNetOnLine(CoordPoint point, CoordPoint nextPoint) {
            SchNetLabel result = null;

            Point point1 = new Point(point.X.ToInt32(), point.Y.ToInt32());
            Point point2 = new Point(nextPoint.X.ToInt32(), nextPoint.Y.ToInt32());
            bool steep = false;
            if (Math.Abs(point1.X - point2.X) < Math.Abs(point1.Y - point2.Y)) {
                (point1.X, point1.Y) = (point1.Y, point1.X);
                (point2.X, point2.Y) = (point2.Y, point2.X);
                steep = true;
            }

            if (point1.X > point2.X) {
                (point1.X, point2.X) = (point2.X, point1.X);
                (point1.Y, point2.Y) = (point2.Y, point1.Y);
            }

            foreach (var netLabel in netLabels) {

                var pointOfNet = new Point(netLabel.Location.X.ToInt32(), netLabel.Location.Y.ToInt32());
                if (steep) {
                    (pointOfNet.X, pointOfNet.Y) = (pointOfNet.Y, pointOfNet.X);
                }
                if ((Utils.IsWithin(pointOfNet.X, point1.X, point2.X)) &&
                    (Utils.IsWithin(pointOfNet.Y, point1.Y, point2.Y))) {
                    var firstVector = new Point(point1.X - pointOfNet.X, point1.Y - pointOfNet.Y);
                    var secondVector = new Point(point2.X - pointOfNet.X, point2.Y - pointOfNet.Y);
                    var pseudoscalar = (firstVector.X) * (secondVector.Y) - (firstVector.Y) * (secondVector.X);
                    if (pseudoscalar == 0) {
                        result = netLabel;
                        break;
                    }
                }
            }

            return result;
        }

        public List<SchComponent> getComponentsConnectedToNetLabel(SchNetLabel netLaber) {
            List<SchComponent> result = new();

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
