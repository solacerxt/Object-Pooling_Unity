using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace solacerxt.Pooling
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PoolPrefabTypeAttribute : PropertyAttribute
    {
        public Type Value { get; private set; }

        public PoolPrefabTypeAttribute(Type type)
        {
            Value = type;
        }
    }
}
