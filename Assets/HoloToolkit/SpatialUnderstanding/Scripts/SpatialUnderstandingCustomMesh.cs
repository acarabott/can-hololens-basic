﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VR.WSA;
using HoloToolkit.Unity.SpatialMapping;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// Handles the custom meshes generated by the understanding dll. The meshes
    /// are generated during the scanning phase and once more on scan finalization.
    /// The meshes can be used to visualize the scanning progress.
    /// </summary>
    public class SpatialUnderstandingCustomMesh : SpatialMappingSource
    {
        // Config
        [Tooltip("Indicate the time in seconds between mesh imports, during the scanning phase. A value of zero will disable pulling meshes from the dll")]
        public float ImportMeshPeriod = 1.0f;
        [Tooltip("Material used to render the custom mesh generated by the dll")]
        public Material MeshMaterial;

        /// <summary>
        /// Used to keep our processing from exceeding our frame budget.
        /// </summary>
        [Tooltip("Max time per frame in milliseconds to spend processing the mesh")]
        public float MaxFrameTime = 5.0f;

        private bool drawProcessedMesh = true;
        // Properties
        /// <summary>
        /// Controls rendering of the mesh. This can be set by the user to hide or show the mesh.
        /// </summary>
        public bool DrawProcessedMesh
        {
            get
            {
                return drawProcessedMesh;
            }
            set
            {
                drawProcessedMesh = value;
                for (int i = 0; i < SurfaceObjects.Count; ++i)
                {
                    SurfaceObjects[i].Renderer.enabled = drawProcessedMesh;
                }
            }
        }

        /// <summary>
        /// Indicates if the previous import is still being processed.
        /// </summary>
        public bool IsImportActive { get; private set; }

        /// <summary>
        /// The material to use if rendering.
        /// </summary>
        protected override Material RenderMaterial { get { return MeshMaterial; } }

        /// <summary>
        /// To prevent us from importing too often, we keep track of the last import.
        /// </summary>
        private DateTime timeLastImportedMesh = DateTime.Now;

        /// <summary>
        /// For a cached SpatialUnderstanding.Instance.
        /// </summary>
        private SpatialUnderstanding spatialUnderstanding;

        /// <summary>
        /// The spatial understanding mesh will be split into pieces so that we don't have to 
        /// render the whole mesh, rather just the parts that are visible to the user.
        /// </summary>
        private Dictionary<Vector3, MeshData> meshSectors = new Dictionary<Vector3, MeshData>();

        /// <summary>
        /// A data structure to manage collecting triangles as we 
        /// subdivide the spatial understanding mesh into smaller sub meshes.
        /// </summary>
        private class MeshData
        {
            /// <summary>
            /// Lists of verts/triangles that describe the mesh geometry.
            /// </summary>
            private List<Vector3> verts = new List<Vector3>();
            private List<int> tris = new List<int>();

            /// <summary>
            /// The mesh object based on the triangles passed in.
            /// </summary>
            public Mesh MeshObject { get; private set;}

            public MeshData()
            {
                MeshObject = new Mesh();
            }

            /// <summary>
            /// Clears the geometry, but does not clear the mesh.
            /// </summary>
            public void Reset()
            {
                verts.Clear();
                tris.Clear();
            }

            /// <summary>
            /// Commits the new geometry to the mesh.
            /// </summary>
            public void Commit()
            {
                MeshObject.Clear();
                if (verts.Count > 2)
                {
                    MeshObject.SetVertices(verts);
                    MeshObject.SetTriangles(tris.ToArray(), 0);
                    MeshObject.RecalculateNormals();
                    MeshObject.RecalculateBounds();
                }
            }

            /// <summary>
            /// Adds a triangle composed of the specified three points to our mesh.
            /// </summary>
            /// <param name="point1">First point on the triangle.</param>
            /// <param name="point2">Second point on the triangle.</param>
            /// <param name="point3">Third point on the triangle.</param>
            public void AddTriangle(Vector3 point1, Vector3 point2, Vector3 point3) 
            {
                // Currently spatial understanding in the native layer voxellizes the space 
                // into ~2000 voxels per cubic meter.  Even in a degerate case we 
                // will use far fewer than 65000 vertices, this check should not fail
                // unless the spatial understanding native layer is updated to have more
                // voxels per cubic meter. 
                if (verts.Count < 65000)
                {
                    tris.Add(verts.Count);
                    verts.Add(point1);

                    tris.Add(verts.Count);
                    verts.Add(point2);

                    tris.Add(verts.Count);
                    verts.Add(point3);
                }
                else
                {
                    Debug.LogError("Mesh would have more vertices than Unity supports");
                }
            }
        }

        private void Start()
        {
            spatialUnderstanding = SpatialUnderstanding.Instance;
            if (gameObject.GetComponent<WorldAnchor>() == null)
            {
                gameObject.AddComponent<WorldAnchor>();
            }
        }

        private void Update()
        {
            Update_MeshImport(Time.deltaTime);
        }

        /// <summary>
        /// Adds a triangle with the specified points to the specified sector. 
        /// </summary>
        /// <param name="sector">The sector to add the triangle to.</param>
        /// <param name="point1">First point of the triangle.</param>
        /// <param name="point2">Second point of the triangle.</param>
        /// <param name="point3">Third point of the triangle.</param>
        private void AddTriangleToSector(Vector3 sector, Vector3 point1, Vector3 point2, Vector3 point3)
        {
            // Grab the mesh container we are using for this sector.
            MeshData nextSectorData;
            if (!meshSectors.TryGetValue(sector, out nextSectorData))
            {
                // Or make it if this is a new sector.
                nextSectorData = new MeshData();
                meshSectors.Add(sector, nextSectorData);
            }

            // Add the vertices to the sector's mesh container.
            nextSectorData.AddTriangle(point1, point2, point3);
        }

        /// <summary>
        /// Imports the custom mesh from the dll. This a a coroutine which will take multiple frames to complete.
        /// </summary>
        /// <returns></returns>
        public IEnumerator Import_UnderstandingMesh()
        {
            if (!spatialUnderstanding.AllowSpatialUnderstanding || IsImportActive)
            {
                yield break;
            }

            IsImportActive = true;

            SpatialUnderstandingDll dll = spatialUnderstanding.UnderstandingDLL;

            Vector3[] meshVertices = null;
            Vector3[] meshNormals = null;
            Int32[] meshIndices = null;

            // Pull the mesh - first get the size, then allocate and pull the data
            int vertCount;
            int idxCount;

            if ((SpatialUnderstandingDll.Imports.GeneratePlayspace_ExtractMesh_Setup(out vertCount, out idxCount) > 0) &&
                (vertCount > 0) &&
                (idxCount > 0))
            {
                meshVertices = new Vector3[vertCount];
                IntPtr vertPos = dll.PinObject(meshVertices);
                meshNormals = new Vector3[vertCount];
                IntPtr vertNorm = dll.PinObject(meshNormals);
                meshIndices = new Int32[idxCount];
                IntPtr indices = dll.PinObject(meshIndices);

                SpatialUnderstandingDll.Imports.GeneratePlayspace_ExtractMesh_Extract(vertCount, vertPos, vertNorm, idxCount, indices);
            }

            // Wait a frame
            yield return null;

            // Create output meshes
            if ((meshVertices != null) &&
                (meshVertices.Length > 0) &&
                (meshIndices != null) &&
                (meshIndices.Length > 0))
            {
                // first get all our mesh data containers ready for meshes.
                foreach (MeshData meshdata in meshSectors.Values)
                {
                    meshdata.Reset();
                }

                DateTime startTime = DateTime.Now;
                // first we need to split the playspace up into segments so we don't always 
                // draw everything.  We can break things up in to cubic meters.  
                for (int index = 0; index < meshIndices.Length; index += 3)
                {
                    Vector3 firstVertex = meshVertices[meshIndices[index]];
                    Vector3 secondVertex = meshVertices[meshIndices[index + 1]];
                    Vector3 thirdVertex = meshVertices[meshIndices[index + 2]];

                    // The triangle may belong to multiple sectors.  We will copy the whole triangle
                    // to all of the sectors it belongs to.  This will fill in seams on sector edges
                    // although it could cause some amount of visible z-fighting if rendering a wireframe.
                    Vector3 firstSector = VectorToSector(firstVertex);

                    AddTriangleToSector(firstSector, firstVertex, secondVertex, thirdVertex);

                    // If the second sector doesn't match the first, copy the triangle to the second sector.
                    Vector3 secondSector = VectorToSector(secondVertex);
                    if(secondSector != firstSector)
                    {
                        AddTriangleToSector(secondSector, firstVertex, secondVertex, thirdVertex);
                    }

                    // If the third sector matches neither the first nor second sector, copy the triangle to the
                    // third sector.
                    Vector3 thirdSector = VectorToSector(thirdVertex);
                    if (thirdSector != firstSector && thirdSector != secondSector)
                    {
                        AddTriangleToSector(thirdSector, firstVertex, secondVertex, thirdVertex);
                    }

                    // Limit our run time so that we don't cause too many frame drops.
                    // Only checking every 10 iterations or so to prevent losing too much time to checking the clock.
                    if (index % 30 == 0 && (DateTime.Now - startTime).TotalMilliseconds > MaxFrameTime)
                    {
                        //  Debug.LogFormat("{0} of {1} processed", index, meshIndices.Length);
                        yield return null;
                        startTime = DateTime.Now;
                    }
                }

                startTime = DateTime.Now;

                // Now we have all of our triangles assigned to the correct mesh, we can make all of the meshes.
                // Each sector will have its own mesh.
                for (int meshSectorsIndex = 0; meshSectorsIndex < meshSectors.Values.Count;meshSectorsIndex++)
                {
                    // Make a object to contain the mesh, mesh renderer, etc or reuse one from before.
                    // It shouldn't matter if we switch which one of these has which mesh from call to call.
                    // (Actually there is potential that a sector won't render for a few frames, but this should
                    // be rare).
                    if (SurfaceObjects.Count <= meshSectorsIndex)
                    {
                        AddSurfaceObject(null, string.Format("SurfaceUnderstanding Mesh-{0}", meshSectorsIndex), transform);
                    }

                    // Get the next MeshData.
                    MeshData meshData = meshSectors.Values.ElementAt(meshSectorsIndex);
                    
                    // Construct the mesh.
                    meshData.Commit();

                    // Assign the mesh to the surface object.
                    SurfaceObjects[meshSectorsIndex].Filter.sharedMesh = meshData.MeshObject;

                    // Make sure we don't build too many meshes in a single frame.
                    if ((DateTime.Now - startTime).TotalMilliseconds > MaxFrameTime)
                    {
                        yield return null;
                        startTime = DateTime.Now;
                    }
                }

                // The current flow of the code shouldn't allow for there to be more Surfaces than sectors.
                // In the future someone might want to destroy meshSectors where there is no longer any 
                // geometry.
                if (SurfaceObjects.Count > meshSectors.Values.Count)
                {
                    Debug.Log("More surfaces than mesh sectors. This is unexpected");
                }
            }

            // Wait a frame
            yield return null;

            // All done - can free up marshal pinned memory
            dll.UnpinAllObjects();

            // Done
            IsImportActive = false;

            // Mark the timestamp
            timeLastImportedMesh = DateTime.Now;
        }

        /// <summary>
        /// Basically floors the Vector so we can use it to subdivide our spatial understanding mesh into parts based
        /// on their position in the world.
        /// </summary>
        /// <param name="vector">The vector to floor.</param>
        /// <returns>A floored vector</returns>
        private Vector3 VectorToSector(Vector3 vector)
        {
            return new Vector3(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z));
        }

        /// <summary>
        /// Updates the mesh import process. This function will kick off the import 
        /// coroutine at the requested internal.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void Update_MeshImport(float deltaTime)
        {
            // Only update every so often
            if (IsImportActive || (ImportMeshPeriod <= 0.0f) ||
                ((DateTime.Now - timeLastImportedMesh).TotalSeconds < ImportMeshPeriod) ||
                (spatialUnderstanding.ScanState != SpatialUnderstanding.ScanStates.Scanning))
            {
                return;
            }

            StartCoroutine(Import_UnderstandingMesh());
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }

}