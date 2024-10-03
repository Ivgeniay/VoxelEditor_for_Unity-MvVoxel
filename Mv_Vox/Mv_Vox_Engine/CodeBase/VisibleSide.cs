using System;

namespace MvVox
{
    [Flags]
    [Serializable]
    public enum VisibleSide
    {
        None = 0,
        IsFront = 1 << 0,
        IsBack = 1 << 1,
        IsLeft = 1 << 2,
        IsRight = 1 << 3,
        IsTop = 1 << 4,
        IsBottom = 1 << 5,
        All = IsFront | IsBack | IsLeft | IsRight | IsTop | IsBottom
    }

}
