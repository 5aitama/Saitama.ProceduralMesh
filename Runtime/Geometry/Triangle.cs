using Unity.Mathematics;
using Unity.Entities;

namespace Saitama.ProceduralMesh.Geometry
{
    [InternalBufferCapacity(16)]
    public struct Triangle : IBufferElementData
    {
        /// <summary>
        /// Triangle indices.
        /// </summary>
        public int3 indices;

        public Triangle(int3 indices)
        {
            this.indices = indices;
        }

        public Triangle(int x, int y, int z)
        {
            this.indices = new int3(x, y, z);
        }

        /// <summary>
        /// Get index value of triangle.
        /// </summary>
        /// <param name="index">index of index in the triangle</param>
        /// <returns></returns>
        public int GetIndex(int index)
        {
            return indices[index];
        }

        /// <summary>
        /// Set index value of triangle.
        /// </summary>
        /// <param name="index">index of index to set in the triangle</param>
        /// <param name="value">index value</param>
        public void SetIndex(int index, int value)
        {
            indices[index] = value;
        }
        
        public int this[int index]
        {
            get => GetIndex(index);
            set => SetIndex(index, value);
        }
    }
}