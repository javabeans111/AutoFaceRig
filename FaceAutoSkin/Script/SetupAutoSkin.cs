using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupAutoSkin : MonoBehaviour
{
    public Transform parentNode;
    public Transform meshNode;
    public  int vertNum;
    private Vector3[] vertDirs;
    private Vector3[] boneDirs;
    private Vector3[] boneOldPos;
    private Vector3[] boneNewPos;
    private Vector3[] boneOffsets;

    public int boneNum;

    public  ComputeShader computeVertsBone;
    private ComputeBuffer vertDirs_Buffer;
    private ComputeBuffer boneDirs_Buffer;
    private ComputeBuffer boneOffsets_Buffer;
    private ComputeBuffer vertsData_Buffer;
    private int computeVertsBone_id;

    public Material mat;

    struct PerVertex
    {
        public int b0, b1, b2, b3;
        public float w0, w1, w2, w3;
    }
    private PerVertex[] vertsData;

    void CollectBoneInfo(Transform thenode)
    {
        Vector3 parentNodePos = parentNode.position;
        boneNum = thenode.childCount; Debug.Log("boneNum"+ boneNum);
        boneDirs = new Vector3[boneNum];
        boneOldPos = new Vector3[boneNum];
        for (int i = 0; i < thenode.childCount; i++)
        {
            boneDirs[i] = Vector3.Normalize(thenode.GetChild(i).position - parentNodePos);
            boneDirs[i] = (thenode.GetChild(i).position - parentNodePos);
            boneOldPos[i] = thenode.GetChild(i).position;
           // Debug.Log(boneDirs[i]);
        }

    }


    void CollectMeshInfo(Transform theMeshNode)
    {
        Vector3 parentNodePos = parentNode.position;
        Mesh m = theMeshNode.GetComponent<MeshFilter>().mesh;
        vertNum = m.vertexCount; ; Debug.Log("vertNum" + vertNum);
        vertDirs = new Vector3[vertNum];
        Vector3[] vertss = m.vertices;
        for (int k = 0; k < vertNum; k++)
        {
            Vector3 worldPos = theMeshNode.TransformPoint(vertss[k]);
            //vertDirs[i] = Vector3.Normalize(worldPos - parentNodePos);
            vertDirs[k] = (worldPos - parentNodePos);
            //Debug.Log(vertDirs[k]);
        }
    }

    void initBuffer()
    {

        vertDirs_Buffer = new ComputeBuffer(vertNum, sizeof(float) * 3);
        vertDirs_Buffer.SetData(vertDirs);
        boneDirs_Buffer = new ComputeBuffer(boneNum, sizeof(float) * 3);
        boneDirs_Buffer.SetData(boneDirs);
        vertsData = new PerVertex[vertNum];
        vertsData_Buffer = new ComputeBuffer(vertNum, sizeof(int) * 4 + sizeof(float) * 4);
        vertsData_Buffer.SetData(vertsData);

        boneOffsets = new Vector3[boneNum];
        boneOffsets_Buffer = new ComputeBuffer(boneNum,  sizeof(float) * 3);
        boneOffsets_Buffer.SetData(boneOffsets);

        computeVertsBone_id = computeVertsBone.FindKernel("GetMax4Weights");
        computeVertsBone.SetInt("bonesCount", boneNum);
        computeVertsBone.SetBuffer(computeVertsBone_id, "vertsDir", vertDirs_Buffer);
        computeVertsBone.SetBuffer(computeVertsBone_id, "bonesDir", boneDirs_Buffer);
        computeVertsBone.SetBuffer(computeVertsBone_id, "vertsData", vertsData_Buffer);

        computeVertsBone.Dispatch(computeVertsBone_id, vertNum, 1, 1);

        vertsData_Buffer.GetData(vertsData);

        for (int j = 0; j < vertNum; j++)
        {
            Debug.Log(j);
            Debug.Log(new Vector4 (vertsData[j].b0, vertsData[j].b1,vertsData[j].b2,vertsData[j].b3));
            Debug.Log(new Vector4 (vertsData[j].w0, vertsData[j].w1, vertsData[j].w2, vertsData[j].w3));
        }
    }

    void updateBoneOffsets()
    {
        mat.SetBuffer("vertsDataA", vertsData_Buffer);
        vertsData_Buffer.SetData(vertsData);
               

        for (int i = 0; i < parentNode.childCount; i++)
        {
            
            boneOffsets[i] = parentNode.GetChild(i).position - boneOldPos[i];
            //Debug.Log("boneOffsets" + boneOffsets[i]);

        }
        mat.SetBuffer("boneOffsetsA", boneOffsets_Buffer);
        boneOffsets_Buffer.SetData(boneOffsets);
        

    }


    // Start is called before the first frame update
    void Start()
    {
        if (parentNode && meshNode)
        {
            Debug.Log("ok");
            CollectBoneInfo(parentNode);
            CollectMeshInfo(meshNode);
            initBuffer();
        }

    }

    // Update is called once per frame
    void Update()
    {
        updateBoneOffsets();
    }

    void OnDestroy()
    {
        vertDirs_Buffer.Release();
        vertDirs_Buffer.Dispose();
        boneDirs_Buffer.Release();
        boneDirs_Buffer.Dispose();
        vertsData_Buffer.Release();
        vertsData_Buffer.Dispose();
        boneOffsets_Buffer.Release();
        boneOffsets_Buffer.Dispose();

    }
}
