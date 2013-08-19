﻿#define PSEUDOCODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;
using ExchangeFormat;

namespace Obfuscator
{
    public static class FakeCode
    {
        /// <summary>
        /// Describes the probability of generating a conditional jump in percents.
        /// </summary>
        private static int prob_of_cond_jump = 30;
        private static int FPO = 10;
        private static int fake_padding = 20;
        private static int fake_padding_variability = 5;

        /// <summary>
        /// Function to change the nop's in the function to actual fake code.
        /// </summary>
        /// <param name="func">The function to work on.</param>
        public static void Generate (Function func)
        {
            /* We have to go through all the nop's in the function. */
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                foreach (Instruction ins in bb.Instructions)
                {
                    if (ins.statementType == StatementTypeType.EnumValues.eNoOperation)
                    {
                        /*
                         * Now we found a nop, so we have to decide what to do with it.
                         * 
                         * At a certain probability we generate a Conditional Jump, which condition
                         * depends on a parameter value, and happens to be always false.
                         * We use these jumps to make the control flow irreducible.
                         * However we must not choose this if we have only one basic block
                         * apart from the "fake exit block". So we must have at least 3 BBs.
                         * 
                         * At the remaining probability we generate instructions that are
                         * using the dead variables present at the point of the actual nop.
                         * However we must not choose this if we have no dead variables
                         * available for using as a left value present.
                         */
                        if (func.BasicBlocks.Count < 3 && GetRandomLeftValue(ins) == null)
                            throw new ObfuscatorException("We cannot really do anything with this instruction right now, sorry.");

                        if (Randomizer.GetSingleNumber(0, 99) < prob_of_cond_jump)
                        {
                            if (func.BasicBlocks.Count < 3)
                            {
                                /* Though randomizer said we should do this, we unfortunately cannot. */
                                GenerateFakeInstruction(ins);
                            }
                            /* This is where we generate a conditional jump.*/
                            GenerateConditionalJump(ins);
                        }
                        else
                        {
                            if (GetRandomLeftValue(ins) == null)
                            {
                                /* Though randomizer said we should do this, we unfortunately cannot. */
                                GenerateConditionalJump(ins);
                            }
                            /* This is where we generate instructions with dead variables. */
                            GenerateFakeInstruction(ins);
                        }

                        /* The instruction is made, so we have to refresh the states of the dead variables. */
                        ins.RefreshNext();
                    }
                }
            }
        }

        /// <summary>
        /// Function to make a Conditional Jump out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void GenerateConditionalJump(Instruction ins)
        {
            /* 
             * Before doing anything, we have to split the basic block holding this
             * instruction, so we can make a conditional jump at the and of the
             * new basic block.
             */
            ins.parent.SplitAfterInstruction(ins);

            BasicBlock jumptarget;
            /*
             * First we want to find a basic block that is inside a loop,
             * so that we can make a jump on it, making the control flow
             * irreducible this way.
             */
            List<BasicBlock> bodies = new List<BasicBlock>();
            foreach (BasicBlock bb in ins.parent.parent.BasicBlocks)
            {
                if (DataAnalysis.isLoopBody(bb) && !bb.Equals(ins.parent))
                    bodies.Add(bb);
            }

            /* If there are such basic blocks. */
            if (bodies.Count() != 0)
            {
                int num = Randomizer.GetSingleNumber(0, bodies.Count() - 1);
                jumptarget = bodies[num];
            }

            /* If not: we choose a random one from all the basic blocks. */
            else
            {
                List<BasicBlock> all_bbs = new List<BasicBlock>();
                foreach (BasicBlock bb in ins.parent.parent.BasicBlocks)
                {
                    /* 
                     * It shouldn't be neither the same basic block we start from,
                     * nor the one called "fake exit block".
                     */
                    if (!bb.Equals(ins.parent) && bb.getSuccessors.Count != 0)
                        all_bbs.Add(bb);
                }

                int num = Randomizer.GetSingleNumber(0, all_bbs.Count() - 1);
                jumptarget = all_bbs[num];
            }

            /* 
             * At this point we have a perfectly chosen jumptarget, so we should
             * split it into two pieces, so we can make a jump right in the middle
             * of the basic block, making it harder to recognize the original control
             * flow.
             */
            Instruction splithere = jumptarget.Instructions[Randomizer.GetSingleNumber(0, jumptarget.Instructions.Count - 1)];
            jumptarget = jumptarget.SplitAfterInstruction(splithere);

            /*
             * When we make a coditional jump here, we use a (fake) parameter with
             * a fixed value (min, max), and we must assure that the condition
             * will always end up as false.
             * 
             * These are the coditions that can satisfy this need:
             * (a, b: const, a < min, b > max)
             *  - p < <= == a    (1)
             *  - p > >= == b    (2)
             *  (These cannot be more complex because of TAC.)
             *  
             * So we get a random parameter with a fixed value, we get a random
             * constant, and we generate the condition according to these.
             */

#if !PSEUDOCODE

            Variable param = ins.parent.parent.GetFixedParam();
            int constant;
            Instruction.RelationalOperationType type;
            
            /* We decide whether we want the (1) or (2) type. */
            switch (Randomizer.GetSingleNumber(1, 2))
            {
                case 1:
                    /* We generate an a, where a < min. */
                    constant = Randomizer.GetSingleNumber(0, param.FixedMin - 1);

                    /* We decide the operator type. */
                    switch (Randomizer.GetSingleNumber(0, 2))
                    {
                        case 0:
                            type = Instruction.RelationalOperationType.Equals;
                            break;
                        case 1:
                            type = Instruction.RelationalOperationType.Less;
                            break;
                        case 2:
                            type = Instruction.RelationalOperationType.LessOrEquals;
                    }
                    break;

                case 2:
                    /* We generate a b, where b > max. */
                    /* TODO: a more reasonable upper bound. */
                    constant = Randomizer.GetSingleNumber(param.FixedMax + 1, param.FixedMax + 1000);

                    /* We decide the operator type. */
                    switch (Randomizer.GetSingleNumber(0, 2))
                    {
                        case 0:
                            type = Instruction.RelationalOperationType.Equals;
                            break;
                        case 1:
                            type = Instruction.RelationalOperationType.Greater;
                            break;
                        case 2:
                            type = Instruction.RelationalOperationType.GreaterOrEquals;
                    }
                    break;
            }

            /* Now we have everything set properly, now we can make the Conditional jump. */
            ins.MakeConditionalJump(param, constant, type, jumptarget);

#endif
        }

        /// <summary>
        /// Function to make an actual fake instruction out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void GenerateFakeInstruction(Instruction ins)
        {
            /* First we get a left value. */
            Variable leftvalue = GetRandomLeftValue(ins);
            
            /* 
             * Now we decide which type of instruction do we want to generate:
             * - Copy                   (1)
             * - Unary Assignment       (2)
             * - Full Assignment        (3)
             */
            switch (Randomizer.GetSingleNumber(1, 3))
            {
                case 1:
                    break;

                case 2:
                    break;

                case 3:
                    break;
            }
        }

        /// <summary>
        /// Returns a variable from the instruction's DeadVariables list with FREE or NOT_INITIALIZED state.
        /// </summary>
        /// <param name="ins">The actual instruction.</param>
        /// <returns>A proper variable, or null, if no such exists.</returns>
        private static Variable GetRandomLeftValue(Instruction ins)
        {
            return GetVariableByStates(ins, Variable.State.Free, Variable.State.Not_Initialized);
        }

        /// <summary>
        /// Returns a variable from the instruction's DeadVariables list with FREE or FILLED state.
        /// </summary>
        /// <param name="ins">The actual instruction.</param>
        /// <returns>A proper variable, or null, if no such exists.</returns>
        private static Variable GetRandomRightValue(Instruction ins)
        {
            return GetVariableByStates(ins, Variable.State.Free, Variable.State.Filled);
        }

        /// <summary>
        /// Function used only by the GetRandomRightValue and GetRandomLeftValue functions.
        /// </summary>
        private static Variable GetVariableByStates(Instruction ins, Variable.State state1, Variable.State state2)
        {
            /* First we gather the variables with the proper state. */
            List<Variable> proper_vars = new List<Variable>();
            foreach (Variable var in ins.DeadVariables.Keys)
            {
                if (ins.DeadVariables[var] == state1 || ins.DeadVariables[var] == state2)
                    proper_vars.Add(var);
            }

            /* If no such exists, we return null. */
            if (proper_vars.Count == 0)
                return null;

            /* If there are ones that fit our needs, then we coose one randomly. */
            else
                return proper_vars[Randomizer.GetSingleNumber(0, proper_vars.Count - 1)];
        }

        public static void GenerateNoOperations(Function func_orig)
        {
            foreach (BasicBlock bb in func_orig.BasicBlocks)
            {
                List<Instruction> insts = Common.DeepClone(bb.Instructions) as List<Instruction>;
                foreach (Instruction inst in insts)
                {
                    if (!inst.isFake)
                    {
                        int fakes_orig = FPO + 1;
                        int original_place = Randomizer.GetSingleNumber(0, fakes_orig);
                        if (inst.statementType == StatementTypeType.EnumValues.eConditionalJump || inst.statementType == StatementTypeType.EnumValues.eUnconditionalJump)
                            original_place = fakes_orig;
                        for (int i = 0; i < fakes_orig; i++)
                        {
                            if (i < original_place)
                                bb.Instructions.Insert(bb.Instructions.BinarySearch(inst), new Instruction(StatementTypeType.EnumValues.eNoOperation, bb));
                            else if (i > original_place)
                                bb.Instructions.Insert(bb.Instructions.BinarySearch(inst) + 1, new Instruction(StatementTypeType.EnumValues.eNoOperation, bb));
                        }
                    }
                }
                if (bb.Instructions.Count < fake_padding)
                {
                    int fakes = Math.Abs(Randomizer.GetSingleNumber(fake_padding - fake_padding_variability, fake_padding + fake_padding_variability) - bb.Instructions.Count);
                    for (int i = 0; i < fakes; i++)
                    {
                        bb.Instructions.Insert(0, new Instruction(StatementTypeType.EnumValues.eNoOperation, bb));
                    }
                }
            }
        }
    }
}
