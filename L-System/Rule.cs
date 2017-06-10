using System;
using System.Collections;


public class Rule : ICloneable {

	public enum TypeEnum { Letter, 
						   Expression, 
						   Parameter,
						   BranchStart,
						   BranchEnd };

	TypeEnum _type;
	String _value;
	Stack _expression;
	ArrayList _branch;
	ArrayList _parentList;
	int _parentIndex;

	public Rule(String value, TypeEnum type) {
		_type = type;
		_value = value;
		if(type == TypeEnum.Expression) {
			_expression = ExpressionEvaluator.ParseExpression(value);
		}
		_parentList = null;
		_parentIndex = 0;
	}
		

	public Rule(ArrayList branch, ArrayList parentList) {
		_branch = branch;
		_type = TypeEnum.BranchStart;
		_value = "[";
		Rule branchStart = (Rule)branch[0];
		branchStart._parentIndex = parentList.Count-1;
		branchStart._parentList = parentList; 
	}

	private Rule(TypeEnum type, String value, Stack expression, ArrayList branch, ArrayList parentList, int parentIndex) {
		
		switch(type) {
			case TypeEnum.BranchStart:
				_branch = (ArrayList)branch.Clone();
				break;
			case TypeEnum.Expression:
				_expression = (Stack)expression.Clone();
				break;
		} 
		_type = type;
		_value = value;

		_parentIndex = parentIndex;
		if(_parentList != null) {
			_parentList = (ArrayList)parentList.Clone();
		} 
	}

	public string Value {
		get { return _value; }
	}

	public TypeEnum Type {
		get { return _type; }
	}

	public Stack Expression {
		get { return _expression; }
	}

	public ArrayList Branch {
		get { return _branch; }
	}

	public ArrayList ParentList {
		get { return _parentList; }
	}
	
	public int ParentIndex {
		get { return _parentIndex; }
	}
	
	public object Clone() {
		return new Rule(_type, _value, _expression, _branch, _parentList, _parentIndex);
	}

}

public class Rules {


	public static ArrayList ConvertSuccessorToRuleList(String rules) {
	
		
		ArrayList ruleList = new ArrayList();
		int end;
		return ConvertSuccessorToRuleList(rules, ruleList, 0, out end);
	
	}

	private static ArrayList ConvertSuccessorToRuleList(String rules, ArrayList ruleList, int startIndex, out int endIndex) {
		
		String token = "";
		
		int bracketCount = 0;
		int charIndex = startIndex;
		
		char[] ruleChars = 	rules.ToCharArray();
		
		while(charIndex < rules.Length) {
			switch(ruleChars[charIndex]) {
				case '(':
					if(bracketCount == 0) {			
						/* start a group */
						token = "";
						charIndex++;
						bracketCount += 1;
						continue;
					}					
					bracketCount += 1;
					break;
				case ')':
					bracketCount -= 1;
					if(bracketCount == 0) {
					
						/* end of the expression */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Expression));
						charIndex++;
						continue;
					}		
					break;			
				case ',':
					if(bracketCount == 1) {
					
						/* end one expression and start the next */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Expression));
						token = "";
						charIndex++;
						continue;
					}					
					break;			
				case ';':
					if(bracketCount == 1) {
					
						/* end one expression and start the next */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Expression));
						token = "";
						charIndex++;
						continue;
					}					
					break;
				case '[':
					ArrayList branch = new ArrayList();
					ruleList.Add(new Rule(ConvertSuccessorToRuleList(rules, branch, ++charIndex, out endIndex), ruleList));
					token = "";
					charIndex = endIndex+1;
					continue;
				case ']':
					ruleList.Add(new Rule("]", Rule.TypeEnum.BranchEnd));
					token = "";
					endIndex = charIndex;
					return ruleList;
			}
			
			if(bracketCount > 0) {
				token += ruleChars[charIndex];
			} else {
				ruleList.Add(new Rule(ruleChars[charIndex].ToString(), Rule.TypeEnum.Letter));
			}
			
			charIndex++;
		}	

		endIndex = charIndex;
		return ruleList;
	}
	

	public static ArrayList ConvertPredecessorToRuleList(String rules) {

		ArrayList ruleList = new ArrayList();

		String token = "";
		
		int bracketCount = 0;
		int charIndex = 0;
		
		char[] ruleChars = 	rules.ToCharArray();
		
		while(charIndex < rules.Length) {
			switch(ruleChars[charIndex]) {
				case'(':
					if(bracketCount == 0) {			
						/* start a group */
						token = "";
						charIndex++;
						bracketCount += 1;
						continue;
					}					
					bracketCount += 1;
					break;
				case ')':
					bracketCount -= 1;
					if(bracketCount == 0) {
					
						/* end of the expression */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Parameter));
						charIndex++;
						continue;
					}	
					break;				
				case ',':
					if(bracketCount == 1) {
					
						/* end one expression and start the next */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Parameter));
						token = "";
						charIndex++;
						continue;
					}					
					break;
				case ';':
					if(bracketCount == 1) {
					
						/* end one expression and start the next */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Parameter));
						token = "";
						charIndex++;
						continue;
					}					
					break;
				case '[':
					Console.WriteLine("Branches are not allow in predecessors: " + rules);	
					break;
				case ']':
					Console.WriteLine("Branch end found predecessor: " + rules);	
					break;
			}
			
			if(bracketCount > 0) {
				token += ruleChars[charIndex];
			} else {
				ruleList.Add(new Rule(ruleChars[charIndex].ToString(), Rule.TypeEnum.Letter));
			}
			
			charIndex++;
		}	

		return ruleList;
	}


	public static ArrayList ConvertContextToRuleList(String rules) {
	
		ArrayList ruleList = new ArrayList();
		
		int end;
		if(rules == "*") {
			return ruleList;
		}	
		return ConvertContextToRuleList(rules, ruleList, 0, out end);
	
	}

	private static ArrayList ConvertContextToRuleList(String rules, ArrayList ruleList, int startIndex, out int endIndex) {

		Console.WriteLine("rules: " + rules);
		Console.WriteLine("ruleList: " + ruleList);
		Console.WriteLine("startIndex: " + startIndex);
			
		String token = "";
		
		int bracketCount = 0;
		int charIndex = startIndex;
		
		char[] ruleChars = 	rules.ToCharArray();
		
		while(charIndex < rules.Length) {
//			Console.WriteLine("rule char[" + charIndex + "] = " + ruleChars[charIndex]);
			switch(ruleChars[charIndex]) {
				case'(':
					if(bracketCount == 0) {			
						/* start a group */
						token = "";
						charIndex++;
						bracketCount += 1;
						continue;
					}					
					bracketCount += 1;
					break;
				case ')':
					bracketCount -= 1;
					if(bracketCount == 0) {
					
						/* end of the expression */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Parameter));
						charIndex++;
						continue;
					}	
					break;				
				case ',':
					if(bracketCount == 1) {
					
						/* end one expression and start the next */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Parameter));
						token = "";
						charIndex++;
						continue;
					}					
					break;
				case ';':
					if(bracketCount == 1) {
					
						/* end one expression and start the next */
						ruleList.Add(new Rule(token, Rule.TypeEnum.Parameter));
						token = "";
						charIndex++;
						continue;
					}					
					break;
				case '[':
					ArrayList branch = new ArrayList();
					ruleList.Add(new Rule(ConvertContextToRuleList(rules, branch, ++charIndex, out endIndex), ruleList));
					token = "";
					charIndex = endIndex+1;
					continue;
				case ']':
					ruleList.Add(new Rule("]", Rule.TypeEnum.BranchEnd));
					token = "";
					endIndex = charIndex;
					return ruleList;
			}
			
			if(bracketCount > 0) {
				token += ruleChars[charIndex];
			} else {
				ruleList.Add(new Rule(ruleChars[charIndex].ToString(), Rule.TypeEnum.Letter));
			}
			
			charIndex++;
		}	

		endIndex = charIndex;
		return ruleList;
	}
	
	public static bool MatchList(ArrayList rules, int startIndex, Production production, Hashtable ignoreHash, Hashtable globalParameters) {

					Console.WriteLine("\nlhContext: " + Rules.RuleListToString(production.LhContext, globalParameters));	
					Console.WriteLine("currentContext: " + Rules.RuleListToString(production.Rule, globalParameters));	
					Console.WriteLine("rhContext: " + Rules.RuleListToString(production.RhContext, globalParameters));						
					Console.WriteLine("rules: " + Rules.RuleListToString(rules, globalParameters));	
					Console.WriteLine("startIndex: " + startIndex);	
	

	
		int index;
		
		 ArrayList lhContext = production.LhContext;
		 ArrayList currentContext = production.Rule;
		 ArrayList rhContext = production.RhContext;
		 Stack booleanExpression = production.BooleanExpression;
		

		Rule ruleToMatch;
		
		if(currentContext == null || startIndex > rules.Count ) {
			/* its all gone wrong */
			return false;
		}		
		
		index = startIndex;

		foreach (Rule rule in currentContext) {
			
			if (index == rules.Count) {
				// This Production is longer than what is left of the rule list.
				return false;
			}	
			
			switch(rule.Type) {
				case Rule.TypeEnum.Letter:
					ruleToMatch = (Rule)rules[index];
					switch(ruleToMatch.Type) {
						case Rule.TypeEnum.Letter: 
							if(ruleToMatch.Value != rule.Value) {
								return false;
							}
							break;
						default:
							return false;
					}
					break;
				case Rule.TypeEnum.Parameter:
					if(Rule.TypeEnum.Expression != ((Rule)rules[index]).Type) {
							return false;
					}
					break;
				case Rule.TypeEnum.BranchStart:
					if(Rule.TypeEnum.BranchStart != ((Rule)rules[index]).Type) {
							return false;
					}
					if(!MatchBranch(((Rule)rules[index]).Branch, rule.Branch, ignoreHash)) {
							return false;
					}
					break;
				case Rule.TypeEnum.BranchEnd:
					/* We should not see this token, it should only be on a branch */
					
					Console.WriteLine("MatchList: Found the end of a branch on context list" + Rules.RuleListToString(currentContext, null));	
					break;
			}
			
			index++;
		}
		

		bool MatchedSoFar = false;
		
		if(rhContext.Count > 0) {
		
			MatchedSoFar = MatchRhContext(rhContext, 0, rules, index, ignoreHash);

			if(false == MatchedSoFar) {
				return false;
			}
		}		

		if(lhContext.Count > 0) {
		
			MatchedSoFar = MatchLhContext(lhContext, lhContext.Count-1, rules,  startIndex - 1, ignoreHash);
			if(false == MatchedSoFar) {
				return false;
			}
		}
			
//			Console.WriteLine("MatchList: Items on boolean stack: " + booleanExpression.Count);	
		if (booleanExpression.Count > 0) {
			Hashtable localParameters = GetLocalParameters(rules, startIndex, globalParameters, lhContext, currentContext, rhContext);
			double value = ExpressionEvaluator.EvaluateStack(booleanExpression, localParameters, globalParameters); 
			if(value == Token.BooleanFalse) {
				return false;
			}
		}
//			Console.WriteLine("Matched\n");	
		return true;
	}


	private static bool MatchRhContext(ArrayList context, int contextIndex, ArrayList rules, int rulesIndex, Hashtable ignoreHash) {


		if(context.Count == contextIndex) {
			return true;
		}
	
		if(rules.Count == rulesIndex) {
			return false;
		}

		Rule contextRule = (Rule) (context[contextIndex]);
		Rule ruleToMatch = (Rule) (rules[rulesIndex]);
		
//			Console.WriteLine("MatchRhContext: context rule" +contextRule.Value);	
//			Console.WriteLine("MatchRhContext: rules rule" + ruleToMatch.Value);	

		bool MatchedSoFar = false;

		while(ignoreHash.ContainsKey(ruleToMatch.Value)){
				rulesIndex++;
				if(rulesIndex >= rules.Count) {
					return false;
				}
				ruleToMatch = (Rule)rules[rulesIndex];
		}
		
		/* 
		   If we are not explicitly looking for a branch, 
		   we should ignore then.
		*/
		if(contextRule.Type != Rule.TypeEnum.BranchStart &&
			ruleToMatch.Type == Rule.TypeEnum.BranchStart){
				/* ignore it by matching the next rule against the current context*/
			return MatchRhContext(context, contextIndex, rules, rulesIndex+1, ignoreHash);
		}

		switch(contextRule.Type) {			
			case Rule.TypeEnum.Letter:
				if(Rule.TypeEnum.Letter == ruleToMatch.Type && ruleToMatch.Value == contextRule.Value) {
						MatchedSoFar = MatchRhContext(context, contextIndex+1, rules, rulesIndex+1, ignoreHash);
				}
				break;
			case Rule.TypeEnum.Parameter:
				if(Rule.TypeEnum.Expression == ruleToMatch.Type) {
					MatchedSoFar = MatchRhContext(context, contextIndex+1, rules, rulesIndex+1, ignoreHash);
				}
				break;
			case Rule.TypeEnum.BranchStart:
				if(Rule.TypeEnum.BranchStart == ruleToMatch.Type) {
					MatchedSoFar = MatchBranch(ruleToMatch.Branch, contextRule.Branch, ignoreHash);
				}

				break;
			case Rule.TypeEnum.BranchEnd:
				/* We should not see this token, it should only be on a branch */
				
				Console.WriteLine("MatchList: Found the end of a branch on context list" + Rules.RuleListToString(context, null));	
				return false;
			}
			
		if(rulesIndex+1 < rules.Count 
			&& Rule.TypeEnum.Expression == ((Rule)rules[rulesIndex]).Type 
			&& Rule.TypeEnum.Expression == ((Rule)rules[rulesIndex+1]).Type) {
			/* 
				we don't what to mistake A(x,y) for A(x)
			*/	
			return false;
		}				
		
		return MatchedSoFar;


	}
	
	private static bool MatchLhContext(ArrayList context, int contextIndex, ArrayList rules, int rulesIndex, Hashtable ignoreHash) {


		if(contextIndex < 0) {
			return true;
		}
	
		if(rulesIndex < 0) {
			if(((Rule)rules[0]).ParentList == null) {
				return false;
			} else {
				return MatchLhContext(context, contextIndex, ((Rule)rules[0]).ParentList, ((Rule)rules[0]).ParentIndex, ignoreHash);
			}
		}

		Rule contextRule = (Rule) (context[contextIndex]);
		Rule ruleToMatch = (Rule) (rules[rulesIndex]);
		
//			Console.WriteLine("MatchRhContext: context rule" +contextRule.Value);	
//			Console.WriteLine("MatchRhContext: rules rule" + ruleToMatch.Value);	

		bool MatchedSoFar = false;


		while(ignoreHash.ContainsKey(ruleToMatch.Value)){
				rulesIndex--;
				if(rulesIndex < 0) {
					if(((Rule)rules[0]).ParentList == null) {
						return false;
					} else {
						return MatchLhContext(context, contextIndex, ((Rule)rules[0]).ParentList, ((Rule)rules[0]).ParentIndex, ignoreHash);
					}					
				}
				ruleToMatch = (Rule)rules[rulesIndex];
		}

		/* 
		   If we are not explicitly looking for a branch, 
		   we should ignore then.
		*/
		if(contextRule.Type != Rule.TypeEnum.BranchStart &&
			ruleToMatch.Type == Rule.TypeEnum.BranchStart){
				/* ignore it by matching the previous rule against the current context*/
			return MatchLhContext(context, contextIndex, rules, rulesIndex-1, ignoreHash);
		}

			
		switch(contextRule.Type) {			
			case Rule.TypeEnum.Letter:
				if(Rule.TypeEnum.Letter == ruleToMatch.Type && ruleToMatch.Value == contextRule.Value) {
						MatchedSoFar = MatchLhContext(context, contextIndex-1, rules, rulesIndex-1, ignoreHash);
				}
				break;
			case Rule.TypeEnum.Parameter:
				if(Rule.TypeEnum.Expression == ruleToMatch.Type) {
					MatchedSoFar = MatchLhContext(context, contextIndex-1, rules, rulesIndex-1, ignoreHash);
				}
				break;
			case Rule.TypeEnum.BranchStart:
				if(Rule.TypeEnum.BranchStart == ruleToMatch.Type) {
					MatchedSoFar = MatchBranch(contextRule.Branch, ruleToMatch.Branch, ignoreHash);
				}

				break;
			case Rule.TypeEnum.BranchEnd:
				/* We should not see this token, it should only be on a branch */
				
				Console.WriteLine("MatchList: Found the end of a branch on context list" + Rules.RuleListToString(context, null));	
				return false;
			}
			
	
		
		return MatchedSoFar;


	}
			
	public static bool MatchBranch(ArrayList ruleBranch, ArrayList contextBranch, Hashtable ignoreHash) {

		int index = 0;
		Rule ruleToMatch;
	
		foreach (Rule rule in contextBranch) {
			if(index >= ruleBranch.Count) {
				return false;
			}
			ruleToMatch = (Rule)ruleBranch[index];
			while(ignoreHash.ContainsKey(ruleToMatch.Value)){
				index++;
				ruleToMatch = (Rule)ruleBranch[index];
			}						
			switch(rule.Type) {
				case Rule.TypeEnum.Letter:
					switch(ruleToMatch.Type) {
						case Rule.TypeEnum.Letter: 
							if(ruleToMatch.Value != rule.Value) {
								return false;
							}
							break;
						default:
							return false;
					}
					break;
				case Rule.TypeEnum.Parameter:
					if(Rule.TypeEnum.Expression != ruleToMatch.Type) {
							return false;
					}
					break;
				case Rule.TypeEnum.BranchStart:
					if(Rule.TypeEnum.BranchStart != ruleToMatch.Type) {
							return false;
					}
					if(!MatchBranch(ruleToMatch.Branch, rule.Branch, ignoreHash)) {
							return false;
					}
					break;
				case Rule.TypeEnum.BranchEnd:
					/* 
						We only need to match the branch to the 
					   	end of the context rule list
					*/
					return true;
			}
			
			index++;
		}
		
		/*
			If we get here then the branch has no end which is an error */
		Console.WriteLine("MatchBranch: Error, no end to branch " + Rules.RuleListToString(contextBranch, null));
		return true;
	}


	public static bool RewriteUsingRule(ArrayList oldRules, int startIndex, Hashtable globalParameters,
						  ArrayList lhContext, ArrayList currentContext, ArrayList rhContext,
						  ArrayList successor, ArrayList newRules) {
	
//			Console.WriteLine("oldRules before: " + Rules.RuleListToString(oldRules));	
//			Console.WriteLine("newRules before: " + Rules.RuleListToString(newRules));	
//								Console.WriteLine("param startIndex  : " + startIndex);	

		Hashtable localParameters;

		
		/* figure out the values for the local parameters */
		localParameters = GetLocalParameters(oldRules, startIndex, globalParameters, 
										   lhContext, currentContext, rhContext);
		
		return RewriteUsingRule(oldRules, startIndex, globalParameters,	localParameters, successor, newRules);
		 			
	}	
				
	public static bool RewriteUsingRule(ArrayList oldRules, int startIndex, Hashtable globalParameters,
										Hashtable localParameters, ArrayList successor, ArrayList newRules) {			
		
		/* now convert the rule by reading through the successor*/
		double value = 0;
		Rule rule;
		
		IEnumerator enumerator = successor.GetEnumerator();
	
  		while ( enumerator.MoveNext() ) {
  			rule = (Rule)enumerator.Current;
  			switch(rule.Type) {
  				case Rule.TypeEnum.Expression: 
//			Console.WriteLine("rule " + rule.Type + " : " + rule.Value);	
					value = ExpressionEvaluator.EvaluateStack(rule.Expression, localParameters, globalParameters);
					newRules.Add(new Rule(value.ToString(), Rule.TypeEnum.Expression));
					startIndex++;
  					break;
  				case Rule.TypeEnum.Letter: 
//			Console.WriteLine("rule " + rule.Type + " : " + rule.Value);	
  					newRules.Add((Rule)rule.Clone());
					startIndex++;
  					break;
  				case Rule.TypeEnum.BranchStart: 
//			Console.WriteLine("rule " + rule.Type + " : " + rule.Value);	
					ArrayList branchRules = new ArrayList();
					RewriteUsingRule(oldRules , 0, globalParameters, localParameters, rule.Branch, branchRules);
  					newRules.Add(new Rule(branchRules, newRules));
					startIndex++;
  					break;
  				case Rule.TypeEnum.BranchEnd:
//			Console.WriteLine("rule " + rule.Type + " : " + rule.Value);	
/*						branchLevel = 1;
					while(branchLevel > 0) {
//								Console.WriteLine("branchLevel : " + branchLevel);	
//								Console.WriteLine("startIndex  : " + startIndex);	

						if(((Rule)oldRules[startIndex]).Type == Rule.TypeEnum.BranchEnd) {
							branchLevel--;
						} else if(((Rule)oldRules[startIndex]).Type == Rule.TypeEnum.BranchStart) {
							branchLevel++;
						}
						newRules.Add(oldRules[startIndex]);
						startIndex++;
					}*/
					newRules.Add(new Rule("]",  Rule.TypeEnum.BranchEnd));
  					break;
  			}
    	}
		
//				Console.WriteLine("oldRules after: " + Rules.RuleListToString(oldRules));	
//				Console.WriteLine("newRules after: " + Rules.RuleListToString(newRules));	
		return true;
		
	}
		
	
	public static bool SameRuleOnLists(ArrayList ruleListA, ArrayList ruleListB) {
	
		Rule aRule;
		Rule bRule;
		
		for(int i=0; i<ruleListA.Count; i++) {
			aRule = (Rule )ruleListA[i];
			bRule = (Rule )ruleListB[i];
			if (aRule.Type != bRule.Type || aRule.Value != bRule.Value ) {
				return false;
			}
		}
		
		return true;
	}
	
	public static String RuleListToString(ArrayList rules, Hashtable globalParameters) {
	
		IEnumerator enumerator = rules.GetEnumerator();
		Rule.TypeEnum previousRuleType = Rule.TypeEnum.Letter;
		String ruleString = "";
		Rule rule;
	
  		while ( enumerator.MoveNext() ) {
  			rule = (Rule)enumerator.Current;
  			switch(rule.Type) {
  				case Rule.TypeEnum.Expression: 
  					if(previousRuleType == Rule.TypeEnum.Letter) {
  						ruleString = ruleString + "(";
  					} else if(previousRuleType == Rule.TypeEnum.Expression) {
  						ruleString = ruleString + ",";
  					}
					ruleString = ruleString + ExpressionEvaluator.EvaluateStack(rule.Expression, null, globalParameters);
					previousRuleType = rule.Type;
					continue;
  				case Rule.TypeEnum.Parameter: 
  					if(previousRuleType == Rule.TypeEnum.Letter) {
  						ruleString = ruleString + "(";
  					} else if(previousRuleType == Rule.TypeEnum.Parameter) {
  						ruleString = ruleString + ",";
  					} 
  					break;
  				case Rule.TypeEnum.Letter: 
					switch(previousRuleType) {
						case Rule.TypeEnum.Letter:
							break;
						case Rule.TypeEnum.BranchStart:
							break;
						case Rule.TypeEnum.BranchEnd:
							break;
						default:	
							ruleString = ruleString + ")";
							break;
					}
  					break;
  				case Rule.TypeEnum.BranchStart: 
					if(previousRuleType == Rule.TypeEnum.Expression || previousRuleType == Rule.TypeEnum.Parameter) {
  						ruleString = ruleString + ")";
  					}
					ruleString = ruleString + "[";
					ruleString = ruleString + RuleListToString(rule.Branch, globalParameters); 
					previousRuleType = rule.Type;     			
					continue;						
  				case Rule.TypeEnum.BranchEnd: 
					if(previousRuleType == Rule.TypeEnum.Expression || previousRuleType == Rule.TypeEnum.Parameter) {
  						ruleString = ruleString + ")";
  					}
					break;
				default:
  					break;	
  			}
			ruleString = ruleString + rule.Value; 
			previousRuleType = rule.Type;     			
    	}
		
		switch(previousRuleType) {
			case Rule.TypeEnum.Letter:
				break;
			case Rule.TypeEnum.BranchStart:
				break;
			case Rule.TypeEnum.BranchEnd:
				break;
			default:	
				ruleString = ruleString + ")";
				break;
		}
		return ruleString;
	
	}
	
	
	public static int CopyUnmatchedToken(ArrayList oldList, ArrayList newList, int startIndex) {
		
//					Console.WriteLine("oldList: " + Rules.RuleListToString(oldList));	
//					Console.WriteLine("newList: " + Rules.RuleListToString(newList));	
//					Console.WriteLine("startIndex: " + startIndex);	

		
		newList.Add(((Rule)oldList[startIndex]).Clone());
//			newList.Add((Rule)oldList[startIndex]);
		startIndex++;

		
		return startIndex;
	}

	private static Hashtable GetLocalParameters(ArrayList oldRules, int startIndex, Hashtable globalParameters,
						    		ArrayList lhContext, ArrayList currentContext, ArrayList rhContext) {

		int index;
		double value;
		Hashtable localParameters = new Hashtable();
		Rule predecessor, oldRule;
		
		index  = startIndex - 1;
		
		/* go through the lhContext backards */
		for(int i=lhContext.Count-1; i >= 0; i--) {
			predecessor = (Rule)lhContext[i];
			if(predecessor.Type == Rule.TypeEnum.Parameter) {
					oldRule = (Rule)oldRules[index];			
					if(Rule.TypeEnum.Expression != oldRule.Type) {
							return null;
					}
					value = ExpressionEvaluator.EvaluateStack(oldRule.Expression,localParameters, globalParameters);
					localParameters.Add(predecessor.Value, value);
//			Console.WriteLine("local parameters " + predecessor.Value + " " + value);
			}
			index--;
		}
		
		index = startIndex;
		
		for(int i=0; i<currentContext.Count; i++) {
			predecessor = (Rule)currentContext[i];
			if(predecessor.Type == Rule.TypeEnum.Parameter) {
					oldRule = (Rule)oldRules[index];			
					if(Rule.TypeEnum.Expression != oldRule.Type) {
							return null;
					}
					value = ExpressionEvaluator.EvaluateStack(oldRule.Expression,localParameters, globalParameters);
					localParameters.Add(predecessor.Value, value);
//			Console.WriteLine("local parameters " + predecessor.Value + " " + value);
			}
			index++;
		}
	
		for(int i=0; i<rhContext.Count; i++) {
			predecessor = (Rule)rhContext[i];
			if(predecessor.Type == Rule.TypeEnum.Parameter) {
					oldRule = (Rule)oldRules[index];			
					if(Rule.TypeEnum.Expression != oldRule.Type) {
							return null;
					}
					value = ExpressionEvaluator.EvaluateStack(oldRule.Expression,localParameters, globalParameters);
					localParameters.Add(predecessor.Value, value);
		Console.WriteLine("local parameters " + predecessor.Value + " " + value);
			}
			index++;
		}
		
	
		return localParameters;

	}
}

	