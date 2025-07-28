using System;
using System.Numerics;

namespace TinyCAMLib
{
    /// <summary>
    /// Represents an arbitrary triangle in 3D space.
    /// </summary>
    public class Triangle
    {
        public Vector3 VertexA { get; }
        public Vector3 VertexB { get; }
        public Vector3 VertexC { get; }

        /// <summary>
        /// Constructs a triangle from three vertices.
        /// </summary>
        /// <param name="a">First vertex</param>
        /// <param name="b">Second vertex</param>
        /// <param name="c">Third vertex</param>
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            VertexA = a;
            VertexB = b;
            VertexC = c;
        }

        /// <summary>
        /// Calculates the area of the triangle.
        /// </summary>
        public float Area()
        {
            Vector3 ab = VertexB - VertexA;
            Vector3 ac = VertexC - VertexA;
            // Area = 0.5 * length of the cross product of two sides
            return 0.5f * Vector3.Cross(ab, ac).Length();
        }

        /// <summary>
        /// Calculates the normal vector of the triangle (oriented according to A, B, C).
        /// </summary>
        public Vector3 Normal()
        {
            Vector3 ab = VertexB - VertexA;
            Vector3 ac = VertexC - VertexA;
            return Vector3.Normalize(Vector3.Cross(ab, ac));
        }

        /// <summary>
        /// Determines whether a point is inside the triangle.
        /// The point must be in the same plane as the triangle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if point is inside the triangle; otherwise, false.</returns>
        public bool ContainsPoint(Vector3 point)
        {
            // Compute vectors
            Vector3 v0 = VertexC - VertexA;
            Vector3 v1 = VertexB - VertexA;
            Vector3 v2 = point - VertexA;

            // Compute dot products
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            float denom = dot00 * dot11 - dot01 * dot01;
            if (denom == 0) return false; // Degenerate triangle

            float u = (dot11 * dot02 - dot01 * dot12) / denom;
            float v = (dot00 * dot12 - dot01 * dot02) / denom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }
    }
}