using System;
using System.Collections;
using System.Text.RegularExpressions;


public class Token {

	public enum TokenTypeEnum { Operator, 
								OpenBracket, 
								CloseBracket, 
								Function, 
								Value, 
								Seperator, 
								Boolean, 
								Variable,
								BranchStart,
								BranchEnd,
								PolygonStart,
								PolygonEnd};

	public string token;
	public Regex regex;
	public TokenTypeEnum tokenType;
	public int numberOfArguements;
	public int precedence;
	
	public static double BooleanFalse {
		get {return 0.0;}
	} 
	
	public static double BooleanTrue {
		get {return 1.0;}
	} 
	

	static ArrayList tokenListArray;

	public static ArrayList TokenListArray {
		get { return tokenListArray; }
	}	
	public string TokenString {
		get { return token; }
		set { token = value; }
	}

	public TokenTypeEnum TokenType {
		get { return tokenType; }
	}
	
			public Regex RegexForMatching {
		get { return regex; }
	}

	public int NumberOfArguements {
		get { return numberOfArguements; }
	}

	public int Precedence {
		get { return precedence; }
	}
	
	public Token(String token, String regex, TokenTypeEnum tokenType, int numberOfArguements, int precedence) {
		this.token = token;
		this.regex = new Regex(regex);
		this.tokenType = tokenType;
		this.numberOfArguements = numberOfArguements;
		this.precedence = precedence;
	}
	
	public bool HasLowerPrecedenceThan(Token otherToken) {
		if(precedence < otherToken.Precedence) {
			return true;
		} else {
			return false;
		}	
	}

	static Token() {
	
		tokenListArray = new ArrayList();
		tokenListArray.Add(new Token("=", "^=", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token("!=", "^!=", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token("<>", "^<>", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token("<=", @"^<=", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token(">=", @"^>=", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token("<", @"^<", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token(">", @"^>", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token("&&", @"^&&", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token("&", @"^&", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token("||", @"^\|\|", Token.TokenTypeEnum.Boolean, 2, 0));
		tokenListArray.Add(new Token("|", @"^\|", Token.TokenTypeEnum.Boolean, 2, 0));

		tokenListArray.Add(new Token("+", "^\\+", Token.TokenTypeEnum.Operator, 2, 10));
		tokenListArray.Add(new Token("-", "^\\-", Token.TokenTypeEnum.Operator, 2, 10));
		tokenListArray.Add(new Token("*", "^\\*", Token.TokenTypeEnum.Operator, 2, 20));
		tokenListArray.Add(new Token("/", "^\\/", Token.TokenTypeEnum.Operator, 2, 20));
		tokenListArray.Add(new Token("^", "^\\^", Token.TokenTypeEnum.Operator, 2, 20));
		tokenListArray.Add(new Token("sin", "^sin\\(", Token.TokenTypeEnum.Function, 1, 30));
		tokenListArray.Add(new Token("cos", "^cos\\(", Token.TokenTypeEnum.Function, 1, 30));
		tokenListArray.Add(new Token("tan", "^tan\\(", Token.TokenTypeEnum.Function, 1, 30));
		tokenListArray.Add(new Token("(", "^\\(", Token.TokenTypeEnum.OpenBracket, 1, 100));
		tokenListArray.Add(new Token("{", "^\\{", Token.TokenTypeEnum.PolygonStart, 1, 100));
		tokenListArray.Add(new Token("[", "^\\[", Token.TokenTypeEnum.BranchStart, 1, 100));
		tokenListArray.Add(new Token(")", "^\\)", Token.TokenTypeEnum.CloseBracket, 1, 100));
		tokenListArray.Add(new Token("}", "^\\}", Token.TokenTypeEnum.PolygonEnd, 1, 100));
		tokenListArray.Add(new Token("]", "^\\]", Token.TokenTypeEnum.BranchEnd, 1, 100));
		tokenListArray.Add(new Token(",", "^,", Token.TokenTypeEnum.Seperator, 1, 100));
		tokenListArray.Add(new Token(";", "^;", Token.TokenTypeEnum.Seperator, 1, 100));
		tokenListArray.Add(new Token("number", @"(^[+-]*\d+[\.\d]*)", Token.TokenTypeEnum.Value, 1, 100));
		tokenListArray.Add(new Token("var", @"(^[\p{Ll}\p{Lu}\p{Lt}\p{Lo}]+)", Token.TokenTypeEnum.Variable, 1, 100));
	
	}


	public static double EvaluateOperator(String op, double[] values) {

	
		/* NB. values on the array read right to left */
		
		if(op.Equals("+")) {
//						Console.WriteLine("Adding {0} + {1}",  values[1], values[0]);
			return values[1] + values[0];
		} else if(op.Equals("-")) {
			return values[1] - values[0];
		} else if(op.Equals("*")) {
			return values[1] * values[0];
		} else if(op.Equals("/")) {
			return values[1] / values[0];
		} else if(op.Equals("^")) {
			return Math.Pow(values[1], values[0]);
		}	
		
		return 0.0;
	}
	
	
	public static double EvaluateFunction(String fn, double[] values) {
	
		/* NB. values on the array read right to left */
		
		if(fn.Equals("cos")) {
			return Math.Cos(values[0]);
		} else if(fn.Equals("sin")) {
			return Math.Sin(values[0]);
		} else if(fn.Equals("tan")) {
			return Math.Tan(values[0]);
		}	
		
		return 0.0;
	}
	
	
	 public static double EvaluateBoolean(String op, double[] values) {
	
		/* NB. values on the array read right to left */
		
//			Console.WriteLine(values[1] + op + values[0]);
		
		if(op.Equals("=")) {
			return values[1] == values[0] ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals("<")) {
			return values[1] < values[0] ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals(">")) {
			return values[1] > values[0] ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals("<=")) {
			return values[1] <= values[0] ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals(">=")) {
			return values[1] >= values[0] ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals("!=")) {
			return values[1] != values[0] ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals("<>")) {
			return values[1] != values[0] ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals("&&")) {
			return values[1] == Token.BooleanTrue && values[0] == Token.BooleanTrue ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals("&")) {
			return values[1] == Token.BooleanTrue && values[0] == Token.BooleanTrue ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals("||")) {
			return values[1] == Token.BooleanTrue || values[0] == Token.BooleanTrue ? Token.BooleanTrue : Token.BooleanFalse;
			
		} else if(op.Equals("|")) {
			return values[1] == Token.BooleanTrue || values[0] == Token.BooleanTrue ? Token.BooleanTrue : Token.BooleanFalse;
		}	

		return Token.BooleanFalse;
	}
	

}	







