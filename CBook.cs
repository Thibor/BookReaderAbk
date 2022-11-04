using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookReaderAbk
{
	class CHeader
	{
		public int header;
		public int bytesInHeader;
		public int bytesPerMove;
		public byte commentLength;
		public char[] comment = new char[120];
		public byte authorLength;
		public char[] author = new char[80];
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

		public void LoadFromBinaryReader(BinaryReader br)
		{
			header = br.ReadInt32();
			bytesInHeader = br.ReadInt32();
			bytesPerMove = br.ReadInt32();
			commentLength = br.ReadByte();
			comment = br.ReadChars(comment.Length);
			authorLength = br.ReadByte();
			author = br.ReadChars(author.Length);
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

		public string Author()
		{
			string result = String.Empty;
			for (int n = 0; n < authorLength; n++)
				result += author[n];
			return result;
		}

		public string Comment()
		{
			string result = String.Empty;
			for (int n = 0; n < commentLength; n++)
				result += comment[n];
			return result;
		}

	}

	class CRec
	{
		public byte fr;
		public byte to;
		public byte promotion;
		public byte priority;
		public int games;
		public int win;
		public int loose;
		public int ply;
		public int nextMove;
		public int nextSibling;

		public long position;

		public void LoadFromBinaryReader(BinaryReader br)
		{
			fr = br.ReadByte();
			to = br.ReadByte();
			promotion = br.ReadByte();
			priority = br.ReadByte();
			games = br.ReadInt32();
			win = br.ReadInt32();
			loose = br.ReadInt32();
			ply = br.ReadInt32();
			nextMove = br.ReadInt32();
			nextSibling = br.ReadInt32();
		}

		string SquareToStr(byte square)
		{
			int y = square >> 3;
			int x = square & 7;
			string file = "abcdefgh";
			string rank = "12345678";
			return $"{file[x]}{rank[y]}";
		}

		string PromotionToStr(byte p)
		{
			switch (p)
			{
				case 1:
					return "r";
				case 2:
					return "n";
				case 3:
					return "b";
				case 4:
					return "q";
				default:
					return String.Empty;
			}
		}

		public string GetUci()
		{
			return SquareToStr(fr) + SquareToStr(to) + PromotionToStr(promotion);
		}

	}

	internal class CBook : List<CRec>
	{
		public const string name = "BookReaderAbk";
		public const string version = "2022-11-04";
		public static Random rnd = new Random();
		readonly CHeader header = new CHeader();

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

		public List<CRec> GetSiblings(CRec rec)
		{
			List<CRec> list = new List<CRec>();
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

		public string GetMove(string moves, CRec rec = null)
		{
			if (Count == 0)
				return String.Empty;
			if (rec == null)
				rec = this[0];
			string[] am = moves.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string m in am)
			{
				rec = GetRec(rec, m);
				if (rec == null)
					return String.Empty;
				rec = GetRec(rec.nextMove * 28);
				if (rec == null)
					return String.Empty;
			}
			List<CRec> list = GetSiblings(rec);
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

	}
}
