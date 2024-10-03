using UnityEditor;
using UnityEngine;

namespace MvVox
{
    internal class Projection
    {
        public Color projectionColor = Color.white;  
        public Projection() { }

        public void Draw(Vector3 worldPosition, Vector3 size)
        {
            Color oldColor = Handles.color;
            Handles.color = projectionColor;
            Handles.DrawWireCube(worldPosition, size); 
            Handles.color = oldColor;
        }
    }
}
