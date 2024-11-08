using UnityEngine;

namespace AnarchyConstructFramework.Core.Common
{
    public static class AnarchyUtilities
    {
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
    }
}