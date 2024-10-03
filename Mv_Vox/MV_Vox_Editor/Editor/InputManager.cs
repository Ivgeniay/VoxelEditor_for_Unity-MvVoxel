using UnityEngine;
using System;


namespace MvVox
{
    internal class InputManager
    {
        public event Action<KeyCode> OnKeyDown; 
        public event Action<Vector2> OnMouse0Down;
        public event Action<Vector2> OnMouse1Down;
        public event Action<Vector2> OnMousePositionChanged;

        private BrushMode currentBrushMode;

        public Vector2 MousePosition { get => _lastMousePosition; } 
        private Vector2 _lastMousePosition;
        private Event lastEvent;

        public InputManager()
        {
            _lastMousePosition = Input.mousePosition;
        }

        public void Update(Event currentEvent)
        {
            if (currentBrushMode == BrushMode.None) return;
            this.lastEvent = currentEvent;
            if (currentEvent == null) return;


            if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode != KeyCode.None)
            { 
                OnKeyDown?.Invoke(currentEvent.keyCode);
            } 

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                OnMouse0Down?.Invoke(currentEvent.mousePosition);
            }

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
            {
                OnMouse1Down?.Invoke(currentEvent.mousePosition);
            }

            if (_lastMousePosition != currentEvent.mousePosition)
            {
                _lastMousePosition = currentEvent.mousePosition;
                OnMousePositionChanged?.Invoke(currentEvent.mousePosition); 
            }
        }

        public void Use() =>
            lastEvent?.Use();

        public void OnBrushModeChanged(BrushMode newBrushMode) =>
            currentBrushMode = newBrushMode;
    }
}
