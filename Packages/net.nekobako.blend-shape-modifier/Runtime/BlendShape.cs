#if BSM_VRCSDK3_AVATARS

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable]
    internal class BlendShape : IEquatable<BlendShape>
    {
        [SerializeField, NotKeyable, BlendShapeName]
        public string Name = string.Empty;

        [SerializeField]
        public float Weight = 0.0f;

        [SerializeReference, NotKeyable]
        public List<BlendShapeFrame> Frames = new();

        public BlendShape Clone()
        {
            return new()
            {
                Name = Name,
                Weight = Weight,
                Frames = Frames.ConvertAll(x => x.Clone()),
            };
        }

        public BlendShape Clone(float weight)
        {
            return new()
            {
                Name = Name,
                Weight = weight,
                Frames = Frames.ConvertAll(x => x.Clone()),
            };
        }

        public bool Equals(BlendShape other)
        {
            return other is not null
                && Name.Equals(other.Name)
                && Weight.Equals(other.Weight)
                && Frames.SequenceEqual(other.Frames);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BlendShape);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = HashCode.Combine(hash, Name);
            hash = HashCode.Combine(hash, Weight);
            hash = Frames.Aggregate(hash, HashCode.Combine);
            return hash;
        }
    }
}

#endif
