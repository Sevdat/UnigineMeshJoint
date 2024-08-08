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
        public Dictionary<string,BodyInWorld.BodyData> allBodies;
        public Library library = new();
        public BodyInWorld bodyInWorld = new();

        public class Path {
            public Path() {}
            public string name;
            public int? jointIndex = default;
            public int? meshVertexIndex = default;
            public Path(string name,int? jointIndex,int? meshVertexIndex){
                this.name = name;
                this.jointIndex = jointIndex;
                this.meshVertexIndex = meshVertexIndex;
            }
            public string pathToString() {
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
            public Path createPath(string name,int? jointIndex,int? meshVertexIndex){
                Path temp;
                bool error = jointIndex == null && meshVertexIndex != null;
                temp = (!error)? new Path(name,jointIndex,meshVertexIndex):null;
                return temp;
            }
        }

        public class BodyInWorld {

            public Axis axis;
            public struct Axis {
                public vec3 origin,x,y,z ;
                public Axis(vec3 origin,vec3 x,vec3 y,vec3 z){
                    this.origin = origin;
                    this.x = x;
                    this.y = y;
                    this.z = z;
                }
                public Axis get(){
                    return new Axis(origin,x,y,z);
                }
                public Axis create(vec3 origin,float distanceFromOrigin){
                    this.origin=origin;
                    scale(distanceFromOrigin);
                    return new Axis(origin,x,y,z);
                }
                public void scale(float distanceFromOrigin){
                    bool gateX = x == vec3.ZERO;
                    bool gateY = y == vec3.ZERO;
                    bool gateZ = z == vec3.ZERO;
                    x = gateX?
                        origin + new vec3(distanceFromOrigin,0,0):
                        origin + direction(x,origin)*distanceFromOrigin;
                    y = gateY?
                        origin + new vec3(0,distanceFromOrigin,0):
                        origin + direction(y,origin)*distanceFromOrigin;
                    z = gateZ?
                        origin + new vec3(0,0,distanceFromOrigin):
                        origin + direction(z,origin)*distanceFromOrigin;                                               
                }
                public vec3 direction(vec3 point,vec3 origin){ 
                    vec3 v = point-origin;
                    return v/ MathLib.Length(v);
                }
            }

            public BodyData bodyData;
            public struct BodyData {
                public Axis globalAxis;
                public Dictionary<int,Joint> bodyStructure;
                public BodyData(Axis globalAxis,Dictionary<int,Joint> bodyStructure){
                    this.globalAxis = globalAxis;
                    this.bodyStructure = bodyStructure;
                }
                public BodyData create(Axis globalAxis,Dictionary<int,Joint> bodyStructure){
                    return new BodyData(globalAxis,bodyStructure);
                }
            }

            public Joint joint;
            public struct Joint {
                public List<int> jointConnections;
                public Axis localAxis;
                public BodyMesh mesh;
                public Joint(List<int> jointConnections,Axis localAxis,BodyMesh mesh){
                    this.jointConnections = jointConnections;
                    this.localAxis = localAxis;
                    this.mesh = mesh;
                }
                public Joint create(List<int> jointConnections,Axis localAxis,BodyMesh mesh){
                    return new Joint(jointConnections,localAxis,mesh);
                }
            }

            public Triangle triangle;
            public struct Triangle {
                public int a,b,c;
                public Triangle(int a,int b,int c){
                    this.a = a;
                    this.b = b;
                    this.c = c;
                }
                public Triangle create(int a,int b,int c){
                    return new Triangle(a,b,c);
                }
            }

            public BodyMesh bodyMesh;
            public struct BodyMesh {
                public List<vec3> vertex;
                public List<Triangle> indices;
                public BodyMesh(List<vec3> vertex,List<Triangle> indices){
                    this.vertex = vertex;
                    this.indices = indices;
                }
                public BodyMesh create(List<vec3> vertex,List<Triangle> indices){
                    return new BodyMesh(vertex,indices);
                }
            }
        }

        public class Library {
            
            public void lol(){
            }
            public class BodyClass: BodyInWorld {
                public Quaternion quaternion = new();

                

                public class Quaternion {
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


            public void codeTest(){

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
        }
    }

	void Init()
	{
        var v = world.bodyInWorld.axis.create(new vec3(5,5,5),2);
        Log.Message(v.origin +"\n");
        Log.Message(v.x+"\n");
        Log.Message(v.y+"\n");
        Log.Message(v.z+"\n");
        v.create(new vec3(5,5,5),5);
        Log.Message(v.origin +"\n");
        Log.Message(v.x+"\n");
        Log.Message(v.y+"\n");
        Log.Message(v.z+"\n");
        
	}

	void Update()
	{
		// write here code to be called before updating each render frame
		
	}
}
