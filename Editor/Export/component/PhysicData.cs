using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

internal class PhysicData
{
    // 定义一个回调委托
    public delegate JSONObject GetMeshDataCallbackDelegate(Mesh mesh, Renderer renderer);

    // 定义一个回调变量
    public static GetMeshDataCallbackDelegate GetMeshDataCallback;


    // 解析移动限制
    private static Vector3 GetPositionConstraints(RigidbodyConstraints constraints)
    {
        return new Vector3(
            (constraints & RigidbodyConstraints.FreezePositionX) != 0 ? 1 : 0,
            (constraints & RigidbodyConstraints.FreezePositionY) != 0 ? 1 : 0,
            (constraints & RigidbodyConstraints.FreezePositionZ) != 0 ? 1 : 0
        );
    }

    // 解析旋转限制
    private static Vector3 GetRotationConstraints(RigidbodyConstraints constraints)
    {
        return new Vector3(
            (constraints & RigidbodyConstraints.FreezeRotationX) != 0 ? 1 : 0,
            (constraints & RigidbodyConstraints.FreezeRotationY) != 0 ? 1 : 0,
            (constraints & RigidbodyConstraints.FreezeRotationZ) != 0 ? 1 : 0
        );
    }

    public static JSONObject GetRigidbodyComponentData(Rigidbody rigidbody, bool isOverride)
    {
        JSONObject compData = new JSONObject(JSONObject.Type.OBJECT);

        compData.AddField("collisionGroup", 1 << rigidbody.gameObject.layer);
        compData.AddField("canCollideWith", 0);

        compData.AddField("mass", rigidbody.mass);
        compData.AddField("isKinematic", rigidbody.isKinematic);
        compData.AddField("linearDamping", rigidbody.drag);
        compData.AddField("angularDamping", rigidbody.angularDrag);
        compData.AddField("useGravity", rigidbody.useGravity);

        Vector3 positionConstraints = GetPositionConstraints(rigidbody.constraints);
        Vector3 rotationConstraints = GetRotationConstraints(rigidbody.constraints);
        compData.AddField("linearFactor", JsonUtils.GetVector3Object(new Vector3(positionConstraints.x == 0 ? 1 : 0, positionConstraints.y == 0 ? 1 : 0, positionConstraints.z == 0 ? 1 : 0)));
        compData.AddField("angularFactor", JsonUtils.GetVector3Object(new Vector3(rotationConstraints.x == 0 ? 1 : 0, rotationConstraints.y == 0 ? 1 : 0, rotationConstraints.z == 0 ? 1 : 0)));

        compData.AddField(isOverride ? "_$override" : "_$type", "Rigidbody3D");

        return compData;
    }

    public static JSONObject GetColliderComponentData(Collider collider, bool isOverride)
    {
        JSONObject compData = new JSONObject(JSONObject.Type.OBJECT);
        JSONObject colliderShape = new JSONObject(JSONObject.Type.OBJECT);
        compData.AddField("colliderShape", colliderShape);
        compData.AddField("collisionGroup", 1 << collider.gameObject.layer);
        compData.AddField("canCollideWith", 0);

        compData.AddField("isTrigger", collider.isTrigger);

        if (collider is BoxCollider)
        {
            BoxCollider boxCollider = collider as BoxCollider;
            colliderShape.AddField("_$type", "BoxColliderShape");
            colliderShape.AddField("localOffset", JsonUtils.GetVector3Object(boxCollider.center));
            colliderShape.AddField("size", JsonUtils.GetVector3Object(boxCollider.size));
        }
        else if (collider is SphereCollider)
        {
            SphereCollider sphereCollider = collider as SphereCollider;
            colliderShape.AddField("_$type", "SphereColliderShape");
            colliderShape.AddField("localOffset", JsonUtils.GetVector3Object(sphereCollider.center));
            colliderShape.AddField("radius", sphereCollider.radius);
        }
        else if (collider is CapsuleCollider)
        {
            CapsuleCollider capsuleCollider = collider as CapsuleCollider;
            colliderShape.AddField("_$type", "CapsuleColliderShape");
            colliderShape.AddField("localOffset", JsonUtils.GetVector3Object(capsuleCollider.center));
            colliderShape.AddField("radius", capsuleCollider.radius);
            colliderShape.AddField("length", capsuleCollider.height);
            colliderShape.AddField("orientation", capsuleCollider.direction);
        }
        else if (collider is MeshCollider)
        {
            MeshCollider meshCollider = collider as MeshCollider;
            colliderShape.AddField("_$type", "MeshColliderShape");
            colliderShape.AddField("mesh", GetMeshDataCallback(meshCollider.sharedMesh, null));
            colliderShape.AddField("convex", meshCollider.convex);
        }

        compData.AddField(isOverride ? "_$override" : "_$type", "PhysicsCollider");


        return compData;
    }
}