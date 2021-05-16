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
	| returnStatement
	| usingStatement;

inlineStatement: assign | vardec | typedec | expr;
semiStatement: inlineStatement Semi;

returnStatement: Return expr Semi;

usingStatement: Using nestedName Semi;

// Value Expressions
expr: term | expr operator expr | operator expr | expr operator;

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

literal: number | bool | stringLiteral | charLiteral;

nestedName: Name (Dot Name)*;

access: (
		instantiateObject
		| invokeFunction
		| Name
		| LeftParen expr RightParen
	) accessChain?;

accessChain: Dot (invokeFunction | Name) accessChain?;

vardec: Extern? (Const | Let) typedName Eq expr;

assign: nestedName Eq expr;

bool: True | False;

number: Double | Int;

stringLiteral: StringLiteral;

charLiteral: CharLiteral;

functionParameterList:
	LeftParen functionArguments RightParen (
		Colon Void
		| typeAnnotation
	)?;
blockFunction: genericHandle? functionParameterList block;
lambdaFunction:
	genericHandle? functionParameterList FatArrow expr;
function: blockFunction | lambdaFunction;

genericHandle: LessThan (Name Comma)* Name GreaterThan;

functionArguments:
	| (Name typeAnnotation Comma)* (Name typeAnnotation)?;

invokeFunction:
	Name genericHandle? LeftParen arguments RightParen;

instantiateObject:
	New nestedName genericHandle? (
		LeftParen arguments RightParen
	)?;

arguments: (expr Comma)* (expr)?;

typedName: Name typeAnnotation?;
typeAnnotation: Colon typeExpr;

// Type expressions
typedec: Extern? Type Name Eq (typeExpr | classExpr);

typeExpr: typeRef | funcTypeExpr;

genericTypeHandle:
	LessThan (typeRef Comma)* typeRef GreaterThan;

typeRef: Name genericTypeHandle?;

classExpr: genericHandle? LeftBrace classMember* RightBrace;

funcTypeExpr:
	genericHandle? funcTypeExprParamList FatArrow (
		typeExpr
		| Void
	);

funcTypeExprParamList:
	LeftParen (typeExpr Comma)* typeExpr? RightParen;

classMember:
	Name ((typeAnnotation)? Eq)? blockFunction Semi?
	| Name ((typeAnnotation)? Eq)? lambdaFunction Semi
	| Name typeAnnotation (Eq expr)? Semi
	| Name Eq expr Semi;

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

Type: 'type';
Void: 'void';

Extern: 'extern';
Using: 'using';

Int: [0-9]+;
Double: Int 'd' | Int? Dot Int 'd'?;

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
