using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace Saitama.ProceduralMesh
{
    /// <summary>
    /// Methods to create/update mesh easily
    /// </summary>
    public static class PMesh
    {
        [BurstCompile]
        private struct ToSVertexArrayJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float3> PosArray;

            [ReadOnly]
            public NativeArray<float3> NormArray;

            [ReadOnly]
            public NativeArray<float2> UVArray;

            [WriteOnly]
            public NativeArray<SVertex> Vertices;

            public void Execute(int index)
            {
                Vertices[index] = new SVertex 
                {
                    pos     = PosArray[index],
                    norm    = NormArray[index],
                    uv      = UVArray[index],
                };
            }
        }

        [BurstCompile]
        private struct ToVertexArrayJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float3> PosArray;

            [ReadOnly]
            public NativeArray<float3> NormArray;

            [ReadOnly]
            public NativeArray<float2> UVArray;

            [ReadOnly]
            public NativeArray<float3> ColorArray;

            [WriteOnly]
            public NativeArray<Vertex> Vertices;

            public void Execute(int index)
            {
                Vertices[index] = new Vertex 
                {
                    pos     = PosArray[index],
                    norm    = NormArray[index],
                    uv      = UVArray[index],
                    col     = ColorArray[index],
                };
            }
        }
        
        [BurstCompile]
        private struct MergeVertexNormalJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float3> NormArray;
            
            [WriteOnly]
            public NativeArray<Vertex> VertArray;

            public void Execute(int index)
                => VertArray[index].norm = NormArray[index];
        }

        /// <summary>
        /// Create simple vertex array from 3 separate array (positions, normals, uvs).
        /// </summary>
        /// <param name="vertices">Vertex array created</param>
        /// <param name="p">Array that contains position of each vertex</param>
        /// <param name="n">Array that contains normal of each vertex</param>
        /// <param name="u">Array that contains texture coordinate of each vertex</param>
        /// <param name="allocator">Allocator of the vertex array</param>
        /// <param name="options">Option of the vertex array</param>
        /// <param name="previousJob">Previous job</param>
        public static JobHandle ToSVertexArray(out NativeArray<SVertex> vertices, NativeArray<float3> p, NativeArray<float3> n, NativeArray<float2> u, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory, JobHandle previousJob = default)
        {
            if(p.Length != n.Length)
                throw new System.Exception("position and normal array must be have same size!");

            vertices = new NativeArray<SVertex>(p.Length, allocator, options);

            return new ToSVertexArrayJob
            {
                PosArray    = p,
                NormArray   = n,
                UVArray     = u,
                Vertices    = vertices,
            }.Schedule(vertices.Length, 64, previousJob);
        }

        /// <summary>
        /// Create vertex array from 4 separate array (positions, normals, uvs, colors).
        /// </summary>
        /// <param name="vertices">Vertex array created</param>
        /// <param name="p">Array that contains position of each vertex</param>
        /// <param name="n">Array that contains normal vector of each vertex</param>
        /// <param name="u">Array that contains texture coordinate of each vertex</param>
        /// <param name="c">Array that contains color of each vertex</param>
        /// <param name="allocator">Allocator of the vertex array</param>
        /// <param name="options">Option of the vertex array</param>
        /// <param name="previousJob">Previous job</param>
        public static JobHandle ToVertexArray(out NativeArray<Vertex> vertices, NativeArray<float3> p, NativeArray<float3> n, NativeArray<float2> u, NativeArray<float3> c, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory, JobHandle previousJob = default)
        {
            if(p.Length != n.Length)
                throw new System.Exception("position and normal array must be have same size!");

            vertices = new NativeArray<Vertex>(p.Length, allocator, options);

            return new ToVertexArrayJob
            {
                PosArray    = p,
                NormArray   = n,
                UVArray     = u,
                ColorArray  = c,
                Vertices    = vertices,
            }.Schedule(vertices.Length, 64, previousJob);
        }

        /// <summary>
        /// Create mesh from triangle and vertex array.
        /// </summary>
        /// <param name="mesh">The mesh to be initialized with triangle and vertex array</param>
        /// <param name="triangles">The array that contains triangle (index)</param>
        /// <param name="vertices">The array that contains vertex</param>
        public static Mesh Create(this Mesh mesh, in NativeArray<Triangle> triangles, in NativeArray<Vertex> vertices)
        {
            mesh.Clear();

            mesh.SetVertexBufferParams(vertices.Length, Vertex.Descriptors(Allocator.Temp).ToArray());
            mesh.SetVertexBufferData<Vertex>(vertices, 0, 0, vertices.Length);

            mesh.SetIndexBufferParams(triangles.Length * 3, IndexFormat.UInt32);
            mesh.SetIndexBufferData<Triangle>(triangles, 0, 0, triangles.Length);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length * 3, MeshTopology.Triangles));

            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Create a new Mesh.
        /// </summary>
        /// <param name="triangles">Triangle array</param>
        /// <param name="vertices">Vertex array</param>
        public static Mesh Create(in NativeArray<Triangle> triangles, in NativeArray<Vertex> vertices) => new Mesh().Create(triangles, vertices);

        /// <summary>
        /// Update Mesh with new geometry
        /// </summary>
        /// <param name="mesh">Mesh to be update</param>
        /// <param name="triangles">New triangle array</param>
        /// <param name="vertices">New vertex array</param>
        public static void Update(this Mesh mesh, in NativeArray<Triangle> triangles, in NativeArray<Vertex> vertices)
        {
            mesh.Clear();

            mesh.SetVertexBufferParams(vertices.Length, Vertex.Descriptors(Allocator.Temp).ToArray());
            mesh.SetVertexBufferData<Vertex>(vertices, 0, 0, vertices.Length);

            mesh.SetIndexBufferParams(triangles.Length * 3, IndexFormat.UInt32);
            mesh.SetIndexBufferData<Triangle>(triangles, 0, 0, triangles.Length);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length * 3, MeshTopology.Triangles));

            mesh.RecalculateBounds();
        }
        
        /// <summary>
        /// Update Mesh with new geometry
        /// </summary>
        /// <param name="mesh">Mesh to be update</param>
        /// <param name="triangles">New triangle array</param>
        /// <param name="vertices">New vertex array</param>
        /// <param name="normals">New normals array</param>
        public static Update(this Mesh mesh, in NativeArray<Triangle> triangles, NativeArray<Vertex> vertices, in NativeArray<float3> normals)
        {
            new MergeVertexNormalJob
            {
                NormArray = normals,
                VertArray = vertices,
            }
            .Schedule(vertices.Length, 64)
            .Complete();
            
            Update(mesh, triangles, vertices);
        }

        /// <summary>
        /// Update Mesh vertices
        /// </summary>
        /// <param name="mesh">Mesh to be update</param>
        /// <param name="vertices">New vertex array</param>
        public static void Update(this Mesh mesh, in NativeArray<Vertex> vertices)
        {
            mesh.SetVertexBufferParams(vertices.Length, Vertex.Descriptors(Allocator.Temp).ToArray());
            mesh.SetVertexBufferData<Vertex>(vertices, 0, 0, vertices.Length);
        }

        /// <summary>
        /// Update Mesh triangles
        /// </summary>
        /// <param name="mesh">Mesh to be update</param>
        /// <param name="triangles">New triangle array</param>
        public static void Update(this Mesh mesh, in NativeArray<Triangle> triangles)
        {
            mesh.SetIndexBufferParams(triangles.Length * 3, IndexFormat.UInt32);
            mesh.SetIndexBufferData<Triangle>(triangles, 0, 0, triangles.Length);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length * 3, MeshTopology.Triangles));
        }
    }
}
