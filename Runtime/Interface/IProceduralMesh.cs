using Unity.Collections;

using Saitama.ProceduralMesh.Geometry;

namespace Saitama.ProceduralMesh
{
    public interface IProceduralMesh
    {
        NativeList<Vertex> Vertices { get; }
        NativeList<Triangle> Triangles { get; }
    }
}