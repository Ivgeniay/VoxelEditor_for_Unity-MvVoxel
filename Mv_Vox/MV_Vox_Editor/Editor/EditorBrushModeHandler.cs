using System;

namespace MvVox
{
    internal class EditorBrushModeHandler
    {
        public event Action<BrushMode> OnBrushModeChanged; 
        public BrushMode CurrentBrushMode { get => _currentBrushMode; } 
        private BrushMode _currentBrushMode;
        private VoxCreater _target;

        public EditorBrushModeHandler(VoxCreater target)
        {
            _target = target;
            _currentBrushMode = _target.CurrentBrushMode;
        }

        public void Initialize()
        {
            OnBrushModeChanged?.Invoke(_currentBrushMode);
        }

        public void SetBrushMode(BrushMode brushMode)
        {
            _currentBrushMode = brushMode;
            _target.CurrentBrushMode = brushMode;
            OnBrushModeChanged?.Invoke(brushMode);
        }
    }
}
