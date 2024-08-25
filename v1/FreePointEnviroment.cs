using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unigine;

[Component(PropertyGuid = "11c8627ecaa24277eefccff039dede7767f5b9cc")]
public class FreePointEnviroment : Component
{
    public class Library {

        public class OctTree {
            public OctTree octTree;
            public int depth;
            public vec3 origin;
            public List<CollisionSphere> root;
            public List<CollisionSphere> a,b,c,d;
            public List<CollisionSphere> e,f,g,h;
        }
        
        public class World {
            public int worldKey;
            public Body[] bodiesInWorld;
            public OctTree octTree;
            public KeyGenerator keyGenerator;
        }

        public class Axis {
            public vec3 origin,x,y,z;
            public float distance;

            public Axis(){}
            public Axis(vec3 origin, float distance){
                this.origin = origin;
                this.distance = distance;
                x = origin + new vec3(1,0,0)*distance;
                y = origin + new vec3(0,1,0)*distance;
                z = origin + new vec3(0,0,1)*distance;
            }

            public void moveAxis(vec3 add){
                origin += add;
                x += add;
                y += add;
                z += add;
            }
            public void setAxis(vec3 newOrigin){
                vec3 newPosition = newOrigin-origin;
                moveAxis(newPosition);
            }
            public void scale(float newDistance){
                if (newDistance > 0){
                    distance = newDistance;
                    x = origin + distanceFromOrign(x,origin);
                    y = origin + distanceFromOrign(y,origin);
                    z = origin + distanceFromOrign(z,origin);
                }
            }
            public vec3 direction(vec3 point,vec3 origin){ 
                vec3 v = point-origin;
                return v/ MathLib.Length(v);
            }
            public vec3 distanceFromOrign(vec3 point,vec3 origin){
                return direction(point,origin)*distance;
            }
        }

        public class KeyGenerator{
            public int maxKeys;
            public int availableKeys;
            public int increaseKeysBy;
            public List<int> freeKeys;

            public KeyGenerator(){}
            public KeyGenerator(int amountOfKeys){
                freeKeys = new List<int>();
                increaseKeysBy = amountOfKeys;
                generateKeys();
                maxKeys = amountOfKeys;
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
                int key = freeKeys[0];
                freeKeys.RemoveAt(0);
                availableKeys -= 1;
                return key;
            }
            public void returnKey(int key){
                freeKeys.Add(key);
                availableKeys +=1;
            }
            public void resetGenerator(int maxKeys){
                freeKeys.Clear();
                this.maxKeys = maxKeys;
                availableKeys = 0;
            }
        }

        public class Body {
            public World world;
            public int worldKey;
            public Axis globalAxis;
            public Joint[] bodyStructure;
            public KeyGenerator keyGenerator;

            public Body(){}
            public Body(int worldKey, Axis globalAxis, int amountOfJoints){
                this.worldKey = worldKey;
                this.globalAxis = globalAxis;
                bodyStructure = new Joint[amountOfJoints];
                keyGenerator = new KeyGenerator(amountOfJoints);
            }

            public void setAll(int worldKey,Axis globalAxis,Joint[] bodyStructure,KeyGenerator keyGenerator){
                this.worldKey = worldKey;
                this.globalAxis = globalAxis;
                this.bodyStructure = bodyStructure;
                this.keyGenerator = keyGenerator;
            }
            public Joint getJoint(int key){
                return bodyStructure[key];
            }
            void resizeJoints(){
                if(keyGenerator.availableKeys == 0) {
                    keyGenerator.generateKeys();
                    int max = keyGenerator.maxKeys;
                    int newSize = max + keyGenerator.increaseKeysBy;
                    Joint[] newJointArray = new Joint[newSize];
                    for (int i = 0; i<max; i++){
                        Joint joint = bodyStructure[i];
                        if (joint != null){
                            newJointArray[i] = joint;
                        }
                    }
                }
            }
            public void addJoint(Joint joint){
                resizeJoints();
                int key = keyGenerator.getKey();
                bodyStructure[key] = joint;
            }
            public void deleteJoint(int key){
                Joint remove = bodyStructure[key];
                if(remove != null){
                    keyGenerator.returnKey(key);
                    bodyStructure[key] = null;
                }
            }
            void keyManager(
                Joint[] joints, int maxKeys, int availableKeys, 
                out int count, out int?[] newKeys, out Joint[] orginizedJoints
                ){
                int index = 0;
                int?[] keys = new int?[maxKeys];
                Joint[] newJoints = new Joint[maxKeys - availableKeys];
                for (int i = 0; i<maxKeys; i++){
                    Joint joint = joints[i];
                    if (joint != null){
                        keys[joint.connection.current] = index;
                        joint.connection.setCurrent(index);
                        newJoints[index] = joint;
                        index++;
                    }
                }
                count = index;
                newKeys = keys;
                orginizedJoints = newJoints;
            }
            public void optimizeBody(){
                int count;
                int?[] newKeys;
                Joint[] joints;
                keyManager(
                    bodyStructure, keyGenerator.maxKeys, keyGenerator.availableKeys,
                    out count, out newKeys, out joints
                    );
                int jointCount = 0;
                for (int i = 0; i<count; i++){
                    Joint joint = joints[i];
                    if (joint != null){
                        joint.connection.replaceConnections(newKeys);
                        joint.optimizeCollisionSpheres();
                        joints[jointCount] = joint;
                        jointCount++;
                    } 
                }
                bodyStructure = joints;
                keyGenerator.resetGenerator(count);
            }
        }

        public class Connection {
            public int current;
            public List<int> past; 
            public List<int> future;

            public Connection(){}
            public Connection(int current, List<int> past,List<int> future){
                this.current = current;
                this.past = past;
                this.future = future;
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
            void addKeys(List<int> connection, int?[] keyManager, out List<int> newKeys){
                List<int> newConnection = new List<int>();
                for (int i = 0; i < connection.Count; i++) {
                    int? index = keyManager[connection[i]];
                    if (index != null){
                        newConnection.Add((int)index);
                    }
                }
                newKeys = newConnection;
            }
            public void replaceConnections(int?[] keyManager){
                List<int> newConnection;
                addKeys(past, keyManager, out newConnection);
                past = newConnection;

                newConnection.Clear();
                addKeys(future, keyManager, out newConnection);
                future = newConnection;
            }
        }

        public class Joint {
            public Body body;
            public Axis localAxis;
            public Connection connection;
            public CollisionSphere[] collisionSpheres;
            public KeyGenerator keyGenerator;

            public Joint(){}
            public Joint(int amountOfKeys, Axis localAxis,Connection connection){
                collisionSpheres = new CollisionSphere[amountOfKeys];
                keyGenerator = new KeyGenerator(amountOfKeys);
                this.localAxis = localAxis;
                this.connection = connection;
            }

            public void setAll(Connection connection,Axis localAxis,CollisionSphere[] collisionSpheres,KeyGenerator keyGenerator){
                this.connection = connection;
                this.localAxis = localAxis;
                this.collisionSpheres = collisionSpheres;
                this.keyGenerator = keyGenerator;
            }
            public void optimizeCollisionSpheres(){
                int maxKeys = keyGenerator.maxKeys;
                int used = keyGenerator.availableKeys;
                CollisionSphere[] newCollision = new CollisionSphere[used];
                int collisionCount = 0;
                for (int j = 0; j<maxKeys; j++){
                    CollisionSphere collision = collisionSpheres[j];
                    if (collision != null){
                        collision.path.setBodyController(body);
                        collision.path.setJointKey(connection.current);
                        collision.path.setCollisionSphereKey(collisionCount);
                        newCollision[collisionCount] = collision;
                        collisionCount++;
                    }
                }
                collisionSpheres = newCollision;
                keyGenerator.resetGenerator(collisionCount);
            }
        }

        public class Path {
            public Body body;
            public int jointKey;
            public int collisionSphereKey;

            public Path(){}
            public Path(Body body, int jointKey, int collisionSphereKey){
                this.body=body;
                this.jointKey=jointKey;
                this.collisionSphereKey=collisionSphereKey;
            }

            public void setBodyController(Body body){
                this.body=body;
            }
            public void setJointKey(int jointKey){
                this.jointKey = jointKey;
            }
            public void setCollisionSphereKey(int collisionSphereKey){
                this.collisionSphereKey = collisionSphereKey;
            }
            public void setAll(Body body,int jointKey,int collisionSphereKey){
                this.body=body;
                this.jointKey=jointKey;
                this.collisionSphereKey=collisionSphereKey;
            }
        }

        public class CollisionSphere {
            public Path path;
            public vec3 origin;
            public float radius;

            public CollisionSphere(){}
            public CollisionSphere(Path path,vec3 origin,float radius){
                this.path = path;
                this.origin = origin;
                this.radius = radius;
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

    class Test:Library {
        public void init(){

        }
    }
    Library.Body v;
	void Init()
	{
        // v = world.bodyInWorld.bodyData.init(1);
        // Log.Message($"{v.keyGenerator.freeKeys[0]}\n");
        
	}
    float lol =0;
	void Update()
	{
        // lol += Game.IFps;
        // if (lol>5) v.DeleteForce();
		// write here code to be called before updating each render frame
	}
}
