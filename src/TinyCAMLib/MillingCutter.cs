using System;
using System.Numerics;

namespace TinyCAMLib
{
    /// <summary>
    /// Represents the type of milling cutter.
    /// </summary>
    public enum MillingCutterType
    {
        /// <summary>
        /// Ball nose cutter with spherical tip.
        /// </summary>
        BallNose,
        
        /// <summary>
        /// Cylindrical flat end cutter.
        /// </summary>
        Cylinder,
        
        /// <summary>
        /// Conical tapered cutter.
        /// </summary>
        Cone,
        
        /// <summary>
        /// Tapered ball nose cutter with spherical tip and tapered sides.
        /// </summary>
        TaperedBallNose
    }

    /// <summary>
    /// Represents a milling cutter with its geometric properties.
    /// </summary>
    public class MillingCutter
    {
        /// <summary>
        /// Gets the type of the milling cutter.
        /// </summary>
        public MillingCutterType Type { get; }

        /// <summary>
        /// Gets the diameter of the milling cutter.
        /// </summary>
        public float Diameter { get; }

        /// <summary>
        /// Gets the tip radius of the milling cutter (for ball nose and tapered ball nose cutters).
        /// </summary>
        public float TipRadius { get; }

        /// <summary>
        /// Gets the cutting length of the milling cutter.
        /// </summary>
        public float CutterLength { get; }

        /// <summary>
        /// Initializes a new instance of the MillingCutter class.
        /// </summary>
        /// <param name="type">The type of the milling cutter.</param>
        /// <param name="diameter">The diameter of the cutter.</param>
        /// <param name="tipRadius">The tip radius (for ball nose cutters).</param>
        /// <param name="cutterLength">The cutting length of the cutter.</param>
        public MillingCutter(MillingCutterType type, float diameter, float tipRadius, float cutterLength)
        {
            Type = type;
            Diameter = diameter;
            TipRadius = tipRadius;
            CutterLength = cutterLength;
        }

        /// <summary>
        /// Gets the radius of the milling cutter.
        /// </summary>
        public float Radius => Diameter / 2.0f;

        /// <summary>
        /// Returns a string representation of the milling cutter.
        /// </summary>
        /// <returns>A string describing the cutter type and dimensions.</returns>
        public override string ToString()
        {
            return $"MillingCutter({Type}, Diameter: {Diameter}, TipRadius: {TipRadius}, Length: {CutterLength})";
        }
    }
}