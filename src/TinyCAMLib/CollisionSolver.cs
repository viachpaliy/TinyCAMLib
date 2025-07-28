using System;
using System.Numerics;

namespace TinyCAMLib
{
    /// <summary>
    /// Static class for calculating collision points between milling cutters and STL surfaces.
    /// </summary>
    public static class CollisionSolver
    {
        /// <summary>
        /// Calculates the collision point between a milling cutter and STL surface.
        /// The cutter moves along the Z-axis through the specified X,Y coordinates.
        /// Uses binary search for optimal performance.
        /// </summary>
        /// <param name="cutter">The milling cutter.</param>
        /// <param name="surface">The STL surface (stationary).</param>
        /// <param name="x">X coordinate of the cutter path.</param>
        /// <param name="y">Y coordinate of the cutter path.</param>
        /// <param name="startZ">Starting Z coordinate (above the surface).</param>
        /// <param name="endZ">Ending Z coordinate (below the surface).</param>
        /// <param name="precision">Precision for collision detection (smaller = more accurate).</param>
        /// <returns>The Z coordinate of the collision point, or null if no collision found.</returns>
        public static float? CalculateCollision(MillingCutter cutter, STLSurf surface, float x, float y, 
            float startZ, float endZ, float precision = 0.01f)
        {
            if (surface.TriangleCount == 0)
                return null;

            // Ensure startZ > endZ for downward movement
            if (startZ <= endZ)
                return null;

            float highZ = startZ;
            float lowZ = endZ;

            // Check if there's any collision in the range
            Vector3 startPosition = new Vector3(x, y, highZ);
            Vector3 endPosition = new Vector3(x, y, lowZ);
            
            bool hasCollisionAtStart = HasCollision(cutter, startPosition, surface);
            bool hasCollisionAtEnd = HasCollision(cutter, endPosition, surface);

            // If no collision at end, there might be no collision at all
            if (!hasCollisionAtEnd)
                return null;

            // If collision at start, the collision point is at or above startZ
            if (hasCollisionAtStart)
                return startZ;

            // Binary search for the collision point
            while ((highZ - lowZ) > precision)
            {
                float midZ = (highZ + lowZ) * 0.5f;
                Vector3 midPosition = new Vector3(x, y, midZ);

                if (HasCollision(cutter, midPosition, surface))
                {
                    // Collision found at midZ, search in upper half
                    lowZ = midZ;
                }
                else
                {
                    // No collision at midZ, search in lower half
                    highZ = midZ;
                }
            }

            // Return the collision point (lowZ will be the highest Z with collision)
            return lowZ;
        }

        /// <summary>
        /// Checks if the milling cutter at the given position collides with the STL surface.
        /// </summary>
        /// <param name="cutter">The milling cutter.</param>
        /// <param name="position">The position of the cutter tip.</param>
        /// <param name="surface">The STL surface.</param>
        /// <returns>True if collision detected, false otherwise.</returns>
        private static bool HasCollision(MillingCutter cutter, Vector3 position, STLSurf surface)
        {
            switch (cutter.Type)
            {
                case MillingCutterType.BallNose:
                    Vector3 center = position + new Vector3(0, 0, cutter.Radius);
                    return CheckSphereCollision(center, cutter.Radius, surface);

                case MillingCutterType.Cylinder:
                    return CheckCylinderCollision(position, cutter.Radius, cutter.CutterLength, surface);
                
                case MillingCutterType.Cone:
                    return CheckConeCollision(position, cutter.Radius, cutter.CutterLength, surface);
                
                case MillingCutterType.TaperedBallNose:
                    Vector3 tipCenter = position + new Vector3(0, 0, cutter.TipRadius);
                    return CheckSphereCollision(tipCenter, cutter.TipRadius, surface);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks collision between a sphere (ball nose cutter) and STL surface.
        /// </summary>
        /// <param name="center">Center of the sphere (cutter tip position).</param>
        /// <param name="radius">Radius of the sphere.</param>
        /// <param name="surface">The STL surface.</param>
        /// <returns>True if collision detected.</returns>
        private static bool CheckSphereCollision(Vector3 center, float radius, STLSurf surface)
        {
            // Create bounding box for the sphere
            Vector3 radiusVector = new Vector3(radius);
            Bbox sphereBbox = new Bbox(center - radiusVector, center + radiusVector);

            foreach (var triangle in surface.Triangles)
            {
                // Quick bounding box check first
                Bbox triangleBbox = new Bbox(triangle);
                if (!sphereBbox.Intersects(triangleBbox))
                    continue;

                // Expensive geometric check only for potential candidates
                float distance = DistancePointToTriangle(center, triangle);
                if (distance <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks collision between a cylinder (flat end cutter) and STL surface.
        /// </summary>
        /// <param name="tipPosition">Position of the cylinder tip (bottom center).</param>
        /// <param name="radius">Radius of the cylinder.</param>
        /// <param name="length">Length of the cylinder.</param>
        /// <param name="surface">The STL surface.</param>
        /// <returns>True if collision detected.</returns>
        private static bool CheckCylinderCollision(Vector3 tipPosition, float radius, float length, STLSurf surface)
        {
            // Create bounding box for the cylinder
            Vector3 radiusVector = new Vector3(radius, radius, 0);
            Vector3 bottomMin = tipPosition - radiusVector;
            Vector3 topMax = tipPosition + new Vector3(radius, radius, length);
            Bbox cylinderBbox = new Bbox(bottomMin, topMax);

            foreach (var triangle in surface.Triangles)
            {
                // Quick bounding box check first
                Bbox triangleBbox = new Bbox(triangle);
                if (!cylinderBbox.Intersects(triangleBbox))
                    continue;

                // Check flat bottom circle
                if (TriangleIntersectsCircle(triangle, tipPosition, radius, Vector3.UnitZ))
                {
                    return true;
                }

                // Check cylindrical sides
                Vector3 topCenter = tipPosition + new Vector3(0, 0, length);
                if (TriangleIntersectsCylinder(triangle, tipPosition, topCenter, radius))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks collision between a cone (tapered cutter) and STL surface.
        /// </summary>
        /// <param name="tipPosition">Position of the cone tip.</param>
        /// <param name="baseRadius">Radius at the base of the cone.</param>
        /// <param name="height">Height of the cone.</param>
        /// <param name="surface">The STL surface.</param>
        /// <returns>True if collision detected.</returns>
        private static bool CheckConeCollision(Vector3 tipPosition, float baseRadius, float height, STLSurf surface)
        {
            // Create bounding box for the cone
            Vector3 radiusVector = new Vector3(baseRadius, baseRadius, 0);
            Vector3 bottomMin = tipPosition - new Vector3(0, 0, 0); // Tip point
            Vector3 topMax = tipPosition + new Vector3(baseRadius, baseRadius, height);
            Vector3 bottomMinExtended = new Vector3(tipPosition.X - baseRadius, tipPosition.Y - baseRadius, tipPosition.Z);
            Bbox coneBbox = new Bbox(
                Vector3.Min(bottomMin, bottomMinExtended), 
                topMax
            );

            Vector3 baseCenter = tipPosition + new Vector3(0, 0, height);
            
            foreach (var triangle in surface.Triangles)
            {
                // Quick bounding box check first
                Bbox triangleBbox = new Bbox(triangle);
                if (!coneBbox.Intersects(triangleBbox))
                    continue;

                // Expensive geometric check only for potential candidates
                if (TriangleIntersectsCone(triangle, tipPosition, baseCenter, baseRadius))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates the distance from a point to a triangle.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="triangle">The triangle.</param>
        /// <returns>The shortest distance from point to triangle.</returns>
        private static float DistancePointToTriangle(Vector3 point, Triangle triangle)
        {
            // Project point onto triangle plane
            Vector3 normal = triangle.Normal();
            float distance = Vector3.Dot(point - triangle.VertexA, normal);
            Vector3 projectedPoint = point - distance * normal;

            // Check if projected point is inside triangle
            if (triangle.ContainsPoint(projectedPoint))
            {
                return Math.Abs(distance);
            }

            // Point is outside triangle, find closest edge
            float minDist = float.MaxValue;
            
            // Check distance to each edge
            minDist = Math.Min(minDist, DistancePointToLineSegment(point, triangle.VertexA, triangle.VertexB));
            minDist = Math.Min(minDist, DistancePointToLineSegment(point, triangle.VertexB, triangle.VertexC));
            minDist = Math.Min(minDist, DistancePointToLineSegment(point, triangle.VertexC, triangle.VertexA));

            return minDist;
        }

        /// <summary>
        /// Calculates the distance from a point to a line segment.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="lineStart">Start of the line segment.</param>
        /// <param name="lineEnd">End of the line segment.</param>
        /// <returns>The shortest distance from point to line segment.</returns>
        private static float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 line = lineEnd - lineStart;
            float lineLength = line.Length();
            
            if (lineLength < float.Epsilon)
                return Vector3.Distance(point, lineStart);

            Vector3 lineDir = line / lineLength;
            Vector3 toPoint = point - lineStart;
            float projection = Vector3.Dot(toPoint, lineDir);

            if (projection <= 0)
                return Vector3.Distance(point, lineStart);
            if (projection >= lineLength)
                return Vector3.Distance(point, lineEnd);

            Vector3 closestPoint = lineStart + projection * lineDir;
            return Vector3.Distance(point, closestPoint);
        }

        /// <summary>
        /// Checks if a triangle intersects with a circle.
        /// </summary>
        /// <param name="triangle">The triangle.</param>
        /// <param name="circleCenter">Center of the circle.</param>
        /// <param name="circleRadius">Radius of the circle.</param>
        /// <param name="circleNormal">Normal vector of the circle plane.</param>
        /// <returns>True if intersection exists.</returns>
        private static bool TriangleIntersectsCircle(Triangle triangle, Vector3 circleCenter, float circleRadius, Vector3 circleNormal)
        {
            // Simplified check: if any vertex of triangle is within circle radius from center
            float radiusSquared = circleRadius * circleRadius;
            
            if (Vector3.DistanceSquared(triangle.VertexA, circleCenter) <= radiusSquared)
                return true;
            if (Vector3.DistanceSquared(triangle.VertexB, circleCenter) <= radiusSquared)
                return true;
            if (Vector3.DistanceSquared(triangle.VertexC, circleCenter) <= radiusSquared)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a triangle intersects with a cylinder.
        /// </summary>
        /// <param name="triangle">The triangle.</param>
        /// <param name="cylinderBottom">Bottom center of the cylinder.</param>
        /// <param name="cylinderTop">Top center of the cylinder.</param>
        /// <param name="cylinderRadius">Radius of the cylinder.</param>
        /// <returns>True if intersection exists.</returns>
        private static bool TriangleIntersectsCylinder(Triangle triangle, Vector3 cylinderBottom, Vector3 cylinderTop, float cylinderRadius)
        {
            // Simplified check: if any vertex is within cylinder radius from axis
            float radiusSquared = cylinderRadius * cylinderRadius;
            Vector3 axis = cylinderTop - cylinderBottom;
            float axisLength = axis.Length();
            
            if (axisLength < float.Epsilon)
                return false;

            Vector3 axisDir = axis / axisLength;

            Vector3[] vertices = { triangle.VertexA, triangle.VertexB, triangle.VertexC };
            
            foreach (var vertex in vertices)
            {
                Vector3 toVertex = vertex - cylinderBottom;
                float projection = Vector3.Dot(toVertex, axisDir);
                
                if (projection >= 0 && projection <= axisLength)
                {
                    Vector3 closestOnAxis = cylinderBottom + projection * axisDir;
                    float distanceToAxisSquared = Vector3.DistanceSquared(vertex, closestOnAxis);
                    
                    if (distanceToAxisSquared <= radiusSquared)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a triangle intersects with a cone.
        /// </summary>
        /// <param name="triangle">The triangle.</param>
        /// <param name="coneTip">Tip of the cone.</param>
        /// <param name="coneBase">Center of the cone base.</param>
        /// <param name="baseRadius">Radius of the cone base.</param>
        /// <returns>True if intersection exists.</returns>
        private static bool TriangleIntersectsCone(Triangle triangle, Vector3 coneTip, Vector3 coneBase, float baseRadius)
        {
            Vector3 axis = coneBase - coneTip;
            float height = axis.Length();
            
            if (height < float.Epsilon)
                return false;

            Vector3 axisDir = axis / height;
            Vector3[] vertices = { triangle.VertexA, triangle.VertexB, triangle.VertexC };

            foreach (var vertex in vertices)
            {
                Vector3 toVertex = vertex - coneTip;
                float projection = Vector3.Dot(toVertex, axisDir);
                
                if (projection >= 0 && projection <= height)
                {
                    float radiusAtHeight = baseRadius * (projection / height);
                    Vector3 centerAtHeight = coneTip + projection * axisDir;
                    float distanceToAxisSquared = Vector3.DistanceSquared(vertex, centerAtHeight);
                    
                    if (distanceToAxisSquared <= radiusAtHeight * radiusAtHeight)
                        return true;
                }
            }

            return false;
        }
    }
}