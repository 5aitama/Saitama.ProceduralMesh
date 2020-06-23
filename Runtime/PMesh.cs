using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace Saitama.ProceduralMesh
{
    /// <summary>
    /// Methods to create/update mesh easily
    /// </summary>
    public static class PMesh
    {
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