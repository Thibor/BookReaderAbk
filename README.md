# BookReaderAbk
BoookReaderAbk can be used as normal UCI chess engine in chess GUI like Arena.
This program reads chess openings moves from Arena openig book with abk extension.
To use this program you need install  <a href="https://dotnet.microsoft.com/download/dotnet-framework/net48">.NET Framework 4.8</a>

## Parameters

**-bf** chess opening Book File name<br/>
**-ef** chess Engine File name<br/>
**-ea** chess Engine Arguments<br/>
**-info** show additional INFOrmation<br/>

## Console commands

**book load** [filename].[abk] - clear and add<br/>
**book save** [filename].[abk] - save book to the file<br/>
**book clear** - clear all moves from the book<br/>
**book moves** [uci] - make sequence of moves in uci format and shows possible continuations<br/>
**book header** - show header of current book<br/>
**book info** - show additional information<br/>
**book getoption** - show options<br/>
**book setoption name [option name] value [option value]** - set option<br/>
**quit** quit the program as soon as possible

### Parameters examples

-bf book.abk -ef stockfish.exe<br/>
book -ef stockfish.exe

The program will first try to find move in chess opening book named book.abk, and if it doesn't find any move in it, it will run a chess engine named stockfish.exe 


