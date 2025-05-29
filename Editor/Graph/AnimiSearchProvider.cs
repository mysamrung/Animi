using Animi.Core;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Animi.Editor {
    public class AnimiSearchProvider : ScriptableObject, ISearchWindowProvider {
        private AnimiGraphView graphView;

        public void Initialize(AnimiGraphView graphView) {
            this.graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsClass && !type.IsAbstract && (type.IsSubclassOf(typeof(AnimiNodeBase)))) {
                        entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
                    }
                }
            }

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            var type = searchTreeEntry.userData as System.Type;
            var node = Activator.CreateInstance(type) as AnimiNodeBase;
            graphView.AddElement(node);

            graphView.ClearSelection();
            graphView.AddToSelection(node);

            return true;
        }
    }
}