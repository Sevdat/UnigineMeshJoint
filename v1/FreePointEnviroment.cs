using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unigine;

[Component(PropertyGuid = "11c8627ecaa24277eefccff039dede7767f5b9cc")]
public class FreePointEnviroment : Component
{
    public World world = new World();

    public class World {
        public Dictionary<string,BodyInWorld> allBodies;
        public Library library = new();

        public class Path {
            public string name;
            public int? jointIndex = default;
            public int? meshVertexIndex = default;
            public Path() {}

            public string pathToString(){
                string temp = $"{name}";
                bool jointCheck = jointIndex != null;
                bool meshCheck = meshVertexIndex != null;
                if (jointCheck) {
                    temp += $"_{jointIndex}";
                    if (meshCheck) 
                        temp += $"_{meshVertexIndex}";
                    };
                return temp;
            }

            public Path(string name,int? jointIndex,int? meshVertexIndex){
                this.name = name;
                this.jointIndex = jointIndex;
                this.meshVertexIndex = meshVertexIndex;
            }
        }

        public struct BodyInWorld {
            public struct Axis {
                public vec3 origin,x,y,z;
            }
            public struct BodyData {
                public Axis globalAxis;
                public Dictionary<int,Joint> bodyStructure;
            }
            public struct Joint {
                public List<int> jointConnections;
                public Axis localAxis;
                public Mesh mesh;
            }
            public struct Triangle{
                public int a,b,c;
            }
            public struct Mesh {
                public List<vec3> vertex;
                public List<Triangle> indices;
            }
        }

        public class Library {
            public void codeTest(){
                Path lol = createPath("lol",null,1);
                Log.Message(lol == null);
            }
            public ObjectMeshDynamic createCube(vec3 size,vec3 position,string name){
                ObjectMeshDynamic cube = Primitives.CreateBox(size);
                cube.TriggerInteractionEnabled = true;
                cube.SetIntersection(true, 0);
                cube.SetIntersectionMask(1, 0);
                cube.SetCollision(true,0);
                cube.SetCollisionMask(1, 0);
                cube.WorldPosition = position;
                cube.Name = name;
                return cube;
            }
            public Path createPath(string name,int? jointIndex,int? meshVertexIndex){
                Path temp;
                bool error = jointIndex == null && meshVertexIndex != null;
                temp = (!error)? new Path(name,jointIndex,meshVertexIndex):null;
                return temp;
            }
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
    }

	void Init()
	{
        world.library.codeTest();

	}

	void Update()
	{
		// write here code to be called before updating each render frame
		
	}
}
