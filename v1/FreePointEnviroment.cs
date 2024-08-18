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
        public Dictionary<int,List<Collision>> octTree;
        public Library library;
        public BodyInWorld bodyInWorld;

        public class Collision:Library{
            public CollisionSphere collisionSphere;
        }

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

            public class Axis {
                public vec3 origin,x,y,z;
                
                public Axis init(){
                    return new Axis();
                }
                public void setOrigin(vec3 origin){
                    this.origin = origin;
                }
                public void setX(vec3 x){
                    this.x = x;
                }
                public void setY(vec3 y){
                    this.y = y;
                }
                public void setZ(vec3 z){
                    this.z = z;
                }
                public void setAll(vec3 origin,vec3 x,vec3 y,vec3 z){
                    setOrigin(origin);
                    setX(x);
                    setY(y);
                    setZ(z);
                }
                public void scale(float distanceFromOrigin){
                    if (distanceFromOrigin > 0){
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
                }
                public vec3 direction(vec3 point,vec3 origin){ 
                    vec3 v = point-origin;
                    return v/ MathLib.Length(v);
                }
            }
            public class KeyGenerator{
                public int maxKeys;
                public int availableKeys;
                public int increaseKeysBy;
                public List<int> freeKeys;

                public void init(int amountOfKeys){
                    increaseKeysBy = amountOfKeys;
                    generateKeys();
                }
                public void generateKeys(){
                    for(int i = 0; i < increaseKeysBy; i++){
                        freeKeys.Add(i+maxKeys);
                    }
                    availableKeys += increaseKeysBy;
                    maxKeys += increaseKeysBy;
                }
                public void setLimit(int newLimit){
                    if(newLimit > 0){
                        increaseKeysBy = newLimit;
                    }
                }
                public int getKey(){
                    if(availableKeys <= 0) generateKeys();
                    int key = freeKeys[0];
                    freeKeys.RemoveAt(0);
                    availableKeys -= 1;
                    return key;
                }
                public void returnKey(int key){
                    freeKeys.Add(key);
                    availableKeys +=1;
                }
                public bool checkSet(int key, int?[] keys, int count){
                    int? check = keys[key];
                    if (check != null){
                        keys[key] = count;
                        return true;
                    }
                    return false;
                }
                public List<int> orginizeKeys(List<int> connectionList, int?[] keyManager){
                    List<int> newConnection = new List<int>();
                    for (int j = 0; j < connectionList.Count; j++) {
                        int? index = keyManager[connectionList[j]];
                        if (index != null){
                            newConnection.Add((int)index);
                        }
                    }
                    return newConnection;
                }
                public void optiomizeKeys(BodyData bodyData, int addExtraKeys){
                    int count = 0;
                    int size = 
                        bodyData.keyGenerator.maxKeys - bodyData.keyGenerator.availableKeys;
                    int?[] keyManager = new int?[size];
                    Joint[] newJoint = new Joint[size + addExtraKeys];
                    for (int i = 0; i < size; i++){
                        Joint joint = bodyData.bodyStructure[i];
                        if(joint != null){
                            int current = joint.connection.current;
                            keyManager[current] = count;
                            joint.connection.setCurrent(count);
                            count++;
                        }
                    }

                    for (int i = 0; i<count; i++) {
                        Joint joint = newJoint[i];
                        joint.connection.setPast(
                            orginizeKeys(joint.connection.past,keyManager)
                            );
                        joint.connection.setFuture(
                            orginizeKeys(joint.connection.future,keyManager)
                            );
                    }  
                    bodyData.bodyStructure = newJoint;  
                    maxKeys = count + addExtraKeys;
                    availableKeys = addExtraKeys;
                    freeKeys.Clear();
                    generateKeys();
                }
            }
            public class BodyData {
                public string BodyDataName;
                public Axis globalAxis;
                public Joint[] bodyStructure;
                public KeyGenerator keyGenerator;

                public BodyData init(int amountOfIndexes){
                    BodyData newBody = new BodyData();
                    bodyStructure = new Joint[amountOfIndexes];
                    newBody.keyGenerator.init(amountOfIndexes);
                    return newBody;
                }
                public void setAll(Axis globalAxis,Joint[] bodyStructure,KeyGenerator keyGenerator){
                    this.globalAxis = globalAxis;
                    this.bodyStructure = bodyStructure;
                    this.keyGenerator = keyGenerator;
                }
                public Joint getJoint(int key){
                   return bodyStructure[key];
                }
                public void addToDictionary(Joint joint){

                }
                public void deleteFromDictionary(int key){
                   Joint remove = bodyStructure[key];
                    if(remove != null){
                        keyGenerator.returnKey(key);
                        bodyStructure[key] = null;
                    }
                }
            }

            public class Connection {
                public int current;
                public List<int> past, future;

                public Connection init(){
                    return new Connection();
                }
                public void setCurrent(int current){
                    this.current = current;
                }
                public void setPast(List<int> past){
                    this.past = past;
                }
                public void setFuture(List<int> future){
                    this.future = future;
                }
                public void setAll(int current,List<int> past,List<int> future){
                    this.current = current;
                    this.past = past;
                    this.future = future;
                }
            }

            public class Joint {
                public Axis localAxis;
                public Connection connection;
                public CollisionSphere[] collisionSphere;
                public KeyGenerator keyGenerator;

                public Joint init(){
                    return new Joint();
                }
                public void setAll(Connection connection,Axis localAxis,CollisionSphere[] collisionSphere,KeyGenerator keyGenerator){
                    this.connection = connection;
                    this.localAxis = localAxis;
                    this.collisionSphere = collisionSphere;
                    this.keyGenerator = keyGenerator;
                }
            }
            public class Path {
                public string collisionSphereName;
                public string bodyDataName;
                public int jointKey;
                public int collisionSphereKey;
                public Path init(){
                    return new Path();
                }
                public void setAll(string collisionSphereName,string bodyDataName,int jointKey,int collisionSphereKey){
                    this.collisionSphereName=collisionSphereName;
                    this.bodyDataName=bodyDataName;
                    this.jointKey=jointKey;
                    this.collisionSphereKey=collisionSphereKey;
                }

            }

            public class CollisionSphere {
                public Path path;
                public vec3 origin;
                public float radius;

                public CollisionSphere init(){
                    return new CollisionSphere();
                }
                public CollisionSphere get(){
                    return this;
                }
                public CollisionSphere setAll(vec3 origin,float radius){
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
                public void setAll(int a,int b,int c){
                    this.a = a;
                    this.b = b;
                    this.c = c;
                }
            }

            public class BodyMesh {
                public List<vec3> vertex;
                public List<Triangle> indices;

                public BodyMesh init(){
                    return new BodyMesh();
                }
                public void setAll(List<vec3> vertex,List<Triangle> indices){
                    this.vertex = vertex;
                    this.indices = indices;
                }
            }

            public class Timer{
                public float time;
                public Timer init(){
                    return new Timer();
                }
                public void setAll(float time){
                    this.time = time;
                }
                public void add(float time){
                    this.time += time;
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
