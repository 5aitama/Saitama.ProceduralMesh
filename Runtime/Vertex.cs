using Unity.Mathematics;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Entities;

using System.Runtime.InteropServices;

namespace Saitama.ProceduralMesh
{
    /// <summary>
    /// Slim vertex (without color).
    /// </summary>
    [StructLayout(LayoutKind.Sequential), InternalBufferCapacity(8)]
    public struct SVertex
    {   
        /// <summary>
        /// Vertex position
        /// </summary>
        public float3 pos;

        /// <summary>
        /// Vertex normal
        /// </summary>
        public float3 norm;

        /// <summary>
        /// Vertex texture coordinate
        /// </summary>
        public float2 uv;

        public static NativeArray<VertexAttributeDescriptor> Descriptors(Allocator allocator)
        {
            var descriptors = new NativeArray<VertexAttributeDescriptor>(3, allocator);

            descriptors[0] = new VertexAttributeDescriptor(VertexAttribute.Position  , VertexAttributeFormat.Float32, 3, 0);
            descriptors[1] = new VertexAttributeDescriptor(VertexAttribute.Normal    , VertexAttributeFormat.Float32, 3, 0);
            descriptors[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0 , VertexAttributeFormat.Float32, 2, 0);

            return descriptors;
        }
    }

    /// <summary>
    /// Basic vertex.
    /// </summary>
    [StructLayout(LayoutKind.Sequential), InternalBufferCapacity(8)]
    public struct Vertex
    {
        /// <summary>
        /// Vertex position
        /// </summary>
        public float3 pos;

        /// <summary>
        /// Vertex normal
        /// </summary>
        public float3 norm;
        
        /// <summary>
        /// Vertex texture coordinate
        /// </summary>
        /// public float2 uv;

        /// <summary>
        /// Vertex color
        /// </summary>
        public float4 col;

        public static NativeArray<VertexAttributeDescriptor> Descriptors(Allocator allocator)
        {
            var descriptors = new NativeArray<VertexAttributeDescriptor>(3, allocator);

            descriptors[0] = new VertexAttributeDescriptor(VertexAttribute.Position  , VertexAttributeFormat.Float32, 3);
            descriptors[1] = new VertexAttributeDescriptor(VertexAttribute.Normal    , VertexAttributeFormat.Float32, 3);
            descriptors[2] = new VertexAttributeDescriptor(VertexAttribute.Color     , VertexAttributeFormat.Float32, 4);

            return descriptors;
        }
    }
}
