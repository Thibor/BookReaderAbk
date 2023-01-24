using System;
using System.IO;
using System.Collections.Generic;
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
			bool isW = false;
			bool isInfo = false;
			bool trySearch = true;
			string lastFen = String.Empty;
			string lastMoves = String.Empty;
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
					case "-info":
						ax = ac;
						isInfo = true;
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
			Console.WriteLine($"info string {CBook.name} ver {CBook.version}");
			bool bookLoaded = SetBookFile(bookFile);
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
							if (uci.GetIndex("clear") > 0)
								book.header.Clear();
							book.ShowHeader();
							break;
						case "load":
							book.LoadFromFile(uci.GetValue("load"));
							Console.WriteLine($"moves {book.recList.Count:N0}");
							break;
						case "save":
							if (book.SaveToFile(uci.GetValue("save")))
								Console.WriteLine("The book has been saved");
							else
								Console.WriteLine("Writing to the file has failed");
							break;
						case "clear":
							book.Clear();
							Console.WriteLine("Book is empty");
							break;
						case "moves":
							book.InfoMoves(uci.GetValue("moves"));
							break;
						case "getoption":
							Console.WriteLine($"option name Book file type string default book{CBook.defExt}");
							Console.WriteLine("optionend");
							break;
						case "setoption":
							switch (uci.GetValue("name", "value").ToLower())
							{
								case "book file":
									SetBookFile(uci.GetValue("value"));
									break;
							}
							break;
					}
				}
				if ((uci.command != "go") && (engineProcess != null))
					engineProcess.StandardInput.WriteLine(msg);
				switch (uci.command)
				{
					case "position":
						lastFen = uci.GetValue("fen", "moves");
						lastMoves = uci.GetValue("moves", "fen");
						if ((lastFen.Length == 0) && (lastMoves.Length < 5))
							trySearch = true;
						if (lastFen.Length > 0)
							trySearch = false;
						break;
					case "go":
						string move = String.Empty;
						if (trySearch)
							move = book.GetMove(lastMoves);
						if (!String.IsNullOrEmpty(move))
							Console.WriteLine($"bestmove {move}");
						else
						{
							trySearch = false;
							if (engineProcess == null)
								Console.WriteLine("enginemove");
							else
								engineProcess.StandardInput.WriteLine(msg);
						}
						break;
				}
			} while (uci.command != "quit");

			bool SetBookFile(string bn)
			{
				bookFile = bn;
				bookLoaded = book.LoadFromFile(bookFile);
				if (bookLoaded)
				{
					if ((book.recList.Count > 0) && File.Exists(book.path))
						Console.WriteLine($"info string book on {book.recList.Count:N0} moves 224 bpm");
					if (isW)
						Console.WriteLine($"info string write on");
					if (isInfo)
						book.ShowInfo();
				}
				else
					isW = false;
				return bookLoaded;
			}
		}
	}
}
