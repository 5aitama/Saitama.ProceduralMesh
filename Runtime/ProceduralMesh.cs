using Unity.Collections;
using UnityEngine;
using System;

using Saitama.ProceduralMesh.Geometry;

namespace Saitama.ProceduralMesh
{
    public class ProceduralMesh : IProceduralMesh, IDisposable
    {
        public virtual NativeList<Vertex> Vertices      { get; protected set; }
        public virtual NativeList<Triangle> Triangles   { get; protected set; }

        public ProceduralMesh(Allocator allocator)
        {
            Vertices = new NativeList<Vertex>(allocator);
            Triangles = new NativeList<Triangle>(allocator);
        }

        public ProceduralMesh(NativeList<Vertex> vertices, NativeList<Triangle> triangles)
        {
            Vertices  = vertices;
            Triangles = triangles;
        }

        public void Dispose()
        {
            if(Vertices.IsCreated) 
                Vertices.Dispose();
                
            if(Triangles.IsCreated) 
                Triangles.Dispose();
        }

        public virtual ProceduralMesh ToMesh(Mesh mesh)
        {
            MeshUtils.UpdateMesh(mesh, Triangles, Vertices);
            return this;
        }

        public virtual Mesh ToMesh()
        {
            return MeshUtils.CreateMesh(Triangles, Vertices);
        }
    }
}