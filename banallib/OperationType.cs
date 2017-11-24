using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace banallib
{
    public enum OperationType
    {
        //Print = 0,
        //JumpIfTrue = 5,
        //JumpIfFalse = 6,
        //JumpTarget = 8,
        //Function = 9,
        //IfEqual = 18

        //OP_PUSH = 1,
        //OP_GETTOP = 2,
        //OP_DISCARD = 3,
        Print = 4,
        JumpIfTrue = 5,
        JumpIfFalse = 6,
        Concatentate = 7,
        JumpTarget = 8,
        Function = 9,
        Minus = 10,
        Divide = 11,
        Multiply = 12,
        IfLessThan = 13,
        IfLessThanOrEqual = 14,
        IfMoreThan = 15,
        IfMoreThanOrEqual = 16,
        IfNotEqual = 17,
        IfEqual = 18,
        Negative = 19,
        Not = 21,
        And = 22,
        Or = 23

        //OP_PUSH = 1,
        //OP_GETTOP = 2,
        //OP_DISCARD = 3,
        //OP_PRINT = 4,
        //OP_JMP = 5,
        //OP_JMPF = 6,
        //OP_CONCAT = 7,
        //JUMPTARGET = 8,
        //OP_FUNCTION = 9,
        //OP_MINUS = 10,
        //OP_DIVIDE = 11,
        //OP_MULTIPLY = 12,
        //OP_LESS_THAN = 13,
        //OP_LESS_THAN_OR_EQUAL = 14,
        //OP_MORE_THAN = 15,
        //OP_MORE_THAN_OR_EQUAL = 16,
        //OP_NOT_EQUAL = 17,
        //OP_EQUAL = 18,
        //OP_NEG = 19,
        //OP_NOT = 21,
        //OP_AND = 22,
        //OP_OR = 23
    }
}
