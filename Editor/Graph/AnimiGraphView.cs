using Animi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animi.Editor {
    public class AnimiGraphView : GraphView {
        public AnimiGraphView() : base() {
            // 親のUIにしたがって拡大縮小を行う設定
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

                AnimiNodeBase curNodeBase = port.node as AnimiNodeBase;
                if (curNodeBase != null) {
                    AnimiConnectedLineData animiConnectedLineData = new AnimiConnectedLineData();
                    animiConnectedLineData.fromHashId = curNodeBase.hashId;
                    foreach (var nextNode in port.connections) {
                        AnimiNodeBase nextNodeBase = nextNode.input.node as AnimiNodeBase;
                        if (nextNodeBase != null)
                            animiConnectedLineData.toHashId.Add(nextNodeBase.hashId);
                    }

                    animiData.connectedLineDatas.Add(animiConnectedLineData);
                }
            }

            foreach(var node in nodes) {
                AnimiNodeBase animiNode = node as AnimiNodeBase;
                if (animiNode != null) {
                    AnimiNodeBaseBehaviour data = GameObject.Instantiate((AnimiNodeBaseBehaviour)animiNode.serializedObject.targetObject);
                    data.hashId = animiNode.hashId;

                    animiData.nodeDataObjects.Add(data);
                }
            }

            return animiData;
        }
    }

}
