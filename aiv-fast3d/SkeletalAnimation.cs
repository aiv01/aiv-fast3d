using System;
using System.Collections.Generic;
using OpenTK;

namespace Aiv.Fast3D
{
	public class SkeletalAnimation
	{

		public class KeyFrame
		{
			public float Time;
			public Vector3 Position;
			public Quaternion Rotation;
			public Vector3 Scale;
		}

		private string name;
		public string Name
		{
			get
			{
				return name;
			}
		}

		private float fps;
		public float Fps
		{
			get
			{
				return fps;
			}
		}

		public Dictionary<string, List<KeyFrame>> KeyFrames;

		public SkeletalAnimation(string name, float fps)
		{
			this.name = name;
			this.fps = fps;
			KeyFrames = new Dictionary<string, List<KeyFrame>>();
		}

		public KeyFrame AddKeyFrame(string subject, float Time, Vector3 position, Quaternion rotation, Vector3 scale)
		{

			if (!KeyFrames.ContainsKey(subject))
				KeyFrames[subject] = new List<KeyFrame>();

			KeyFrame keyFrame = new KeyFrame();
			keyFrame.Time = Time;
			keyFrame.Position = position;
			keyFrame.Rotation = rotation;
			keyFrame.Scale = scale;

			KeyFrames[subject].Add(keyFrame);
			return keyFrame;
		}
	}
}
