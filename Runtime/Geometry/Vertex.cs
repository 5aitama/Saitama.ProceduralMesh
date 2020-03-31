using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Entities;

namespace Saitama.ProceduralMesh.Geometry
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [InternalBufferCapacity(16)]
    public struct Vertex : IBufferElementData
    {
        public float3 pos;
        public float3 norm;
        public float4 color;
        public float2 uv;

        public static NativeArray<VertexAttributeDescriptor> Descriptors(Allocator allocator)
        {
            var descriptors = new NativeArray<VertexAttributeDescriptor>(4, allocator);

            descriptors[0] = new VertexAttributeDescriptor(VertexAttribute.Position  , VertexAttributeFormat.Float32, 3, 0);
            descriptors[1] = new VertexAttributeDescriptor(VertexAttribute.Normal    , VertexAttributeFormat.Float32, 3, 0);
            descriptors[2] = new VertexAttributeDescriptor(VertexAttribute.Color     , VertexAttributeFormat.Float32, 4, 0);
            descriptors[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0 , VertexAttributeFormat.Float32, 2, 0);

            return descriptors;
        }
    }
}