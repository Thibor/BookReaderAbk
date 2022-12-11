using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSChess;

namespace BookReaderAbk
{
	class CHeader
	{
		public uint header;
		public int bytesInHeader;
		public int bytesPerMove;
		public byte commentLength;
		public byte[] comment = new byte[120];
		public byte authorLength;
		public byte[] author = new byte[80];
		public int depth;
		public int moves;
		public int minGames;
		public int minWin;
		public int proWinWhite;
		public int proWinBlack;
		public int probabilityPriority;
		public int probabilityGames;
		public int probabilityWin;
		public int halfMoves;
		public byte[] filter = new byte[24946];

		public void Clear()
		{
			header = 0x0341424b;
			bytesInHeader = 25200;
			bytesPerMove = 28;
			commentLength = 0;
			Array.Clear(comment, 0, comment.Length);
			authorLength = 0;
			Array.Clear(author, 0, author.Length);
			depth = 25;
			moves = 0;
			minGames = 1;
			proWinWhite = 0;
			proWinBlack = 0;
			probabilityPriority = 15;
			probabilityGames = 15;
			probabilityWin = 15;
			halfMoves = 25;
			for (int n = 0; n < filter.Length; n++)
				filter[n] = 0x78;
		}

		public void Fill()
		{
			Clear();
			moves = Program.book.Count;
		}

		public void LoadFromBinaryReader(BinaryReader br)
		{
			header = br.ReadUInt32();
			bytesInHeader = br.ReadInt32();
			bytesPerMove = br.ReadInt32();
			commentLength = br.ReadByte();
			comment = br.ReadBytes(comment.Length);
			authorLength = br.ReadByte();
			author = br.ReadBytes(author.Length);
			depth = br.ReadInt32();
			moves = br.ReadInt32();
			minGames = br.ReadInt32();
			minWin = br.ReadInt32();
			proWinWhite = br.ReadInt32();
			proWinBlack = br.ReadInt32();
			probabilityPriority = br.ReadInt32();
			probabilityGames = br.ReadInt32();
			probabilityWin = br.ReadInt32();
			halfMoves = br.ReadInt32();
			filter = br.ReadBytes(filter.Length);
		}

		public void SaveToBinaryWriter(BinaryWriter bw)
		{
			bw.Write(header);
			bw.Write(bytesInHeader);
			bw.Write(bytesPerMove);
			bw.Write(commentLength);
			bw.Write(comment);
			bw.Write(authorLength);
			bw.Write(author);
			bw.Write(depth);
			bw.Write(moves);
			bw.Write(minGames);
			bw.Write(minWin);
			bw.Write(proWinWhite);
			bw.Write(proWinBlack);
			bw.Write(probabilityPriority);
			bw.Write(probabilityGames);
			bw.Write(probabilityWin);
			bw.Write(halfMoves);
			bw.Write(filter);
		}

		public string Author()
		{
			string result = String.Empty;
			for (int n = 0; n < authorLength; n++)
				result += (char)author[n];
			return result;
		}

		public string Comment()
		{
			string result = String.Empty;
			for (int n = 0; n < commentLength; n++)
				result += (char)comment[n];
			return result;
		}

	}

	internal class CBook : List<CRec>
	{
		public const string name = "BookReaderAbk";
		public const string version = "2022-11-04";
		public static Random rnd = new Random();
		public CHeader header = new CHeader();
		readonly CChess chess = new CChess();

		public CRec GetRec(int position)
		{
			foreach (CRec rec in this)
				if (rec.position == position)
					return rec;
			return null;
		}

		public CRec GetRec(CRec rec, string umo)
		{
			if (rec.GetUci() == umo)
				return rec;
			rec = GetRec(rec.nextSibling * 28);
			if (rec == null)
				return rec;
			return GetRec(rec, umo);
		}

		public CRecList GetSiblings(CRec rec)
		{
			CRecList list = new CRecList();
			while (rec != null)
			{
				list.Add(rec);
				rec = GetRec(rec.nextSibling * 28);
			}
			return list;
		}

		public void ShowHeader()
		{
			Console.WriteLine($"bytes per move {header.bytesPerMove}");
			Console.WriteLine($"depth {header.depth}");
			Console.WriteLine($"moves {header.moves}");
			Console.WriteLine($"minimum number of games {header.minGames}");
			Console.WriteLine($"minimum number of wins {header.minWin}");
			Console.WriteLine($"procent win white {header.proWinWhite}");
			Console.WriteLine($"procent win black {header.proWinBlack}");
			Console.WriteLine($"probability priority {header.probabilityPriority}");
			Console.WriteLine($"probability games {header.probabilityGames}");
			Console.WriteLine($"probability win {header.probabilityWin}");
			Console.WriteLine($"use book to half move {header.halfMoves}");
			Console.WriteLine($"author {header.Author()}");
			Console.WriteLine($"comment {header.Comment()}");
		}

		CRecList GetMoves(string moves)
		{
			CRecList list = new CRecList();
			if (Count == 0)
				return list;
			CRec rec = this[0];
			string[] am = moves.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string m in am)
			{
				rec = GetRec(rec, m);
				if (rec == null)
					return list;
				rec = GetRec(rec.nextMove * 28);
				if (rec == null)
					return list;
			}
			return GetSiblings(rec);
		}

		public string GetMove(string moves)
		{
			CRecList list = GetMoves(moves);
			if (list.Count == 0)
				return String.Empty;
			return list[rnd.Next(list.Count)].GetUci();
		}

		public bool LoadFromFile(string path)
		{
			Clear();
			if (!File.Exists(path))
				return false;
			using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (BinaryReader reader = new BinaryReader(fs))
			{
				header.LoadFromBinaryReader(reader);
				while (reader.BaseStream.Position != reader.BaseStream.Length)
				{
					CRec rec = new CRec();
					rec.position = reader.BaseStream.Position;
					rec.LoadFromBinaryReader(reader);
					Add(rec);
				}
			}
			return true;
		}

		public bool SaveToFile(string path)
		{
			using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (BinaryWriter writer = new BinaryWriter(fs))
			{
				header.moves = Count;
				header.SaveToBinaryWriter(writer);
				foreach (CRec rec in this)
					rec.SaveToBinaryWriter(writer);
			}
			return true;
		}

		public void InfoMoves(string moves = "")
		{
			chess.SetFen();
			if (!chess.MakeMoves(moves))
				Console.WriteLine("wrong moves");
			else
			{
				CRecList rl = GetMoves(moves);
				if (rl.Count == 0)
					Console.WriteLine("no moves found");
				else
				{
					Console.WriteLine("id move  games   win loose");
					Console.WriteLine();
					int i = 1;
					foreach (CRec r in rl)
					{
						Console.WriteLine(String.Format("{0,2} {1,-4} {2,6} {3,5} {4,5}", i++, r.GetUci(), r.games, r.win,r.loose));
					}
				}
			}
		}

		public void ShowInfo()
		{
			InfoMoves();
		}

	}
}
