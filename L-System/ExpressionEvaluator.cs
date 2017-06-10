using System;
using System.Collections;
using System.Text.RegularExpressions;


public class ExpressionEvaluator {
	
			
	public static Stack ParseExpression (String expression) {
		
		Stack operators;
		Stack arguements;
		Token token;
		    
		String tmpExpression = expression;
		bool operatorIsAllowedHere = false; /* allows for signed numbers */
		
		operators = new Stack();
		arguements = new Stack();
				
		
		while(tmpExpression.Length > 0) {
			token = GetNextToken(tmpExpression, operatorIsAllowedHere);
//				Console.WriteLine("token " + token.TokenString);
			
		   /* check its an operator */
			if (token.TokenType == Token.TokenTypeEnum.Operator || token.TokenType == Token.TokenTypeEnum.Boolean) {
					if(operators.Count != 0) {
					    /* 
					       If the new token has a lower presedence then we need to put the top 
						   token on the stack on the arguement stack unless its an open bracket 
						   which stay on the stack until matched with a close bracket
						*/
						if(((Token )operators.Peek()).TokenType != Token.TokenTypeEnum.OpenBracket &&
							token.HasLowerPrecedenceThan((Token )operators.Peek())) {
							arguements.Push(operators.Pop());
						}
					}

					operators.Push(token);
					
					/* we can't have two operators on a trot. */
					operatorIsAllowedHere = false;
				
			} else if (token.TokenType == Token.TokenTypeEnum.CloseBracket ) {
			   	/* 
			   		force presedence by moving all the operators onto the argument list 
			   	*/
				while(operators.Count > 0  && ((Token )operators.Peek()).TokenType != Token.TokenTypeEnum.OpenBracket) {
//				Console.WriteLine("Moving " + ((Token )operators.Peek()).TokenString + " on arguements stack");
					arguements.Push(operators.Pop());
				} 
				
				if(operators.Count > 0) {
				    /* 
				    	get rid off the open bracket 
				    */
//				Console.WriteLine("Poping " + ((Token )operators.Peek()).TokenString + " on arguements stack");
					operators.Pop();
				}  
					
					/* 
						we can have an operator after a close bracket 
					*/
				operatorIsAllowedHere = true;
					
			} else {
				/* it must be a variable or constant or open bracket */
				if(token.TokenType == Token.TokenTypeEnum.OpenBracket) {
//						Console.WriteLine("Pushing " + token.TokenString + " on operator stack");
					operatorIsAllowedHere = false;
					operators.Push(token);
				} else {
//						Console.WriteLine("Pushing " + token.TokenString + " on arguement stack");
					operatorIsAllowedHere = true;
					arguements.Push(token);
				}
			}
			
			tmpExpression = tmpExpression.Substring(token.TokenString.Length);
		}
		
		while(operators.Count > 0) {
			arguements.Push(operators.Pop());
		} 
		return arguements;
			
	}
	

	public static double EvaluateStack(Stack stackToEvaluate, Hashtable localParameters, Hashtable globalParameters) {

			IEnumerator etor = stackToEvaluate.GetEnumerator();
	
			return EvaluateStack(etor , localParameters, globalParameters);
	}

	static double EvaluateStack(IEnumerator etor, Hashtable localParameters, Hashtable globalParameters) {

		double[] values;
		

		etor.MoveNext();
		
		Token element = (Token)etor.Current;
//			Console.WriteLine("Popped " + element.TokenString);

		
		
		switch(element.TokenType) {
			case Token.TokenTypeEnum.Value:
				return Double.Parse(element.TokenString);
			case Token.TokenTypeEnum.Operator:
				values = new double[element.NumberOfArguements];
				for(int i = 0; i < element.NumberOfArguements; i++) {
					values[i] = EvaluateStack(etor, localParameters, globalParameters);
				}
				return Token.EvaluateOperator(element.TokenString, values);
			case Token.TokenTypeEnum.Function:
				values = new double[element.NumberOfArguements];
				for(int i = 0; i < element.NumberOfArguements; i++) {
					values[i] = EvaluateStack(etor, localParameters, globalParameters);
				}
				return Token.EvaluateFunction(element.TokenString, values);
			case Token.TokenTypeEnum.Boolean:
				values = new double[element.NumberOfArguements];
				for(int i = 0; i < element.NumberOfArguements; i++) {
					values[i] = EvaluateStack(etor, localParameters, globalParameters);
				}
				return Token.EvaluateBoolean(element.TokenString, values);
			case Token.TokenTypeEnum.Variable:
				/* check the locals first */
				if(localParameters != null && localParameters.ContainsKey(element.token)) {
					return (double)localParameters[element.TokenString];
				}
				if(globalParameters != null && globalParameters.ContainsKey(element.token)) {
					return (double)globalParameters[element.TokenString];
				}
				break;
		}
		
		return 0;

	}




public static bool SameExpressions (Stack expressionA, Stack expressionB) {

	IEnumerator enumeratorA = expressionA.GetEnumerator();
	IEnumerator enumeratorB = expressionB.GetEnumerator();
	
  	while ( enumeratorA.MoveNext() ) {
    	 	enumeratorB.MoveNext();
    	 	if(((Token )enumeratorA.Current).TokenString != (( Token )enumeratorB.Current).TokenString) {
    	 		return false;
    	 	}
    }
    
    return true;
}

public static Token GetNextToken(String expression, bool operatorIsAllowedHere) {

//			Console.WriteLine("getNextToken " + expression + " " + operatorIsAllowedHere);
		
		Match regexResult = null;
		Token tokenResult = null;

	
		foreach(Token token in Token.TokenListArray) {
//			Console.WriteLine("token " + token.TokenString);
			if(operatorIsAllowedHere == false) {
				if(token.TokenType != Token.TokenTypeEnum.Operator) {
					regexResult = token.RegexForMatching.Match(expression);
					if(regexResult.Success) {
						tokenResult = token;
						break;
					} 
				}
			} else {
				regexResult = token.RegexForMatching.Match(expression); 
				if(regexResult.Success) {
					tokenResult = token;
					break;
				} 
			}
		}
		
		if(null != tokenResult) {
			switch(tokenResult.TokenType) {
				case Token.TokenTypeEnum.Value:
					return new Token(regexResult.Groups[0].Captures[0].ToString(), 
									 "", 
									 Token.TokenTypeEnum.Value, 
									 1, 
									 100);
				case Token.TokenTypeEnum.Variable:
					return new Token(regexResult.Groups[0].Captures[0].ToString(), 
									 "", 
									 Token.TokenTypeEnum.Variable, 
									 1, 
									 100);
				default:
					break; /* to nothing */
			}
		}	
		return tokenResult;
	
	}
	
}

