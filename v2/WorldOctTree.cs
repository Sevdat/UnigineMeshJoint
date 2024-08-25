using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "e32ab798c77a29531fcfa8afe4d20f357a074402")]
public class WorldOctTree : Component
{
	class Quaternion {
		public quat angledAxis(float angle, vec3 rotationAxis){
			return new quat(rotationAxis, angle);
		}
		public vec3 rotate(vec3 origin, vec3 point, quat angledAxis){
			quat q = angledAxis;
			vec3 v = point - origin;
			vec3 rotatedOffset = q * v;
			return origin + rotatedOffset;
		}                    
	}

	class Rotate {
		vec3 rotationOrigin;
		quat angledAxis;
	}
	class Axis{
		public vec3 origin,x,y,z;
	}
	class Joint{
		Axis axis;
		List<Joint> connectedTo;
		List<Joint> connectedFrom;
		
		public Axis rotateAxis{
			get { return axis; }
			set { axis = value; }
		}
	}
	class Sphere{
		public Joint attachedTo;
		public vec3 origin;
		public float radius;

	}
	void Init()
	{

	}
	
	void Update()
	{
		// write here code to be called before updating each render frame
		
	}
}