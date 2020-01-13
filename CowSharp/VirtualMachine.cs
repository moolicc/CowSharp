using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CowSharp
{
    // Resources:
    // http://bigzaphod.github.io/COW/semantics-cow-english.pdf
    // http://bigzaphod.github.io/COW/

    enum Instructions : byte
    {
        //This command is connected to the MOO command. When encountered during normal execution, it searches the program code in reverse looking
        //for a matching MOO command and begins executing again starting from the found MOO command. When searching,
        //it skips the instruction that is immediately before it (see MOO).
        moo = 0,

        //Moves current memory position back one block.
        mOo = 1,

        //Moves current memory position forward one block.
        moO = 2,

        //Execute value in current memory block as if it were an instruction. The command executed is based on the instruction code value
        //(for example, if the current memory block contains a 2, then the moO command is executed). An invalid command exits the
        //running program. Value 3 is invalid as it would cause an infinite loop.
        mOO = 3,

        //If current memory block has a 0 in it, read a single ASCII character from STDIN and store it in the current memory block.
        //If the current memory block is not 0, then print the ASCII character that corresponds to the value in the current memory block to STDOUT.
        Moo = 4,

        //Decrement current memory block value by 1.
        MOo = 5,

        //Increment current memory block value by 1.
        MoO = 6,

        //If current memory block value is 0, skip next command and resume execution after the next matching moo command.
        //If current memory block value is not 0, then continue with next command. Note that the fact that it skips the command
        //immediately following it has interesting ramifications for where the matching moo command really is. For example,
        //the following will match the second and not the first moo: OOO MOO moo moo
        MOO = 7,

        //Set current memory block value to 0.
        OOO = 8,

        //If no current value in register, copy current memory block value. If there is a value in the register,
        //then paste that value into the current memory block and clear the register.
        MMM = 9,

        //Print value of current memory block to STDOUT as an integer.
        OOM = 10,

        //Read an integer from STDIN and put it into the current memory block.
        oom = 11,

        EOF = 255,
    }

    class VirtualMachine
    {
        private char[] _readBuffer;
        private List<Instructions> _program;
        private List<int> _memory;
        private int? _register;
        private int _programPosition;
        private int _memoryPosition;

        private bool _exit;

        public VirtualMachine()
        {
        }

        public void LoadStream(StreamReader input)
        {
            _readBuffer = new char[3];
            _program = new List<Instructions>((int)input.BaseStream.Length / 3);
            _memory = new List<int>();
            _register = null;
            _programPosition = 0;
            _memoryPosition = 0;
            _memory.Add(0);
            _exit = false;

            LoadInstructions(input);
        }

        public void DebugExecute()
        {
            while (!_exit)
            {
                Console.WriteLine("Executing: {0}", _program[_programPosition]);
                ExecuteNext();
                PrintState();
            }
        }

        public void DebugExecuteStep()
        {
            while (!_exit)
            {
                Console.WriteLine("Press [return] to execute command at pointer {0} ({1})...", _programPosition, _program[_programPosition]);
                Console.ReadLine();
                ExecuteNext();
                PrintState();
            }
        }

        public void Execute()
        {
            while (!_exit)
            {
                ExecuteNext();
            }
        }

        private void ExecuteNext()
        {
            ExecuteInstruction(_program[_programPosition]);
        }

        public void PrintState()
        {
            Console.WriteLine("MEM===============");
            for (int i = 0; i < _memory.Count; i++)
            {
                if(i + 1 < _memory.Count)
                {
                    Console.WriteLine("{0}:  {1}\t{2}:  {3}", i, _memory[i], i + 1, _memory[i + 1]);
                    i++;
                }
                else
                {
                    Console.WriteLine("{0}:  {1}", i, _memory[i]);
                }
            }
            Console.WriteLine("==================");
            Console.WriteLine("Register: {0}", _register);
            Console.WriteLine("Current memory value: {0}", _memory[_memoryPosition]);

            Console.WriteLine("Program pointer: {0}", _programPosition);
            Console.WriteLine("Memory pointer: {0}", _memoryPosition);

            if (_programPosition > 0)
            {
                Console.WriteLine("Last instruction: {0}", _program[_programPosition - 1]);
            }
            Console.WriteLine("Next instruction: {0}", _program[_programPosition]);

            Console.WriteLine();
            Console.WriteLine();
        }

        private void ExecuteInstruction(Instructions instruction)
        {
            switch (instruction)
            {
                case Instructions.moo:
                    Do_moo();
                    break;
                case Instructions.mOo:
                    Do_mOo();
                    break;
                case Instructions.moO:
                    Do_moO();
                    break;
                case Instructions.mOO:
                    Do_mOO();
                    break;
                case Instructions.Moo:
                    Do_Moo();
                    break;
                case Instructions.MOo:
                    Do_MOo();
                    break;
                case Instructions.MoO:
                    Do_MoO();
                    break;
                case Instructions.MOO:
                    Do_MOO();
                    break;
                case Instructions.OOO:
                    Do_OOO();
                    break;
                case Instructions.MMM:
                    Do_MMM();
                    break;
                case Instructions.OOM:
                    Do_OOM();
                    break;
                case Instructions.oom:
                    Do_oom();
                    break;
                case Instructions.EOF:
                default:
                    _exit = true;
                    break;
            }
        }

        private void LoadInstructions(StreamReader input)
        {
            Instructions curInstruction = ReadNext(input);
            while (curInstruction != Instructions.EOF)
            {
                _program.Add(curInstruction);
                curInstruction = ReadNext(input);
            }
            _program.Add(Instructions.EOF);
        }

        private Instructions ReadNext(StreamReader input)
        {
            if (input.EndOfStream)
            {
                return Instructions.EOF;
            }

            input.Read(_readBuffer, 0, 1);

            while (char.IsWhiteSpace(_readBuffer[0]))
            {
                input.Read(_readBuffer, 0, 1);
            }
            while (_readBuffer[0] == ';' || _readBuffer[0] == '/')
            {
                input.ReadLine();
                input.Read(_readBuffer, 0, 1);
            }
            while (char.IsWhiteSpace(_readBuffer[0]))
            {
                input.Read(_readBuffer, 0, 1);
            }

            input.Read(_readBuffer, 1, 2);

            var instructionName = string.Join(null, _readBuffer);
            return (Instructions)Enum.Parse(typeof(Instructions), instructionName);
        }

        private void IncrementCurrentMemoryPosition()
        {
            _memoryPosition++;
            if(_memoryPosition >= _memory.Count)
            {
                _memory.Add(0);
            }
        }

        private void DecrementCurrentMemoryPosition()
        {
            _memoryPosition--;
            if (_memoryPosition < 0)
            {
                throw new InvalidOperationException();
            }
        }

        private void IncrementCurrentMemoryValue()
        {
            _memory[_memoryPosition]++;
        }

        private void DecrementCurrentMemoryValue()
        {
            _memory[_memoryPosition]--;
        }

        private void Do_moo()
        {
            bool found = false;
            int level = 0;
            for (int i = _programPosition - 2; i >= 0; i--)
            {
                if(_program[i] == Instructions.moo)
                {
                    level++;
                }
                if(_program[i] == Instructions.MOO)
                {
                    if(level == 0)
                    {
                        _programPosition = i;
                        found = true;
                        break;
                    }
                    else
                    {
                        level--;
                    }
                }
            }
            if(!found)
            {
                _programPosition++;
            }
        }

        private void Do_mOo()
        {
            DecrementCurrentMemoryPosition();
            _programPosition++;
        }

        private void Do_moO()
        {
            IncrementCurrentMemoryPosition();
            _programPosition++;
        }

        private void Do_mOO()
        {
            var curValue = _memory[_memoryPosition];
            if (curValue > byte.MaxValue || curValue < byte.MinValue
                || !Enum.IsDefined(typeof(Instructions), curValue))
            {
                _exit = true;
                return;
            }
            Instructions instruction = (Instructions)curValue;
            if (instruction == Instructions.mOO)
            {
                throw new InvalidOperationException("Halting to prevent infinite loop.");
            }
            ExecuteInstruction(instruction);
        }

        private void Do_Moo()
        {
            if(_memory[_memoryPosition] == 0)
            {
                _memory[_memoryPosition] = Console.Read();
            }
            else
            {
                Console.WriteLine((char)_memory[_memoryPosition]);
            }
            _programPosition++;
        }

        private void Do_MOo()
        {
            DecrementCurrentMemoryValue();
            _programPosition++;
        }

        private void Do_MoO()
        {
            IncrementCurrentMemoryValue();
            _programPosition++;
        }

        private void Do_MOO()
        {
            if (_memory[_memoryPosition] != 0)
            {
                _programPosition++;
                return;
            }
            bool found = false;
            for (int i = _programPosition + 2; i < _program.Count; i++)
            {
                if(_program[i] == Instructions.moo)
                {
                    _programPosition = i + 1;
                    found = true;
                    break;
                }
            }
            if(!found)
            {
                _programPosition++;
            }
        }

        private void Do_OOO()
        {
            _memory[_memoryPosition] = 0;
            _programPosition++;
        }

        private void Do_MMM()
        {
            if(!_register.HasValue)
            {
                _register = _memory[_memoryPosition];
            }
            else
            {
                _memory[_memoryPosition] = _register.Value;
                _register = null;
            }
            _programPosition++;
        }

        private void Do_OOM()
        {
            Console.WriteLine(_memory[_memoryPosition]);
            _programPosition++;
        }

        private void Do_oom()
        {
            _memory[_memoryPosition] = Console.Read();
            _programPosition++;
        }
    }
}
