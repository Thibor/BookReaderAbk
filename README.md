# BookReaderAbk
BoookReaderAbk can be used as normal UCI chess engine in chess GUI like Arena.
This program reads chess openings moves from Arena openig book with abk extension.
To use this program you need install  <a href="https://dotnet.microsoft.com/download/dotnet-framework/net48">.NET Framework 4.8</a>

## Parameters

**-bf** chess opening Book File name<br/>
**-ef** chess Engine File name<br/>
**-ea** chess Engine Arguments<br/>

### Examples

-bf book.abk -ef stockfish.exe<br/>
book -ef stockfish.exe

The program will first try to find move in chess opening book named book.abk, and if it doesn't find any move in it, it will run a chess engine named stockfish.exe 


