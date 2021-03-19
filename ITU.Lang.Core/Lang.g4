grammar Lang;

/* RULES */

prog: statements? EOF;

// Statements
statements: (statement)+;

statement:
	semiStatement
	| ifStatement
	| forStatement
	| whileStatement
	| doWhileStatement
	| loopStatement
	| returnStatement;

inlineStatement: assign | vardec | expr;
semiStatement: inlineStatement Semi;

returnStatement: Return expr Semi;

// Values/Expressions
expr:
	term
	| invokeFunction
	| instantiateObject
	| expr operator expr
	| operator expr
	| expr operator
	| LeftParen expr RightParen;

operator:
	(
		Star
		| Slash
		| Percent
		| Plus
		| Minus
		| And
		| Pipe
		| Hat
		| ExPoint
		| Eq
		| LessThan
		| GreaterThan
	)+;

term: literal | access | function;

literal: integer | bool | stringLiteral | charLiteral;

access: Name;

vardec: (Const | Let) typedName Eq expr;

assign: Name Eq expr;

bool: True | False;

integer: Int;

stringLiteral: StringLiteral;

charLiteral: CharLiteral;

function:
	LeftParen functionArguments RightParen typeAnnotation? FatArrow (
		block
		| expr
	);

functionArguments:
	| (Name typeAnnotation Comma)* (Name typeAnnotation)?;

invokeFunction:
	Name LeftParen arguments RightParen; // f(), LONG_FUNCTION_NAME(a,37, 4+9)

instantiateObject: New Name (LeftParen arguments RightParen)?;

arguments: (expr Comma)* (expr)?;

typedName: Name typeAnnotation?;
typeAnnotation: Colon Name;

// Concrete statements
block: LeftBrace statements? RightBrace;
ifStatement:
	If LeftParen expr RightParen block elseIfStatement* elseStatement?;
elseIfStatement: Elseif LeftParen expr RightParen block;
elseStatement: Else block;

forStatement:
	For LeftParen forDecStatement? Semi forConExpression? Semi forIncStatement? RightParen (
		block
		| statement
	);
forDecStatement: inlineStatement;
forConExpression: expr;
forIncStatement: inlineStatement;

whileStatement:
	While LeftParen expr RightParen (block | statement);

doWhileStatement: Do block While LeftParen expr RightParen Semi;

loopStatement: Loop (block | statement);

/* SYMBOLS */
Semi: ';';
Colon: ':';
Comma: ',';
Dot: '.';

Eq: '=';

Plus: '+'; // Operators
Minus: '-';
Star: '*';
Slash: '/';
Percent: '%';
And: '&';
Pipe: '|';
Hat: '^';
ExPoint: '!';
LessThan: '<';
GreaterThan: '>';

SingleQuote: '\'';
DoubleQuote: '"';

FatArrow: '=>';

LeftParen: '(';
RightParen: ')';
LeftBracket: '[';
RightBracket: ']';
LeftBrace: '{';
RightBrace: '}';

/* KEYWORDS */
False: 'false';
True: 'true';

Const: 'const';
Let: 'let';

While: 'while';
Do: 'do';
For: 'for';
Loop: 'loop';

Return: 'return';

If: 'if';
Else: 'else';
Elseif: Else ' '? If;

New: 'new';

Int: [0-9]+;
// TODO: Investigate viability of using UnicodeChar/UnicodeAlpha here, to allow unicode variable names
Name: [a-zA-Z_@$][a-zA-Z_@$0-9]*;

CharLiteral:
	SingleQuote (~['\\\r\n\u0085\u2028\u2029] | EscapeSequence) SingleQuote;
StringLiteral:
	DoubleQuote (~["\\\r\n\u0085\u2028\u2029] | EscapeSequence)* DoubleQuote;

// Inherited directly from C# Gramma
EscapeSequence:
	'\\\''
	| '\\"'
	| '\\\\'
	| '\\0'
	| '\\a'
	| '\\b'
	| '\\f'
	| '\\n'
	| '\\r'
	| '\\t'
	| '\\v';

// NewLine: [\r\n];
WhiteSpace: [ \r\t\n]+ -> skip;

/* COMMENTS */

MultiLineComment: '/*' .*? '*/' -> channel(HIDDEN);
SingleLineComment: '//' ~[\r\n]* -> channel(HIDDEN);
