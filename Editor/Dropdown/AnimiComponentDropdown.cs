using Animi.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace Animi.Editor {
    public class AnimiComponentDropdown : AdvancedDropdown {
        public System.Action<Type> onSelectedCallBack;

        private Dictionary<string, Type> itemDic = new Dictionary<string, Type>(); 

        public AnimiComponentDropdown(AdvancedDropdownState state) : base(state) {
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
        }

        protected override AdvancedDropdownItem BuildRoot() {
            var root = new AdvancedDropdownItem("Root");

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsClass && !type.IsAbstract && (type.IsSubclassOf(typeof(AnimiComponent)))) {
                        root.AddChild(new AdvancedDropdownItem(type.ToString()));
                        itemDic.Add(type.ToString(), type);
                    }
                }
            }
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item) {
            Type type = itemDic[item.name]; 
            onSelectedCallBack?.Invoke(type);
        }
    }
}