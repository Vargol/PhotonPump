using System;
using System.Collections.Generic;
using System.IO;

namespace SunflowSharp.Systems
{

    public class Parser
    {
        //private FileStream file;
        private StreamReader bf;
        private string[] lineTokens;
        private int index;

        public Parser(string filename)
            : this(File.OpenRead(filename))
        {
        }

        public Parser(Stream stream)
        {
            bf = new StreamReader(stream);
            lineTokens = new string[0];
            index = 0;
        }

        public void close()
        {
            //if (file != null)
            //    file.close();
            bf.Close();
            bf = null;
        }

        public string getNextToken()
        {
            while (true)
            {
                string tok = fetchNextToken();
                if (tok == null)
                    return null;
                if (tok == "/*")
                {
                    do
                    {
                        tok = fetchNextToken();
                        if (tok == null)
                            return null;
                    } while (tok != "*/");
                }
                else
                    return tok;
            }
        }

        public bool peekNextToken(string tok)
        {
            while (true)
            {
                string t = fetchNextToken();
                if (t == null)
                    return false; // nothing left
                if (t == "/*")
                {
                    do
                    {
                        t = fetchNextToken();
                        if (t == null)
                            return false; // nothing left
                    } while (t != "*/");
                }
                else if (t == tok)
                {
                    // we found the right token, keep parsing
                    return true;
                }
                else
                {
                    // rewind the token so we can try again
                    index--;
                    return false;
                }
            }
        }

        private string fetchNextToken()
        {
            if (bf == null)
                return null;
            while (true)
            {
                if (index < lineTokens.Length)
                    return lineTokens[index++];
                else if (!getNextLine())
                    return null;
            }
        }

        private bool getNextLine()
        {
            string line = bf.ReadLine();

            if (line == null)
                return false;

            List<string> tokenList = new List<string>();
            string current = string.Empty;
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (current.Length == 0 && (c == '%' || c == '#'))
                    break;

                bool quote = c == '\"';
                inQuotes = inQuotes ^ quote;

                if (!quote && (inQuotes || !char.IsWhiteSpace(c)))
                    current += c;
                else if (current.Length > 0)
                {
                    tokenList.Add(current);
                    current = string.Empty;
                }
            }

            if (current.Length > 0)
                tokenList.Add(current);
            lineTokens = tokenList.ToArray();
            index = 0;
            return true;
        }

        public string getNextCodeBlock()
        {
            // read a java code block
            string code = string.Empty;
            checkNextToken("<code>");
            while (true)
            {
                string line;
                try
                {
                    line = bf.ReadLine();
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.StackTrace);
                    return null;
                }
                if (line.Trim() == "</code>")
                    return code;
                code += line + Environment.NewLine;
            }
        }

        public bool getNextbool()
        {
            return bool.Parse(getNextToken());
        }

        public int getNextInt()
        {
            return int.Parse(getNextToken());
        }

        public float getNextFloat()
        {
            return float.Parse(getNextToken());
        }

		public double getNextDouble()
		{
			return double.Parse(getNextToken());
		}

        public void checkNextToken(string token)
        {
            string found = getNextToken();
            if (token != found)
            {
                close();
                throw new ParserException(token, found);
            }
        }

        public class ParserException : Exception
        {
            public ParserException(string token, string found)
                : base(string.Format("Expecting {0} found {1}", token, found))
            {
            }
        }
    }
}