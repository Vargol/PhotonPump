using System;
using System.Collections;
using System.Collections.Generic;
using SunflowSharp.Core;
using SunflowSharp;
using SunflowSharp.Core.Display;
using SunflowSharp.Maths;

public struct Parameters {

        public Vector3 roll;
		public Vector3 pitch;
        public Vector3 yaw;

    public Point3 position;
    public Point3 localRotation;
	public Vector3 localScale;
	public int objectCount;
	public float angle;
	public float length;
	public float thickness;
	public string primitiveType;
}

public class LSystem {

	//	Stack<Parameters> parameterStack = new Stack<Parameters>();

	static private ArrayList _ruleList;
    const float d2r = (float)(Math.PI / 180.0);


	static void Main(string[] args)
    {
        LSystem ls = new LSystem();

        ls.Run(args);
    }


	public void Run(string[] args)
	{


		// Use this for initialization

		LParser lparser = new LParser();
			
		System.String[] rules = new System.String[9];
/*		
		rules[0] = "#define a 1";
		rules[1] = "#thickness 0.2";
		rules[2] = "#recursion 4";
		rules[3] = "#angle 22.5";
		rules[4] = @"#axiom ++++F";
			
//		rules[5] = "X -> F-[[X]+X]+F[+FX]-X";
		rules[5] = @"F -> FF-[-F+F+F]+[+F-F-F]";
		rules[5] = @"F -> FF&[&F^F^F]^[^F&F&F]";
		rules[6] = @"Y -> F-F";

*/			
		rules[0] = "#define p 3.14";
		rules[1] = "#thickness 0.2";
		rules[2] = "#recursion 3";
		rules[3] = "#angle 90";
		rules[4] = "#axiom A";
		rules[5] = "A -> B-F+CFC+F-D&F^D-F+&&CFC+F+B//";
		rules[6] = "B -> A&F^CFB^F^D^^-F-D^|F^B|FC^F^A//";
		rules[7] = "C -> |D^|F^B-F+C^F^A&&FA&F^C+F+B^F^D//";
		rules[8] = "D -> |CFB-F+B|FA&F^A&&FB-F+B|FC//";
	
		 
	 
	   
	 

			
		bool statusOK = lparser.ParseStringArray(rules); 

		_ruleList = 	lparser.RunLSystem();
		
	    Console.WriteLine("RULES: " + Rules.RuleListToString(_ruleList, lparser.GlobalParameters));	

		Parameters currentParameters = new Parameters();

		SunflowAPI sunflow = new SunflowAPI();
        //		SetupSunflow(sunflow);

        SetupSunflow(sunflow);

        currentParameters.roll = new Vector3(1.0f, 0.0f, 0.0f);
		currentParameters.pitch      = new Vector3(0.0f, 1.0f, 0.0f);
		currentParameters.yaw    = new Vector3(0.0f, 0.0f, 1.0f);
		currentParameters.position = new Point3(0.0f, 0.0f, 0.0f);
		currentParameters.primitiveType = "box";
		currentParameters.length = 2.0f;	
		currentParameters.angle = (float)lparser.Angle;
		currentParameters.thickness = (float)lparser.Thickness;
        currentParameters.localRotation = new Point3(0.0f, 0.0f, 0.0f);
        currentParameters.localScale = new Vector3(1.0f, currentParameters.thickness, currentParameters.thickness);

		currentParameters.objectCount = 0;
		sunflow.geometry(currentParameters.primitiveType + currentParameters.objectCount++, currentParameters.primitiveType);



		//	Vector3 scale = new Vector3(currentParameters.length1f, currentParameters.thickness , currentParameters.thickness);



		currentParameters.objectCount = interpretProduction(currentParameters, _ruleList, lparser, sunflow);

        sunflow.render(SunflowAPI.DEFAULT_OPTIONS, new FileDisplay("lsystem.png"));


		return;
		
	}


    public void AddNewObject(SunflowAPI sunflow, Parameters currentParameters) {


		// transformation we want is local, so scale, rotate then translate

		Matrix4 t = Matrix4.translation(currentParameters.position.x, currentParameters.position.y, currentParameters.position.z)
                           .multiply(getRotationMatrix(currentParameters)
						   .multiply(Matrix4.scale(currentParameters.localScale.x, currentParameters.localScale.y, currentParameters.localScale.z)
                   )
                           );



        sunflow.geometry(currentParameters.primitiveType + currentParameters.objectCount, currentParameters.primitiveType);
		sunflow.parameter("transform", t);
		sunflow.parameter("shaders", "sps");
		sunflow.instance(currentParameters.primitiveType + currentParameters.objectCount + ".instance", currentParameters.primitiveType + currentParameters.objectCount);

	}

    Matrix4 getRotationMatrix(Parameters currentParameters)
    {
        
        return Matrix4.vectorOrientation(currentParameters.roll, currentParameters.yaw, currentParameters.pitch);

    }
	
		
	private int interpretProduction(Parameters currentParameters, ArrayList production, LParser lparser, SunflowAPI sunflow) {

		// we start with a cube, axis in centre, we want it to behave like a line with axis at one end. 
		// treat currentParameters.gObject as axis

		float moveLength;
		float rotation;
        Vector3 nvec;
        Matrix4 rotateMatrix;
        		
		for(int i = 0; i < production.Count; i++) {	
			
			Rule rule = (Rule)production[i];
//			Console.WriteLine("rule: " + rule); 
//			Console.WriteLine("Switch Type: " + rule.Type); 
			switch(rule.Type) {
			case Rule.TypeEnum.Letter:
//				Console.WriteLine("Switch Value: " + rule.Value[0]); 
				switch(rule.Value[0]) {
				case 'F':

					if(i+1 < production.Count && ((Rule)production[i+1]).Type == Rule.TypeEnum.Expression) {
						Stack expression = (Stack)((Rule)production[i+1]).Expression;
						moveLength = (float)ExpressionEvaluator.EvaluateStack(expression, null, lparser.GlobalParameters);
						if (moveLength <= 0) {
							moveLength = currentParameters.length;							
						}	
					} else {
						moveLength = currentParameters.length;
					}
	        			
//					Console.WriteLine("moveLength: " + moveLength);

							//sunflow.geometry("sphere" + currentParameters.objectCount, "sphere");
							//sunflow.parameter("shaders", "sps");
							//                     sunflow.parameter("transform", Matrix4.translation(currentParameters.position.x,
							//                                                                        currentParameters.position.y,
							//                                                                        currentParameters.position.z).multiply(Matrix4.scale(currentParameters.thickness * 1.1f)));

							//sunflow.instance("sphere" + currentParameters.objectCount + ".instance", "sphere" + currentParameters.objectCount);
							//currentParameters.objectCount++;


                    nvec = currentParameters.roll.mul(moveLength * 0.5f);    // axis that we roll on is the forward vector
			        currentParameters.position.x += nvec.x;
					currentParameters.position.y += nvec.y;
					currentParameters.position.z += nvec.z;

					AddNewObject(sunflow, currentParameters);
					currentParameters.objectCount++;

					currentParameters.position.x += nvec.x;
					currentParameters.position.y += nvec.y;
					currentParameters.position.z += nvec.z;

					break;
					
				case 'f':
					
					if(i+1 < production.Count && ((Rule)production[i+1]).Type == Rule.TypeEnum.Expression) {
						Stack expression = (Stack)((Rule)production[i+1]).Expression;
						moveLength = (float)ExpressionEvaluator.EvaluateStack(expression, null, lparser.GlobalParameters);
						if (moveLength <= 0) {
							moveLength = currentParameters.length;							
						}	
					} else {
						moveLength = currentParameters.length;
					}

					nvec = currentParameters.roll.mul(moveLength * 0.5f);    // axis that we roll on is the forward vector
					currentParameters.position.x += nvec.x;
                    currentParameters.position.y += nvec.y;
                    currentParameters.position.z += nvec.z;
					break;
					
				case '+':

					if(i+1 < production.Count && ((Rule)production[i+1]).Type == Rule.TypeEnum.Expression) {
						Stack expression = (Stack)((Rule)production[i+1]).Expression;
						rotation = (float)ExpressionEvaluator.EvaluateStack(expression, null, lparser.GlobalParameters);
						if (rotation <= 0) {
							rotation = currentParameters.angle;							
						}	
					} else {
						rotation = currentParameters.angle;
					}

							// yawing so rotate pitch and roll against the yaw vector
							rotateMatrix = Matrix4.rotate(currentParameters.yaw.x, currentParameters.yaw.y, currentParameters.yaw.z, rotation * d2r);
							currentParameters.pitch = rotateMatrix.transformV(currentParameters.pitch);
							currentParameters.roll = rotateMatrix.transformV(currentParameters.roll);
//							currentParameters.localRotation.z += rotation;
                           
					break;
					
				case '-':

					if(i+1 < production.Count && ((Rule)production[i+1]).Type == Rule.TypeEnum.Expression) {
						Stack expression = (Stack)((Rule)production[i+1]).Expression;
						rotation = (float)ExpressionEvaluator.EvaluateStack(expression, null, lparser.GlobalParameters);
						if (rotation <= 0) {
							rotation = currentParameters.angle;							
						}	
					} else {
						rotation = currentParameters.angle;
					}

							// yawing so rotate pitch and roll against the yaw vector
							rotateMatrix = Matrix4.rotate(currentParameters.yaw.x, currentParameters.yaw.y, currentParameters.yaw.z, -rotation * d2r);
							currentParameters.pitch = rotateMatrix.transformV(currentParameters.pitch);
							currentParameters.roll = rotateMatrix.transformV(currentParameters.roll);
//							currentParameters.localRotation.z -= rotation;
					break;
					
				case '\\':

					if(i+1 < production.Count && ((Rule)production[i+1]).Type == Rule.TypeEnum.Expression) {
						Stack expression = (Stack)((Rule)production[i+1]).Expression;
						rotation = (float)ExpressionEvaluator.EvaluateStack(expression, null, lparser.GlobalParameters);
						if (rotation <= 0) {
							rotation = currentParameters.angle;							
						}	
					} else {
						rotation = currentParameters.angle;
					}

                            // rolling so rotate pitch and yaw against the row vector
                    rotateMatrix = Matrix4.rotate(currentParameters.roll.x, currentParameters.roll.y, currentParameters.roll.z, rotation * d2r);
                    currentParameters.pitch = rotateMatrix.transformV(currentParameters.pitch);
                    currentParameters.yaw = rotateMatrix.transformV(currentParameters.yaw);

					break;

				case '/':

					if(i+1 < production.Count && ((Rule)production[i+1]).Type == Rule.TypeEnum.Expression) {
						Stack expression = (Stack)((Rule)production[i+1]).Expression;
						rotation = (float)ExpressionEvaluator.EvaluateStack(expression, null, lparser.GlobalParameters);
						if (rotation <= 0) {
							rotation = currentParameters.angle;							
						}	
					} else {
						rotation = currentParameters.angle;
					}

							rotateMatrix = Matrix4.rotate(currentParameters.roll.x, currentParameters.roll.y, currentParameters.roll.z, -rotation * d2r);
							currentParameters.pitch = rotateMatrix.transformV(currentParameters.pitch);
							currentParameters.yaw = rotateMatrix.transformV(currentParameters.yaw);


                    break;

				case '&':

					if(i+1 < production.Count && ((Rule)production[i+1]).Type == Rule.TypeEnum.Expression) {
						Stack expression = (Stack)((Rule)production[i+1]).Expression;
						rotation = (float)ExpressionEvaluator.EvaluateStack(expression, null, lparser.GlobalParameters);
						if (rotation <= 0) {
							rotation = currentParameters.angle;							
						}	
					} else {
						rotation = currentParameters.angle;
					}

							// pitching so rotate yaw and roll against the pitch vector
							rotateMatrix = Matrix4.rotate(currentParameters.pitch.x, currentParameters.pitch.y, currentParameters.pitch.z, rotation * d2r);
							currentParameters.yaw = rotateMatrix.transformV(currentParameters.yaw);
							currentParameters.roll = rotateMatrix.transformV(currentParameters.roll);

					break;

				case '^':
					
					if(i+1 < production.Count && ((Rule)production[i+1]).Type == Rule.TypeEnum.Expression) {
						Stack expression = (Stack)((Rule)production[i+1]).Expression;
						rotation = (float)ExpressionEvaluator.EvaluateStack(expression, null, lparser.GlobalParameters);
						if (rotation <= 0) {
							rotation = currentParameters.angle;							
						}	
					} else {
						rotation = currentParameters.angle;
					}

							rotateMatrix = Matrix4.rotate(currentParameters.pitch.x, currentParameters.pitch.y, currentParameters.pitch.z, -rotation * d2r);
							currentParameters.yaw = rotateMatrix.transformV(currentParameters.yaw);
							currentParameters.roll = rotateMatrix.transformV(currentParameters.roll);

					break;
					
				case '|':

							rotateMatrix = Matrix4.rotate(currentParameters.yaw.x, currentParameters.yaw.y, currentParameters.yaw.z, 180.0f * d2r);
							currentParameters.pitch = rotateMatrix.transformV(currentParameters.pitch);
							currentParameters.roll = rotateMatrix.transformV(currentParameters.roll);

					break;
				}
				break;			
			case Rule.TypeEnum.BranchStart:

                Parameters tmp = new Parameters();
				tmp.roll = new Vector3(currentParameters.roll);
				tmp.pitch = new Vector3(currentParameters.pitch);
				tmp.yaw = new Vector3(currentParameters.yaw);
				tmp.position = new Point3(currentParameters.position);
                tmp.localRotation = new Point3(currentParameters.localRotation);
	            tmp.localScale = new Vector3(currentParameters.localScale);
	            tmp.angle = currentParameters.angle;
				tmp.length = currentParameters.length;
				tmp.thickness = currentParameters.thickness;
				tmp.primitiveType = currentParameters.primitiveType;
                tmp.objectCount = currentParameters.objectCount;

                currentParameters.objectCount = interpretProduction(tmp, rule.Branch, lparser, sunflow);
                    					
				break;
			case Rule.TypeEnum.BranchEnd:
				break;
			}	
		}
        return currentParameters.objectCount;
	}

	public void SetupSunflow(SunflowAPI a)
	{

		a.parameter("threads", Environment.ProcessorCount);
		//          parameter ("threads", 1);
		a.options(SunflowAPI.DEFAULT_OPTIONS);
		//The render's resolution. 1920 by 1080 is full HD.
		int resolutionX = 3840;
		int resolutionY = 1920;

  //      resolutionX = 1920;
    //    resolutionY = 1920;

//        resolutionX = 384;
//		resolutionY = 192;
//		      int resolutionX = 3840;
//		      int resolutionY = 960;
		a.parameter("resolutionX", resolutionX);
		a.parameter("resolutionY", resolutionY);

		//The anti-aliasing. Negative is subsampling and positive is supersampling.
		a.parameter("aa.min", 1);
		a.parameter("aa.max", 2);

		//Number of samples.
		a.parameter("aa.samples", 1);

		//The contrast needed to increase anti-aliasing.
		a.parameter("aa.contrast", .016f);

		//Subpixel jitter.
		a.parameter("aa.jitter", true);

		//The filter.
		a.parameter("filter", "mitchell");
		a.options(SunflowAPI.DEFAULT_OPTIONS);

		

//				Point3 eye = new Point3(7.0f, -7.0f, -7.0f);
//				Point3 target = new Point3(0.0f, -7.0f, 0.0f);
		

		Point3 target = new Point3(7.0f, -7.0f, -7.0f);
		Point3 eye = new Point3(-6.0f, -10.0f, 2.0f);
        Vector3 up = new Vector3(0, 1, 0);

		a.parameter("transform", Matrix4.lookAt(eye, target, up));

        String name = "Camera";


        /*     thinlens camera */
        /*
                //Aspect Ratio.
                float aspect = ((float)resolutionX) / ((float)resolutionY);
                a.parameter("aspect", aspect);
                a.camera(name, "thinlens");
        */

        /* 360 3D VR camera */
        /*

                a.parameter("lens.eyegap", 0.5f);
        //		a.camera(name, "spherical3d");

                a.camera(name, "spherical1803d");


        */
        a.parameter("lens.eyegap", 0.1f);
        a.camera(name, "vr180fisheye");


        a.parameter("camera", name);
		a.options(SunflowAPI.DEFAULT_OPTIONS);

		//Trace depths. Higher numbers look better.
		a.parameter("depths.diffuse", 1);
		a.parameter("depths.reflection", 2);
		a.parameter("depths.refraction", 2);
		a.options(SunflowAPI.DEFAULT_OPTIONS);

		//Setting up the shader for the ground.
		a.parameter("diffuse", null, 0.4f, 0.4f, 0.4f);
		a.parameter("shiny", .1f);
		a.shader("ground", "shiny_diffuse");
		a.options(SunflowAPI.DEFAULT_OPTIONS);

		//Setting up the shader for the big metal sphere.
		a.parameter("diffuse", null, 0.3f, 0.3f, 0.3f);
		a.parameter("shiny", .95f);
		a.shader("metal", "shiny_diffuse");
		a.options(SunflowAPI.DEFAULT_OPTIONS);


		//Setting up the shader for the cube of spheres.
		a.parameter("diffuse", null, 1.0f, 1.0f, 1.0f);
		a.shader("sps", "diffuse");
		a.options(SunflowAPI.DEFAULT_OPTIONS);

		//Instancing the floor.
		a.parameter("center", new Point3(0, -14.2f, 0));
		a.parameter("normal", new Vector3(0, 1, 0));
		a.geometry("floor", "plane");
		a.parameter("shaders", "ground");
		a.instance("FloorInstance", "floor");
		a.options(SunflowAPI.DEFAULT_OPTIONS);

		//Creating the lighting system with the sun and sky.
		a.parameter("up", new Vector3(0, 1, 0));
		a.parameter("east", new Vector3(1, 0, 0));
		//            double sunRad = (Math.PI * 1.05);
		//          a.parameter("sundir", new Vector3((float)Math.Cos(sunRad), (float)Math.Sin(sunRad), (float)(.5 * Math.Sin(sunRad))).normalize());
		a.parameter("sundir", new Vector3(0.8f, 0.8f, 0.5f).normalize());
		a.parameter("turbidity", 4f);
		a.parameter("samples", 4);
		a.light("sunsky", "sunsky");
		a.options(SunflowAPI.DEFAULT_OPTIONS);

	}			
	
	void Update () {

	}
}
