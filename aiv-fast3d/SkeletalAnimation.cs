using System;
using System.Collections.Generic;
using OpenTK;

namespace Aiv.Fast3D
{
    public class SkeletalAnimation
    {

        public class KeyFrame
        {
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

        private int nFrames;
        public int FramesCount
        {
            get
            {
                return nFrames;
            }
        }

        public float Duration
        {
            get
            {
                return (1.0f / this.fps) * nFrames;
            }
        }

        public Dictionary<string, KeyFrame[]> KeyFrames;

        public SkeletalAnimation(string name, int nFrames, float fps)
        {
            this.name = name;
            this.nFrames = nFrames;
            this.fps = fps;
            KeyFrames = new Dictionary<string, KeyFrame[]>();
        }

        public void SetKeyFrame(string subject, int frame, Vector3 position, Quaternion rotation, Vector3 scale)
        {

            if (!KeyFrames.ContainsKey(subject))
                KeyFrames[subject] = new KeyFrame[this.nFrames];

            KeyFrame keyFrame = new KeyFrame();
            keyFrame.Position = position;
            keyFrame.Rotation = rotation;
            keyFrame.Scale = scale;

            KeyFrames[subject][frame] = keyFrame;
        }
    }
}
