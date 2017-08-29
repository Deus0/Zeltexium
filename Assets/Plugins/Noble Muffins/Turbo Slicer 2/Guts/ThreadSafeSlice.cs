using UnityEngine;
using System.Collections.Generic;
using System;

namespace NobleMuffins.TurboSlicer.Guts
{
	public static class ThreadSafeSlice
	{
		enum Shape
		{
			None,
			Triangle = 3,
			Quad = 4

		}

		// This is a little different as indices are -not- retained. This is how much we need to allocate for each resultant mesh,
		//compared to the original. I have set it assume that resultant meshes may be up to 90% the complexity of originals because
		//a highly uneven slice (a common occurrence) will result in this.
		const float factorOfSafetyIndices = 0.9f;

		private static readonly CollectionPool<ArrayBuilder<int>,int> intBuilderPool = new CollectionPool<ArrayBuilder<int>,int> (32, (instance) => instance.Capacity,  (cap) => new ArrayBuilder<int> (cap));
		private static readonly CollectionPool<ArrayBuilder<SplitAction>,SplitAction> splitActionBuilderPool = new CollectionPool<ArrayBuilder<SplitAction>,SplitAction> (32, (instance) => instance.Capacity, (cap) => new ArrayBuilder<SplitAction> (cap));

		private static readonly ArrayPool<SplitAction> splitActionArrayPool = new ArrayPool<SplitAction> (32);
		private static readonly ArrayPool<int> intArrayPool = new ArrayPool<int> (32);
		private static readonly ArrayPool<Shape> shapeArrayPool = new ArrayPool<Shape> (32);
		private static readonly ArrayPool<Vector3> vectorThreePool = new ArrayPool<Vector3> (32);
		private static readonly ArrayPool<float> floatArrayPool = new ArrayPool<float> (32);
		private static readonly ArrayPool<PlaneTriResult> sidePlaneArrayPool = new ArrayPool<PlaneTriResult> (32);
		private static readonly ArrayPool<Vector2> vectorTwoPool = new ArrayPool<Vector2> (32);
		private static readonly ArrayPool<Vector4> vectorFourPool = new ArrayPool<Vector4> (32);
		private static readonly ArrayPool<bool> boolPool = new ArrayPool<bool> (32);

		public static void Slice (object _jobState)
		{
			try {
				var jobState = (JobState)_jobState;
				Slice (jobState);
			} catch (System.InvalidCastException) {
				Debug.LogFormat ("ThreadSafeSlice called with wrong kind of state object: {0}", _jobState);
			} catch (System.Exception ex) {
				Debug.LogException (ex);
			}
		}

		public static void Slice (JobState jobState)
		{			
			try {
				#if NOBLEMUFFINS
				var stopwatch = new System.Diagnostics.Stopwatch ();
				stopwatch.Start ();
				#endif
					
				var jobSpec = jobState.Specification;

				var disposables = new List<IDisposable> ();

				var alfaBuilder = new List<MeshSnapshot>();
				var bravoBuilder = new List<MeshSnapshot>();

				foreach(var _readOnlyMesh in jobSpec.Data) {
					var readOnlyMesh = _readOnlyMesh;
					try {
						var submeshCount = readOnlyMesh.indices.Length;
						var sourceVertexCount = readOnlyMesh.vertices.Length;

						Vector4 planeInLocalSpace;
						{
							var plane = jobSpec.PlaneInLocalSpace; // A shorter name
							var commonFactor = -plane.w / (plane.x * plane.x + plane.y * plane.y + plane.z * plane.z);
							var point = new Vector3(plane.x * commonFactor, plane.y * commonFactor, plane.z * commonFactor);
							var normal = new Vector3(-plane.x, -plane.y, -plane.z);
							normal = readOnlyMesh.rootToLocalTransformation.MultiplyVector(normal).normalized;
							point = readOnlyMesh.rootToLocalTransformation.MultiplyPoint(point);
							planeInLocalSpace = Helpers.PlaneFromPointAndNormal(point, normal);
						}

						var alfaIndicesBySubmesh = new ArrayBuilder<int>[ submeshCount ];
						var bravoIndicesBySubmesh = new ArrayBuilder<int>[ submeshCount ];

						PlaneTriResult[] sidePlanes;
						disposables.Add (sidePlaneArrayPool.Get (sourceVertexCount, false, out sidePlanes));

						for (int i = 0; i < sourceVertexCount; i++) {
							var p = readOnlyMesh.vertices [i];
							sidePlanes [i] = p.x * planeInLocalSpace.x + p.y * planeInLocalSpace.y + p.z * planeInLocalSpace.z + planeInLocalSpace.w > 0f ? PlaneTriResult.PTR_FRONT : PlaneTriResult.PTR_BACK;
						}

						var indicesBuffer = new int[3];

						for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++) {	
							int initialCapacityIndices = Mathf.RoundToInt ((float)readOnlyMesh.indices [submeshIndex].Length * factorOfSafetyIndices);

							var alfaIndicesBuilderBundle = ThreadSafeSlice.intBuilderPool.Get (initialCapacityIndices);
							disposables.Add (alfaIndicesBuilderBundle);

							var bravoIndicesBuilderBundle = ThreadSafeSlice.intBuilderPool.Get (initialCapacityIndices);
							disposables.Add (bravoIndicesBuilderBundle);

							var pendingSplitsBuilderBundle = ThreadSafeSlice.intBuilderPool.Get (initialCapacityIndices);
							disposables.Add (pendingSplitsBuilderBundle);

							var alfaIndicesBuilder = alfaIndicesBySubmesh [submeshIndex] = alfaIndicesBuilderBundle.Object;
							var bravoIndicesBuilder = bravoIndicesBySubmesh [submeshIndex] = bravoIndicesBuilderBundle.Object;

							int[] rawSourceIndices = readOnlyMesh.indices [submeshIndex];

							var pendingSplits = pendingSplitsBuilderBundle.Object;

							for (int i = 0; i < rawSourceIndices.Length;) {	
								indicesBuffer [0] = rawSourceIndices [i++];
								indicesBuffer [1] = rawSourceIndices [i++];
								indicesBuffer [2] = rawSourceIndices [i++];

								// compute the side of the plane each vertex is on
								PlaneTriResult r1 = sidePlanes [indicesBuffer [0]];
								PlaneTriResult r2 = sidePlanes [indicesBuffer [1]];
								PlaneTriResult r3 = sidePlanes [indicesBuffer [2]];

								if (r1 == r2 && r1 == r3) { // if all three vertices are on the same side of the plane.
									if (r1 == PlaneTriResult.PTR_FRONT) { // if all three are in front of the plane, then copy to the 'front' output triangle.
										alfaIndicesBuilder.AddArray (indicesBuffer);
									} else {
										bravoIndicesBuilder.AddArray (indicesBuffer);
									}
								} else {
									pendingSplits.AddArray (indicesBuffer);
								}
							}

							if (pendingSplits.length > 0) {						
								var doInfill = readOnlyMesh.infillBySubmesh != null;

								var doNormals = jobSpec.ChannelNormals && readOnlyMesh.normals.Length > 0;
								var doTangents = jobSpec.ChannelTangents && readOnlyMesh.tangents.Length > 0 && doNormals;
								var doUV2 = jobSpec.ChannelUV2 && readOnlyMesh.coords2.Length > 0;

								var sourceGeometry = readOnlyMesh.vertices;
								var sourceNormals = readOnlyMesh.normals;
								var sourceCoords = readOnlyMesh.coords;
								var sourceCoords2 = readOnlyMesh.coords2;

								//Now we're going to do the decision making pass. This is where we assess the side figures and produce actions...
								int inputTriangleCount = pendingSplits.length / 3;

								//A good action count estimate can avoid reallocations.
								//We expect exactly five actions per triangle.
								int estimatedSplitActionCount = inputTriangleCount * 5;

								ArrayBuilder<SplitAction> splitActionsBuilder;
								disposables.Add (splitActionBuilderPool.Get (estimatedSplitActionCount, out splitActionsBuilder));

								Shape[] alfaShapes, bravoShapes;

								disposables.Add (shapeArrayPool.Get (inputTriangleCount, false, out alfaShapes));
								disposables.Add (shapeArrayPool.Get (inputTriangleCount, false, out bravoShapes));

								using (var _pointClassifications = floatArrayPool.Get (pendingSplits.length, false)) {
									var pointClassifications = _pointClassifications.Object;

									for (int i = 0; i < pendingSplits.length; i++) {
										var p = sourceGeometry [pendingSplits.array [i]];
										pointClassifications [i] = p.x * planeInLocalSpace.x + p.y * planeInLocalSpace.y + p.z * planeInLocalSpace.z + planeInLocalSpace.w;
									}

									var sides = new float[3];

									for (int i = 0; i < pendingSplits.length; i += 3) {
										indicesBuffer [0] = pendingSplits.array [i];
										indicesBuffer [1] = pendingSplits.array [i + 1];
										indicesBuffer [2] = pendingSplits.array [i + 2];

										sides [0] = pointClassifications [i];
										sides [1] = pointClassifications [i + 1];
										sides [2] = pointClassifications [i + 2];

										int indexA = 2;

										int alfaVertexCount = 0, bravoVertexCount = 0;

										for (int indexB = 0; indexB < 3; indexB++) {
											float sideA = sides [indexA];
											float sideB = sides [indexB];

											if (sideB > 0f) {
												if (sideA < 0f) {
													//Find intersection between A, B. Add to BOTH
													splitActionsBuilder.Add (new SplitAction (indicesBuffer [indexA], indicesBuffer [indexB]));
													alfaVertexCount++;
													bravoVertexCount++;
												}
												//Add B to FRONT.
												splitActionsBuilder.Add (new SplitAction (true, false, indicesBuffer [indexB]));
												alfaVertexCount++;
											} else if (sideB < 0f) {
												if (sideA > 0f) {
													//Find intersection between A, B. Add to BOTH
													splitActionsBuilder.Add (new SplitAction (indicesBuffer [indexA], indicesBuffer [indexB]));
													alfaVertexCount++;
													bravoVertexCount++;
												}
												//Add B to BACK.
												splitActionsBuilder.Add (new SplitAction (false, true, indicesBuffer [indexB]));
												bravoVertexCount++;
											} else {
												//Add B to BOTH.
												splitActionsBuilder.Add (new SplitAction (true, true, indicesBuffer [indexB]));
												alfaVertexCount++;
												bravoVertexCount++;
											}

											indexA = indexB;
										}

										int j = i / 3; //This is the triangle counter.

										alfaShapes [j] = (Shape)alfaVertexCount;
										bravoShapes [j] = (Shape)bravoVertexCount;
									}
								}

								// We're going to iterate through the splits only several times, so let's
								//find the subset once now.
								// Since these are STRUCTs, this is going to COPY the array content. The
								//intersectionInverseRelation table made below helps us put it back into the
								//main array before we use it.
								var intersectionCount = 0;
								SplitAction[] intersectionActions;
								int[] intersectionInverseRelations;

								for (int i = 0; i < splitActionsBuilder.length; i++) {
									var sa = splitActionsBuilder.array [i];
									if ((sa.flags & SplitAction.INTERSECT) == SplitAction.INTERSECT)
										intersectionCount++;
								}

								disposables.Add (splitActionArrayPool.Get (intersectionCount, false, out intersectionActions));
								disposables.Add (intArrayPool.Get (intersectionCount, false, out intersectionInverseRelations));

								{
									int j = 0;
									for (int i = 0; i < splitActionsBuilder.length; i++) {
										SplitAction sa = splitActionsBuilder.array [i];
										if ((sa.flags & SplitAction.INTERSECT) == SplitAction.INTERSECT) {
											intersectionActions [j] = sa;
											intersectionInverseRelations [j] = i;
											j++;
										}
									}
								}

								// Next, we're going to find out which splitActions replicate the work of other split actions.
								//A given SA replicates another if and only if it _both_ calls for an intersection _and_ has
								//the same two parent indices (index0 and index1). This is because all intersections are called
								//with the same other parameters, so any case with an index0 and index1 matching will yield the
								//same results.
								// Only caveat is that two given splitActions might have the source indices in reverse order, so
								//we'll arbitrarily decide that "greater first" or something is the correct order. Flipping this
								//order has no consequence until after the intersection is found (at which point flipping the order
								//necessitates converting intersection i to 1-i to flip it as well.)
								// We can assume that every SA has at most 1 correlation. For a given SA, we'll search the list
								//UP TO its own index and, if we find one, we'll take the other's index and put it into the CLONE OF
								//slot.
								// So if we had a set like AFDBAK, than when the _latter_ A comes up for assessment, it'll find
								//the _first_ A (with an index of 0) and set the latter A's cloneOf figure to 0. This way we know
								//any latter As are a clone of the first A.

								for (int i = 0; i < intersectionCount; i++) {
									SplitAction a = intersectionActions [i];

									//Ensure that the index0, index1 figures are all in the same order.
									//(We'll do this as we walk the list.)
									if (a.index0 > a.index1) {
										int j = a.index0;
										a.index0 = a.index1;
										a.index1 = j;
									}

									//Only latters clone formers, so we don't need to search up to and past the self.
									for (int j = 0; j < i; j++) {
										SplitAction b = intersectionActions [j];

										bool match = a.index0 == b.index0 && a.index1 == b.index1;

										if (match) {
											a.cloneOf = j;
										}
									}

									intersectionActions [i] = a;
								}

								//Next, we want to perform all INTERSECTIONS. Any action which has an intersection needs to have that, like, done.

								for (int i = 0; i < intersectionCount; i++) {
									SplitAction sa = intersectionActions [i];

									if (sa.cloneOf == SplitAction.nullIndex) {
										var p1 = sourceGeometry [sa.index1];
										var p2 = sourceGeometry [sa.index0];

										var distanceToPoint = p1.x * planeInLocalSpace.x + p1.y * planeInLocalSpace.y + p1.z * planeInLocalSpace.z + planeInLocalSpace.w;

										var dir = p2 - p1;

										var dot1 = dir.x * planeInLocalSpace.x + dir.y * planeInLocalSpace.y + dir.z * planeInLocalSpace.z;
										var dot2 = distanceToPoint - planeInLocalSpace.w;

										sa.intersectionResult = -(planeInLocalSpace.w + dot2) / dot1;

										intersectionActions [i] = sa;
									}

								}

								int newIndexStartsAt = readOnlyMesh.vertices.Length;

								// Let's create a table that relates an INTERSECTION index to a GEOMETRY index with an offset of 0 (for example
								//to refer to our newVertices or to the transformedVertices or whatever; internal use.)
								// We can also set up our realIndex figures in the same go.
								int uniqueVertexCount = 0;
								int[] localIndexByIntersection;
								disposables.Add (intArrayPool.Get (intersectionCount, false, out localIndexByIntersection));
								{
									int currentLocalIndex = 0;
									for (int i = 0; i < intersectionCount; i++) {
										SplitAction sa = intersectionActions [i];

										int j;

										if (sa.cloneOf == SplitAction.nullIndex) {
											j = currentLocalIndex++;
										} else {
											//This assumes that the widget that we are a clone of already has its localIndexByIntersection assigned.
											//We assume this because above – where we seek for clones – we only look behind for cloned elements.
											j = localIndexByIntersection [sa.cloneOf];
										}

										sa.realIndex = newIndexStartsAt + j;

										localIndexByIntersection [i] = j;

										intersectionActions [i] = sa;
									}
									uniqueVertexCount = currentLocalIndex;
								}

								//Let's figure out how much geometry we might have.
								//The infill geometry is a pair of clones of this geometry, but with different NORMALS and UVs. (Each set has different normals.)

								var numberOfVerticesAdded = uniqueVertexCount * (doInfill ? 3 : 1);

								//In this ACTION pass we'll act upon intersections by fetching both referred vertices and LERPing as appropriate.
								//The resultant indices will be written out over the index0 figures.

								//LERP to create vertices
								Vector3[] newVertices, newNormals;
								Vector2[] newUVs, newUV2s;
								Vector4[] newTangents;

								disposables.Add (vectorThreePool.Get (numberOfVerticesAdded, false, out newVertices));
								disposables.Add (vectorTwoPool.Get (numberOfVerticesAdded, false, out newUVs));

								if (doTangents) {
									disposables.Add (vectorFourPool.Get (numberOfVerticesAdded, false, out newTangents));
								} else {
									newTangents = new Vector4[0];
								}
								
								if (doNormals) {
									disposables.Add (vectorThreePool.Get (numberOfVerticesAdded, false, out newNormals));
								} else {
									newNormals = new Vector3[0];
								}

								if (doUV2) {
									disposables.Add (vectorTwoPool.Get (numberOfVerticesAdded, false, out newUV2s));
								} else {
									newUV2s = new Vector2[0];
								}

								{
									int currentNewIndex = 0;
									for (int i = 0; i < intersectionCount; i++) {
										var sa = intersectionActions [i];
										if (sa.cloneOf == SplitAction.nullIndex) {
											var v = sourceGeometry [sa.index0];
											var v2 = sourceGeometry [sa.index1];
											newVertices [currentNewIndex] = Vector3.Lerp (v2, v, sa.intersectionResult);

											var uv = sourceCoords [sa.index0];
											var uv2 = sourceCoords [sa.index1];
											newUVs [currentNewIndex] = Vector2.Lerp (uv2, uv, sa.intersectionResult);

											if (doNormals) {
												var n = sourceNormals [sa.index0];
												var n2 = sourceNormals [sa.index1];
												newNormals [currentNewIndex] = Vector3.Lerp (n2, n, sa.intersectionResult);
											}

											if (doUV2) {
												var uvB = sourceCoords2 [sa.index0];
												var uvB2 = sourceCoords2 [sa.index1];
												newUV2s [currentNewIndex] = Vector2.Lerp (uvB2, uvB, sa.intersectionResult);
											}

											currentNewIndex++;
										}
									}

									Debug.Assert (currentNewIndex == uniqueVertexCount);
								}


								//All the polygon triangulation algorithms depend on having a 2D polygon. We also need the slice plane's
								//geometry in two-space to map the UVs.

								//NOTE that as we only need this data to analyze polygon geometry for triangulation, we can TRANSFORM (scale, translate, rotate)
								//these figures any way we like, as long as they retain the same relative geometry. So we're going to perform ops on this
								//data to create the UVs by scaling it around, and we'll feed the same data to the triangulator.

								//Our's exists in three-space, but is essentially flat... So we can transform it onto a flat coordinate system.
								//The first three figures of our plane four-vector describe the normal to the plane, so if we can create
								//a transformation matrix from that normal to the up normal, we can transform the vertices for observation.
								//We don't need to transform them back; we simply refer to the original vertex coordinates by their index,
								//which (as this is an ordered set) will match the indices of coorisponding transformed vertices.

								//This vector-vector transformation comes from Benjamin Zhu at SGI, pulled from a 1992
								//forum posting here: http://steve.hollasch.net/cgindex/math/rotvecs.html

								/*	"A somewhat "nasty" way to solve this problem:

									Let V1 = [ x1, y1, z1 ], V2 = [ x2, y2, z2 ]. Assume V1 and V2 are already normalized.
									
									    V3 = normalize(cross(V1, V2)). (the normalization here is mandatory.)
									    V4 = cross(V3, V1).
									             
									         [ V1 ]
									    M1 = [ V4 ]
									         [ V3 ]
									
									    cos = dot(V2, V1), sin = dot(V2, V4)
									            
									         [ cos   sin    0 ]
									    M2 = [ -sin  cos    0 ]
									         [ 0     0      1 ]
									         
									The sought transformation matrix is just M1^-1 * M2 * M1. This might well be a standard-text solution."
									
									-Ben Zhu, SGI, 1992
								 */

								Vector2[] transformedVertices;
								int infillFrontOffset = 0, infillBackOffset = 0;

								if (doInfill) {
									disposables.Add (vectorTwoPool.Get (uniqueVertexCount, false, out transformedVertices));

									// Based on the algorithm described above, this will create a matrix permitting us
									//to multiply a given vertex yielding a vertex transformed to an XY plane (where Z is
									//undefined.)
									// This algorithm cannot work if we're already in that plane. We know if we're already
									//in that plane if X and Y are both zero and Z is nonzero.
									bool canUseSimplifiedTransform = Mathf.Approximately (planeInLocalSpace.x, 0f) && Mathf.Approximately (planeInLocalSpace.y, 0f);
									if (!canUseSimplifiedTransform) {
										Matrix4x4 flattenTransform;

										Vector3 v1 = Vector3.forward;
										Vector3 v2 = new Vector3 (planeInLocalSpace.x, planeInLocalSpace.y, planeInLocalSpace.z).normalized;
										Vector3 v3 = Vector3.Cross (v1, v2).normalized;
										Vector3 v4 = Vector3.Cross (v3, v1);

										float cos = Vector3.Dot (v2, v1);
										float sin = Vector3.Dot (v2, v4);

										Matrix4x4 m1 = Matrix4x4.identity;
										m1.SetRow (0, (Vector4)v1);
										m1.SetRow (1, (Vector4)v4);
										m1.SetRow (2, (Vector4)v3);

										Matrix4x4 m1i = m1.inverse;

										Matrix4x4 m2 = Matrix4x4.identity;
										m2.SetRow (0, new Vector4 (cos, sin, 0, 0));
										m2.SetRow (1, new Vector4 (-sin, cos, 0, 0));

										flattenTransform = m1i * m2 * m1;

										for (int i = 0; i < uniqueVertexCount; i++) {
											transformedVertices [i] = (Vector2)flattenTransform.MultiplyPoint3x4 (newVertices [i]);
										}
									} else {
										var sign = Mathf.Sign (planeInLocalSpace.z);
										for (int i = 0; i < uniqueVertexCount; i++) {
											transformedVertices [i] = new Vector2 (newVertices [i].x, sign * newVertices [i].y);
										}
									}

									// We want to normalize the entire transformed vertices. To do this, we find the largest
									//floats in either (by abs). Then we scale. Of course, this normalizes us to figures
									//in the range of [-1f,1f] (not necessarily extending all the way on both sides), and
									//what we need are figures between 0f and 1f (not necessarily filling, but necessarily
									//not spilling.) So we'll shift it here.

//									if(false) //Uncomment this and the else block to activate stretch texturing on the infill
//									{
										float x = 0f, y = 0f;

										for (int i = 0; i < uniqueVertexCount; i++) {
											Vector2 v = transformedVertices [i];

											v.x = Mathf.Abs (v.x);
											v.y = Mathf.Abs (v.y);

											if (v.x > x)
												x = v.x;
											if (v.y > y)
												y = v.y;
										}

										//We would use 1f/x, 1f/y but we also want to scale everything to half (and perform an offset) as
										//described above.
										x = 0.5f / x;
										y = 0.5f / y;

										var r = readOnlyMesh.infillBySubmesh [submeshIndex];

										for (int i = 0; i < uniqueVertexCount; i++) {
											Vector2 v = transformedVertices [i];
											v.x *= x;
											v.y *= y;
											v.x += 0.5f;
											v.y += 0.5f;
											v.x *= r.width;
											v.y *= r.height;
											v.x += r.x;
											v.y += r.y;
											transformedVertices [i] = v;
										}
//									}
//									else {
//										float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
//
//										for(int i = 0; i < uniqueVertexCount; i++) {
//											Vector2 v = transformedVertices [i];
//											xMin = Mathf.Min(xMin, v.x);
//											yMin = Mathf.Min(yMin, v.y);
//											xMax = Mathf.Max(xMax, v.x);
//											yMax = Mathf.Max(yMax, v.y);
//										}
//
//										float width = xMax - xMin, height = yMax - yMin;
//
//										var r = readOnlyMesh.infillBySubmesh [submeshIndex];
//
//										float xScale = r.width / width, yScale = r.height / height;
//										float xShift = -xMin, yShift = -yMin;
//
//										for(int i = 0; i < uniqueVertexCount; i++) {
//											Vector2 v = transformedVertices [i];
//											v.x = (v.x + xShift) * xScale + r.xMin;
//											v.y = (v.y + yShift) * yScale + r.yMin;
//											transformedVertices[i] = v;
//										}
//									}

									//Now let's build the geometry for the two slice in-fills.
									//One is for the front side, and the other for the back side. Each has differing normals.

									infillFrontOffset = uniqueVertexCount;
									infillBackOffset = uniqueVertexCount * 2;

									//The geometry is identical...

									System.Array.Copy (newVertices, 0, newVertices, infillFrontOffset, uniqueVertexCount);
									System.Array.Copy (newVertices, 0, newVertices, infillBackOffset, uniqueVertexCount);

									System.Array.Copy (transformedVertices, 0, newUVs, infillFrontOffset, uniqueVertexCount);
									System.Array.Copy (transformedVertices, 0, newUVs, infillBackOffset, uniqueVertexCount);

									if (doUV2) {
										for (int i = 0; i < uniqueVertexCount; i++) {
											newUV2s [i + infillFrontOffset] = Vector2.zero;
										}
										for (int i = 0; i < uniqueVertexCount; i++) {
											newUV2s [i + infillBackOffset] = Vector2.zero;
										}
									}

									if (doNormals) {
										Vector3 infillFrontNormal = ((Vector3)planeInLocalSpace) * -1f;
										infillFrontNormal.Normalize ();

										for (int i = infillFrontOffset; i < infillBackOffset; i++)
											newNormals [i] = infillFrontNormal;

										Vector3 infillBackNormal = (Vector3)planeInLocalSpace;
										infillBackNormal.Normalize ();

										for (int i = infillBackOffset; i < numberOfVerticesAdded; i++)
											newNormals [i] = infillBackNormal;
									}
								} else {
									transformedVertices = new Vector2[0];
								}
				
								//Note that here we refer to split actions again, so let's copy back the updated splitActions.
								for (int i = 0; i < intersectionCount; i++) {
									int j = intersectionInverseRelations [i];
									splitActionsBuilder.array [j] = intersectionActions [i];
								}

								DirectTransferStage (splitActionsBuilder, alfaShapes, inputTriangleCount, SplitAction.TO_ALFA, alfaIndicesBuilder);
								DirectTransferStage (splitActionsBuilder, bravoShapes, inputTriangleCount, SplitAction.TO_BRAVO, bravoIndicesBuilder);

								//Let's add this shiznit in!

								readOnlyMesh = new MeshSnapshot (
									readOnlyMesh.key,
									Combine (readOnlyMesh.vertices, newVertices, numberOfVerticesAdded),
									Combine (readOnlyMesh.normals, newNormals, numberOfVerticesAdded),
									Combine (readOnlyMesh.coords, newUVs, numberOfVerticesAdded),
									Combine (readOnlyMesh.coords2, newUV2s, numberOfVerticesAdded),
									Combine (readOnlyMesh.tangents, newTangents, numberOfVerticesAdded),
									readOnlyMesh.indices,
									readOnlyMesh.infillBySubmesh, readOnlyMesh.rootToLocalTransformation
								);

								//Now we need to fill in the slice hole.

								//We need to find the POLYGON[s] representing the slice hole[s]. There may be more than one. 
								//Then we need to TRIANGULATE these polygons and write them out.

								//Above we've built the data necessary to pull this off. We have:

								// - Geometry for the polygon around the edges in Vertex3 / Normal / UV format, already added
								//to the geometry setup.
								// - Geometry for the polygon in Vertex2 format in matching order, aligned to the slice plane.
								// - A collection of all data points and 1:1 hashes representing their physical location.

								//In this mess of data here may be 0 or non-zero CLOSED POLYGONS. We need to walk the list and
								//identify each CLOSED POLYGON (there may be none, or multiples). Then, each of these must be
								//triangulated separately.

								//Vertices connected to each other in a closed polygon can be found to associate with each other
								//in two ways. Envision a triangle strip that forms a circular ribbon – and that we slice through
								//the middle of this ribbon. Slice vertices come in two kinds of pairs; there are pairs that COME FROM
								//the SAME triangle, and pairs that come from ADJACENT TRIANGLES. The whole chain is formed from
								//alternating pair-types.

								//So for example vertex A comes from the same triangle as vertex B, which in turn matches the position
								//of the NEXT triangle's vertex A.

								//The data is prepared for us to be able to identify both kinds of associations. First,
								//association by parent triangle is encoded in the ORDERING. Every PAIR from index 0 shares a parent
								//triangle; so indices 0-1, 2-3, 4-5 and so on are each a pair from a common parent triangle.

								//Meanwhile, vertices generated from the common edge of two different triangles will have the SAME
								//POSITION in three-space.

								//We don't have to compare Vector3s, however; this has already been done. Uniques were eliminated above.
								//What we have is a table; localIndexByIntersection. This list describes ALL SLICE VERTICES in terms
								//of which VERTEX (in the array – identified by index) represents that slice vertex. So if we see that
								//localIndexByIntersection[0] == localIndexByIntersection[4], than we know that slice vertices 0 and 4
								//share the same position in three space.

								//With that in mind, we're going to go through the list in circles building chains out of these
								//connections.

								if (doInfill) {

									ArrayBuilder<int> polygonBuilder = null;
									var allPolys = new List<ArrayBuilder<int>> ();

									using (var _availabilityBuffer = boolPool.Get (intersectionCount, false))
									using (var _sqrMags = floatArrayPool.Get (uniqueVertexCount, false)) {

										var availabilityBuffer = _availabilityBuffer.Object;
										for (int i = 0; i < intersectionCount; i++) {
											availabilityBuffer [i] = true;
										}

										var sqrMags = _sqrMags.Object;

										for (int i = 0; i < uniqueVertexCount; i++) {
											sqrMags [i] = newVertices [i].sqrMagnitude;
										}

										var availabilityCount = intersectionCount;

										int? seekingFor = null;

										while (seekingFor.HasValue || availabilityCount > 0) {
											const int NotFound = -1;
											var recountAvailability = false;

											if (seekingFor.HasValue) {
												var seekingForLocalIndex = localIndexByIntersection [seekingFor.Value];

												//It would seem we could use the 2D transformed vertices but I want the
												//least manipulated figures.
												var seekingForValue = newVertices [seekingForLocalIndex];
												var seekingForValueX = (double)seekingForValue.x;
												var seekingForValueY = (double)seekingForValue.y;
												var seekingForValueZ = (double)seekingForValue.z;

												var loopStartIndex = polygonBuilder.array [0];

												var bestMatchIndex = NotFound;
												var bestMatchDelta = double.MaxValue;

												for (int i = 0; i < intersectionCount; i++) {
													var isAvailable = i == loopStartIndex || availabilityBuffer [i];
													if (isAvailable) {
														var candidateLocalIndex = localIndexByIntersection [i];

														//The quickest way to show they match is if they have matching vertex index.
														if (candidateLocalIndex == seekingForLocalIndex) {
															bestMatchIndex = i;
															bestMatchDelta = 0.0;
														} else {
															//Otherwise, let's just check if it's closer than the current best candidate.

															var candidateValue = newVertices [candidateLocalIndex];

															var candidateX = (double)candidateValue.x;
															var candidateY = (double)candidateValue.y;
															var candidateZ = (double)candidateValue.z;
															var dx = seekingForValueX - candidateX;
															var dy = seekingForValueY - candidateY;
															var dz = seekingForValueZ - candidateZ;
															var candidateDelta = dx * dx + dy * dy + dz * dz;
															if (candidateDelta < bestMatchDelta) {
																bestMatchIndex = i;
																bestMatchDelta = candidateDelta;
															}
														}
													}
												}

												if (bestMatchIndex == NotFound) {
													//Fail; drop the current polygon.
													seekingFor = null;
													polygonBuilder = null;
												} else if (bestMatchIndex == loopStartIndex) {
													//Loop complete; consume the current polygon.
													seekingFor = null;

													if (polygonBuilder.Count >= 3) {
														allPolys.Add (polygonBuilder);
													}
												} else {
													int partnerByParent = bestMatchIndex % 2 == 1 ? bestMatchIndex - 1 : bestMatchIndex + 1;
													availabilityBuffer [bestMatchIndex] = false;
													availabilityBuffer [partnerByParent] = false;
													recountAvailability = true;
													seekingFor = partnerByParent;

													bool isDegenerate;

													//Before we add this to the polygon let's check if it's the same as the last one.

													var bestMatchLocalIndex = localIndexByIntersection [bestMatchIndex];
													var lastAddedLocalIndex = localIndexByIntersection [polygonBuilder.array [polygonBuilder.Count - 1]];

													//The cheapest way to spot a match is if they have the same index in the vertex array.
													if (bestMatchLocalIndex != lastAddedLocalIndex) {
														const float ePositive = 1.0f / 65536.0f;
														const float eMinus = -1.0f / 65536.0f;
														
														var sqrMagDelta = sqrMags [bestMatchLocalIndex] - sqrMags [lastAddedLocalIndex];

														//The cheapest way to show they _don't_ match is to see if their square magnitudes differ.
														if (sqrMagDelta < ePositive && sqrMagDelta > eMinus) {

															//If all else fails, check the different on a dimension by dimension basis.
															//Note that we can use the two dimensional "transformed vertices" set to do this
															//since we only care about degerency with respect to the infill, which is done in
															//two dimensional space.

															var alfa = transformedVertices [bestMatchLocalIndex];
															var bravo = transformedVertices [lastAddedLocalIndex];
															var delta = alfa - bravo;

															isDegenerate = delta.x > eMinus && delta.y > eMinus && delta.x < ePositive && delta.y < ePositive;
														} else {
															isDegenerate = false;
														}
													} else {
														isDegenerate = true;
													}

													if (!isDegenerate) {
														polygonBuilder.Add (bestMatchIndex);
													}
												}
											} else {

												//Here we're going to try to start a loop. Find any unclaimed index and go from there.

												var loopStartIndex = NotFound;

												for (int i = 0; i < intersectionCount; i++) {
													if (availabilityBuffer [i]) {
														loopStartIndex = i;
														break;
													}
												}

												if (loopStartIndex != NotFound) {
													int partnerByParent = loopStartIndex % 2 == 1 ? loopStartIndex - 1 : loopStartIndex + 1;
													availabilityBuffer [loopStartIndex] = false;
													availabilityBuffer [partnerByParent] = false;
													recountAvailability = true;
													seekingFor = partnerByParent;
													disposables.Add (intBuilderPool.Get (availabilityCount, out polygonBuilder));
													polygonBuilder.Add (loopStartIndex);
												}
											}

											if (recountAvailability) {
												availabilityCount = 0;
												for (int i = 0; i < intersectionCount; i++) {
													if (availabilityBuffer [i]) {
														availabilityCount++;
													}
												}
											}
										}
									}

									for (int polyIndex = 0; polyIndex < allPolys.Count; polyIndex++) {
										var intersectionIndicesForThisPolygon = allPolys [polyIndex];

										var triangleBufferSize = Triangulation.GetArraySize (intersectionIndicesForThisPolygon.Count);

										using (var _geometryForThisPolygon = vectorTwoPool.Get (intersectionIndicesForThisPolygon.Count, false))
										using (var _triangleBuffer = intArrayPool.Get (triangleBufferSize, true)) {
											var geometryForThisPolygon = _geometryForThisPolygon.Object;

											for (int i = 0; i < intersectionIndicesForThisPolygon.Count; i++) {
												int j = localIndexByIntersection [intersectionIndicesForThisPolygon.array [i]];
												geometryForThisPolygon [i] = transformedVertices [j];
											}

											var triangleBuffer = _triangleBuffer.Object;

											if (Triangulation.Triangulate (geometryForThisPolygon, intersectionIndicesForThisPolygon.Count, triangleBuffer)) {
												using (var _alfa = intArrayPool.Get (triangleBufferSize, false))
												using (var _bravo = intArrayPool.Get (triangleBufferSize, false)) {
												
													var alfa = _alfa.Object;
													var bravo = _bravo.Object;

													for (int i = 0; i < triangleBufferSize; i++) {
														int intersection = intersectionIndicesForThisPolygon.array [triangleBuffer [i]];
														int local = localIndexByIntersection [intersection];
														alfa [i] = local + infillFrontOffset + newIndexStartsAt;
														bravo [i] = local + infillBackOffset + newIndexStartsAt;
													}

													//Invert the winding on the alfa side
													for (int i = 0; i < triangleBufferSize; i += 3) {
														int j = alfa [i];
														alfa [i] = alfa [i + 2];
														alfa [i + 2] = j;
													}

													alfaIndicesBuilder.AddArray (alfa, triangleBufferSize);
													bravoIndicesBuilder.AddArray (bravo, triangleBufferSize);
												}
											}
										}
									}
								}
							}
						}

						var alfaIndices = new int[submeshCount][];
						var bravoIndices = new int[submeshCount][];

						for (int i = 0; i < submeshCount; i++) {
							alfaIndices [i] = alfaIndicesBySubmesh [i].ToArray ();
							bravoIndices [i] = bravoIndicesBySubmesh [i].ToArray ();
						}

						var alfaMesh = readOnlyMesh.WithIndices (alfaIndices);
						var bravoMesh = readOnlyMesh.WithIndices (bravoIndices);

						if (jobSpec.ChannelTangents) {
							alfaMesh = alfaMesh.WithSomeRecalculatedTangents (sourceVertexCount);
							bravoMesh = bravoMesh.WithSomeRecalculatedTangents (sourceVertexCount);
						}

						alfaMesh = alfaMesh.Strip ();
						bravoMesh = bravoMesh.Strip ();

						alfaBuilder.Add(alfaMesh);
						bravoBuilder.Add(bravoMesh);
					}
					finally {
						for (int i = 0; i < disposables.Count; i++) {
							disposables [i].Dispose ();
						}
						disposables.Clear();
					}
				}

				jobState.Yield = new JobYield (jobSpec, alfaBuilder, bravoBuilder);

				#if NOBLEMUFFINS
				stopwatch.Stop ();
				Debug.LogFormat ("Slice completed in {0} ms", stopwatch.ElapsedMilliseconds.ToString ());
				#endif
			} catch (System.Exception ex) {
				jobState.Exception = ex;
			}
		}

		private static void DirectTransferStage (ArrayBuilder<SplitAction> splitActionsBuilder, Shape[] shapes, int inputTriangleCount, int flag, ArrayBuilder<int> indexBuilder)
		{
			using (var _newIndexBuilder = intBuilderPool.Get (0)) {
				var newIndexBuilder = _newIndexBuilder.Object;

				for (int i = 0; i < splitActionsBuilder.length; i++) {
					var sa = splitActionsBuilder.array [i];
					if ((sa.flags & flag) == flag) {
						newIndexBuilder.Add (sa.realIndex);
					}
				}

				//Now we need to triangulate sets of quads.
				//We recorded earlier whether we're looking at triangles or quads – in order. So we have a pattern like TTQTTQQTTTQ, and
				//we can expect these vertices to match up perfectly to what the above section of code dumped out.

				int startIndex = 0;

				int[] _indices3 = new int[3];
				int[] _indices4 = new int[6];

				for (int i = 0; i < inputTriangleCount; i++) {
					var s = shapes [i];
					switch (s) {
					case Shape.Triangle:
						_indices3 [0] = newIndexBuilder.array [startIndex];
						_indices3 [1] = newIndexBuilder.array [startIndex + 1];
						_indices3 [2] = newIndexBuilder.array [startIndex + 2];
						indexBuilder.AddArray (_indices3);
						startIndex += 3;
						break;
					case Shape.Quad:
						_indices4 [0] = newIndexBuilder.array [startIndex];
						_indices4 [1] = newIndexBuilder.array [startIndex + 1];
						_indices4 [2] = newIndexBuilder.array [startIndex + 3];
						_indices4 [3] = newIndexBuilder.array [startIndex + 1];
						_indices4 [4] = newIndexBuilder.array [startIndex + 2];
						_indices4 [5] = newIndexBuilder.array [startIndex + 3];
						indexBuilder.AddArray (_indices4);
						startIndex += 4;
						break;
					default:
						break;
						//Do nothing
//						throw new System.NotImplementedException ();
					}
				}
			}
		}

		private static T[] Combine<T> (T[] alfa, T[] bravo, int bravoCount)
		{
			var addition = System.Math.Min (bravoCount, bravo.Length);
			var count = alfa.Length + addition;
			var charlie = new T[count];
			System.Array.Copy (alfa, charlie, alfa.Length);
			System.Array.Copy (bravo, 0, charlie, alfa.Length, addition);
			return charlie;
		}

		private static MeshSnapshot WithSomeRecalculatedTangents (this MeshSnapshot rom, int forVerticesStartingFromThisIndex)
		{
			//Based on code here:
			//http://www.cs.upc.edu/~virtual/G/1.%20Teoria/06.%20Textures/Tangent%20Space%20Calculation.pdf
			
			var vertices = rom.vertices;
			var normals = rom.normals;
			var coords = rom.coords;

			int vertexCount = vertices.Length;

			var tangents = new Vector4[vertexCount];
			System.Array.Copy (rom.tangents, tangents, forVerticesStartingFromThisIndex);
				
			Vector3[] tan1, tan2;

			using (vectorThreePool.Get (vertexCount, true, out tan1))
			using (vectorThreePool.Get (vertexCount, true, out tan2)) {
				
				for (int i = 0; i < rom.indices.Length; i++) {
					var triangles = rom.indices [i];

					for (int j = 0; j < triangles.Length;) {
						var j1 = triangles [j++];
						var j2 = triangles [j++];
						var j3 = triangles [j++];

						Debug.Assert (j1 != j2 && j1 != j3);

						var isRelevant = j1 >= forVerticesStartingFromThisIndex || j2 >= forVerticesStartingFromThisIndex || j3 >= forVerticesStartingFromThisIndex;

						if (isRelevant) {
							var v1 = vertices [j1];
							var v2 = vertices [j2];
							var v3 = vertices [j3];

							//We have to test for degeneracy.
							var e1 = v1 - v2;
							var e2 = v1 - v3;
							const float epsilon = 1.0f / 65536.0f;
							if (e1.sqrMagnitude > epsilon && e2.sqrMagnitude > epsilon) {
								var w1 = coords [j1];
								var w2 = coords [j2];
								var w3 = coords [j3];

								var x1 = v2.x - v1.x;
								var x2 = v3.x - v1.x;
								var y1 = v2.y - v1.y;
								var y2 = v3.y - v1.y;
								var z1 = v2.z - v1.z;
								var z2 = v3.z - v1.z;

								var s1 = w2.x - w1.x;
								var s2 = w3.x - w1.x;
								var t1 = w2.y - w1.y;
								var t2 = w3.y - w1.y;

								var r = 1.0f / (s1 * t2 - s2 * t1);

								var sX = (t2 * x1 - t1 * x2) * r;
								var sY = (t2 * y1 - t1 * y2) * r;
								var sZ = (t2 * z1 - t1 * z2) * r;

								var tX = (s1 * x2 - s2 * x1) * r;
								var tY = (s1 * y2 - s2 * y1) * r;
								var tZ = (s1 * z2 - s2 * z1) * r;

								var tan1j1 = tan1 [j1];
								var tan1j2 = tan1 [j2];
								var tan1j3 = tan1 [j3];
								var tan2j1 = tan2 [j1];
								var tan2j2 = tan2 [j2];
								var tan2j3 = tan2 [j3];

								tan1j1.x += sX;
								tan1j1.y += sY;
								tan1j1.z += sZ;
								tan1j2.x += sX;
								tan1j2.y += sY;
								tan1j2.z += sZ;
								tan1j3.x += sX;
								tan1j3.y += sY;
								tan1j3.z += sZ;

								tan2j1.x += tX;
								tan2j1.y += tY;
								tan2j1.z += tZ;
								tan2j2.x += tX;
								tan2j2.y += tY;
								tan2j2.z += tZ;
								tan2j3.x += tX;
								tan2j3.y += tY;
								tan2j3.z += tZ;

								tan1 [j1] = tan1j1;
								tan1 [j2] = tan1j2;
								tan1 [j3] = tan1j3;
								tan2 [j1] = tan2j1;
								tan2 [j2] = tan2j2;
								tan2 [j3] = tan2j3;
							}
						}
					}
				}

				for (int i = forVerticesStartingFromThisIndex; i < vertexCount; i++) {
					var n = normals [i];
					var t = tan1 [i];

					// Gram-Schmidt orthogonalize
					Vector3.OrthoNormalize (ref n, ref t);

					tangents [i].x = t.x;
					tangents [i].y = t.y;
					tangents [i].z = t.z;

					// Calculate handedness
					tangents [i].w = (Vector3.Dot (Vector3.Cross (n, t), tan2 [i]) < 0.0f) ? -1.0f : 1.0f;
				}

				return rom.WithTangents (tangents);
			}
		}

	}
}
