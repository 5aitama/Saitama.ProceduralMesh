/* 
 * ============================================================================================================ *
 * The following code was an adaptation of the code from :                                                      |
 * https://schemingdeveloper.com/2017/03/26/better-method-recalculate-normals-unity-part-2/                     |
 * to Unity Jobs friendly code.                                                                                 |
 * Special thank to him for his incredible work !! <3                                                           |
 * ============================================================================================================ *
 */

using UnityEngine;

using Unity.Collections;
using Unity.Mathematics;

using Unity.Jobs;
using Unity.Burst;

namespace Saitama.ProceduralMesh
{
    public static class NormalSolver
    {
        [BurstCompile]
        public struct RecalculateNormalsJob : IJob
        {
            [ReadOnly]
            public float angle;

            [ReadOnly]
            public NativeArray<Triangle> tris;

            public NativeArray<Vertex> vert;

            public void Execute()
            {
                 var cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);
                var triNormals = new NativeArray<float3>(tris.Length, Allocator.Temp);
                var dictionary = new NativeMultiHashMap<VertexKey, VertexEntry>(vert.Length * 2, Allocator.Temp);

                for(var i = 0; i < tris.Length; i++)
                {
                    var i1 = tris[i].indices[0];
                    var i2 = tris[i].indices[1];
                    var i3 = tris[i].indices[2];

                    // Calculate the normal of the triangle
                    var p1 = vert[i2].pos - vert[i1].pos;
                    var p2 = vert[i3].pos - vert[i1].pos;
                    var normal = math.normalize(math.cross(p1, p2));

                    triNormals[i] = normal;
                    
                    dictionary.Add(new VertexKey(vert[i1].pos), new VertexEntry(0, i, i1));
                    dictionary.Add(new VertexKey(vert[i2].pos), new VertexEntry(0, i, i2));
                    dictionary.Add(new VertexKey(vert[i3].pos), new VertexEntry(0, i, i3));
                }

                var keys = dictionary.GetKeyArray(Allocator.Temp);

                for(var i = 0; i < keys.Length; i++)
                {
                    var enumerator1 = dictionary.GetValuesForKey(keys[i]);
                    do
                    {
                        var sum = new float3();
                        var lhs = enumerator1.Current;
                        var enumerator2 = dictionary.GetValuesForKey(keys[i]);
                        do
                        {
                            var rhs = enumerator2.Current;

                            if (lhs.VertexIndex == rhs.VertexIndex) 
                                sum += triNormals[rhs.TriangleIndex];
                            else 
                            {
                                // The dot product is the cosine of the angle between the two triangles.
                                // A larger cosine means a smaller angle.
                                var dot = math.dot(triNormals[lhs.TriangleIndex], triNormals[rhs.TriangleIndex]);

                                if (dot >= cosineThreshold)
                                    sum += triNormals[rhs.TriangleIndex];
                            }
                        } while(enumerator2.MoveNext());

                        var v = vert[lhs.VertexIndex];
                        v.norm = math.normalize(sum);
                        vert[lhs.VertexIndex] = v;

                        //vert[lhs.VertexIndex].norm = math.normalize(sum);

                    } while(enumerator1.MoveNext());
                }
            }
        }

        /// <summary>
        /// Smooth mesh.
        /// </summary>
        /// <param name="vert">Vertex array that normals would be stored in</param>
        /// <param name="tris">Triangle array</param>
        /// <param name="angle">Smoothing angle</param>
        public static void RecalculateNormals(ref NativeArray<Vertex> vert, NativeArray<Triangle> tris, float angle = 60)
        {
            var cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);
            var triNormals = new NativeArray<float3>(tris.Length, Allocator.Temp);
            var dictionary = new NativeMultiHashMap<VertexKey, VertexEntry>(vert.Length * 2, Allocator.Temp);

            for(var i = 0; i < tris.Length; i++)
            {
                var i1 = tris[i].indices[0];
                var i2 = tris[i].indices[1];
                var i3 = tris[i].indices[2];

                // Calculate the normal of the triangle
                var p1 = vert[i2].pos - vert[i1].pos;
                var p2 = vert[i3].pos - vert[i1].pos;
                var normal = math.normalize(math.cross(p1, p2));

                triNormals[i] = normal;
                
                dictionary.Add(new VertexKey(vert[i1].pos), new VertexEntry(0, i, i1));
                dictionary.Add(new VertexKey(vert[i2].pos), new VertexEntry(0, i, i2));
                dictionary.Add(new VertexKey(vert[i3].pos), new VertexEntry(0, i, i3));
            }

            var keys = dictionary.GetKeyArray(Allocator.Temp);

            for(var i = 0; i < keys.Length; i++)
            {
                var enumerator1 = dictionary.GetValuesForKey(keys[i]);
                do
                {
                    var sum = new float3();
                    var lhs = enumerator1.Current;
                    var enumerator2 = dictionary.GetValuesForKey(keys[i]);
                    do
                    {
                        var rhs = enumerator2.Current;

                        if (lhs.VertexIndex == rhs.VertexIndex) 
                            sum += triNormals[rhs.TriangleIndex];
                        else 
                        {
                            // The dot product is the cosine of the angle between the two triangles.
                            // A larger cosine means a smaller angle.
                            var dot = math.dot(triNormals[lhs.TriangleIndex], triNormals[rhs.TriangleIndex]);

                            if (dot >= cosineThreshold)
                                sum += triNormals[rhs.TriangleIndex];
                        }
                    } while(enumerator2.MoveNext());

                    var v = vert[lhs.VertexIndex];
                    v.norm = math.normalize(sum);
                    vert[lhs.VertexIndex] = v;

                    //vert[lhs.VertexIndex].norm = math.normalize(sum);

                } while(enumerator1.MoveNext());
            }
        }
        
        /// <summary>
        /// Smooth mesh.
        /// </summary>
        /// <param name="vert">Vertex array</param>
        /// <param name="tris">Triangle array</param>
        /// <param name="normals">Array that normals would be stored in</param>
        /// <param name="angle">Smoothing angle</param>
        public static void RecalculateNormals(NativeArray<Vertex> vert, NativeArray<Triangle> tris, ref NativeArray<float3> normals, float angle = 60)
        {
            var cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);
            var triNormals = new NativeArray<float3>(tris.Length, Allocator.Temp);
            var dictionary = new NativeMultiHashMap<VertexKey, VertexEntry>(vert.Length * 2, Allocator.Temp);

            for(var i = 0; i < tris.Length; i++)
            {
                var i1 = tris[i].indices[0];
                var i2 = tris[i].indices[1];
                var i3 = tris[i].indices[2];

                // Calculate the normal of the triangle
                var p1 = vert[i2].pos - vert[i1].pos;
                var p2 = vert[i3].pos - vert[i1].pos;
                var normal = math.normalize(math.cross(p1, p2));

                triNormals[i] = normal;
                
                dictionary.Add(new VertexKey(vert[i1].pos), new VertexEntry(0, i, i1));
                dictionary.Add(new VertexKey(vert[i2].pos), new VertexEntry(0, i, i2));
                dictionary.Add(new VertexKey(vert[i3].pos), new VertexEntry(0, i, i3));
            }

            var keys = dictionary.GetKeyArray(Allocator.Temp);

            for(var i = 0; i < keys.Length; i++)
            {
                var enumerator1 = dictionary.GetValuesForKey(keys[i]);
                do
                {
                    var sum = new float3();
                    var lhs = enumerator1.Current;
                    var enumerator2 = dictionary.GetValuesForKey(keys[i]);
                    do
                    {
                        var rhs = enumerator2.Current;

                        if (lhs.VertexIndex == rhs.VertexIndex) 
                            sum += triNormals[rhs.TriangleIndex];
                        else 
                        {
                            // The dot product is the cosine of the angle between the two triangles.
                            // A larger cosine means a smaller angle.
                            var dot = math.dot(triNormals[lhs.TriangleIndex], triNormals[rhs.TriangleIndex]);

                            if (dot >= cosineThreshold)
                                sum += triNormals[rhs.TriangleIndex];
                        }
                    } while(enumerator2.MoveNext());
                    
                    normals[lhs.VertexIndex] = math.normalize(sum);

                } while(enumerator1.MoveNext());
            }
        }

        public struct VertexKey : System.IEquatable<VertexKey>
        {
            private readonly long _x;
            private readonly long _y;
            private readonly long _z;
    
            // Change this if you require a different precision.
            private const int Tolerance = 100000;
    
            // Magic FNV values. Do not change these.
            private const long FNV32Init = 0x811c9dc5;
            private const long FNV32Prime = 0x01000193;
    
            public VertexKey(Vector3 position) {
                _x = (long)(math.round(position.x * Tolerance));
                _y = (long)(math.round(position.y * Tolerance));
                _z = (long)(math.round(position.z * Tolerance));
            }
    
            public override bool Equals(object obj) {
                var key = (VertexKey)obj;
                return _x == key._x && _y == key._y && _z == key._z;
            }
    
            public override int GetHashCode() {
                long rv = FNV32Init;
                rv ^= _x;
                rv *= FNV32Prime;
                rv ^= _y;
                rv *= FNV32Prime;
                rv ^= _z;
                rv *= FNV32Prime;
    
                return rv.GetHashCode();
            }

            public bool Equals(VertexKey other)
            {
                return _x == other._x && _y == other._y && _z == other._z;
            }
        }
    
        public struct VertexEntry {
            public int MeshIndex;
            public int TriangleIndex;
            public int VertexIndex;
    
            public VertexEntry(int meshIndex, int triIndex, int vertIndex) {
                MeshIndex = meshIndex;
                TriangleIndex = triIndex;
                VertexIndex = vertIndex;
            }
        }
    }
}
