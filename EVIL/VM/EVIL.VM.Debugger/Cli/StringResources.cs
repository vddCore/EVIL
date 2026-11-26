namespace EVIL.VirtualMachine.Debugger.Cli;

internal static class StringResources
{
    public const string GeneralHelp =
        """

        EVIL Command-Line Debugger (evdb)
        ---------------------------------
        Execution:
          c, continue                 Resume program execution until next breakpoint or pause.
          s, step                     Step into the next statement or function call.
          n, next                     Step over the next statement.
          fin, finish                 Step out of current function to caller.
          si, stepi                   Execute exactly one VM opcode instruction.
          q, quit                     Abort execution and exit debugger.

        Breakpoints:
          b, break <line>             Set a breakpoint on current file at <line>.
          b, break <file>:<line>      Set a breakpoint on <file> at <line>.
          b *<address>                Set an address breakpoint at bytecode offset (e.g. b *0x0010).
          b <target> <condition>      Set a conditional breakpoint (e.g. b 12 x > 5).
          d, delete <id>              Delete breakpoint by ID.
          breakpoints, bps            List all active breakpoints.

        Inspection:
          p, print <expr>             Evaluate and print an expression in the active stack frame.
          set <var> = <expr>          Mutate a local variable in the active stack frame.
          set disasm-on-step <on|off> Toggle automatic disassembly snippet display on every step.
          bt, backtrace               Display call stack frames.
          f, frame <index>            Select active stack frame for inspection.
          loc, locals                 Display local variables in the active stack frame.
          args                        Display arguments in the active stack frame.
          fibers                      Display all running fibers.
          fiber <id>                  Select active fiber.
          l, list [line]              Display source code context.
          i, info <topic>             Display execution info (locals, args, breakpoints, fibers).
          dis, disasm [flags] [tgt]   Disassemble bytecode (defaults to window around current IP).
          h, help [command]           Display general help or detailed help for a specific command.

        Type 'help <command>' for detailed help on any command (e.g. 'help dis', 'help break').

        """;
    
    public const string DisassembleSubCommandHelp =
        """

        Command: dis, disasm
        Description: Disassemble bytecode instructions in the current or targeted chunk.

        Usage:
          dis [flags] [target] [r <from>:<to>]

        Relative Range Windowing:
          r <from>:<to>               Disassemble a window relative to the current instruction pointer.
          range <from>:<to>           '<from>' is offset relative to current IP; '<to>' is offset after current IP.
                                      e.g. 'dis r 0:10'   - 10 instructions after current IP (including current).
                                      e.g. 'dis r -10:10' - 10 instructions before and 10 after current IP.
                                      e.g. 'dis r -5:0'   - 5 instructions before up to current IP.
                                      e.g. 'dis r 5:15'   - 5 to 15 instructions after current IP.

        Flags:
          -a, --all, all              Disassemble all instructions in chunk (bypasses windowing).
          -b, --bytes                 Include raw hex bytecode bytes column.
          -m, --mixed                 Interleave source code lines with disassembly instructions.
          --no-mixed, --no-source     Suppress interleaved source code comments.
          -s, --subchunks             Recursively disassemble all nested subchunks/functions.

        Flag Bundling:
          Short flags can be combined into a single token, e.g.:
            'dis -am'                 All instructions with interleaved source code lines.
            'dis -abm'                All instructions with raw bytes and interleaved source code.
            'dis -bm r 0:10'          Relative window with raw bytes and interleaved source code.

        Targets:
          *<address>                  Disassemble starting from absolute hex/dec offset (e.g. dis *0x0010).
          0x<address>                 Disassemble starting from hex offset (e.g. dis 0x0010).
          <line>                      Disassemble instructions compiled for specific source line (e.g. dis 15).
          <function_name>             Disassemble a specific named function or subchunk (e.g. dis take_damage).
          <count>                     Number of instructions to show in window/range (default: 11).

        Examples:
          dis                         Show default 11-instruction window around current IP.
          dis r 0:10                  Show current instruction and 10 instructions after.
          dis r -10:10                Show 10 instructions before and 10 instructions after current IP.
          dis -a -m                   Show all instructions with interleaved source code.
          dis -abm                    Show all instructions with raw bytes and source code.
          dis r -5:5 -b -m            Show 5 before / 5 after with raw bytes and source lines.
          dis take_damage             Disassemble function 'take_damage'.
          dis *0x0014                 Disassemble starting at offset 0x0014.
          dis 8                       Disassemble instructions compiled for line 8.

        """;
    
    public const string BreakpointSubCommandHelp =
        """

        Command: break, b
        Description: Set a source line breakpoint or bytecode address breakpoint.

        Usage:
          b <line>                    Set breakpoint at line in current file.
          b <file>:<line>             Set breakpoint at line in specified file.
          b *<address>                Set address breakpoint at bytecode offset (e.g. b *0x0010).
          b <target> <condition>      Set conditional breakpoint that breaks only when expression evaluates to true.

        Examples:
          b 15                        Break at line 15 of current source file.
          b combat.vil:22             Break at line 22 of 'combat.vil'.
          b *0x0018                   Break when instruction pointer reaches bytecode offset 0x0018.
          b 25 x > 100                Break at line 25 only when local variable 'x' is greater than 100.
          b *0x0018 count == 0        Break at offset 0x0018 only when 'count' equals 0.

        """;
    
    public const string DeleteSubCommandHelp =
        """

        Command: delete, d
        Description: Remove a breakpoint by its numeric ID.

        Usage:
          d <id>
          delete <id>

        Example:
          d 1                         Delete breakpoint #1.

        """;
    
    public const string BreakpointsSubCommandHelp =
        """

        Command: breakpoints, bps
        Description: Display all registered breakpoints.

        Usage:
          breakpoints
          bps

        Shows a table listing breakpoint IDs, file paths, line numbers, bound bytecode offsets (IP),
        hit counts, and conditional expressions.

        """;
    
    public const string ContinueSubCommandHelp =
        """

        Command: continue, c, cont
        Description: Resume execution.

        Usage:
          c
          continue

        Continues program execution until the next breakpoint is encountered, an unhandled
        exception is thrown, or the program finishes.

        """;
    
    public const string StepSubCommandHelp =
        """

        Command: step, s
        Description: Step into the next statement.

        Usage:
          s
          step

        Executes the current statement. If the statement contains a function invocation,
        execution steps into the first statement of the called function.

        """;
        
    public const string NextSubCommandHelp =
        """

        Command: next, n
        Description: Step over the next statement.

        Usage:
          n
          next

        Executes the current statement. Function invocations are executed to completion
        without stepping into their internal statements.

        """;
    
    public const string FinishSubCommandHelp =
        """

        Command: finish, fin
        Description: Step out of the current function.

        Usage:
          fin
          finish

        Resumes execution until the current function returns to its caller.

        """;
    
    public const string StepiSubCommandHelp =
        """

        Command: stepi, si
        Description: Step exactly one bytecode instruction.

        Usage:
          si
          stepi

        Executes exactly one virtual machine bytecode opcode instruction and displays
        a disassembly snippet with the active instruction highlighted.

        """;
    
    public const string PrintSubCommandHelp =
        """

        Command: print, p
        Description: Evaluate an EVIL expression in the context of the active stack frame.

        Usage:
          p <expression>
          print <expression>

        Examples:
          p 1 + 2
          p player.hp
          p self.name
          p table.keys(my_table)

        """;
    
    public const string SetSubCommandHelp =
        """

        Command: set
        Description: Mutate a local variable or configure debugger settings.

        Usage:
          set <var> = <expr>          Mutate a local variable in the active stack frame.
          set disasm-on-step <on|off> Toggle automatic disassembly snippet display on every step.

        Examples:
          set player.hp = 100
          set multiplier = 2.5
          set disasm-on-step on
          set disasm-on-step off

        """;
    
    public const string BacktraceSubCommandHelp =
        """

        Command: backtrace, bt, where
        Description: Display the call stack backtrace.

        Usage:
          bt
          backtrace

        Lists all active stack frames from innermost to outermost, including frame indices,
        function names, source file paths, line numbers, and instruction pointers.

        """;
    
    public const string FrameSubCommandHelp =
        """

        Command: frame, f
        Description: Select an active stack frame for inspection.

        Usage:
          f <index>
          frame <index>

        Selects frame <index> (0 is the current innermost frame) so that commands such as
        'locals', 'args', 'print', and 'dis' operate in that frame's context.

        Example:
          f 1                         Inspect caller frame.

        """;
    
    public const string LocalsSubCommandHelp =
        """

        Command: locals
        Description: Display local variables defined in the selected stack frame.

        Usage:
          locals

        Lists each variable name, its current dynamic value, and its type.

        """;
    
    public const string ArgsSubCommandHelp =
        """

        Command: args
        Description: Display arguments in the selected stack frame.

        Usage:
          args

        Lists each argument index, parameter name (if available), dynamic value, and type.

        """;
    
    public const string FibersSubCommandHelp =
        """

        Command: fibers
        Description: List all coroutines / fibers in the virtual machine.

        Usage:
          fibers

        Lists fiber IDs, current states (Running, Yielded, Finished, Crashed), and stack frame depths.

        """;
    
    public const string FiberSubCommandHelp =
        """

        Command: fiber
        Description: Switch the active debugging fiber.

        Usage:
          fiber <id>

        Switches the debugger focus to the fiber with the given numeric ID.

        """;
    
    public const string InfoSubCommandHelp =
        """

        Command: info
        Description: Display status information about program execution.

        Usage:
          info <topic>

        Topics:
          locals                      Show local variables in current stack frame.
          args                        Show arguments in current stack frame.
          break, breakpoints          List all active breakpoints.
          fibers                      List all running fibers.

        """;
    
    public const string ListSubCommandHelp =
        """

        Command: list, l
        Description: Display source code context.

        Usage:
          l                           Show lines around current execution pointer.
          l <line>                    Show lines around specified line number.

        """;
    
    public const string QuitSubCommandHelp =
        """

        Command: quit, q, exit
        Description: Abort program execution and exit the debugger.

        Usage:
          q
          quit

        """;
}