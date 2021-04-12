using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ConeCreatorEditor : MonoBehaviour
{

    [MenuItem("GameObject/3D Object/Cube", false, priority = 7)]
    public static void CreateCone()
    {
        SpawnConeInHierarchy();
    }


    private static void SetMesh(GameObject go)
    {
        if (null == go)
            return;
        // Imitating Cylinder parameter
        float myRadius = 0.5f;
        int myAngleStep = 20;
        Vector3 myTopCenter = new Vector3(0, 1, 0);
        Vector3 myBottomCenter = Vector3.zero;
        // Constructing Vertex Array and UV Array
        Vector3[] myVertices = new Vector3[360 / myAngleStep * 2 + 2];
        //
        Vector2[] myUV = new Vector2[myVertices.Length];
        // Here I put the vertex of the cone at the end of the vertex array.
        myVertices[0] = myBottomCenter;
        myVertices[myVertices.Length - 1] = myTopCenter;
        myUV[0] = new Vector2(0.5f, 0.5f);
        myUV[myVertices.Length - 1] = new Vector2(0.5f, 0.5f);
        // Because the coordinates of the vertices on the circle are the same, but the indexes are different, the general length of the loop can be used here.
        for (int i = 1; i <= (myVertices.Length - 2) / 2; i++)
        {
            float curAngle = i * myAngleStep * Mathf.Deg2Rad;
            float curX = myRadius * Mathf.Cos(curAngle);
            float curZ = myRadius * Mathf.Sin(curAngle);
            myVertices[i] = myVertices[i + (myVertices.Length - 2) / 2] = new Vector3(curX, 0, curZ);
            myUV[i] = myUV[i + (myVertices.Length - 2) / 2] = new Vector2(curX + 0.5f, curZ + 0.5f);

        }
        // Constructing triangular arrays
        int[] myTriangle = new int[(myVertices.Length - 2) * 3];
        for (int i = 0; i <= myTriangle.Length - 3; i = i + 3)
        {
            if (i + 2 < myTriangle.Length / 2)
            {
                myTriangle[i] = 0;
                myTriangle[i + 1] = i / 3 + 1;
                myTriangle[i + 2] = i + 2 == myTriangle.Length / 2 - 1 ? 1 : i / 3 + 2;
            }
            else
            {
                // Draw the cone part. The index group starts at the tip of the cone.
                myTriangle[i] = myVertices.Length - 1;
                // The index value of the middle vertex of the last triangle of the cone is 19
                myTriangle[i + 1] = i == myTriangle.Length - 3 ? 19 : i / 3 + 2;
                myTriangle[i + 2] = i / 3 + 1;
            }
        }

        // Constructing mesh
        Mesh myMesh = new Mesh();
        myMesh.name = "Cone";
        myMesh.vertices = myVertices;
        myMesh.triangles = myTriangle;
        myMesh.uv = myUV;
        myMesh.RecalculateBounds();
        myMesh.RecalculateNormals();
        myMesh.RecalculateTangents();
        // Allocate mesh
        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = myMesh;
        // Distribution material
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        Material myMat = new Material(Shader.Find("Standard"));
        mr.sharedMaterial = myMat;
    }

    private static void SpawnConeInHierarchy()
    {
        Transform[] selections = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.ExcludePrefab);

        if (selections.Length <= 0)
        {
            GameObject cone = new GameObject("Cube");
            cone.transform.position = Vector3.zero;
            cone.transform.rotation = Quaternion.identity;
            cone.transform.localScale = Vector3.one;
            // Setting the creation operation to be revocable
            Undo.RegisterCreatedObjectUndo(cone, "Undo Creating Cube");
            SetMesh(cone);
            return;
        }

        foreach (Transform selection in selections)
        {
            GameObject cone = new GameObject("Cube");
            cone.transform.SetParent(selection);
            cone.transform.localPosition = Vector3.zero;
            cone.transform.localRotation = Quaternion.identity;
            cone.transform.localScale = Vector3.one;
            // Setting the creation operation to be revocable
            Undo.RegisterCreatedObjectUndo(cone, "Undo Creating Cube");
            SetMesh(cone);
        }
    }
}
