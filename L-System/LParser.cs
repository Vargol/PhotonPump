using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

public class LParser {

	Hashtable _globalParameters = new Hashtable();
	Hashtable _ignoreHash = new Hashtable();
	ArrayList _axiom = new ArrayList();
	Productions _productions = new Productions();
	Random _random;

	
	int _recursion 	  = 5;
	double _thickness = 0.1;
	double _angle = 30.0;
	 
	static Regex parameter 	= new Regex(@"^\#define\s+(?'parameter'.+)\s+(?'value'[+-]?\d+\.?\d*)");
	static Regex axiom 		= new Regex(@"^#axiom\s+(?'axiom'.+)");
	static Regex recursion  = new Regex(@"^#recursion\s+(?'recursion'\d+)");
	static Regex thickness  = new Regex(@"^#thickness\s+(?'thickness'\d+\.?\d*)");
	static Regex seed  		= new Regex(@"^#seed\s+(?'seed'\d+)");
	static Regex ignore  	= new Regex(@"^#ignore\s+(?'ignore'.+)");
	static Regex angle  	= new Regex(@"^#angle\s+(?'angle'[+-]?\d+\.?\d*)");
	static Regex comment  	= new Regex(@"^\/\*");

	public LParser() {}
	
	public double Angle {
		get {return _angle;}
		set {_angle = value;}
	}
	

	public double Thickness {
		get {return _thickness;}
	}

	public int Recursion {
		get {return _recursion;}
		set {_recursion = value;}
	}

	public Hashtable GlobalParameters {
		get {return _globalParameters;}
	}
	
					
	public ArrayList readLSFile(string filename) {
	
		string line;
		ArrayList lines = new ArrayList();
		
		try {
			StreamReader file = new StreamReader(filename);
			
			line = file.ReadLine();
			
			while(line != null) {
				if(line != "") {
					lines.Add(line);
				}
				line = file.ReadLine();
			}
			
			file.Close();
		} catch(IOException e) {
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
		}

		return lines;	
	}


	public bool ParseStringArrayList(ArrayList lines) {
	
		Match lineMatch;
		string line;
	
		for(int i=0; i<lines.Count; i++) {
			line = (string)(lines[i]);
			lineMatch = comment.Match(line);
			if(lineMatch.Success) {
				continue;
			}
			lineMatch = parameter.Match(line);
			if(lineMatch.Success) {
				Console.WriteLine("parsed parameter rule: " + line);	
				_globalParameters.Add(lineMatch.Groups["parameter"].Value, Double.Parse(lineMatch.Groups["value"].Value));
				continue;
			}

			lineMatch = axiom.Match(line);	
			if(lineMatch.Success) {
				Console.WriteLine("parsed axiom rule: " + line);	
				_axiom = Rules.ConvertSuccessorToRuleList(lineMatch.Groups["axiom"].Value);
				continue;
			}

			lineMatch = recursion.Match(line);
			if(lineMatch.Success) {
				Console.WriteLine("parsed recursion rule: " + line);	
				_recursion = Int32.Parse(lineMatch.Groups["recursion"].Value);
				continue;
			}
			
			lineMatch = thickness.Match(line);			
			if(lineMatch.Success) {
				Console.WriteLine("parsed thickness rule: " + line);	
				_thickness = Double.Parse(lineMatch.Groups["thickness"].Value);
				continue;
			}
			
			lineMatch = seed.Match(line);			
			if(lineMatch.Success) {
				Console.WriteLine("parsed seed rule: " + line);	
				_random = new Random(Int32.Parse(lineMatch.Groups["seed"].Value));
				continue;
			}
			
			lineMatch = angle.Match(line);
			if(lineMatch.Success) {
				Console.WriteLine("parsed angle rule: " + line);	
				_angle = Double.Parse(lineMatch.Groups["angle"].Value);
				continue;
			}

			lineMatch = ignore.Match(line);
			if(lineMatch.Success) {
				Console.WriteLine("parsed ignore rule: " + line);	
				String[] ignoreSplit = ((String)(lineMatch.Groups["ignore"].Value)).Split(new Char[] {' '});
				foreach (String ignoreString in ignoreSplit) {
					_ignoreHash[ignoreString] = true;
				Console.WriteLine("Adding to ignore hash: " + ignoreString);	
				}
				continue;
			}

			/* see if its a production */
			if(!_productions.AddProduction(line)) {
				Console.WriteLine("Can not parse rule: " + line);	
				return false;
			} else {
				Console.WriteLine("parsed production rule: " + line);	
			}
		}	
	
		if (null == _random) {
			_random = new Random();
		}
		
		return true;
	}
	
	public bool ParseStringArray(String[] lines) {
	
		ArrayList strings = new ArrayList(lines);

		return ParseStringArrayList(strings);
	}

	public ArrayList RunLSystem() {
	
		ArrayList oldList = _axiom;
		ArrayList newList = oldList;
		
		for(int i=0; i<_recursion; i++) {
			newList = new ArrayList();
			Console.WriteLine("_recursion: " + i);
			ParseRules(oldList, newList);	
			Console.WriteLine("End of _recursion: " + i);	
			Console.WriteLine("new rule : " + Rules.RuleListToString(newList, _globalParameters));	
			oldList = newList;	
		}
		
		return newList;
	} 
	

	private void ParseRules(ArrayList oldList, ArrayList newList) {
	
		ArrayList successor = new ArrayList();
		int startIndex = 0;
		bool foundAMatch;

			Console.WriteLine("ParseRules: " + oldList.Count);	
		
		
		IEnumerator enumerator = _productions.ProductionList.GetEnumerator();
		
		while (startIndex < oldList.Count) {	
				Console.WriteLine("matching : " +((Rule)oldList[startIndex]).Value);	
			foundAMatch = false;	
			if(Rule.TypeEnum.BranchStart == ((Rule)oldList[startIndex]).Type) {
				ArrayList branchList = new ArrayList();
//				Console.WriteLine("found branch : " + Rules.RuleListToString(branchList));	
				ParseRules(((Rule)oldList[startIndex]).Branch, branchList);
				newList.Add(new Rule(branchList, newList));
				startIndex++;
			} else {	
				while ( enumerator.MoveNext() ) {
					Production production = (Production)enumerator.Current;
					
					Console.WriteLine("Production: " + Rules.RuleListToString(production.Rule, _globalParameters));	

					
					if(Rules.MatchList(oldList, startIndex, production, _ignoreHash, _globalParameters)) {
						successor = production.GetSuccessor(_random.NextDouble());
						if(!Rules.RewriteUsingRule(oldList, startIndex, _globalParameters,
								  production.LhContext, production.Rule, production.RhContext,
								  successor, newList)) {
							
							// Throw a big error 		  
								  
						}
						
						
						startIndex += production.PredecessorLength;
						Console.WriteLine("Matched");
						foundAMatch = true;
						break;
					}
				}		
				if(!foundAMatch) {
					/* move the arrayList to the next Token copying as we go */
					startIndex = Rules.CopyUnmatchedToken(oldList, newList, startIndex);
				}
			}
			enumerator.Reset();
		}
		return;	

	} 





} /* end LParser */

