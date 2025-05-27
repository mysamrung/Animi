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

        public void Load(AnimiData data)
        {
            AnimiData animiData = ScriptableObjectCloner.DeepClone(data);
            foreach(var nodeData in animiData.nodeDataObjects)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(AnimiNodeBase)) && Attribute.IsDefined(type, typeof(AnimiCustomEditor)))
                        {
                            var attr = (AnimiCustomEditor)Attribute.GetCustomAttribute(type, typeof(AnimiCustomEditor));
                            if(attr.GetType() == nodeData.GetType())
                            {
                                var node = Activator.CreateInstance(type, args:nodeData) as AnimiNodeBase;
                                AddElement(node);
                            }
                        }
                    }
                }
            }

            foreach(var edgeData in animiData.edgeDataObjects)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(AnimiEdgeBase)) && Attribute.IsDefined(type, typeof(AnimiCustomEditor)))
                        {
                            var attr = (AnimiCustomEditor)Attribute.GetCustomAttribute(type, typeof(AnimiCustomEditor));
                            if (attr.GetType() == edgeData.GetType())
                            {
                                var edge = Activator.CreateInstance(type, args: edgeData) as AnimiEdgeBase;
                                AddElement(edge);
                            }
                        }
                    }
                }
            }
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
