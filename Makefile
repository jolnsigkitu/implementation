GRAMMAR = ITU.Lang.Core/Lang.g4

grammar: $(GRAMMAR)
	antlr4 -Dlanguage=CSharp $(GRAMMAR)
