using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast3D
{
	public class PerspectiveCamera : Camera
	{
		private Vector3 position3;
		public Vector3 Position3
		{
			get
			{
				return this.position3;
			}
			set
			{
				this.position3 = value;
			}
		}

		private Vector3 direction;
		public Vector3 Direction
		{
			get
			{
				return this.direction;
			}
			set
			{
				this.direction = value;
			}
		}


		public PerspectiveCamera(Vector3 position, Vector3 direction)
		{
			this.position3 = position;
			this.direction = direction;
		}

		public override Matrix4 Matrix()
		{
			return Matrix4.LookAt(position3, position3 + direction, Vector3.UnitY);
		}
	}
}
