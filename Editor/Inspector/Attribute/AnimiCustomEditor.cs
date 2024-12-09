using System;
using UnityEngine;

public class AnimiCustomEditor : Attribute
{
    private Type type;

    public AnimiCustomEditor(Type type) {
        this.type = type;
    }

    public Type GetType() {
        return type;
    }
}
