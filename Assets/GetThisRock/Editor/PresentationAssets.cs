using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace GetThisRock.Editor
{
    public static class PresentationAssets
    {
        const string Root="Assets/GetThisRock/Art";
        public static void Generate(GameContent content)
        {
            Directory.CreateDirectory(Root);
            content.toolMaterial=Mat("Tool metal",new Color(.95f,.6f,.12f));
            var frame=Mat("Tablet metal",new Color(.055f,.065f,.075f));
            var glass=Mat("Tablet glass",new Color(.025f,.04f,.06f));
            var rubber=Mat("Tablet rubber",new Color(.12f,.14f,.15f));
            var glove=Mat("Work glove",new Color(.8f,.54f,.23f));
            var cuff=Mat("Work cuff",new Color(.13f,.25f,.3f));
            var tablet=new GameObject("RockCo Tablet"); tablet.layer=2;
            Part(tablet,"Body",MeshAsset("TabletBody",.96f,.64f,.045f,.04f,.006f),frame,Vector3.zero);
            Part(tablet,"Screen",MeshAsset("TabletScreen",.89f,.55f,.004f,.018f,.001f),glass,new Vector3(0,0,-.026f));
            Part(tablet,"Power button",MeshAsset("PowerButton",.012f,.065f,.017f,.005f,.002f),rubber,new Vector3(.482f,.13f,0));
            Part(tablet,"USB-C port",MeshAsset("USBPort",.047f,.009f,.01f,.004f,.001f),glass,new Vector3(0,-.319f,0));
            for(int i=0;i<6;i++) Part(tablet,"Speaker",MeshAsset("SpeakerSlot",.023f,.003f,.004f,.001f,.0005f),rubber,new Vector3(-.34f+i*.035f,.298f,-.024f));
            Part(tablet,"Camera",MeshAsset("CameraLens",.014f,.014f,.006f,.006f,.001f),glass,new Vector3(.33f,.298f,-.024f));
            var brand=new GameObject("Brand"); brand.transform.SetParent(tablet.transform,false); brand.transform.localPosition=new Vector3(0,0,.027f); brand.transform.localRotation=Quaternion.Euler(0,180,0);
            var text=brand.AddComponent<TextMesh>(); text.text="ROCK CO."; text.fontSize=48; text.characterSize=.002f; text.anchor=TextAnchor.MiddleCenter;
            content.tabletPrefab=PrefabUtility.SaveAsPrefabAsset(tablet,Root+"/RockCoTablet.prefab"); Object.DestroyImmediate(tablet);
            var hand=new GameObject("Work Glove Hand"); hand.layer=2;
            Part(hand,"Palm",MeshAsset("GlovePalm",.115f,.13f,.055f,.027f,.008f),glove,Vector3.zero);
            Part(hand,"Cuff",MeshAsset("GloveCuff",.095f,.13f,.065f,.015f,.006f),cuff,new Vector3(0,-.117f,.01f));
            for(int i=0;i<4;i++) Part(hand,"Finger "+i,MeshAsset("GloveFinger"+i,.023f,.078f-i*.006f,.033f,.01f,.005f),glove,new Vector3(-.043f+i*.028f,.092f-i*.003f,0));
            var thumb=Part(hand,"Thumb",MeshAsset("GloveThumb",.033f,.067f,.035f,.014f,.006f),glove,new Vector3(-.071f,.015f,0)); thumb.transform.localRotation=Quaternion.Euler(0,0,34);
            content.handPrefab=PrefabUtility.SaveAsPrefabAsset(hand,Root+"/WorkGloveHand.prefab"); Object.DestroyImmediate(hand);
            EditorUtility.SetDirty(content);
        }
        static Material Mat(string name,Color color)
        {
            string path=Root+"/"+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){ mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path); }
            mat.SetColor("_BaseColor",color); mat.SetFloat("_Smoothness",.32f); EditorUtility.SetDirty(mat); return mat;
        }
        static GameObject Part(GameObject parent,string name,Mesh mesh,Material material,Vector3 position)
        {
            var go=new GameObject(name); go.layer=2; go.transform.SetParent(parent.transform,false); go.transform.localPosition=position;
            go.AddComponent<MeshFilter>().sharedMesh=mesh; go.AddComponent<MeshRenderer>().sharedMaterial=material; return go;
        }
        static Mesh MeshAsset(string name,float width,float height,float depth,float radius,float bevel)
        {
            string path=Root+"/"+name+".asset"; var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path); if(mesh!=null)return mesh;
            var vertices=new List<Vector3>(); var triangles=new List<int>(); const int steps=8,ringSize=32;
            for(int ring=0;ring<4;ring++)
            {
                float inset=ring==0||ring==3?bevel:0;
                float w=width*.5f-inset,h=height*.5f-inset,r=Mathf.Max(.0001f,radius-inset);
                float z=ring==0?-depth*.5f:ring==1?-depth*.5f+bevel:ring==2?depth*.5f-bevel:depth*.5f;
                for(int corner=0;corner<4;corner++)for(int j=0;j<steps;j++)
                {
                    float angle=(corner*90+j*90f/(steps-1))*Mathf.Deg2Rad;
                    float cx=(corner==0||corner==3?1:-1)*(w-r), cy=(corner<2?1:-1)*(h-r);
                    vertices.Add(new Vector3(cx+Mathf.Cos(angle)*r,cy+Mathf.Sin(angle)*r,z));
                }
            }
            for(int ring=0;ring<3;ring++)for(int i=0;i<ringSize;i++)
            { int a=ring*ringSize+i,b=ring*ringSize+(i+1)%ringSize,c=b+ringSize,d=a+ringSize; triangles.AddRange(new[]{a,b,c,a,c,d}); }
            int front=vertices.Count; vertices.Add(new Vector3(0,0,-depth*.5f)); int back=vertices.Count; vertices.Add(new Vector3(0,0,depth*.5f));
            for(int i=0;i<ringSize;i++){ int n=(i+1)%ringSize; triangles.AddRange(new[]{front,n,i,back,3*ringSize+i,3*ringSize+n}); }
            mesh=new Mesh{name=name}; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh,path); return mesh;
        }
    }
}
