using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NSUci;

namespace BookReaderAbk
{

	internal class Program
	{
		public static CUci uci = new CUci();
		public static CBook book = new CBook();

		static void Main(string[] args)
		{
			int emptyTotal = 0;
			string lastFen = String.Empty;
			string lastMoves = String.Empty;
			Console.WriteLine($"info string book {CBook.name} ver {CBook.version}");
			string ax = "-bf";
			List<string> listBf = new List<string>();
			List<string> listEf = new List<string>();
			List<string> listEa = new List<string>();
			for (int n = 0; n < args.Length; n++)
			{
				string ac = args[n];
				switch (ac)
				{
					case "-bf"://book file
					case "-ef"://engine file
					case "-ea"://engine arguments
						ax = ac;
						break;
					default:
						switch (ax)
						{
							case "-bf":
								listBf.Add(ac);
								break;
							case "-ef":
								listEf.Add(ac);
								break;
							case "-ea":
								listEa.Add(ac);
								break;
						}
						break;
				}
			}
			string bookFile = String.Join(" ", listBf);
			string engineFile = String.Join(" ", listEf);
			string engineArguments = String.Join(" ", listEa);
			bool bookLoaded = book.LoadFromFile(bookFile);
			if (bookLoaded)
				Console.WriteLine($"info string book on");
			Process engineProcess = null;
			if (File.Exists(engineFile))
			{
				engineProcess = new Process();
				engineProcess.StartInfo.FileName = engineFile;
				engineProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(engineFile);
				engineProcess.StartInfo.UseShellExecute = false;
				engineProcess.StartInfo.RedirectStandardInput = true;
				engineProcess.StartInfo.Arguments = engineArguments;
				engineProcess.Start();
				Console.WriteLine($"info string engine on");
			}
			else if (engineFile != String.Empty)
				Console.WriteLine($"info string missing engine  [{engineFile}]");

			do
			{
				string msg = Console.ReadLine().Trim();
				uci.SetMsg(msg);
				if (uci.command == "book")
				{
					switch (uci.tokens[1])
					{
						case "header":
							book.ShowHeader();
							break;
					}
				}
				if ((uci.command != "go") && (engineProcess != null))
					engineProcess.StandardInput.WriteLine(msg);
				switch (uci.command)
				{
					case "ucinewgame":
						emptyTotal = 0;
						break;
					case "position":
						lastFen = uci.GetValue("fen", "moves");
						lastMoves = uci.GetValue("moves", "fen");
						break;
					case "go":
						string move = String.Empty;
						if((emptyTotal == 0) && String.IsNullOrEmpty(lastFen))
							move = book.GetMove(lastMoves);
						if (!String.IsNullOrEmpty(move))
							Console.WriteLine($"bestmove {move}");
						else
						{
							emptyTotal++;
							if (engineProcess == null)
								Console.WriteLine("enginemove");
							else
								engineProcess.StandardInput.WriteLine(msg);
						}
						break;
				}
			} while (uci.command != "quit");
		}
	}
}
