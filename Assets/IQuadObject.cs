using System;
using UnityEngine;

namespace CSharpQuadTree
{
    public interface IQuadObject
    {
        Rect Bounds { get; }
        event EventHandler BoundsChanged;
    }
}