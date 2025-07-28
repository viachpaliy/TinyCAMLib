using System;
using System.Collections.Generic;
using System.Numerics;

namespace TinyCAMLib
{
    /// <summary>
    /// Represents an axis-aligned bounding box (AABB) in 3D space.
    /// </summary>
    public class Bbox
    {
        /// <summary>
        /// The minimum corner of the bounding box.
        /// </summary>
        public Vector3 Min { get; private set; }

        /// <summary>
        /// The maximum corner of the bounding box.
        /// </summary>
        public Vector3 Max { get; private set; }

        /// <summary>
        /// Gets the center point of the bounding box.
        /// </summary>
        public Vector3 Center => (Min + Max) * 0.5f;

        /// <summary>
        /// Gets the size (extents) of the bounding box along each axis.
        /// </summary>
        public Vector3 Size => Max - Min;

        /// <summary>
        /// Gets the volume of the bounding box.
        /// </summary>
        public float Volume
        {
            get
            {
                Vector3 size = Size;
                return size.X * size.Y * size.Z;
            }
        }

        /// <summary>
        /// Gets the surface area of the bounding box.
        /// </summary>
        public float SurfaceArea
        {
            get
            {
                Vector3 size = Size;
                return 2.0f * (size.X * size.Y + size.Y * size.Z + size.Z * size.X);
            }
        }

        /// <summary>
        /// Gets whether the bounding box is valid (Min <= Max for all components).
        /// </summary>
        public bool IsValid => Min.X <= Max.X && Min.Y <= Max.Y && Min.Z <= Max.Z;

        /// <summary>
        /// Initializes a new instance of the Bbox class with specified minimum and maximum points.
        /// </summary>
        /// <param name="min">The minimum corner of the bounding box.</param>
        /// <param name="max">The maximum corner of the bounding box.</param>
        public Bbox(Vector3 min, Vector3 max)
        {
            Min = Vector3.Min(min, max);
            Max = Vector3.Max(min, max);
        }

        /// <summary>
        /// Initializes a new instance of the Bbox class from a single point (creates a degenerate box).
        /// </summary>
        /// <param name="point">The point to create the bounding box from.</param>
        public Bbox(Vector3 point)
        {
            Min = point;
            Max = point;
        }

        /// <summary>
        /// Initializes a new instance of the Bbox class from a collection of points.
        /// </summary>
        /// <param name="points">The points to create the bounding box from.</param>
        /// <exception cref="ArgumentException">Thrown when points collection is empty.</exception>
        public Bbox(IEnumerable<Vector3> points)
        {
            bool first = true;
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;

            foreach (Vector3 point in points)
            {
                if (first)
                {
                    min = max = point;
                    first = false;
                }
                else
                {
                    min = Vector3.Min(min, point);
                    max = Vector3.Max(max, point);
                }
            }

            if (first)
                throw new ArgumentException("Points collection cannot be empty.", nameof(points));

            Min = min;
            Max = max;
        }

        /// <summary>
        /// Initializes a new instance of the Bbox class from a triangle.
        /// </summary>
        /// <param name="triangle">The triangle to create the bounding box from.</param>
        public Bbox(Triangle triangle)
        {
            Min = Vector3.Min(Vector3.Min(triangle.VertexA, triangle.VertexB), triangle.VertexC);
            Max = Vector3.Max(Vector3.Max(triangle.VertexA, triangle.VertexB), triangle.VertexC);
        }

        /// <summary>
        /// Initializes a new instance of the Bbox class from a collection of triangles.
        /// </summary>
        /// <param name="triangles">The triangles to create the bounding box from.</param>
        /// <exception cref="ArgumentException">Thrown when triangles collection is empty.</exception>
        public Bbox(IEnumerable<Triangle> triangles)
        {
            bool first = true;
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;

            foreach (Triangle triangle in triangles)
            {
                Vector3 triangleMin = Vector3.Min(Vector3.Min(triangle.VertexA, triangle.VertexB), triangle.VertexC);
                Vector3 triangleMax = Vector3.Max(Vector3.Max(triangle.VertexA, triangle.VertexB), triangle.VertexC);

                if (first)
                {
                    min = triangleMin;
                    max = triangleMax;
                    first = false;
                }
                else
                {
                    min = Vector3.Min(min, triangleMin);
                    max = Vector3.Max(max, triangleMax);
                }
            }

            if (first)
                throw new ArgumentException("Triangles collection cannot be empty.", nameof(triangles));

            Min = min;
            Max = max;
        }

        /// <summary>
        /// Determines whether the bounding box contains the specified point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if the point is inside or on the boundary of the bounding box; otherwise, false.</returns>
        public bool Contains(Vector3 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y &&
                   point.Z >= Min.Z && point.Z <= Max.Z;
        }

        /// <summary>
        /// Determines whether this bounding box completely contains another bounding box.
        /// </summary>
        /// <param name="other">The other bounding box to test.</param>
        /// <returns>True if this bounding box completely contains the other; otherwise, false.</returns>
        public bool Contains(Bbox other)
        {
            return Contains(other.Min) && Contains(other.Max);
        }

        /// <summary>
        /// Determines whether this bounding box intersects with another bounding box.
        /// </summary>
        /// <param name="other">The other bounding box to test.</param>
        /// <returns>True if the bounding boxes intersect; otherwise, false.</returns>
        public bool Intersects(Bbox other)
        {
            return Max.X >= other.Min.X && Min.X <= other.Max.X &&
                   Max.Y >= other.Min.Y && Min.Y <= other.Max.Y &&
                   Max.Z >= other.Min.Z && Min.Z <= other.Max.Z;
        }

        /// <summary>
        /// Expands the bounding box to include the specified point.
        /// </summary>
        /// <param name="point">The point to include.</param>
        public void Expand(Vector3 point)
        {
            Min = Vector3.Min(Min, point);
            Max = Vector3.Max(Max, point);
        }

        /// <summary>
        /// Expands the bounding box to include another bounding box.
        /// </summary>
        /// <param name="other">The other bounding box to include.</param>
        public void Expand(Bbox other)
        {
            Min = Vector3.Min(Min, other.Min);
            Max = Vector3.Max(Max, other.Max);
        }

        /// <summary>
        /// Expands the bounding box by the specified amount in all directions.
        /// </summary>
        /// <param name="amount">The amount to expand by.</param>
        public void Expand(float amount)
        {
            Vector3 expansion = new Vector3(amount);
            Min -= expansion;
            Max += expansion;
        }

        /// <summary>
        /// Returns the intersection of this bounding box with another bounding box.
        /// </summary>
        /// <param name="other">The other bounding box.</param>
        /// <returns>The intersection bounding box, or null if they don't intersect.</returns>
        public Bbox? Intersection(Bbox other)
        {
            if (!Intersects(other))
                return null;

            Vector3 min = Vector3.Max(Min, other.Min);
            Vector3 max = Vector3.Min(Max, other.Max);

            return new Bbox(min, max);
        }

        /// <summary>
        /// Returns the union of this bounding box with another bounding box.
        /// </summary>
        /// <param name="other">The other bounding box.</param>
        /// <returns>The union bounding box.</returns>
        public Bbox Union(Bbox other)
        {
            Vector3 min = Vector3.Min(Min, other.Min);
            Vector3 max = Vector3.Max(Max, other.Max);
            return new Bbox(min, max);
        }

        /// <summary>
        /// Gets the closest point on or inside the bounding box to the specified point.
        /// </summary>
        /// <param name="point">The reference point.</param>
        /// <returns>The closest point on or inside the bounding box.</returns>
        public Vector3 ClosestPoint(Vector3 point)
        {
            return Vector3.Clamp(point, Min, Max);
        }

        /// <summary>
        /// Calculates the squared distance from a point to the bounding box.
        /// </summary>
        /// <param name="point">The point to calculate distance to.</param>
        /// <returns>The squared distance to the bounding box (0 if point is inside).</returns>
        public float DistanceSquared(Vector3 point)
        {
            Vector3 closest = ClosestPoint(point);
            return Vector3.DistanceSquared(point, closest);
        }

        /// <summary>
        /// Calculates the distance from a point to the bounding box.
        /// </summary>
        /// <param name="point">The point to calculate distance to.</param>
        /// <returns>The distance to the bounding box (0 if point is inside).</returns>
        public float Distance(Vector3 point)
        {
            return (float)Math.Sqrt(DistanceSquared(point));
        }

        /// <summary>
        /// Gets all eight corner points of the bounding box.
        /// </summary>
        /// <returns>An array of the eight corner points.</returns>
        public Vector3[] GetCorners()
        {
            return new Vector3[]
            {
                new Vector3(Min.X, Min.Y, Min.Z), // 000
                new Vector3(Max.X, Min.Y, Min.Z), // 100
                new Vector3(Min.X, Max.Y, Min.Z), // 010
                new Vector3(Max.X, Max.Y, Min.Z), // 110
                new Vector3(Min.X, Min.Y, Max.Z), // 001
                new Vector3(Max.X, Min.Y, Max.Z), // 101
                new Vector3(Min.X, Max.Y, Max.Z), // 011
                new Vector3(Max.X, Max.Y, Max.Z)  // 111
            };
        }

        /// <summary>
        /// Returns a string representation of the bounding box.
        /// </summary>
        /// <returns>A string representation showing the min and max points.</returns>
        public override string ToString()
        {
            return $"Bbox(Min: {Min}, Max: {Max})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current bounding box.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is Bbox other && Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        /// <summary>
        /// Returns the hash code for this bounding box.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }

        /// <summary>
        /// Creates an empty bounding box (invalid box with Max &lt; Min).
        /// </summary>
        /// <returns>An empty bounding box.</returns>
        public static Bbox Empty()
        {
            return new Bbox(
                new Vector3(float.MaxValue), 
                new Vector3(float.MinValue)
            );
        }

        /// <summary>
        /// Creates a bounding box that encompasses the entire coordinate system.
        /// </summary>
        /// <returns>An infinite bounding box.</returns>
        public static Bbox Infinite()
        {
            return new Bbox(
                new Vector3(float.MinValue), 
                new Vector3(float.MaxValue)
            );
        }
    }
}