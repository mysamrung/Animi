using Animi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animi.Editor {
    public class AnimiGraphView : GraphView {
        public AnimiGraphView() : base() {
            // e‚ÌUI‚É‚µ‚½‚ª‚Á‚ÄŠg‘åk¬‚ðs‚¤Ý’è
            style.flexGrow = 1;
            style.flexShrink = 1;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            Insert(0, new GridBackground());

            this.AddManipulator(new SelectionDragger());

            var searchProvider = new AnimiSearchProvider();
            searchProvider.Initialize(this);

            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchProvider);
            };
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) 
        {
            var compatiblePorts = new List<Port>();
            foreach (var port in ports.ToList()) {
                if (startPort.node == port.node ||
                    startPort.direction == port.direction ||
                    startPort.portType != port.portType) {
                    continue;
                }

                compatiblePorts.Add(port);
            }
            return compatiblePorts;
        }

        public AnimiData Save() {
            AnimiData animiData = new AnimiData();

            foreach(var port in ports) {
                if (!port.connected)
                    continue;

                if (port.direction == Direction.Output)
                    continue;

                foreach (var connection in port.connections)
                {
                    if (connection is AnimiEdgeBase animiEdgeBase)
                    {
                        AnimiEdgeBaseBehaviour data = (AnimiEdgeBaseBehaviour)animiEdgeBase.serializedObject.targetObject;
                        animiData.edgeDataObjects.Add(data);
                    }
                }

            }

            foreach(var node in nodes) {
                AnimiNodeBase animiNode = node as AnimiNodeBase;
                if (animiNode != null) {
                    AnimiNodeBaseBehaviour data = (AnimiNodeBaseBehaviour)animiNode.serializedObject.targetObject;
                    animiData.nodeDataObjects.Add(data);
                }
            }

            animiData = ScriptableObjectCloner.DeepClone(animiData);
            return animiData;
        }
    }

}
