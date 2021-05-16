#!/bin/bash
mkdir -p AntlrDebug
java -Xmx500M org.antlr.v4.Tool Lang.g4 -o AntlrDebug
cd AntlrDebug
javac *.java
java -Xmx500M org.antlr.v4.gui.TestRig Lang prog -tree "../../examples/$1" "${@:2}"
