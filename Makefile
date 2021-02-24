GRAMMAR = ITU.Lang.Core/Lang.g4

grammar: $(GRAMMAR)
	java -Xmx500M org.antlr.v4.Tool -Dlanguage=CSharp $(GRAMMAR)
