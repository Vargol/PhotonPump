using System;
using System.Collections;
using System.Text.RegularExpressions;

public class Production {

	private ArrayList successor = new ArrayList();
	private ArrayList probability = new ArrayList();
	private Stack booleanExpression = new Stack();
	private int predecessorLength = 0;
	private double probabilitySoFar = 0.0;

	protected ArrayList rule = new ArrayList();
	protected ArrayList lhContext = new ArrayList();
	protected ArrayList rhContext = new ArrayList();
	
	public ArrayList Rule {
		get {return rule;}
	}

	public ArrayList LhContext {
		get {return lhContext;}
	}

	public ArrayList RhContext {
		get {return rhContext;}
	}

	public int PredecessorLength {
		get {return predecessorLength;}
	}
	
	public Stack BooleanExpression {
		get {return booleanExpression;}
	}


	public ArrayList GetSuccessor(double probability) {
		
		int i;
		
		for(i=0; i<this.probability.Count-1; i++) {
			if((double)this.probability[i] <= probability && (double)this.probability[i+1] > probability) {
				return (ArrayList)successor[i];
			}
        }
        
        /* by default return the last entry */
		return (ArrayList)successor[i];
	}		
	
	public void AddSuccessor(ArrayList successor, double probability) {
	
		this.successor.Add(successor);
		probabilitySoFar += probability;
		if(probabilitySoFar > 1.0) {
			/* throw an error or something */
		}
		this.probability.Add(probabilitySoFar);
	}

	public void SetPredecessor(ArrayList lhContext, ArrayList rule, ArrayList rhContext, Stack booleanExpression) {
		this.lhContext = lhContext;
		this.rule = rule;
		this.rhContext = rhContext;
		this.booleanExpression = booleanExpression;
		predecessorLength = rule.Count;
	}

	public bool IsSameProduction (ArrayList lhContext, ArrayList rule, ArrayList rhContext, Stack booleanExpression) {
		
		
//			Console.WriteLine("lhContext " + lhContext );	
//			Console.WriteLine("rule " + rule );	
//			Console.WriteLine("rhContext " + rhContext );	
//			Console.WriteLine("booleanExpression " + booleanExpression );	

		/* get the quick checks out of the way */
		if(rule.Count != this.rule.Count 
			|| booleanExpression.Count != this.booleanExpression.Count
			|| lhContext.Count != this.lhContext.Count
			|| rhContext.Count != this.rhContext.Count) {
			return false;
		}
		
		/* Do a detailed check now we know all the collections have matching sizes */
		
		if(Rules.SameRuleOnLists(this.rule, rule) == false) {
			return false;
		}
		
		if(ExpressionEvaluator.SameExpressions(this.booleanExpression, booleanExpression) == false) {
			return false;
		}

		if(Rules.SameRuleOnLists(this.lhContext, lhContext) == false) {
			return false;
		}
		
		if(Rules.SameRuleOnLists(this.rhContext, rhContext) == false) {
			return false;
		}
		
		/* guess its all the same then */
		return true;
	}	
}

class Productions {

	
	static Regex removeSpaces = new Regex(@"\s");
	static Regex stochastic = new Regex(@"(.+)->\((\d*\.\d*)\)(.+)");
	static Regex notStochastic = new Regex(@"(.+)->(.+)");
	
/*		private String predecessor;
	private String successor;
	private double probability;
*/
	protected ArrayList productionList = new ArrayList();
	
	public ArrayList ProductionList {
		get { return productionList; }
	}
		
	public bool AddProduction (String production) {
	
		String predecessor = "";
		String successor = "";
		double probability = 0.0;
	
		ArrayList strictPredecessor = new ArrayList();
		ArrayList lhContext = new ArrayList();
		ArrayList rhContext = new ArrayList();
		Stack booleanExpression = new Stack();
		
		/* 
			split the rule down 
		
			First check if its Stochastic or not
		*/
		production = removeSpaces.Replace(production, "");
	
		Match stochasticRule = stochastic.Match(production);
		
		if(stochasticRule.Success) {
			predecessor = stochasticRule.Groups[1].Value;
			successor = stochasticRule.Groups[3].Value;
			probability = Double.Parse(stochasticRule.Groups[2].Value);
		} else {
			Match nonStochasticRule = notStochastic.Match(production);
			if(nonStochasticRule.Success) {
				predecessor = nonStochasticRule.Groups[1].Value;
				successor = nonStochasticRule.Groups[2].Value;
				probability = 1.0;
			} else {

				Console.WriteLine("can not parse non Stochastic Production " + production );	
				return false;
				
				/* I don't know what it is */
			}
		}
	
		/* now split up the predecessor */
		String[] predecessorSplit = predecessor.Split(new Char[] {':'});
		predecessor = predecessorSplit[0];
		if(predecessorSplit.Length > 1) {
			Console.WriteLine("\tFound boolean test " + predecessorSplit[1]);	
			if(predecessorSplit[1] != "*" ) {
				booleanExpression = ExpressionEvaluator.ParseExpression(predecessorSplit[1]);
			}
		}
		/* 
			now check for context deals with the predecessor in more detail 
		*/
		
		predecessorSplit = predecessor.Split(new Char[] {'<', '>'});
		
		int index = predecessor.IndexOf('<');
		if(index != -1) {
			Console.WriteLine("\tFound left hand context " + predecessorSplit[0]);	
			lhContext = Rules.ConvertContextToRuleList(predecessorSplit[0]);
			Console.WriteLine("\tFound strict predecessor rule " + predecessorSplit[1]);	
			strictPredecessor = Rules.ConvertPredecessorToRuleList(predecessorSplit[1]);
		} else {
			Console.WriteLine("\tFound strict predecessor rule " + predecessorSplit[0]);	
			strictPredecessor = Rules.ConvertPredecessorToRuleList(predecessorSplit[0]);
		}
		
		index = predecessor.IndexOf('>');
		
		if(index != -1) {
			Console.WriteLine("\tFound right hand context " + predecessorSplit[predecessorSplit.Length - 1]);	
			rhContext = Rules.ConvertContextToRuleList(predecessorSplit[predecessorSplit.Length - 1]);
		} 
		
		bool found = false;
		
		foreach (Production knownProduction in productionList) {
			if(knownProduction.IsSameProduction (lhContext, strictPredecessor, rhContext, booleanExpression)) {
				knownProduction.AddSuccessor(Rules.ConvertSuccessorToRuleList(successor), probability);
				Console.WriteLine("Seen this predecessor before" + predecessor);	
				found = true;
				break;
			}
		}
		
		/* we've not seen this one before so add a new production to the list */
		if(found == false) {
			Production newProduction = new Production();
			newProduction.SetPredecessor(lhContext, strictPredecessor, rhContext, booleanExpression);
			newProduction.AddSuccessor(Rules.ConvertSuccessorToRuleList(successor), probability);
			productionList.Add(newProduction);
		}
		
		return true;
	}
}
