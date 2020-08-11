using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace IRWS_Term_Project
{
    public class Program
    {
		//stores distinct terms
        public static HashSet<string> distinctTerm = new HashSet<string>();
		//stores document id and its contents without splitting
        public static Dictionary<int, string> documentContentList = new Dictionary<int, string>();
		//stores document and its terms collection
        public static Dictionary<string, List<string>> documentCollection = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<int>> termDocumentIncidenceMatrix = new Dictionary<string, List<int>>();
		//stop words collection
        public static List<string> stopWords = new List<string> { "ON", "OF", "THE", "AN", "A" };
		//stores list of retrieved docs
        public static List<string> RetrievedDocs = new List<string>();
		//stores list of relevant docs
        public static List<string> RelevantDocs = new List<string>();
		//stores text files retrieved from five different categories.
        public static IEnumerable<string> Category1;
        public static IEnumerable<string> Category2;
        public static IEnumerable<string> Category3;
        public static IEnumerable<string> Category4;
        public static IEnumerable<string> Category5;
        public static void Main(string[] args)
        {
			//read query-document-relevance json file
            var myJString = File.ReadAllText("D:\\SkBansal_New\\Shrikrishn Bansal\\Mtech\\Sem-2\\Information retrieval & Web search\\project\\Query-Document-Relevance.json");
			//deserialize json file as a object
            dynamic myJObject = JsonConvert.DeserializeObject(myJString);
            int count = 0;
			//read all the text document on the specified directory;
            Category1 = Directory.EnumerateFiles("D:\\SkBansal_New\\Shrikrishn Bansal\\Mtech\\Sem-2\\Information retrieval & Web search\\project\\Documents\\Sports\\", "*.txt");
            Category2 = Directory.EnumerateFiles("D:\\SkBansal_New\\Shrikrishn Bansal\\Mtech\\Sem-2\\Information retrieval & Web search\\project\\Documents\\Science\\", "*.txt");
            Category3 = Directory.EnumerateFiles("D:\\SkBansal_New\\Shrikrishn Bansal\\Mtech\\Sem-2\\Information retrieval & Web search\\project\\Documents\\Entertainment\\", "*.txt");
            Category4 = Directory.EnumerateFiles("D:\\SkBansal_New\\Shrikrishn Bansal\\Mtech\\Sem-2\\Information retrieval & Web search\\project\\Documents\\Politics\\", "*.txt");
            Category5 = Directory.EnumerateFiles("D:\\SkBansal_New\\Shrikrishn Bansal\\Mtech\\Sem-2\\Information retrieval & Web search\\project\\Documents\\Technology\\", "*.txt");
			//concatenate all text files retrieved from different categories
            var allDocsCollection = Concatenate(Category1, Category2, Category3, Category4, Category5);
            foreach (string file in allDocsCollection)
            {
                string contents = File.ReadAllText(file);
                String[] termsCollection = RemoveStopsWords(contents.ToUpper().Split(' '));
                foreach (string term in termsCollection)
                {
					//prepeare distinct terms collection
                    //remove stop words
                    if (!stopWords.Contains(term))
                    {
                        distinctTerm.Add(term);
                    }
                }
				//add document and their terms collection
                documentCollection.Add(file, termsCollection.ToList());
				//add document and its content for displaying the search result
                string fileName = Path.GetFileNameWithoutExtension(file);
                documentContentList.Add(count, fileName);
                count++;
            }

            termDocumentIncidenceMatrix = GetTermDocumentIncidenceMatrix(distinctTerm, documentCollection);
			//user input quey
            Console.WriteLine("Enter your query here:");
            string query = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine("Based on your query retrieved documents are:");
            List<int> lst = ProcessQuery(query);
            count = 0;
			//print all retrieved docs and adding them to list also
            if (lst != null)
            {
                foreach (int a in lst)
                {
                    if (a == 1)
                    {
                        Console.WriteLine(documentContentList[count]);
                        RetrievedDocs.Add(documentContentList[count]);
                    }
                    count++;
                }
            }
            else
            {
                Console.WriteLine("No search result found");
            }
			//extract all relevant docs based on query from json file and store them to list
            if (myJObject[query] != null)
            {
                foreach (var item in myJObject[query])
                {
                    RelevantDocs.Add(item.ToString());
                }
				//finding intersection from both lists
                IEnumerable<string> CommonDocs = RetrievedDocs.AsQueryable().Intersect(RelevantDocs);
				//calculating Precision
                decimal Precision = (decimal)CommonDocs.Count() / (decimal)RetrievedDocs.Count();
				//calculating Recall
                decimal Recall = (decimal)CommonDocs.Count() / (decimal)RelevantDocs.Count();
				//calculating F1_Score
                decimal F1_Score = (2 * Precision * Recall) / (Precision + Recall);
                Console.WriteLine();
                Console.WriteLine("System performance is:");
                Console.WriteLine($"Precision:{String.Format("{0:0.##}", Precision)}");
                Console.WriteLine($"Recall:{String.Format("{0:0.##}", Recall)}");
                Console.WriteLine($"F1 Score:{String.Format("{0:0.##}", F1_Score)}");
            }
        }

		//concatenation of list
        public static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }
		
        private static void FilterQueryTerm(ref string[] str)
        {
            List<string> _queryTerm = new List<string>();


            foreach (string queryTerm in str)
            {
                if (termDocumentIncidenceMatrix.ContainsKey(queryTerm.ToUpper()))
                {
                    _queryTerm.Add(queryTerm);

                }
            }

            str = _queryTerm.ToArray();
        }

		//prepares Term Document Incidence Matrix
        public static Dictionary<string, List<int>> GetTermDocumentIncidenceMatrix(HashSet<string> distinctTerms, Dictionary<string, List<string>> documentCollection)
        {
            Dictionary<string, List<int>> termDocumentIncidenceMatrix = new Dictionary<string, List<int>>();
            List<int> incidenceVector = new List<int>();
            foreach (string term in distinctTerms)
            {
				//incidence vector for each terms
                incidenceVector = new List<int>();
                foreach (KeyValuePair<string, List<string>> p in documentCollection)
                {

                    if (p.Value.Contains(term))
                    {
						//document contains the term
                        incidenceVector.Add(1);

                    }
                    else
                    {
						//document do not contains the term
                        incidenceVector.Add(0);
                    }
                }
                termDocumentIncidenceMatrix.Add(term, incidenceVector);

            }
            return termDocumentIncidenceMatrix;
        }


		//removes all stop words
        public static string[] RemoveStopsWords(string[] str)
        {
            List<string> terms = new List<string>();
            foreach (string term in str)
            {
                if (!stopWords.Contains(term))
                {
                    terms.Add(term);
                }
            }
            return terms.ToArray();
        }

		//process the query
        public static List<int> ProcessQuery(string query)
        {

            string bitWiseOp = string.Empty;
            string[] queryTerm = RemoveStopsWords(query.ToUpper().Split(' '));

            FilterQueryTerm(ref queryTerm);
            List<int> previousTermIncidenceV = null;
            List<int> nextTermsIncidenceV = null;
            List<int> resultSet = null;
            Boolean hasPreviousTerm = false;
            foreach (string term in queryTerm)
            {
                if (!hasPreviousTerm)
                {
                    previousTermIncidenceV = GetTermIncidenceVector(term);
                    resultSet = previousTermIncidenceV;
                    hasPreviousTerm = true; 
                }
                else
                {
                    nextTermsIncidenceV = GetTermIncidenceVector(term);
                }

                if (nextTermsIncidenceV != null)
                {
                    resultSet = ProcessOperator(previousTermIncidenceV, nextTermsIncidenceV);
                    previousTermIncidenceV = resultSet;
                    hasPreviousTerm = true;
                    nextTermsIncidenceV = null;
                }
            }

            return resultSet;
        }

        public static List<int> ProcessOperator(List<int> previousTermV, List<int> nextTermV)
        {
            List<int> resultSet = new List<int>();
            for (int a = 0; a < previousTermV.Count; a++)
            {
                if (previousTermV[a] == 1 && nextTermV[a] == 1)
                {
                    resultSet.Add(1);
                }
                else
                {
                    resultSet.Add(0);
                }
            }
            return resultSet;
        }


        public static List<int> GetTermIncidenceVector(string term)
        {
            return termDocumentIncidenceMatrix[term.ToUpper()];

        }
    }
}