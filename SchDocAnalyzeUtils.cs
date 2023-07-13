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
                addElementToList(element, ports);
                if (element is SchHarnessConnector connector) {
                    harnessConnectors.Add(new HarnessConnector(connector));
                }

                if (element is SchHarnessEntry harnessEntry) {
                    harnessConnectors.Last().addEntry(harnessEntry);
                }
                if (element is SchHarnessType harnessType) {
                    harnessConnectors.Last().type = harnessType;
                }
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
            var endOfPin = getEndOfPin(pin);
            return findWireContainingPoint(endOfPin);
        }

        public CoordPoint getEndOfPin(SchPin pin) {
            var endOfPin = pin.Location;
            int addedValue = (pin.Orientation.HasFlag(TextOrientations.Flipped) ? -pin.PinLength : pin.PinLength);
            if (pin.Orientation.HasFlag(TextOrientations.Rotated)) {
                endOfPin = new CoordPoint(endOfPin.X, endOfPin.Y + addedValue);
            } else {
                endOfPin = new CoordPoint(endOfPin.X + addedValue, endOfPin.Y);
            }
                
            return endOfPin;
        }

        public HashSet<SchWire> findWiresContainingPoint(CoordPoint point, SchWire excludeWire = null) {
            HashSet<SchWire>? result = new();

            foreach (var wire in wires) {
                for (int i = 0; i < wire.Vertices.Count - 1; i++) {
                    if (pointIsOnLine(wire.Vertices[i], wire.Vertices[i + 1], point)) {
                        if (wire != excludeWire) {
                            result.Add(wire);
                        }
                        
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

        public HashSet<SchWire> findWiresConnectedToWire(SchWire wire) {
            var result = new HashSet<SchWire>();

            foreach (var pointOfWire in wire.Vertices) {
                foreach (var searchWire in wires) { 
                    if (wire != searchWire) {
                        foreach (var point in searchWire.Vertices) {
                            if (point == pointOfWire) {
                                result.Add(searchWire);
                            }
                        }
                    }
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

            foreach (var netLabel in netLabels) {
                if (pointIsOnLine(point, nextPoint, netLabel.Location)) {
                    result = netLabel;
                    break;
                }                
            }

            return result;
        }

        public bool pointIsOnLine(CoordPoint firstPointLine, CoordPoint secondPointLine, CoordPoint targetPoint) {
            bool result = false;

            Point point1 = new Point(firstPointLine.X.ToInt32(), firstPointLine.Y.ToInt32());
            Point point2 = new Point(secondPointLine.X.ToInt32(), secondPointLine.Y.ToInt32());
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

            var target = new Point(targetPoint.X.ToInt32(), targetPoint.Y.ToInt32());
            if (steep) {
                (target.X, target.Y) = (target.Y, target.X);
            }
            if ((Utils.IsWithin(target.X, point1.X, point2.X)) &&
                (Utils.IsWithin(target.Y, point1.Y, point2.Y))) {
                var firstVector = new Point(point1.X - target.X, point1.Y - target.Y);
                var secondVector = new Point(point2.X - target.X, point2.Y - target.Y);
                var pseudoscalar = (firstVector.X) * (secondVector.Y) - (firstVector.Y) * (secondVector.X);
                result = pseudoscalar == 0;
            }

            return result;
        }

        public Dictionary<SchPin, SchComponent> findMapPinToComponentsConnectedToNet(SchNetLabel net) {
            Dictionary<SchPin, SchComponent> result = new();

            List<SchNetLabel> allNetLabelsWithThisName = new();

            foreach (var schNetLabel in netLabels) {
                if (schNetLabel.Text == net.Text) {
                    allNetLabelsWithThisName.Add(schNetLabel);
                }
            }

            HashSet<SchWire> wiresConnectedToThisNet = new();

            foreach (var netLabel in allNetLabelsWithThisName) {
                var wiresConnectedToNet = findWiresContainingNetLabel(netLabel);
                foreach (var element in wiresConnectedToNet) {
                    wiresConnectedToThisNet.Add(element);
                }
            }
            
            foreach (var wire in wiresConnectedToThisNet) {
                var mapPinToComponentsConnectedToWire = findMapPinToComponentsConnectedToWire(wire);
                result = result.Concat(mapPinToComponentsConnectedToWire).ToDictionary(x => x.Key, x => x.Value);
                var harnessConnectedToWire = findHarnessConnectedToWire(wire);
            }

            

            return result;
        }

        private HashSet<SchHarnessEntry> findHarnessConnectedToWire(SchWire wire) {
            HashSet<SchHarnessEntry> result = new();

            foreach (var harnessConnector in harnessConnectors) {
                
            }

            return result;
        }

        public HashSet<SchWire> findWiresContainingNetLabel(SchNetLabel net) {
            HashSet<SchWire> result = new();

            var wiresConnectToNetPoint = findWiresContainingPoint(net.Location);

            foreach (var wire in wiresConnectToNetPoint) {
                result.Add(wire);
                var wireConnectedToCurrentWire = recursiveFindWiresConnectedToWire(wire);
                foreach (var addedWire in wireConnectedToCurrentWire) {
                    result.Add(addedWire);
                }
            }

            return result;
        }

        public HashSet<SchWire> recursiveFindWiresConnectedToWire(SchWire wire, SchWire excludeWire = null) {
            var result = new HashSet<SchWire>();

            var wiresConnectedToWire = findWiresConnectedToWire(wire);

            if (excludeWire != null) { 
                wiresConnectedToWire.RemoveWhere(x => x == excludeWire);
            }

            foreach (var wireConnectedToWire in wiresConnectedToWire) {
                result.Add(wireConnectedToWire);

                var findWires = recursiveFindWiresConnectedToWire(wireConnectedToWire, wire);
                foreach (var findWire in findWires) {
                    result.Add(findWire);
                }
            }

            return result;
        }

        public Dictionary<SchPin, SchComponent> findMapPinToComponentsConnectedToWire(SchWire wire) {
            Dictionary<SchPin, SchComponent> result = new();

            foreach (var component in components) {
                foreach (var primitive in component.Primitives) {
                    if (primitive is SchPin pin) {
                        var endOfPin = getEndOfPin(pin);
                        foreach (var pointOfWire in wire.Vertices) {
                            if (endOfPin == pointOfWire) {
                                result.Add(pin, component);
                                break;
                            }
                        }
                    }
                }
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
        public List<SchPort> ports = new();
        public List<HarnessConnector> harnessConnectors = new();
    }
}
