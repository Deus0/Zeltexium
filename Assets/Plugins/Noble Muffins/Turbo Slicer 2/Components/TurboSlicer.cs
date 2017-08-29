using UnityEngine;
using System.Collections.Generic;
using NobleMuffins.TurboSlicer.Guts;

namespace NobleMuffins.TurboSlicer
{
	public class TurboSlicer : MonoBehaviour
	{
		public Mesh[] preloadMeshes;
		public WorkerThreadMode workerThreadMode;

		private readonly HashSet<JobState> jobStates = new HashSet<JobState> ();
		private readonly List<JobState> jobStateQueue = new List<JobState>();
		private readonly ICollection<JobState> jobStateRemovalQueue = new List<JobState> ();
		private readonly IDictionary<int, MeshSnapshot> preloadedMeshes = new Dictionary<int, MeshSnapshot> ();

		private readonly ICollection<Transform> transformBuffer = new List<Transform>();
		private readonly ICollection<string> nameBuffer = new HashSet<string>();

		public void Awake ()
		{
			#if UNITY_WEBGL
			if (workerThreadMode == WorkerThreadMode.Asynchronous) {
				Debug.LogWarning ("Turbo Slicer will run synchronously because WebGL does not support threads.", this);
				workerThreadMode = WorkerThreadMode.Synchronous;
			}
			#endif
			
			if (preloadMeshes != null) {
				for (int i = 0; i < preloadMeshes.Length; i++) {
					var mesh = preloadMeshes [i];
					var indices = new int[mesh.subMeshCount][];
					for (int j = 0; j < mesh.subMeshCount; j++) {
						indices [j] = mesh.GetIndices (j);
					}
					//Not that we don't pull the tangents at all.
					var rom = new MeshSnapshot (null, mesh.vertices, mesh.normals, mesh.uv, mesh.uv2, mesh.tangents, indices, null, Matrix4x4.identity);
					preloadedMeshes [mesh.GetInstanceID ()] = rom;
				}
			}
		}

		private void Slice (GameObject subject, Vector4 planeInLocalSpace, int shatterStep, bool destroyOriginal)
		{
			if(subject == null) {
				throw new System.ArgumentNullException();
			}

			//Sanity check: are we already slicing this?
			foreach (var jobState in jobStates) {
				if (object.ReferenceEquals (jobState.Specification.Subject, subject)) {
					return;
//					Debug.Log ("Turbo Slicer was asked to slice '{0}' but this target is already enqueued.", subject.name);
				}
			}

			//If names in the hierarchy are replicated anywhere than we have a problem.

			transformBuffer.Clear();
			concatenateHierarchy(subject.transform, transformBuffer);
			nameBuffer.Clear();
			foreach(var t in transformBuffer) {
				nameBuffer.Add(t.gameObject.name);
			}
			if(nameBuffer.Count != transformBuffer.Count) {
				Debug.LogWarning("Turbo Slicer needs each object under its hierarchy to have a unique name.", subject);
				return;
			}

				var sliceable = subject.GetComponent<Sliceable> ();

				bool channelTangents, channelNormals, channelUV2;

				if (sliceable != null) {
					channelNormals = sliceable.channelNormals;
					channelTangents = sliceable.channelTangents;
					channelUV2 = sliceable.channelUV2;
				} else {
					channelTangents = false;
					channelNormals = true;
					channelUV2 = false;
				}

				var renderers = subject.GetComponentsInChildren<Renderer>();

				IEnumerable<MeshSnapshot> snapshots;

				var forwardPassAgent = subject.GetComponent<ForwardPassAgent> ();

				if (forwardPassAgent != null) {
					snapshots = forwardPassAgent.Snapshots;
				} else {
					var snapshotsBuilder = new List<MeshSnapshot>();

				foreach(var renderer in renderers) {
					Mesh mesh;
					bool meshIsABufferMesh;

					if (renderer is MeshRenderer) {
						var filter = renderer.GetComponent<MeshFilter> ();
						mesh = filter.sharedMesh;
						meshIsABufferMesh = false;
					} else if (renderer is SkinnedMeshRenderer) {
						var smr = (SkinnedMeshRenderer)renderer;
						meshIsABufferMesh = true;
						mesh = new Mesh ();
						smr.BakeMesh (mesh);
					} else {
						throw new System.NotImplementedException ("Turbo Slicer encountered a Renderer that is neither a MeshRenderer nor a SkinnedMeshRenderer");
					}

					var rootToLocalTransform = renderer.transform.worldToLocalMatrix * subject.transform.localToWorldMatrix;

					var unitRect = new Rect (0.0f, 0.0f, 1.0f, 1.0f);
					var infillRects = new Rect[mesh.subMeshCount];

					for (int i = 0; i < infillRects.Length; i++) {
						Rect? rect = null;
						if (renderer.sharedMaterials.Length > i) {
							var mat = renderer.sharedMaterials [i];
							if (sliceable != null) {
								for (int j = 0; j < sliceable.infillers.Length; j++) {
									var ifc = sliceable.infillers [j];
									if (object.ReferenceEquals (ifc.material, mat)) {
										rect = ifc.regionForInfill;
									}
								}
							}
							infillRects [i] = rect.GetValueOrDefault (unitRect);
						}
					}

					var isThisTheRoot = renderer.gameObject == subject;
					var key = isThisTheRoot ? MeshSnapshot.RootKey : renderer.gameObject.name;

					MeshSnapshot snapshot;

					if (preloadedMeshes.TryGetValue (mesh.GetInstanceID (), out snapshot) == false) {

						if(mesh.isReadable == false) {
							Debug.LogErrorFormat(subject, "Turbo Slicer needs to read mesh '{0}' on object '{1}', but cannot. If this object is " +
								"an original, go to its mesh and enable readability. If this object is a slice result, go to the " +
								"sliceable configuration on the original and turn on 'shreddable.", mesh.name, subject.name);
							return;
						}

						var indices = new int[mesh.subMeshCount][];
						for (int i = 0; i < mesh.subMeshCount; i++) {
							indices [i] = mesh.GetIndices (i);
						}
						var coords = mesh.uv;
						if (coords == null || coords.Length < mesh.vertexCount)
						{
							//These might be null or empty but the core doesn't have a branch for UVs. So we'll put some junk in there if we need to.
							coords = new Vector2[mesh.vertexCount];
						}

						snapshot = new MeshSnapshot (
							key,
							mesh.vertices,
							channelNormals ? mesh.normals : new Vector3[0],
							coords,
							channelUV2 ? mesh.uv2 : new Vector2[0],
							channelTangents ? mesh.tangents : new Vector4[0],
							indices,
							infillRects,
							rootToLocalTransform);
					} else {
						snapshot = new MeshSnapshot (
							key,
							snapshot.vertices,
							channelNormals ? snapshot.normals : new Vector3[0],
							snapshot.coords,
							channelUV2 ? snapshot.coords2 : new Vector2[0],
							channelTangents ? snapshot.tangents : new Vector4[0],
							snapshot.indices,
							infillRects,
							rootToLocalTransform);
					}

					if (meshIsABufferMesh) {
						GameObject.DestroyImmediate (mesh);
					}

					snapshotsBuilder.Add(snapshot);
				}

				snapshots = snapshotsBuilder;
			}

			var jobSpec = new JobSpecification (subject, snapshots, planeInLocalSpace, channelTangents, channelNormals, channelUV2, shatterStep, destroyOriginal);

			try {
				var jobState = new JobState (jobSpec);

				switch (workerThreadMode) {
				case WorkerThreadMode.Asynchronous:
					jobStates.Add (jobState);
					#if NETFX_CORE && !UNITY_EDITOR
					System.Threading.Tasks.Task.Factory.StartNew(ThreadSafeSlice.Slice, jobState);
					#else
					System.Threading.ThreadPool.QueueUserWorkItem (ThreadSafeSlice.Slice, jobState);
					#endif
                    break;
				case WorkerThreadMode.Synchronous:
					ThreadSafeSlice.Slice (jobState);
					if (jobState.HasYield) {
						ConsumeJobYield (jobState.Specification, jobState.Yield);
					} else if (jobState.HasException) {
						throw jobState.Exception;
					}
					break;
				default:
					throw new System.NotImplementedException ();
				}
			} catch (System.Exception ex) {
				Debug.LogException (ex, subject);
			}
		}

		void Update ()
		{
			jobStateRemovalQueue.Clear ();
			jobStateQueue.Clear();
			jobStateQueue.AddRange(jobStates);
			foreach (var jobState in jobStateQueue) {
				if (jobState.IsDone) {
					try {
						if (jobState.HasYield) {
							ConsumeJobYield (jobState.Specification, jobState.Yield);
						} else if (jobState.HasException) {
							throw jobState.Exception;
						}
					} catch (System.Exception ex) {
						Debug.LogException (ex, jobState.Specification.Subject);
						
					} finally {
						jobStateRemovalQueue.Add (jobState);
					}
				}
			}
			jobStates.ExceptWith (jobStateRemovalQueue);
		}

		class ConsumptionTuple
		{
			public ConsumptionTuple(GameObject root, IEnumerable<MeshSnapshot> snapshot) {
				this.root = root;
				this.snapshot = snapshot;
			}
			public readonly GameObject root;
			public readonly IEnumerable<MeshSnapshot> snapshot;
		}

		readonly Dictionary<object, MeshSnapshot> snapshotByKeyBuffer = new Dictionary<object, MeshSnapshot>();
		readonly List<Transform> traversalBuffer = new List<Transform>();
		readonly List<GameObject> targetBuffer = new List<GameObject>();

		private void ConsumeJobYield (JobSpecification jobSpec, JobYield jobYield)
		{
            #if NOBLEMUFFINS
			var stopwatch = new System.Diagnostics.Stopwatch ();
			stopwatch.Start ();
            #endif

			var go = jobSpec.Subject;

			if (go == null) {
				throw new System.Exception ("Turbo Slicer was asked to slice an object, but the object has been destroyed.");
			}

			var onlyHaveOne = false;

			{
				var sides = new [] { jobYield.Alfa, jobYield.Bravo };
				for(int i = 0; i < sides.Length && !onlyHaveOne; i++) {
					var side = sides[i];
					var indexCount = 0;
					foreach(var snapshot in side) {
						for (int j = 0; j < snapshot.indices.Length; j++) {
							indexCount += snapshot.indices[j].Length;
						}
					}
					onlyHaveOne |= indexCount == 0;
				}
			}

			if (onlyHaveOne) {	
				//Do nothing
			} else {
				
				GameObject alfaObject, bravoObject;

				var sliceable = go.GetComponent<Sliceable> ();

				var goTransform = go.transform;

				Dictionary<string,Transform> transformByName;
				Dictionary<string,bool> alfaPresence, bravoPresence;

				determinePresence (goTransform, jobSpec.PlaneInLocalSpace, out transformByName, out alfaPresence, out bravoPresence);

				Object alfaSource, bravoSource;

				if (sliceable != null) {
					bool useAlternateForAlfa, useAlternateForBravo;

					if (sliceable.alternatePrefab == null) {
						useAlternateForAlfa = false;
						useAlternateForBravo = false;
					} else if (sliceable.alwaysCloneFromAlternate) {
						useAlternateForAlfa = true;
						useAlternateForBravo = true;
					} else {
						useAlternateForAlfa = sliceable.cloneAlternate (alfaPresence);
						useAlternateForBravo = sliceable.cloneAlternate (bravoPresence);
					}

					alfaSource = useAlternateForAlfa ? sliceable.alternatePrefab : go;
					bravoSource = useAlternateForBravo ? sliceable.alternatePrefab : go;
				} else {
					alfaSource = bravoSource = go;
				}

				alfaObject = (GameObject)GameObject.Instantiate (alfaSource);
				bravoObject = (GameObject)GameObject.Instantiate (bravoSource);

				bravoObject.name = alfaObject.name = alfaSource.name;

				handleHierarchy (alfaObject.transform, alfaPresence, transformByName);
				handleHierarchy (bravoObject.transform, bravoPresence, transformByName);

				var originalRigidBody = go.GetComponent<Rigidbody> ();

				var tuples = new [] {
					new ConsumptionTuple(alfaObject, jobYield.Alfa),
					new ConsumptionTuple(bravoObject, jobYield.Bravo)
				};

				for (int i = 0; i < tuples.Length; i++) {
					var tuple = tuples [i];

					var transform = tuple.root.GetComponent<Transform> ();

					transform.SetParent (goTransform.parent, false);
					transform.localPosition = goTransform.localPosition;
					transform.localRotation = goTransform.localRotation;
					transform.localScale = goTransform.localScale;

					tuple.root.layer = go.layer;

					if (originalRigidBody != null) {
						var rigidBody = tuple.root.GetComponent<Rigidbody> ();

						if (rigidBody != null) {
							rigidBody.angularVelocity = originalRigidBody.angularVelocity;
							rigidBody.velocity = originalRigidBody.velocity;
						}
					}

					var doColliders = sliceable != null && sliceable.refreshColliders;
					var doForwardPass = sliceable != null && sliceable.shreddable;

					snapshotByKeyBuffer.Clear();

					foreach(var snapshot in tuple.snapshot) {
						snapshotByKeyBuffer[snapshot.key] = snapshot;
					}

					traversalBuffer.Clear();
					targetBuffer.Clear();

					traversalBuffer.Add(tuple.root.transform);

					while(traversalBuffer.Count > 0) {
						var lastIndex = traversalBuffer.Count - 1;
						var last = traversalBuffer[lastIndex];
						traversalBuffer.RemoveAt(lastIndex);

						for(var childIndex = 0; childIndex < last.childCount; childIndex++) {
							var child = last.GetChild(childIndex);
							traversalBuffer.Add(child);
						}

						var isThisHolderTheRoot = last.gameObject == tuple.root;
						var key = isThisHolderTheRoot ? MeshSnapshot.RootKey : last.name;
						if(snapshotByKeyBuffer.ContainsKey(key)) {
							targetBuffer.Add(last.gameObject);
						}
					}

					for(int targetIndex = 0; targetIndex < targetBuffer.Count; targetIndex++) {
						var holder = targetBuffer[targetIndex];

						var isThisHolderTheRoot = holder == tuple.root;

						var key = isThisHolderTheRoot ? MeshSnapshot.RootKey : holder.name;

						var snapshot = snapshotByKeyBuffer[key];

						var mesh = new Mesh ();
						mesh.name = "Turbo Slicer mesh";
						mesh.vertices = snapshot.vertices;
						mesh.uv = snapshot.coords;
						if (snapshot.normals.Length > 0) {
							mesh.normals = snapshot.normals;
						}
						if (snapshot.coords2.Length > 0) {
							mesh.uv2 = snapshot.coords2;
						}
						if (snapshot.tangents.Length > 0) {
							mesh.tangents = snapshot.tangents;
						}
						mesh.subMeshCount = snapshot.indices.Length;
						for (int j = 0; j < snapshot.indices.Length; j++) {
							int[] array;
							array = snapshot.indices [j];
							mesh.SetIndices (array, MeshTopology.Triangles, j);
						}
						if (doColliders) {
							mesh.RecalculateBounds ();
						}

						SetMesh (holder, mesh);

						if (doColliders) {
							var collider = holder.GetComponent<Collider> ();

							if (collider != null) {

								var holderTransform = holder.GetComponent<Transform> ();
								var rootTransform = tuple.root.GetComponent<Transform> ();

								var bounds = mesh.bounds;

								var pointOne = bounds.min;
								var pointTwo = bounds.max;

								if (holder != tuple.root) {
									var matrix = holderTransform.localToWorldMatrix * rootTransform.worldToLocalMatrix;
									pointOne = matrix.MultiplyPoint3x4 (pointOne);
									pointTwo = matrix.MultiplyPoint3x4 (pointTwo);
									var center = (pointOne + pointTwo) * 0.5f;
									bounds = new Bounds (center, Vector3.zero);
									bounds.Encapsulate (pointOne);
									bounds.Encapsulate (pointTwo);
								}

								if (collider is BoxCollider) {
									var boxCollider = (BoxCollider)collider;
									boxCollider.center = bounds.center;
									boxCollider.size = bounds.extents * 2.0f;
								} else if (collider is SphereCollider) {
									var sphereCollider = (SphereCollider)collider;
									sphereCollider.center = bounds.center;
									sphereCollider.radius = bounds.extents.magnitude;
								} else if (collider is MeshCollider) {
									var mc = (MeshCollider)collider;
									mc.sharedMesh = mesh;
								}
							}
						}

						mesh.UploadMeshData (true);
					}

					if (doForwardPass) {
						var forwardPassAgent = tuple.root.GetComponent<ForwardPassAgent> ();
						if (forwardPassAgent == null) {
							forwardPassAgent = tuple.root.AddComponent<ForwardPassAgent> ();
						}
						forwardPassAgent.Snapshots = tuple.snapshot;
					}
				}

				if(jobSpec.ShatterStep == 0 && sliceable != null) {
					sliceable.RaiseSliced (alfaObject, bravoObject);
				}
				else {
					var nextShatterStep = jobSpec.ShatterStep - 1;
					Shatter(alfaObject, nextShatterStep);
					Shatter(bravoObject, nextShatterStep);
				}

				if (jobSpec.DestroyOriginal) {
					GameObject.Destroy (go);
				}
			}
            #if NOBLEMUFFINS
			stopwatch.Stop ();
			Debug.LogFormat ("Slice result consumed in {0} ms", stopwatch.ElapsedMilliseconds.ToString ());
            #endif
			
		}

		private void handleHierarchy (Transform root, Dictionary<string,bool> presenceByName, Dictionary<string,Transform> originalsByName)
		{
			List<Transform> allChildren = new List<Transform> (presenceByName.Count);

			concatenateHierarchy (root, allChildren);

//			foreach (Transform t in allChildren) {
//				GameObject go = t.gameObject;
//
//				string key = t.name;
//
//				bool shouldBePresent = presenceByName.ContainsKey (key) ? presenceByName [key] : false;
//
//				shouldBePresent &= originalsByName.ContainsKey (key) && originalsByName [key].gameObject.activeSelf;
//
//				go.SetActive (shouldBePresent);
//			}

			foreach (Transform t in allChildren) {
				string key = t.name;

				if (originalsByName.ContainsKey (key)) {
					Transform original = originalsByName [key];

					t.localPosition = original.localPosition;
					t.localRotation = original.localRotation;
					t.localScale = original.localScale;
				}
			}
		}

		private void determinePresence (Transform root, Vector4 plane, out Dictionary<string,Transform> transformByName, out Dictionary<string,bool> frontPresence, out Dictionary<string,bool> backPresence)
		{
			List<Transform> allChildren = new List<Transform> ();

			concatenateHierarchy (root, allChildren);

			Vector3[] positions = new Vector3[allChildren.Count];

			for (int i = 0; i < positions.Length; i++) {
				positions [i] = allChildren [i].position;
			}

			Matrix4x4 worldToLocal = root.worldToLocalMatrix;

			for (int i = 0; i < positions.Length; i++) {
				positions [i] = worldToLocal.MultiplyPoint3x4 (positions [i]);
			}

			PlaneTriResult[] ptr = new PlaneTriResult[positions.Length];

			for (int i = 0; i < positions.Length; i++) {
				var p = positions [i];
				ptr [i] = p.x * plane.x + p.y * plane.y + p.z * plane.z + plane.w > 0f ? PlaneTriResult.PTR_FRONT : PlaneTriResult.PTR_BACK;

			}

			transformByName = new Dictionary<string, Transform> ();
			frontPresence = new Dictionary<string, bool> ();
			backPresence = new Dictionary<string, bool> ();

			bool duplicateNameWarning = false;

			for (int i = 0; i < ptr.Length; i++) {
				Transform t = allChildren [i];
				string key = t.name;

				if (transformByName.ContainsKey (key))
					duplicateNameWarning = true;

				transformByName [key] = t;

				frontPresence [key] = ptr [i] == PlaneTriResult.PTR_FRONT;
				backPresence [key] = ptr [i] == PlaneTriResult.PTR_BACK;
			}

			if (duplicateNameWarning)
				Debug.LogWarning ("Sliceable has children with non-unique names. Behaviour is undefined!");
		}

		void concatenateHierarchy (Transform t, ICollection<Transform> results)
		{
			foreach (Transform child in t) {
				results.Add (child);
				concatenateHierarchy (child, results);
			}
		}

		private static void SetMesh (GameObject meshHolder, Mesh mesh)
		{
			Renderer renderer = meshHolder.GetComponent<Renderer> ();
			MeshFilter filter = null;
			if (renderer is MeshRenderer) {
				filter = meshHolder.GetComponent<MeshFilter> ();
			} else if (renderer is SkinnedMeshRenderer) {
				meshHolder = renderer.gameObject;
				var allMats = renderer.sharedMaterials;
				GameObject.DestroyImmediate (renderer);
				renderer = meshHolder.AddComponent<MeshRenderer> ();
				renderer.sharedMaterials = allMats;
				filter = meshHolder.AddComponent<MeshFilter> ();
			}
			if (filter != null) {
				filter.mesh = mesh;
			}
		}

		public void Slice (GameObject subject, Vector4 planeInLocalSpace, bool destroyOriginal)
		{
			Slice(subject, planeInLocalSpace, 0, destroyOriginal);
		}

		public void SliceByLine (GameObject subject, Camera camera, Vector3 _start, Vector3 _end, bool destroyOriginal)
		{
			Vector3 targetPositionRelativeToCamera = camera.transform.worldToLocalMatrix.MultiplyPoint3x4 (subject.transform.position);

			_start.z = targetPositionRelativeToCamera.z;
			_end.z = targetPositionRelativeToCamera.z;

			Vector3 _middle = (_start + _end) / 2f;
			_middle.z *= 2f;

			Vector3 start = camera.ScreenToWorldPoint (_start);
			Vector3 middle = camera.ScreenToWorldPoint (_middle);
			Vector3 end = camera.ScreenToWorldPoint (_end);

			SliceByTriangle (subject, new[] { start, middle, end }, destroyOriginal);
		}

		public void SliceByTriangle (GameObject subject, Vector3[] triangleInWorldSpace, bool destroyOriginal)
		{
			Vector3[] t = new Vector3[3];

			Matrix4x4 matrix = subject.transform.worldToLocalMatrix;

			t [0] = matrix.MultiplyPoint3x4 (triangleInWorldSpace [0]);
			t [1] = matrix.MultiplyPoint3x4 (triangleInWorldSpace [1]);
			t [2] = matrix.MultiplyPoint3x4 (triangleInWorldSpace [2]);

			Vector4 plane = Vector4.zero;

			plane.x = t [0].y * (t [1].z - t [2].z) + t [1].y * (t [2].z - t [0].z) + t [2].y * (t [0].z - t [1].z);
			plane.y = t [0].z * (t [1].x - t [2].x) + t [1].z * (t [2].x - t [0].x) + t [2].z * (t [0].x - t [1].x);
			plane.z = t [0].x * (t [1].y - t [2].y) + t [1].x * (t [2].y - t [0].y) + t [2].x * (t [0].y - t [1].y);
			plane.w = -(t [0].x * (t [1].y * t [2].z - t [2].y * t [1].z) + t [1].x * (t [2].y * t [0].z - t [0].y * t [2].z) + t [2].x * (t [0].y * t [1].z - t [1].y * t [0].z));

			Slice (subject, plane, destroyOriginal);
		}

		public void Shatter (GameObject subject, int steps = 3, bool destroyOriginal = true)
		{
			var normal = UnityEngine.Random.onUnitSphere;
			var planeInLocalSpace = Helpers.PlaneFromPointAndNormal(Vector3.zero, normal);
			Slice(subject, planeInLocalSpace, steps, destroyOriginal);
		}
	}
}