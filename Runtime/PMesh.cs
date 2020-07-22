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
        private struct ToVertexArrayJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float3> PosArray;

            [ReadOnly]
            public NativeArray<float3> NormArray;

            [ReadOnly]
            public NativeArray<float2> UVArray;

            [WriteOnly]
            public NativeArray<Vertex> Vertices;

            public void Execute(int index)
            {
                Vertices[index] = new Vertex 
                {
                    pos     = PosArray[index],
                    norm    = NormArray[index],
                    uv0     = UVArray[index],
                };
            }
        }

        [BurstCompile]
        private struct ToColorVertexArrayJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float3> PosArray;

            [ReadOnly]
            public NativeArray<float3> NormArray;

            [ReadOnly]
            public NativeArray<float4> ColArray;

            [WriteOnly]
            public NativeArray<ColorVertex> Vertices;

            public void Execute(int index)
            {
                Vertices[index] = new ColorVertex 
                {
                    pos     = PosArray[index],
                    norm    = NormArray[index],
                    col     = ColArray[index],
                };
            }
        }

        /// <summary>
        /// Create vertex array from 3 separate array (positions, normals, uvs).
        /// </summary>
        /// <param name="vertices">Vertex array created</param>
        /// <param name="p">Array that contains position of each vertex</param>
        /// <param name="n">Array that contains normal vector of each vertex</param>
        /// <param name="u">Array that contains uv coordinate of each vertex</param>
        /// <param name="allocator">Allocator of the vertex array</param>
        /// <param name="options">Option of the vertex array</param>
        /// <param name="previousJob">Previous job</param>
        public static JobHandle ToVertexArray(out NativeArray<Vertex> vertices, NativeArray<float3> p, NativeArray<float3> n, NativeArray<float2> u, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory, JobHandle previousJob = default)
        {
            if(p.Length != n.Length)
                throw new System.Exception("position and normal array must be have same size!");

            vertices = new NativeArray<Vertex>(p.Length, allocator, options);

            return new ToVertexArrayJob
            {
                PosArray    = p,
                NormArray   = n,
                UVArray     = u,
                Vertices    = vertices,
            }.Schedule(vertices.Length, 64, previousJob);
        }

        /// <summary>
        /// Create vertex array from 3 separate array (positions, normals, colors).
        /// </summary>
        /// <param name="vertices">Vertex array created</param>
        /// <param name="p">Array that contains position of each vertex</param>
        /// <param name="n">Array that contains normal vector of each vertex</param>
        /// <param name="c">Array that contains color of each vertex</param>
        /// <param name="allocator">Allocator of the vertex array</param>
        /// <param name="options">Option of the vertex array</param>
        /// <param name="previousJob">Previous job</param>
        public static JobHandle ToColorVertexArray(out NativeArray<ColorVertex> vertices, NativeArray<float3> p, NativeArray<float3> n, NativeArray<float4> c, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory, JobHandle previousJob = default)
        {
            if(p.Length != n.Length)
                throw new System.Exception("position and normal array must be have same size!");

            vertices = new NativeArray<ColorVertex>(p.Length, allocator, options);

            return new ToColorVertexArrayJob
            {
                PosArray    = p,
                NormArray   = n,
                ColArray    = c,
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
            => Create(mesh, triangles, vertices, Vertex.Descriptors(Allocator.Temp));

        /// <summary>
        /// Create mesh from triangle and color vertex array.
        /// </summary>
        /// <param name="mesh">The mesh to be initialized with triangle and vertex array</param>
        /// <param name="triangles">The array that contains triangle (index)</param>
        /// <param name="vertices">The array that contains vertex</param>
        public static Mesh Create(this Mesh mesh, in NativeArray<Triangle> triangles, in NativeArray<ColorVertex> vertices)
            => Create(mesh, triangles, vertices, ColorVertex.Descriptors(Allocator.Temp));

        private static Mesh Create<T>(Mesh mesh, in NativeArray<Triangle> triangles, in NativeArray<T> vertices, in NativeArray<VertexAttributeDescriptor> descriptors) where T : struct
        {
            mesh.Clear();

            mesh.SetVertexBufferParams(vertices.Length, descriptors.ToArray());
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            mesh.SetIndexBufferParams(triangles.Length * 3, IndexFormat.UInt32);
            mesh.SetIndexBufferData(triangles, 0, 0, triangles.Length);

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
        public static Mesh Create(in NativeArray<Triangle> triangles, in NativeArray<Vertex> vertices) 
            => new Mesh().Create(triangles, vertices);
        
        /// <summary>
        /// Create a new Mesh.
        /// </summary>
        /// <param name="triangles">Triangle array</param>
        /// <param name="vertices">Color Vertex array</param>
        public static Mesh Create(in NativeArray<Triangle> triangles, in NativeArray<ColorVertex> vertices) 
            => new Mesh().Create(triangles, vertices);

        /// <summary>
        /// Update Mesh with new geometry
        /// </summary>
        /// <param name="mesh">Mesh to be update</param>
        /// <param name="triangles">New triangle array</param>
        /// <param name="vertices">New vertex array</param>
        public static void Update(this Mesh mesh, in NativeArray<Triangle> triangles, in NativeArray<Vertex> vertices)
            => Update(mesh, triangles, vertices, Vertex.Descriptors(Allocator.Temp));

        private static void Update<T>(Mesh mesh, in NativeArray<Triangle> triangles, in NativeArray<T> vertices, in NativeArray<VertexAttributeDescriptor> descriptors) where T : struct
        {
            mesh.Clear();

            mesh.SetVertexBufferParams(vertices.Length, descriptors.ToArray());
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            mesh.SetIndexBufferParams(triangles.Length * 3, IndexFormat.UInt32);
            mesh.SetIndexBufferData(triangles, 0, 0, triangles.Length);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length * 3, MeshTopology.Triangles));

            mesh.RecalculateBounds();
        }

        /// <summary>
        /// Update Mesh vertices
        /// </summary>
        /// <param name="mesh">Mesh to be update</param>
        /// <param name="vertices">New vertex array</param>
        public static void Update(this Mesh mesh, in NativeArray<Vertex> vertices)
        {
            mesh.SetVertexBufferParams(vertices.Length, Vertex.Descriptors(Allocator.Temp).ToArray());
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
        }

        /// <summary>
        /// Update Mesh vertices
        /// </summary>
        /// <param name="mesh">Mesh to be update</param>
        /// <param name="vertices">New vertex array</param>
        public static void Update(this Mesh mesh, in NativeArray<ColorVertex> vertices)
        {
            mesh.SetVertexBufferParams(vertices.Length, ColorVertex.Descriptors(Allocator.Temp).ToArray());
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
        }

        /// <summary>
        /// Update Mesh triangles
        /// </summary>
        /// <param name="mesh">Mesh to be update</param>
        /// <param name="triangles">New triangle array</param>
        public static void Update(this Mesh mesh, in NativeArray<Triangle> triangles)
        {
            mesh.SetIndexBufferParams(triangles.Length * 3, IndexFormat.UInt32);
            mesh.SetIndexBufferData(triangles, 0, 0, triangles.Length);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length * 3, MeshTopology.Triangles));
        }
    }
}
