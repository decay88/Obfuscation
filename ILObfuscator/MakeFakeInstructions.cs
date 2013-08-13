﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Obfuscator;

namespace Internal
{
    public partial class Instruction
    {

        /// <summary>
        /// Makes FullAssignment instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value (variable only)</param>
        /// <param name="right_value1">Right value before operator</param>
        /// <param name="right_value2">Right value after operator (for variable), null for number</param>
        /// <param name="right_value_int">Right value after operator (for number), null for variable</param>
        /// <param name="operation">Desired arithmetic operation</param>
        public void MakeFullAssignment(Variable left_value, Variable right_value1, Variable right_value2, int? right_value_int, ArithmeticOperationType operation)
        {
            if (statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if ((right_value2 == null && right_value_int == null) || (right_value2 != null && right_value_int != null) || left_value == null || right_value1 == null || operation == null)
                throw new ObfuscatorException("Wrong parameter passing.");

            string left1 = left_value.name;
            string right1 = right_value1.name;
            string right2 = right_value_int == null ? right_value2.name : right_value_int.ToString();
            string op;
            switch (operation)
            {
                case ArithmeticOperationType.Addition:
                    op = "+";
                    break;
                case ArithmeticOperationType.Subtraction:
                    op = "-";
                    break;
                case ArithmeticOperationType.Multiplication:
                    op = "*";
                    break;
                case ArithmeticOperationType.Division:
                    op = @"/";
                    break;
                default:
                    throw new ObfuscatorException("Unsupported arithmetic operation type.");
            }
            RefVariables.Clear();
            RefVariables.Add(left_value);
            RefVariables.Add(right_value1);
            if (right_value_int == null)
                RefVariables.Add(right_value2);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eFullAssignment;
            TACtext = string.Join(" ", left1, ":=", right1, op, right2);
        }


        /// <summary>
        /// Makes Copy instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value (variable only)</param>
        /// <param name="right_value">Right value (variable), null for number</param>
        /// <param name="right_value_int">Right value (number), null for variable</param>
        public void MakeCopy(Variable left_value, Variable right_value, int? right_value_int)
        {
            if (statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if (left_value == null || (right_value == null && right_value_int == null) || (right_value != null && right_value_int != null))
                throw new ObfuscatorException("Wrong parameter passing.");

            RefVariables.Clear();
            RefVariables.Add(left_value);
            if (right_value_int == null)
                RefVariables.Add(right_value);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eCopy;
            TACtext = right_value_int == null ? string.Join(" ", left_value.name, ":=", right_value.name) : string.Join(" ", left_value.name, ":=", right_value_int);
        }


        /// <summary>
        /// Makes ConditionalJump instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value in relation (only variable)</param>
        /// <param name="right_value">Left value in relation (only numerical value)</param>
        /// <param name="relop">Relational operation</param>
        /// <param name="target">Target basic block the control flow is transfered to, if the relation holds true.</param>
        public void MakeConditionalJump(Variable left_value, int? right_value, RelationalOperationType? relop, BasicBlock target)
        {
            if (statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if(target.parent != parent.parent)
                throw new ObfuscatorException("Target basic block and original are in different functions.");

            if(parent.getSuccessors.Count != 1)
                throw new ObfuscatorException("The basic block should have exactly one successor.");

            if(left_value==null || right_value==null || relop==null)
                throw new ObfuscatorException("Wrong parameter passing.");

            parent.SplitAfterInstruction(this);
            RefVariables.Add(left_value);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump;
            string strRelop = string.Empty;
            switch (relop)
            {
                case RelationalOperationType.Equals:
                    strRelop = "==";
                    break;
                case RelationalOperationType.NotEquals:
                    strRelop = "!=";
                    break;
                case RelationalOperationType.Greater:
                    strRelop = ">";
                    break;
                case RelationalOperationType.GreaterOrEquals:
                    strRelop = ">=";
                    break;
                case RelationalOperationType.Less:
                    strRelop = "<";
                    break;
                case RelationalOperationType.LessOrEquals:
                    strRelop = "<=";
                    break;
                default:
                    throw new ObfuscatorException("Unsupported relational operation type.");
            }
            TACtext = string.Join(" ", "if", left_value.name, strRelop, right_value, "goto", target.ID);
            parent.LinkSuccessor(target);
        }
    }
}
