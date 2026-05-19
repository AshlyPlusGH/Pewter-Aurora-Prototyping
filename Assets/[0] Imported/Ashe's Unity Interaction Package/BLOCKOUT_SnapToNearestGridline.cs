using UnityEngine;
using NaughtyAttributes;
using System;

namespace Ash {
    [ExecuteAlways]
    public class BLOCKOUT_SnapToNearestGridline : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        public float gridSize = 1;

        public bool snapScaleX = false;
        public bool snapScaleY = false;
        public bool snapScaleZ = false;

        [Button]
        public void SnapAll()
        {
            foreach (var block in FindObjectsByType<BLOCKOUT_SnapToNearestGridline>())
            {
                block.transform.localPosition = PositionToGridPosition(block.transform.localPosition, gridSize);

                if (!block.snapScaleX && !block.snapScaleY && !block.snapScaleZ){ continue; }
                
                Vector3 adjustedScale = ScaleToGridScale(block.transform.localScale, gridSize);
                Vector3 newScale = new Vector3(block.transform.localScale.x, block.transform.localScale.y, block.transform.localScale.z);

                //Debug.Log("Before: " + newScale.y);

                if (block.snapScaleX){ newScale.x = adjustedScale.x; }
                if (block.snapScaleY){ newScale.y = adjustedScale.y; }
                if (block.snapScaleZ){ newScale.z = adjustedScale.z; }

                //Debug.Log("After: " + newScale.y);

                block.transform.localScale = newScale;
            }
        }

        public static Vector3 PositionToGridPosition(Vector3 position, float gridSize){ return new Vector3(RoundToNearest(position.x, gridSize), RoundToNearest(position.y, gridSize), RoundToNearest(position.z, gridSize)); }

        public static Vector3 ScaleToGridScale(Vector3 scale, float gridSize){ return PositionToGridPosition(scale, gridSize); }

        public static float RoundToNearest(float value, float multiple)
        {
            double v = value;
            double m = multiple;
            return (float)(Math.Round(v / m) * m);
        }
    }
}