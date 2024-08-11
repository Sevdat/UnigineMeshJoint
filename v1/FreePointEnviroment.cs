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
        public Library library;
        public BodyInWorld bodyInWorld;
        public Library.Path path;
        

        public class BodyInWorld:Library {
            public Axis axis;
            public BodyData bodyData;
            public Joint joint;
            public CollisionSphere collisionSphere;
            public Quaternion quaternion;
            public Triangle triangle;
            public BodyMesh bodyMesh;


        }

        public class Library {

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

            public class Axis {
                public vec3 origin,x,y,z;
                public Axis init(){
                    return new Axis();
                }
                public Axis get(){
                    return this;
                }
                public Axis set(vec3 origin,vec3 x,vec3 y,vec3 z){
                    this.origin = origin;
                    this.x = x;
                    this.y = y;
                    this.z = z;
                    return get();
                }
                public Axis scale(float distanceFromOrigin){
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
                    return get();
                }
                public vec3 direction(vec3 point,vec3 origin){ 
                    vec3 v = point-origin;
                    return v/ MathLib.Length(v);
                }
            }
            public class BodyData {
                public Axis globalAxis;
                public Dictionary<int,Joint> bodyStructure;
                public List<int> freeKeys;
                public BodyData init(){
                    return new BodyData();
                }
                public BodyData get(){
                    return this;
                }
                public BodyData set(Axis globalAxis,Dictionary<int,Joint> bodyStructure){
                    this.globalAxis = globalAxis;
                    this.bodyStructure = bodyStructure;
                    return get();
                }
            }

            public class Joint {
                public Axis localAxis;
                public int connectedFrom;
                public List<int> connectedTo;
                public Dictionary<int,CollisionSphere> collisionSphere;
                public Joint init(){
                    return new Joint();
                }
                public Joint get(){
                    return this;
                }
                public Joint set(int connectedFrom,List<int> connectedTo,Axis localAxis,Dictionary<int,CollisionSphere> collisionSphere){
                    this.connectedFrom = connectedFrom;
                    this.connectedTo = connectedTo;
                    this.localAxis = localAxis;
                    this.collisionSphere = collisionSphere;
                    return get();
                }
            }

            public class CollisionSphere {
                public vec3 origin;
                public float radius;
                public CollisionSphere init(){
                    return new CollisionSphere();
                }
                public CollisionSphere get(){
                    return this;
                }
                public CollisionSphere set(vec3 origin,float radius){
                    this.origin = origin;
                    this.radius = radius;
                    return get();
                }
            }

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

            public class Triangle {
                public int a,b,c;
                public Triangle init(){
                    return new Triangle();
                }
                public Triangle get(){
                    return this;
                }
                public Triangle set(int a,int b,int c){
                    this.a = a;
                    this.b = b;
                    this.c = c;
                    return get();
                }
            }

            public class BodyMesh {
                public List<vec3> vertex;
                public List<Triangle> indices;
                public BodyMesh init(){
                    return new BodyMesh();
                }                
                public BodyMesh get(){
                    return this;
                }
                public BodyMesh set(List<vec3> vertex,List<Triangle> indices){
                    this.vertex = vertex;
                    this.indices = indices;
                    return get();
                }
            }

            public class Timer{
                public Timer() {}
                public float time;
                public Timer(float time){
                    this.time = time;
                }
                public Timer init(){
                    return new Timer();
                }
                public Timer get(){
                    return this;
                }
                public Timer set(float time){
                    this.time = time;
                    return get();
                }
                public Timer add(float time){
                    this.time += time;
                    return get();
                }
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
    ObjectMeshDynamic v;
	void Init()
	{
        v = world.library.createCube(new vec3(1,1,1),new vec3(1,1,1),"lol");
        
	}
    float lol =0;
	void Update()
	{
        lol += Game.IFps;
        if (lol>5) v.DeleteForce();
		// write here code to be called before updating each render frame
	}
}
