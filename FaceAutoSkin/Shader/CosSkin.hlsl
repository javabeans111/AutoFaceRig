#ifndef COSSKIN_INCLUDED
#define COSSKIN_INCLUDED
#pragma target 5.0

struct perVertex 
{
	int   bonesId[4];
	float bonesWeight[4];
};
StructuredBuffer<perVertex> vertsDataA;
StructuredBuffer<float3> boneOffsetsA;

void CosSkin_float(float vertid,float3 vert,out float3 outPos)
{   
    perVertex vdata = vertsDataA[(int)vertid];
    int boneIds[4] = vdata.bonesId;
    float boneWeights[4] = vdata.bonesWeight;
   
    float3 of0 = boneWeights[0]*boneOffsetsA[boneIds[0]];
    float3 of1 = boneWeights[1]*boneOffsetsA[boneIds[1]];
    float3 of2 = boneWeights[2]*boneOffsetsA[boneIds[2]];
    float3 of3 = boneWeights[3]*boneOffsetsA[boneIds[3]];
    outPos = vert ;
    outPos += of0 + of1 + of2 + of3 ;

}

#endif