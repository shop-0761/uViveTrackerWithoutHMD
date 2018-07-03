using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


namespace uViveTrackerWithoutHMD
{

	public class SelfManagementOfTrackedDevices : MonoBehaviour
	{
		public List<GameObject> targetObjs = new List<GameObject>();

		[SerializeField]
		bool useTargetClass = false;
		[SerializeField]
		private ETrackedDeviceClass targetClass = ETrackedDeviceClass.GenericTracker;

		[SerializeField]
		private KeyCode resetDeviceIds = KeyCode.Tab;

		private CVRSystem _vrSystem;
		private List<int> _validDeviceIds = new List<int>();

		void Start()
		{
			var error = EVRInitError.None;
			_vrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);

			if (error != EVRInitError.None) { Debug.LogWarning("Init error: " + error); return; }

			Debug.Log("init done");
			foreach (var item in targetObjs) { item.SetActive(false); }
			SetDeviceIds();
		}

		void SetDeviceIds()
		{
			_validDeviceIds.Clear();
			for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
			{
				var deviceClass = _vrSystem.GetTrackedDeviceClass(i);
				var condition = useTargetClass ? deviceClass == targetClass : true;
				if (deviceClass != ETrackedDeviceClass.Invalid && condition)
				{
					Debug.Log("OpenVR device at " + i + ": " + deviceClass);
					_validDeviceIds.Add((int)i);
					if (_validDeviceIds.Count - 1 < targetObjs.Count)
					{
						targetObjs[_validDeviceIds.Count - 1].SetActive(true);
					}
				}
			}
		}
		private void Update()
		{
			UpdateTrackedObj();

			if (Input.GetKeyDown(resetDeviceIds))
			{
				SetDeviceIds();
			}
		}

		private void UpdateTrackedObj()
		{
			TrackedDevicePose_t[] allPoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

			_vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, allPoses);

			for (int i = 0; i < _validDeviceIds.Count; i++)
			{
				if (i < targetObjs.Count)
				{
					var pose = allPoses[_validDeviceIds[i]];
					var absTracking = pose.mDeviceToAbsoluteTracking;
					var mat = new SteamVR_Utils.RigidTransform(absTracking);
					targetObjs[i].transform.SetPositionAndRotation(mat.pos, mat.rot);
				}
			}
		}

		private void OnDestroy()
		{
			OpenVR.Shutdown();
		}
	}
}