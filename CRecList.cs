using System;
using System.Collections.Generic;
using System.IO;

namespace BookReaderAbk
{
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

		public int GetValue()
		{
			int value=games+win-loose;
			if(value <1)
				value=1;
			return value;
		}

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

		public void SaveToBinaryWriter(BinaryWriter bw)
		{
			bw.Write(fr);
			bw.Write(to);
			bw.Write(promotion);
			bw.Write(priority);
			bw.Write(games);
			bw.Write(win);
			bw.Write(loose);
			bw.Write(ply);
			bw.Write(nextMove);
			bw.Write(nextSibling);
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

	internal class CRecList:List<CRec>
	{
		public void SortGames()
		{
			Sort(delegate (CRec r1, CRec r2)
			{
				return r2.games - r1.games;
			});
		}

	}
}
