# EVIL
EVIL stands for Ensign Virtual Interpreted Language. It's a toy language I've been developing 
over the course of 3+ years. The virtual machine is supposed supposed to be embedded into .NET
hosts to allow a completely sandboxed environment with runtime configuration left entirely
up for the implementer to decide.

Major motivations:
 - Learn "how to compiler" and "i accidently a virtual machine".
 - I've always wanted a toy like this to play around with and test my ideas in.
 - To finally complete Advent of Code using my own language in this decade.
 - To be used in a future game where scripting is a heavily used tool.

Goals:
 - Easy to integrate and interoperate with the .NET world.
 - Syntax that's easy to extend or remove from the language and compiler.
 - Concise and fun to write scripts in. 

Absolute highest performance is ___not___ the main goal of this project.

# Project Structure
> ### **Core** 
> Contains implementations of the lexer and the grammar parser.

> ### **VirtualMachine**
> Contains _Ceres_ Translation & Execution Engine and accompanying language tests.

> ### **Shared**
> Everything else that doesn't fit the code stuff.EVIL source examples, syntax highlighters for various editors.

# Formal grammar definition
To be done.

# Language reference manual
To be done.
