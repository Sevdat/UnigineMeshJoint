using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unigine;

[Component(PropertyGuid = "11c8627ecaa24277eefccff039dede7767f5b9cc")]
public class FreePointEnviroment : Component
{
    public class Library {

        public class SphericalOctTree {
            public SphericalOctTree sphereOctTree;
            public int depth;
            public vec3 origin;
            public List<CollisionSphere> root;
            public List<CollisionSphere> a,b,c,d;
            public List<CollisionSphere> e,f,g,h;
        }

        public class World {
            public Body[] bodiesInWorld;
            public SphericalOctTree sphereOctTree;
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
                    freeKeys.Add(maxKeys+increaseKeysBy-i);
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
                int key = freeKeys[availableKeys];
                freeKeys.RemoveAt(availableKeys);
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

           public void resizeJoints(int amount){
                int availableKeys = keyGenerator.availableKeys;
                int maxKeys = keyGenerator.maxKeys;
                int limitCheck = availableKeys + amount;
                if(limitCheck > maxKeys) {
                    keyGenerator.setLimit(limitCheck - availableKeys);
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
            public void returnJointKey(int key){
                Joint remove = bodyStructure[key];
                if(remove != null){
                    keyGenerator.returnKey(key);
                    bodyStructure[key] = null;
                }
            }
            void jointOrginizer(
                Joint[] joints, int maxKeys, int availableKeys, 
                out int existingJoints, out int?[] orginizedKeys, out Joint[] orginizedJoints
                ){
                int count = 0;
                int?[] keys = new int?[maxKeys];
                Joint[] newJoints = new Joint[maxKeys - availableKeys];
                for (int i = 0; i<maxKeys; i++){
                    Joint joint = joints[i];
                    if (joint != null){
                        keys[joint.connection.current] = count;
                        newJoints[count] = joint;
                        count++;
                    }
                }
                existingJoints = count;
                orginizedKeys = keys;
                orginizedJoints = newJoints;
            }
            public void optimizeBody(){
                int existingJoints;
                int?[] orginizedKeys;
                Joint[] orginizedJoints;
                jointOrginizer(
                    bodyStructure, keyGenerator.maxKeys, keyGenerator.availableKeys,
                    out existingJoints, out orginizedKeys, out orginizedJoints
                    );
                int smallestKey = 0;
                for (int i = 0; i<existingJoints; i++){
                    Joint joint = orginizedJoints[i];
                    joint.connection.replaceConnections(orginizedKeys,smallestKey);
                    joint.optimizeCollisionSpheres();
                }
                bodyStructure = orginizedJoints;
                keyGenerator.resetGenerator(existingJoints);
            }
        }

        public class Connection {
            public int current;
            public List<Joint> past; 
            public List<Joint> future;

            public Connection(){}
            public Connection(int current, List<Joint> past,List<Joint> future){
                this.current = current;
                this.past = past;
                this.future = future;
            }
            public void setCurrent(int current){
                this.current = current;
            }
            public void setPast(List<Joint> past){
                this.past = past;
            }
            public void setFuture(List<Joint> future){
                this.future = future;
            }
            public void replaceConnections(int?[] keyManager, int smallestKey){
                setCurrent((int)keyManager[current - smallestKey]);
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

            public void setBody(Body body){
                this.body=body;
            }
            public void getFutureConnections(
                out List<Joint> connectionTree, 
                out List<Joint> connectionEnd,
                out int treeSize,out int biggestKey,out int smallestKey
                ){
                bool futureOnly = true;
                connectionTracker(
                    futureOnly,
                    out List<Joint> tree, out List<Joint> end,
                    out int size, out int biggest, out int smallest
                    );
                connectionTree = tree;
                connectionEnd = end;
                treeSize = size;
                biggestKey = biggest;
                smallestKey = smallest;
            }
            public void getPastConnections(
                out List<Joint> connectionTree, 
                out List<Joint> connectionEnd,
                out int treeSize,out int biggestKey,out int smallestKey
                ){
                bool pastOnly = false;
                connectionTracker(
                    pastOnly,
                    out List<Joint> tree, out List<Joint> end,
                    out int size, out int biggest, out int smallest
                    );
                connectionTree = tree;
                connectionEnd = end;
                treeSize = size;
                biggestKey = biggest;
                smallestKey = smallest;
            }
            public void connectJointTo(Joint newJoint){
                getFutureConnections( 
                    out List<Joint> connectionTree,
                    out List<Joint> connectionEnds,
                    out int treeSize,out int biggestKey,out int smallestKey
                    );
                if (body != newJoint.body){
                    newJoint.body.resizeJoints(treeSize);

                    int?[] newKeys = new int?[biggestKey-smallestKey];
                    for (int i = 0; i<treeSize;i++){
                        newKeys[connectionTree[i].connection.current - smallestKey] = keyGenerator.getKey();
                    }
                    disconnectPast();
                    connectPastTo(newJoint);
                    for (int i =0; i< treeSize;i++){
                        Joint joint = connectionTree[i];
                        joint.body.returnJointKey(joint.connection.current);
                        joint.connection.replaceConnections(newKeys,smallestKey);
                        joint.setBody(newJoint.body);
                        newJoint.body.bodyStructure[joint.connection.current] = joint;
                    }   
                } else if (!connectionTree.Contains(newJoint)) {
                    disconnectPast();
                    connectPastTo(newJoint);
                }
            }
            void connectFutureTo(Joint joint){
                List<Joint> connectTo = joint.connection.past;
                if (!connectTo.Contains(this)) connectTo.Add(this);
                if (!connection.future.Contains(joint)) connection.future.Add(joint);
            }
            void connectPastTo(Joint joint){
                List<Joint> connectTo = joint.connection.future;
                if (!connectTo.Contains(this)) connectTo.Add(this);
                if (!connection.past.Contains(joint)) connection.past.Add(joint);
            }
            public void disconnectFuture(){
                bool futureOnly = true;
                disconnect(connection.future,futureOnly);
                connection.future.Clear();
            }
            public void disconnectPast(){
                bool pastOnly = false;
                disconnect(connection.past,pastOnly);
                connection.past.Clear();
            }
            void disconnect(List<Joint> joints, bool pastOrFuture){
                int size = joints.Count;
                if (pastOrFuture) 
                    for (int i =0; i<size;i++){
                        joints[i].connection.future.Remove(this);
                    }
                 else 
                    for (int i =0; i<size;i++){
                        joints[i].connection.past.Remove(this);
                    }
            }
            void connectionTracker(
                bool pastOrFuture,
                out List<Joint> connectionTree, out List<Joint> connectionEnd,
                out int treeSize, out int biggestKey,out int smallestKey
                ){
                Joint[] joints = body.bodyStructure;
                List<Joint> tree = new List<Joint>{this};  
                if (pastOrFuture) 
                    tree.AddRange(connection.future); 
                    else tree.AddRange(connection.past);               
                int size = tree.Count;
                int biggest = 0;
                int smallest = connection.current;
                List<Joint> end = new List<Joint>();
                for (int i=0; i< size; i++){
                    List<Joint> tracker = pastOrFuture ?
                        joints[tree[i].connection.current].connection.future:
                        joints[tree[i].connection.current].connection.past;
                    int trackerSize = tracker.Count;
                    if (trackerSize > 0){
                        for(int e = 0; e < trackerSize; e++){
                            Joint joint = tracker[e];
                            int current = joint.connection.current;
                            if (current > biggest) biggest = current;
                            if (current < smallest) smallest = current;
                            tree.Add(joint);
                            size++;
                        };
                    } else {
                        end.Add(tree[i]);
                    }
                }
                connectionTree = tree;
                connectionEnd = end;
                treeSize = size;
                biggestKey = biggest;
                smallestKey = smallest;
            }
            public void optimizeCollisionSpheres(){
                int maxKeys = keyGenerator.maxKeys;
                int used = keyGenerator.availableKeys;
                CollisionSphere[] newCollision = new CollisionSphere[used];
                int collisionCount = 0;
                for (int j = 0; j<maxKeys; j++){
                    CollisionSphere collision = collisionSpheres[j];
                    if (collision != null){
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

            public void setBody(Body body){
                this.body=body;
            }
            public void setJointKey(int jointKey){
                this.jointKey = jointKey;
            }
            public void setCollisionSphereKey(int collisionSphereKey){
                this.collisionSphereKey = collisionSphereKey;
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
