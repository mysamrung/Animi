using Animi.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animi.Editor {

    [InitializeOnLoad]
    public class AnimiGraphView : GraphView {

        private static Action onReloadScript;

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


            this.RegisterCallback<PointerDownEvent>(OnPointerDownInGraphView);
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
            RemoveAllGraphElementsExceptBackground();
            AnimiData animiData = ScriptableObjectCloner.DeepClone(data);

            Dictionary<AnimiNodeBaseBehaviour, AnimiNodeBase> nodePairs = new Dictionary<AnimiNodeBaseBehaviour, AnimiNodeBase>();
            foreach (var nodeData in animiData.nodeDataObjects)
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

                                nodePairs.Add(nodeData, node);
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

                                foreach (var child in nodePairs[edgeData.from].inputContainer.Children())
                                {
                                    if (child is Port port && port.portName == edgeData.fromPortName)
                                    {
                                        edge.input = port;
                                        port.Connect(edge);
                                    }    
                                }

                                foreach (var child in nodePairs[edgeData.to].outputContainer.Children())
                                {
                                    if (child is Port port && port.portName == edgeData.toPortName)
                                    {
                                        edge.output = port;
                                        port.Connect(edge);
                                    }
                                }

                                AddElement(edge);
                            }
                        }
                    }
                }
            }
        }

        public AnimiData Save() {
            if (ports.Count() <= 0 && nodes.Count() <= 0)
                return null;

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

        private void RemoveAllGraphElementsExceptBackground()
        {
            // index 0（GridBackgroundなど）以外のGraphElementをすべて削除
            for (int i = 1; i < childCount; i++)
            {
                if (this[i] is GraphElement element)
                {
                    RemoveElement(element);
                }
            }
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            OnSelectionChanged();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            OnSelectionChanged();
        }

        private void OnSelectionChanged()
        {
            var windows = Resources.FindObjectsOfTypeAll<Animi.Editor.AnimiInspectorWindow>();
            foreach (var window in windows)
            {
                window.Repaint();
            }
        }
        private void OnPointerDownInGraphView(PointerDownEvent evt)
        {
            // クリックした要素がGraphView自身（空白）なら選択をクリア
            if (evt.target == this)
            {
                ClearSelection();
                OnSelectionChanged();
            }
        }
    }

}
