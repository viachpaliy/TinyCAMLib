using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace TinyCAMLib
{
    /// <summary>
    /// Represents an STL surface composed of triangular facets.
    /// </summary>
    public class STLSurf
    {
        /// <summary>
        /// The collection of triangles that make up the STL surface.
        /// </summary>
        public List<Triangle> Triangles { get; private set; }

        /// <summary>
        /// Gets the bounding box of the entire STL surface.
        /// </summary>
        public Bbox BoundingBox
        {
            get
            {
                if (Triangles.Count == 0)
                    return Bbox.Empty();
                return new Bbox(Triangles);
            }
        }

        /// <summary>
        /// Gets the number of triangles in the surface.
        /// </summary>
        public int TriangleCount => Triangles.Count;

        /// <summary>
        /// Gets the total surface area of all triangles.
        /// </summary>
        public float SurfaceArea => Triangles.Sum(t => t.Area());

        /// <summary>
        /// Gets all unique vertices in the surface.
        /// </summary>
        public IEnumerable<Vector3> Vertices
        {
            get
            {
                var vertices = new HashSet<Vector3>();
                foreach (var triangle in Triangles)
                {
                    vertices.Add(triangle.VertexA);
                    vertices.Add(triangle.VertexB);
                    vertices.Add(triangle.VertexC);
                }
                return vertices;
            }
        }

        /// <summary>
        /// Gets the number of unique vertices in the surface.
        /// </summary>
        public int VertexCount => Vertices.Count();

        /// <summary>
        /// Initializes a new instance of the STLSurf class with an empty triangle collection.
        /// </summary>
        public STLSurf()
        {
            Triangles = new List<Triangle>();
        }

        /// <summary>
        /// Initializes a new instance of the STLSurf class with the specified triangles.
        /// </summary>
        /// <param name="triangles">The triangles to initialize the surface with.</param>
        public STLSurf(IEnumerable<Triangle> triangles)
        {
            Triangles = new List<Triangle>(triangles);
        }

        /// <summary>
        /// Adds a triangle to the surface.
        /// </summary>
        /// <param name="triangle">The triangle to add.</param>
        public void AddTriangle(Triangle triangle)
        {
            Triangles.Add(triangle);
        }

        /// <summary>
        /// Adds a triangle to the surface using three vertices.
        /// </summary>
        /// <param name="a">First vertex.</param>
        /// <param name="b">Second vertex.</param>
        /// <param name="c">Third vertex.</param>
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Triangles.Add(new Triangle(a, b, c));
        }

        /// <summary>
        /// Adds multiple triangles to the surface.
        /// </summary>
        /// <param name="triangles">The triangles to add.</param>
        public void AddTriangles(IEnumerable<Triangle> triangles)
        {
            Triangles.AddRange(triangles);
        }

        /// <summary>
        /// Removes a triangle from the surface.
        /// </summary>
        /// <param name="triangle">The triangle to remove.</param>
        /// <returns>True if the triangle was removed; otherwise, false.</returns>
        public bool RemoveTriangle(Triangle triangle)
        {
            return Triangles.Remove(triangle);
        }

        /// <summary>
        /// Removes the triangle at the specified index.
        /// </summary>
        /// <param name="index">The index of the triangle to remove.</param>
        public void RemoveTriangleAt(int index)
        {
            if (index < 0 || index >= Triangles.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            Triangles.RemoveAt(index);
        }

        /// <summary>
        /// Clears all triangles from the surface.
        /// </summary>
        public void Clear()
        {
            Triangles.Clear();
        }

        /// <summary>
        /// Determines whether the surface contains the specified triangle.
        /// </summary>
        /// <param name="triangle">The triangle to search for.</param>
        /// <returns>True if the triangle is found; otherwise, false.</returns>
        public bool Contains(Triangle triangle)
        {
            return Triangles.Contains(triangle);
        }

        /// <summary>
        /// Transforms all triangles in the surface by the specified transformation matrix.
        /// </summary>
        /// <param name="transform">The transformation matrix to apply.</param>
        public void Transform(Matrix4x4 transform)
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                var triangle = Triangles[i];
                var transformedA = Vector3.Transform(triangle.VertexA, transform);
                var transformedB = Vector3.Transform(triangle.VertexB, transform);
                var transformedC = Vector3.Transform(triangle.VertexC, transform);
                
                Triangles[i] = new Triangle(transformedA, transformedB, transformedC);
            }
        }

        /// <summary>
        /// Translates all triangles in the surface by the specified offset.
        /// </summary>
        /// <param name="offset">The translation vector.</param>
        public void Translate(Vector3 offset)
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                var triangle = Triangles[i];
                Triangles[i] = new Triangle(
                    triangle.VertexA + offset,
                    triangle.VertexB + offset,
                    triangle.VertexC + offset
                );
            }
        }

        /// <summary>
        /// Scales all triangles in the surface by the specified scale factor.
        /// </summary>
        /// <param name="scale">The scale factor.</param>
        public void Scale(float scale)
        {
            Scale(new Vector3(scale));
        }

        /// <summary>
        /// Scales all triangles in the surface by the specified scale factors for each axis.
        /// </summary>
        /// <param name="scale">The scale factors for X, Y, and Z axes.</param>
        public void Scale(Vector3 scale)
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                var triangle = Triangles[i];
                Triangles[i] = new Triangle(
                    triangle.VertexA * scale,
                    triangle.VertexB * scale,
                    triangle.VertexC * scale
                );
            }
        }

        /// <summary>
        /// Creates a copy of the STL surface.
        /// </summary>
        /// <returns>A new STLSurf instance with the same triangles.</returns>
        public STLSurf Clone()
        {
            return new STLSurf(Triangles);
        }

        /// <summary>
        /// Merges another STL surface into this surface.
        /// </summary>
        /// <param name="other">The STL surface to merge.</param>
        public void Merge(STLSurf other)
        {
            Triangles.AddRange(other.Triangles);
        }

        /// <summary>
        /// Validates the STL surface by checking for degenerate triangles.
        /// </summary>
        /// <returns>True if all triangles are valid; otherwise, false.</returns>
        public bool IsValid()
        {
            return Triangles.All(t => t.Area() > float.Epsilon);
        }

        /// <summary>
        /// Removes degenerate triangles (triangles with zero or near-zero area).
        /// </summary>
        /// <returns>The number of triangles removed.</returns>
        public int RemoveDegenerateTriangles()
        {
            int originalCount = Triangles.Count;
            Triangles.RemoveAll(t => t.Area() <= float.Epsilon);
            return originalCount - Triangles.Count;
        }

        /// <summary>
        /// Loads an STL surface from a binary STL file.
        /// </summary>
        /// <param name="filePath">The path to the STL file.</param>
        /// <returns>A new STLSurf instance loaded from the file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the file format is invalid.</exception>
        public static STLSurf LoadFromBinarySTL(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"STL file not found: {filePath}");

            var surface = new STLSurf();
            
            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                // Skip 80-byte header
                reader.ReadBytes(80);
                
                // Read number of triangles
                uint triangleCount = reader.ReadUInt32();
                
                for (uint i = 0; i < triangleCount; i++)
                {
                    // Skip normal vector (12 bytes)
                    reader.ReadSingle(); // Normal X
                    reader.ReadSingle(); // Normal Y
                    reader.ReadSingle(); // Normal Z
                    
                    // Read vertices
                    Vector3 v1 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Vector3 v2 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Vector3 v3 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    
                    // Skip attribute byte count
                    reader.ReadUInt16();
                    
                    surface.AddTriangle(v1, v2, v3);
                }
            }
            
            return surface;
        }

        /// <summary>
        /// Loads an STL surface from an ASCII STL file.
        /// </summary>
        /// <param name="filePath">The path to the STL file.</param>
        /// <returns>A new STLSurf instance loaded from the file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the file format is invalid.</exception>
        public static STLSurf LoadFromAsciiSTL(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"STL file not found: {filePath}");

            var surface = new STLSurf();
            var lines = File.ReadAllLines(filePath);
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim().ToLowerInvariant();
                
                if (line.StartsWith("facet normal"))
                {
                    // Skip to outer loop
                    i++;
                    if (i >= lines.Length || !lines[i].Trim().ToLowerInvariant().StartsWith("outer loop"))
                        throw new InvalidDataException("Invalid ASCII STL format: expected 'outer loop'");
                    
                    Vector3[] vertices = new Vector3[3];
                    
                    // Read three vertices
                    for (int v = 0; v < 3; v++)
                    {
                        i++;
                        if (i >= lines.Length)
                            throw new InvalidDataException("Invalid ASCII STL format: unexpected end of file");
                        
                        var vertexLine = lines[i].Trim();
                        if (!vertexLine.ToLowerInvariant().StartsWith("vertex"))
                            throw new InvalidDataException("Invalid ASCII STL format: expected 'vertex'");
                        
                        var parts = vertexLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 4)
                            throw new InvalidDataException("Invalid ASCII STL format: vertex line format error");
                        
                        if (!float.TryParse(parts[1], out float x) ||
                            !float.TryParse(parts[2], out float y) ||
                            !float.TryParse(parts[3], out float z))
                            throw new InvalidDataException("Invalid ASCII STL format: invalid vertex coordinates");
                        
                        vertices[v] = new Vector3(x, y, z);
                    }
                    
                    // Skip endloop and endfacet
                    i++; // endloop
                    i++; // endfacet
                    
                    surface.AddTriangle(vertices[0], vertices[1], vertices[2]);
                }
            }
            
            return surface;
        }

        /// <summary>
        /// Saves the STL surface to a binary STL file.
        /// </summary>
        /// <param name="filePath">The path where to save the STL file.</param>
        public void SaveToBinarySTL(string filePath)
        {
            using (var writer = new BinaryWriter(File.Create(filePath)))
            {
                // Write 80-byte header
                var header = new byte[80];
                var headerText = Encoding.ASCII.GetBytes("TinyCAMLib STL Export");
                Array.Copy(headerText, header, Math.Min(headerText.Length, 80));
                writer.Write(header);
                
                // Write number of triangles
                writer.Write((uint)Triangles.Count);
                
                foreach (var triangle in Triangles)
                {
                    var normal = triangle.Normal();
                    
                    // Write normal vector
                    writer.Write(normal.X);
                    writer.Write(normal.Y);
                    writer.Write(normal.Z);
                    
                    // Write vertices
                    writer.Write(triangle.VertexA.X);
                    writer.Write(triangle.VertexA.Y);
                    writer.Write(triangle.VertexA.Z);
                    
                    writer.Write(triangle.VertexB.X);
                    writer.Write(triangle.VertexB.Y);
                    writer.Write(triangle.VertexB.Z);
                    
                    writer.Write(triangle.VertexC.X);
                    writer.Write(triangle.VertexC.Y);
                    writer.Write(triangle.VertexC.Z);
                    
                    // Write attribute byte count (always 0)
                    writer.Write((ushort)0);
                }
            }
        }

        /// <summary>
        /// Saves the STL surface to an ASCII STL file.
        /// </summary>
        /// <param name="filePath">The path where to save the STL file.</param>
        public void SaveToAsciiSTL(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("solid TinyCAMLibSTL");
                
                foreach (var triangle in Triangles)
                {
                    var normal = triangle.Normal();
                    
                    writer.WriteLine($"  facet normal {normal.X:F6} {normal.Y:F6} {normal.Z:F6}");
                    writer.WriteLine("    outer loop");
                    writer.WriteLine($"      vertex {triangle.VertexA.X:F6} {triangle.VertexA.Y:F6} {triangle.VertexA.Z:F6}");
                    writer.WriteLine($"      vertex {triangle.VertexB.X:F6} {triangle.VertexB.Y:F6} {triangle.VertexB.Z:F6}");
                    writer.WriteLine($"      vertex {triangle.VertexC.X:F6} {triangle.VertexC.Y:F6} {triangle.VertexC.Z:F6}");
                    writer.WriteLine("    endloop");
                    writer.WriteLine("  endfacet");
                }
                
                writer.WriteLine("endsolid TinyCAMLibSTL");
            }
        }

        /// <summary>
        /// Returns a string representation of the STL surface.
        /// </summary>
        /// <returns>A string showing the number of triangles and surface area.</returns>
        public override string ToString()
        {
            return $"STLSurf(Triangles: {TriangleCount}, Surface Area: {SurfaceArea:F2})";
        }
    }
}