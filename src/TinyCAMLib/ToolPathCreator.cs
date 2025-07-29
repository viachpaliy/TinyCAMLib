using System;
using System.Numerics;
using System.Collections.Generic;

namespace TinyCAMLib
{
    /// <summary>
    /// Represents a tool path strategy that generates paths for CNC machining.
    /// </summary>
    public enum ToolPathType
    {
        Cross,
        Circular,
        ZigZag,
        Spiral
    }

    /// <summary>
    /// Static class represents a tool path creator that generates paths based on the specified type.
    /// </summary>
    public static class ToolPathCreator
    {
        /// <summary>
        /// Creates a tool path based on the specified type and parameters.
        /// </summary>
        /// <param name="toolPathType">The type of tool path to create.</param>
        /// <param name="cutter">The milling cutter.</param>
        /// <param name="surface">The STL surface (stationary).</param>
        /// <param name="step">Step between points in the path.</param>
        /// <param name="startZ">Starting Z coordinate (above the surface).</param>
        /// <param name="endZ">Ending Z coordinate (below the surface).</param>
        /// <param name="precision">Precision for collision detection (smaller = more accurate).</param>
        /// <returns>List with points a tool path </returns>
        public static List<Vector3> CreateToolPath(ToolPathType toolPathType, MillingCutter cutter, STLSurf surface, float step,
            float startZ, float endZ, float precision = 0.01f)
        {
            List<Vector3> path = new List<Vector3>();
            switch (toolPathType)
            {
                case ToolPathType.Cross:
                    path = CreateCrossPath(cutter, surface, step, startZ, endZ, precision);
                    break;
                case ToolPathType.Circular:
                    path = CreateCircularPath(cutter, surface, step, startZ, endZ, precision);
                    break;
                case ToolPathType.ZigZag:
                    path = CreateZigZagPath(cutter, surface, step, startZ, endZ, precision);
                    break;
                case ToolPathType.Spiral:
                    path = CreateSpiralPath(cutter, surface, step, startZ, endZ, precision);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(toolPathType), toolPathType, null);
            }
            return path;
        }

        private static List<Vector3> CreateCrossPath(MillingCutter cutter, STLSurf surface, float step, float startZ, float endZ, float precision)
        {
            // Implementation for cross path generation
            float startX = surface.BoundingBox.Min.X;
            float endX = surface.BoundingBox.Max.X;
            float startY = surface.BoundingBox.Min.Y;
            float endY = surface.BoundingBox.Max.Y;
            List<Vector3> path = new List<Vector3>();
            // Generate a path along X axis
            for (float y = startY; y <= endY; y += 2 * step)
            {
                for (float x = startX; x <= endX; x += 2 * step)
                {
                   
                        path.Add(new Vector3(x, y, endZ));
                   
                }
                y += 2 * step; // Move to the next row
                for (float x = endX; x >= startX; x -= 2 * step)
                {
                   
                        path.Add(new Vector3(x, y , endZ));
                   
                }
            }
            // Generate a path along Y axis
            for (float x = startX; x <= endX; x += 2 * step)
            {
                for (float y = endY; y >= startY; y -= 2 * step)
                {
                    
                        path.Add(new Vector3(x, y, endZ));
                    
                }
                x += 2 * step; // Move to the next column
                for (float y = startY; y <= endY; y += 2 * step)
                {
                   
                        path.Add(new Vector3(x, y, endZ));
                   
                }       
            }

            Parallel.ForEach(path, point =>
            {
                 float? z = CollisionSolver.CalculateCollision(cutter, surface, point.X, point.Y, startZ, endZ, precision);
                    if (z != null)
                    {
                        point.Z = z.Value;
                    }   
            });
            
            return path;
        }

        private static List<Vector3> CreateCircularPath(MillingCutter cutter, STLSurf surface, float step, float startZ, float endZ, float precision)
        {
            // Implementation for circular path generation
            return new List<Vector3>();
        }

        private static List<Vector3> CreateZigZagPath(MillingCutter cutter, STLSurf surface, float step, float startZ, float endZ, float precision)
        {
            // Implementation for zigzag path generation
            return new List<Vector3>();
        }

        private static List<Vector3> CreateSpiralPath(MillingCutter cutter, STLSurf surface, float step, float startZ, float endZ, float precision)
        {
            // Implementation for spiral path generation
            return new List<Vector3>();
        }   

    }
}
