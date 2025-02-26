﻿

using System;
using UnityEngine;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A blend shape.
    /// </summary>
    [Serializable]
    public struct BlendShape
    {
        /// <summary>
        /// The name of the blend shape.
        /// </summary>
        public string ShapeName;
        /// <summary>
        /// The blend shape frames.
        /// </summary>
        public BlendShapeFrame[] Frames;

        /// <summary>
        /// Creates a new blend shape.
        /// </summary>
        /// <param name="shapeName">The name of the blend shape.</param>
        /// <param name="frames">The blend shape frames.</param>
        public BlendShape(string shapeName, BlendShapeFrame[] frames)
        {
            this.ShapeName = shapeName;
            this.Frames = frames;
        }
    }

    /// <summary>
    /// A blend shape frame.
    /// </summary>
    [Serializable]
    public struct BlendShapeFrame
    {
        /// <summary>
        /// The name of the blend shape this frame is associated with.
        /// </summary>
        public string shapeName;
        /// <summary>
        /// The weight of the blend shape frame.
        /// </summary>
        public float frameWeight;
        /// <summary>
        /// The delta vertices of the blend shape frame.
        /// </summary>
        public Vector3[] deltaVertices;
        /// <summary>
        /// The delta normals of the blend shape frame.
        /// </summary>
        public Vector3[] deltaNormals;
        /// <summary>
        /// The delta tangents of the blend shape frame.
        /// </summary>
        public Vector3[] deltaTangents;
        /// <summary>
        /// The vertex offset to be used in the combined mesh vertex array.
        /// </summary>
        public int vertexOffset;


        /// <summary>
        /// Creates a new blend shape frame.
        /// </summary>
        /// <param name="frameWeight">The weight of the blend shape frame.</param>
        /// <param name="deltaVertices">The delta vertices of the blend shape frame.</param>
        /// <param name="deltaNormals">The delta normals of the blend shape frame.</param>
        /// <param name="deltaTangents">The delta tangents of the blend shape frame.</param>
        public BlendShapeFrame(float frameWeight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)
        {
            this.frameWeight = frameWeight;
            this.deltaVertices = deltaVertices;
            this.deltaNormals = deltaNormals;
            this.deltaTangents = deltaTangents;
            this.shapeName = "";
            this.vertexOffset = -1;
        }


        public BlendShapeFrame(string shapeName, float frameWeight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents, int vertexOffset)
        {
            this.shapeName = shapeName;
            this.frameWeight = frameWeight;
            this.deltaVertices = deltaVertices;
            this.deltaNormals = deltaNormals;
            this.deltaTangents = deltaTangents;
            this.vertexOffset = vertexOffset;
        }
    }
}
