using UnityEngine;
using System.Collections;
using System.Collections.Generic;

 struct Parameters {
		public GameObject gObject;
		public float angle;
		public float length;
		public float thickness;
		public PrimitiveType primitiveType;
	}

public class BLOCKS : MonoBehaviour {

	Stack<Parameters> parameterStack = new Stack<Parameters>();
	
	private ArrayList _ruleList;
	
	// Use this for initialization
	void Start () {

	LParser lparser = new LParser();
		
	System.String[] rules = new System.String[7];
	
	rules[0] = "#define a 1";
	rules[1] = "#thickness 0.4";
	rules[2] = "#recursion 6";
	rules[3] = "#angle 25";
	rules[4] = "#axiom ^(90)X";
		
	rules[5] = "X -> F-[[X]+X]+F[+FX]-X";
	rules[6] = "F -> FF";

	/*	
	rules[0] = "#define p 3.14";
	rules[1] = "#thickness 0.2";
	rules[2] = "#recursion 4";
	rules[3] = "#angle 90";
	rules[4] = "#axiom A";
	rules[5] = "A -> B-F+CFC+F-D&F^D-F+&&CFC+F+B//";
	rules[6] = "B -> A&F^CFB^F^D^^-F-D^|F^B|FC^F^A//";
	rules[7] = "C -> |D^|F^B-F+C^F^A&&FA&F^C+F+B^F^D//";
	rules[8] = "D -> |CFB-F+B|FA&F^A&&FB-F+B|FC//";
	*/
	 
 
   
 

		
	bool statusOK = lparser.ParseStringArray(rules); 

	_ruleList = 	lparser.RunLSystem();
	
	Debug.Log("RULES: " + Rules.RuleListToString(_ruleList, lparser.GlobalParameters));	

	Parameters currentParameters = new Parameters();
	
		
	currentParameters.primitiveType = PrimitiveType.Cube;
	currentParameters.length = 1.0f;	
	currentParameters.angle = (float)lparser.Angle;
	currentParameters.gObject = GameObject.CreatePrimitive(currentParameters.primitiveType);
	currentParameters.thickness = (float)lparser.Thickness;

	
//	Vector3 scale = new Vector3(currentParameters.length1f, currentParameters.thickness , currentParameters.thickness);

			
	currentParameters.gObject.transform.position = new Vector3(0, 0, 0);
	currentParameters.gObject.transform.localRotation = Quaternion.identity;
	currentParameters.gObject.transform.localScale = new Vector3(currentParameters.thickness,currentParameters.thickness,currentParameters.length);	
	
	interpretProduction(currentParameters, _ruleList, lparser);	
	
//	Debug.Log(_ruleList);	
	Destroy(currentParameters.gObject);

	}
	
	
		
	private void interpretProduction(Parameters currentParameters, ArrayList production, LParser lparser) {

		// we start with a cube, axis in centre, we want it to behave like a line with axis at one end. 
		// treat currentParameters.gObject as axis

		float moveLength;
		Vector3 tempV3;
		float rotation;
		
		for(int i = 0; i < production.Count; i++) {	
			
			Rule rule = (Rule)production[i];
//			Debug.Log("rule: " + rule); 
//			Debug.Log("Switch Type: " + rule.Type); 
			switch(rule.Type) {
			case Rule.TypeEnum.Letter:
//				Debug.Log("Switch Value: " + rule.Value[0]); 
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
	        			
					Debug.Log("moveLength: " + moveLength);
					
					GameObject cube = GameObject.CreatePrimitive(currentParameters.primitiveType);
					
	        		cube.transform.position = currentParameters.gObject.transform.position;
			  		cube.transform.localRotation = currentParameters.gObject.transform.localRotation;


					// cube centre now positioned and rotated to match axis, move cube foward half length
					
					cube.transform.Translate(Vector3.forward*moveLength*0.5f);

					// scale cube to match length
					tempV3 = currentParameters.gObject.transform.localScale;
					tempV3.z = moveLength;
					cube.transform.localScale = tempV3;
					cube.renderer.material.shader = Shader.Find ("Decal");
					cube.renderer.material.mainTexture = (Texture2D)Resources.LoadAssetAtPath("Assets/Standard Assets/Terrain Textures/GoodDirt.psd", typeof(Texture2D));
					currentParameters.gObject.transform.Translate(Vector3.forward*moveLength);
	    			currentParameters.gObject.transform.localScale = cube.transform.localScale;
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

					tempV3 = currentParameters.gObject.transform.localScale;
					tempV3.z = moveLength;
					currentParameters.gObject.transform.localScale = tempV3;
					currentParameters.gObject.transform.Translate(Vector3.forward*moveLength);
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
					
					currentParameters.gObject.transform.Rotate(0.0f, -rotation, 0.0f);
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
					
					currentParameters.gObject.transform.Rotate(0.0f, rotation, 0.0f);
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
					
					currentParameters.gObject.transform.Rotate(0.0f, 0.0f, rotation);
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
					
					currentParameters.gObject.transform.Rotate(0.0f, 0.0f, -rotation);
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
					
					currentParameters.gObject.transform.Rotate(rotation, 0.0f, 0.0f);					
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
					
					
					currentParameters.gObject.transform.Rotate(-rotation, 0.0f, 0.0f);
					break;
					
				case '|':
					
					currentParameters.gObject.transform.Rotate(0.0f, 180f, 0.0f);

					break;
				}
				break;			
			case Rule.TypeEnum.BranchStart:
//				Debug.Log("Hello: " + rule.Value[0]); 
	//			parameterStack.Push(currentParameters);	
				Parameters tmp = new Parameters();
				tmp.angle = currentParameters.angle;
				tmp.length = currentParameters.length;
				tmp.thickness = currentParameters.thickness;
				tmp.primitiveType = currentParameters.primitiveType;
				tmp.gObject = GameObject.CreatePrimitive(currentParameters.primitiveType);
				tmp.gObject.transform.localScale = currentParameters.gObject.transform.localScale;
	    			tmp.gObject.transform.position = currentParameters.gObject.transform.position;
		  		tmp.gObject.transform.localRotation = currentParameters.gObject.transform.localRotation;
				
			 	interpretProduction(tmp, rule.Branch, lparser);
									
				Destroy(tmp.gObject);	
				break;
			case Rule.TypeEnum.BranchEnd:
				break;
			}	
		}			

	}
		 
			
	
	void Update () {

	}
}
