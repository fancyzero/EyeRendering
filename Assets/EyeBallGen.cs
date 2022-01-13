using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBallGen : MonoBehaviour
{
    [Range(1,100)]
    public   float verticesDensity = 1;
	[Range(1,10)]
	public int smoothOrder;	
	[Range(0,0.5f)]
	public float bump;
	[Range(0,1.5f)]
	public float smooth;
	[Range(0.1f,2.0f)]
	public float eyeballRadius;
	float smoothstep(float edge0, float edge1, float x) {
	// Scale, bias and saturate x to 0..1 range
	//x = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f); 
	// Evaluate polynomial

	return generalSmoothStep( smoothOrder, x);
	//return x * x * (3 - 2 * x);
	}
	float GetRadius(float r, float angle)
	{
		float fix = 1.0f- generalSmoothStep(smoothOrder,Mathf.Clamp(angle/smooth, 0.0f, 1.0f));
		return r+fix*bump;

		
	}
float generalSmoothStep(int a, float x)
 { //Generalized smoothstep
 x = Mathf.Clamp(x, 0.0f, 1.0f);
  float result = 0;
  for (var n = 0; n <= a - 1; n++) {
    result += (pascalTriangle(-a, n) * pascalTriangle(2 * a - 1, a - n - 1) * Mathf.Pow(x, a + n));
  }
  return result;
}

int pascalTriangle(int a, int b){
  float result = 1; 

  for(float i = 1; i <= b; i++){
      
    result *= ((a - (i - 1)) / i);

    
  }

  return Mathf.FloorToInt(result);
}

void Start()
{
    Debug.Log(pascalTriangle(3,3));
}

    Vector3 GetVertexPos( float theta, float phi)
    {
				float radius = GetRadius(eyeballRadius, theta);
                float h = Mathf.Cos(theta);
                float r = Mathf.Sin(theta)*radius;
                
                Vector3 p = new Vector3(Mathf.Cos(phi)*r,Mathf.Sin(phi)*r, h*radius);
                return p;
    }
    // Start is called before the first frame update
    void Update()
    {
        Mesh mesh;
        mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();   
        List<Vector3> normals = new List<Vector3>();

        int a = 30;
        int b = 30;
        for ( int i = 0; i <=a; i++)
        {
            for (int j = 0; j < b; j++)
            {
                float progress = i/(float)(a);
                float theta = Mathf.Pow(progress, verticesDensity)*Mathf.PI;
                float phi = j/(float)(b)*Mathf.PI*2;
                Vector3 p = GetVertexPos(theta, phi);
                vertices.Add(p);
                float delta = 0.01f;
                Vector3 p1 = GetVertexPos(theta+delta, phi)-p;
                //Vector3 p2 = GetVertexPos(theta-0.01f, phi);
                Vector3 p3 = GetVertexPos(theta, phi+delta)-p;
                float p3mag = p3.magnitude;
                Vector3 n = Vector3.Cross(p1.normalized,p3.normalized ).normalized;
                //Vector3 p4 = GetVertexPos(theta, phi-0.01f);
                if (p3mag<= float.Epsilon )
                {
                    if ( i == 0 )
                        n = new Vector3(0,0,1);
                    else
                        n = new Vector3(0,0,-1);
                }
                if( n.magnitude <= 0.0f)
                    Debug.DebugBreak();
                normals.Add(n);
            }
        }
        mesh.vertices = vertices.ToArray();
        List<int> indices = new List<int>();
        
        for ( int i = 0 ; i < a; i++)
        for ( int j = 0; j < b; j++)
        {
            int r = i*a;

            int q1 = (r+j);
            int q2 = q1 + 1;
            int q3 = (r+a+j);
            int q4 = q3 + 1;

            if (q2 >= (i+1)*a )
                q2 = i*a;
            if (q4 >= (i+2)*a )
                q4 = (i+1)*a;                
            indices.Add(q3);
            indices.Add(q2);
            indices.Add(q1);

            indices.Add(q4);
            indices.Add(q2);
            indices.Add(q3);
        }
        mesh.normals = normals.ToArray();
        mesh.triangles =  indices.ToArray();

        GetComponent<MeshFilter>().mesh = mesh;

        
    }

    // Update is called once per frame

}