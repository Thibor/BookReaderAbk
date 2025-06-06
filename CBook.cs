﻿using NSChess;
using System;
using System.Collections.Generic;
using System.IO;

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
            moves = Program.book.recList.Count;
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

    internal class CBook
    {
        public const string name = "BookReaderAbk";
        public const string version = "2022-11-04";
        public const string defExt = ".abk";
        public string path = string.Empty;
        public static Random rnd = new Random();
        public CHeader header = new CHeader();
        public CRecList recList = new CRecList();
        readonly CChess chess = new CChess();

        public void Clear()
        {
            header.Clear();
            recList.Clear();
        }

        public CRec GetRec(int position)
        {
            foreach (CRec rec in recList)
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
            if (recList.Count == 0)
                return list;
            CRec rec = recList[0];
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

        string GetMove(CRecList rl)
        {
            string move = String.Empty;
            int w = 0;
            foreach (CRec r in rl)
            {
                int v = r.GetValue();
                w += v;
                if (CChess.rnd.Next(w) < v)
                    move = r.GetUci();
            }
            return move;
        }

        public string GetMove(string moves)
        {
            CRecList list = GetMoves(moves);
            return GetMove(list);
        }

        public bool LoadFromFile(string p)
        {
            path = p;
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
                    recList.Add(rec);
                }
            }
            return true;
        }

        public bool SaveToFile(string p)
        {
            string ext = Path.GetExtension(p).ToLower();
            if (ext == defExt)
                return SaveToAbk(p);
            if (ext == ".uci")
                return SaveToUci(p);
            return false;
        }

        public bool SaveToAbk(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                header.moves = recList.Count;
                header.SaveToBinaryWriter(writer);
                foreach (CRec rec in recList)
                    rec.SaveToBinaryWriter(writer);
            }
            return true;
        }

        public bool SaveToUci(string p)
        {
            List<string> sl = GetGames();
            FileStream fs = File.Open(p, FileMode.Create, FileAccess.Write, FileShare.None);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (String uci in sl)
                    sw.WriteLine(uci);
            }
            return true;
        }

        void GetGames(string moves, ref List<string> list)
        {
            CRecList rl = GetMoves(moves);
            if (rl.Count == 0)
            {
                list.Add(moves);
                Console.Write($"\rgame {list.Count}");
            }
            foreach (CRec rec in rl)
                GetGames($"{moves} {rec.GetUci()}".Trim(), ref list);
        }

        List<string> GetGames()
        {
            List<string> sl = new List<string>();
            GetGames("", ref sl);
            Console.WriteLine();
            Console.WriteLine("finish");
            Console.Beep();
            sl.Sort();
            return sl;
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
                    rl.SortGames();
                    string mask = "{0,2} {1,-5} {2,6} {3,6} {4,6}";
                    Console.WriteLine();
                    Console.WriteLine(mask, "id", "move", "games", "win", " loose");
                    int i = 1;
                    foreach (CRec r in rl)
                    {
                        Console.WriteLine(String.Format(mask, i++, r.GetUci(), r.games, r.win, r.loose));
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
