# EVIL Programming Language & Syntax Reference

## 1. Script overview
This document aims to provide a simple definition of an EVIL script file and familiarize the reader
with the basic script structure of a program written in the EVIL language.

### 1.1. Script file definition
A valid EVIL program consists of sequentially grouped top-level statements outlined in section 
[3.1](03_statements.md#top-level-statements).  
EVIL script files usually end with `.vil` file extension, but this is not enforced.

### 1.2. Basic script structure
The following is an example of a valid "Hello, world" program written in EVIL:

```evil
#[entry(max_args = 3)]
fn main(args) {
  core.println("Hello, world!");
  
  if (#args) {
    core.println("I have been passed " + @#args + " arguments: ");
  
    for (var i = 0; i < #args; i++) {
      core.println(@i + ": " + @args[i]); 
    }
  }
  
  core.println("Goodbye, world!");
}
```