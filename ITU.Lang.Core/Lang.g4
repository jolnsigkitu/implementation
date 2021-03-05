grammar Lang;

/* RULES */

prog: statements? EOF;

// Statements
statements: (statement)+;

statement: semiStatement | ifStatement;

semiStatement: (expr | vardec) Semi;

// Values/Expressions
expr: term | expr operator expr | LeftParen expr RightParen;

operator: Star | Div | Plus | Minus;

term: literal | access | function;

literal: int | bool;

access: Name;

vardec: (Const | Let) typedName Eq expr;

bool: True | False;

int: Int;

function:
	LeftParen functionArguments RightParen FatArrow (
		block
		| expr
	);

functionArguments:
	| typedName Comma functionArguments
	| typedName;

typedName: Name typeAnnotation?;
typeAnnotation: Colon Name;

// Concrete statements
block: LeftBrace statements? RightBrace;
ifStatement:
	If LeftParen expr RightParen block elseIfStatement* elseStatement?;
elseIfStatement: Elseif LeftParen expr RightParen block;
elseStatement: Else block;

/* SYMBOLS */

Semi: ';';
Colon: ':';
Comma: ',';

// Algebraic operators
Plus: '+';
PlusPlus: '++';
Minus: '-';
MinusMinus: '--';
Star: '*';
Div: '/';
Mod: '%';
Eq: '=';

FatArrow: '=>';

LeftParen: '(';
RightParen: ')';
LeftBracket: '[';
RightBracket: ']';
LeftBrace: '{';
RightBrace: '}';

// NewLine: [\r\n];
WhiteSpace: [ \r\t\n]+ -> skip;

/* KEYWORDS */
False: 'false';
True: 'true';

Const: 'const';
Let: 'let';

While: 'while';
Do: 'do';
For: 'for';

If: 'if';
Else: 'else';
Elseif: Else ' '? If;

Int: [0-9]+;
Name: [a-zA-Z_]+;

/* COMMENTS */

MultiLineComment: '/*' .*? '*/' -> channel(HIDDEN);
SingleLineComment: '//' ~[\r\n]* -> channel(HIDDEN);
