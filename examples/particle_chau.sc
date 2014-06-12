bucket 32 spiral

image {
   %% resolution 768 432
   resolution 1000 1000 
   aa 0 2
   filter gaussian
   jitter true
}
  
trace-depths {
	diff 4
	refl 8
	refr 8
}

shader {
   name grey 
   type diffuse
   diff 1 .5 .5
}

shader {
  name metal
  type phong
  diff 0.1 0.1 0.1
  spec 0.2 0.2 0.2 30
  samples 4
}

shader {
   name glass
   type glass
   eta 1.5
   color 0.5 0.05 0.5
   absorption.distance 5
}


photons {
  caustics 2000000 kd 200 0.5
}

gi {
  type igi
  samples 64         % number of virtual photons per set
  sets 6             % number of sets (increase this to translate shadow boundaries into noise)
  b 0.0000003          % bias - decrease this values until bright spots dissapear
  bias-samples 0     % set this >0 to make the algorithm unbiased
}


background {
    color 1 1 1
}



%% camera definitions
camera {
   type pinhole
   eye    30 -250 -75
   target 30 0 -75 
   up 0 0 1
   fov 50
   aspect 1
}


light  {
 type cornellbox
 corner0 -165, -115 -200
 corner1 170  220 135.0 
 left { "sRGB linear" 0.8 0.25 0.25 }
 right { "sRGB linear" 0.25 0.25 0.8 }
 top { "sRGB linear" 0.7 0.7 0.7 }
 bottom { "sRGB linear" 0.7 0.7 0.7 }
 back { "sRGB linear" 0.7 0.7 0.7 }
 emit { "sRGB linear" 25.0 25.0 25.0 }
 samples 32 
}

modifier {
name perlinName
type perlin
function 4
size 1
scale 10
}

object {
shader glass
%modifier perlinName
transform {
	rotatex -180
	scale 6 6 6
	translate 20 -40 -70
}
accel kdtree
type csharp-tesselatable
name fred
<code>
/*
 * Created by SharpDevelop.
 * User: D13439
 * Date: 01/05/2014
 * Time: 14:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

using SunflowSharp.Maths;
using SunflowSharp.Core.Primitive;

namespace SunflowSharp.Core.Tesselatable
{
	/// <summary>
	/// Description of FunctionPipe.
	/// </summary>
		public class LorenezParticle : ParticleSurface, ITesselatable
		{		
		 	
			float[] m =  {0.9f/7f, -3f/7f, 3.5f/7f, -2.7f/7f, 4f/7f, -2.4f/7f};
			float[] c =  {1f, 2.15f, 3.6f, 6.2f, 9f};
			
			float a, b;
			float dt;       // attractor parameters 
			BoundingBox bb = new BoundingBox();  
		 	
		 	public LorenezParticle () : base() {
	                // set the parameters that control the size of the pipe

				    a = 9.0f;
				    b = 14.286f;
				    dt = 0.5f;
				    
				    r = 0.02f;
				    r2 = r * r;
				    GetParticles() ;
				    
			}
	
			// calculate next attractor position
			public void GetParticles() 
			{
			    
			    int particlesCount = 100;
			    int particleIndex = 0;
			    
			    n = particlesCount;
			    
			    particles = new float[particlesCount * 3];

			 	float x, y, z;
			 	
			 	Vector3 step = new Vector3(); 
			 	
			 	x = y = z = 1;
			    
			    for (int i=0; i<particlesCount; i++) {
			    	
				    step.x = a * (y - sumChauFunction(x));
				    step.y = x - y + z;      
				    step.z = -b * y;    			    
				    step.mul(dt / step.Length()); 
				    
				    x += step.x;
				    y += step.y;
				    z += step.z; 
					
				    particles[particleIndex++] = x;
				    particles[particleIndex++] = y;
				    particles[particleIndex++] = z;
			    	
			    	    bb.include(x+r,y+r,z+r);
			    	    bb.include(x-r,y-r,z-r);
			    	
//				Console.WriteLine("particle {0}, {1}, {2}", x,y,z);
			    	
			    }
     
			}
			
		  float sumChauFunction(float x) {
   
    
		     float tmp = (float)(((m[0] - m[1]) *  (Math.Abs(x+c[0]) - Math.Abs(x-c[0]))) +
				 ((m[1] - m[2]) *  (Math.Abs(x+c[1]) - Math.Abs(x-c[1]))) +
				 ((m[2] - m[3]) *  (Math.Abs(x+c[2]) - Math.Abs(x-c[2]))) +
				 ((m[3] - m[4]) *  (Math.Abs(x+c[3]) - Math.Abs(x-c[3]))) +
				 ((m[4] - m[5]) *  (Math.Abs(x+c[4]) - Math.Abs(x-c[4]))));
		
				return (m[5] * x) + (0.5f * tmp);
                
      
      		  }
      		  

			public BoundingBox GetWorldBounds(Matrix4 o2w) {
				
				if (o2w == null)
					return bb;
				return o2w.transform(bb);

			}

			public PrimitiveList Tesselate() {
				return this;
			}	
			
			public bool Update(ParameterList pl, SunflowAPI api) {
				return true;
			}
	
	}
	

}

</code>
}


