grammar Lang;

/* RULES */

prog: statements? EOF;

// Statements
statements: (statement)+;

statement: semiStatement | ifStatement | returnStatement;

semiStatement: (expr | vardec) Semi;

returnStatement: Return expr Semi;

// Values/Expressions
expr:
	term
	| invokeFunction
	| expr operator expr
	| LeftParen expr RightParen;

operator: Star | Div | Plus | Minus;

term: literal | access | function;

literal: integer | bool;

access: Name;

vardec: (Const | Let) typedName Eq expr;

bool: True | False;

integer: Int;

function:
	LeftParen functionArguments RightParen typeAnnotation? FatArrow (
		block
		| expr
	);

functionArguments:
	| (Name typeAnnotation Comma)* (Name typeAnnotation)?;

invokeFunction:
	Name LeftParen (expr Comma)* (expr)? RightParen; // f(), LONG_FUNCTION_NAME(a,37, 4+9)

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

Return: 'return';

If: 'if';
Else: 'else';
Elseif: Else ' '? If;

Int: [0-9]+;
Name: [a-zA-Z_@$][a-zA-Z_@$0-9]*;

/* COMMENTS */

MultiLineComment: '/*' .*? '*/' -> channel(HIDDEN);
SingleLineComment: '//' ~[\r\n]* -> channel(HIDDEN);
