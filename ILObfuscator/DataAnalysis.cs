﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class DataAnalysis
    {
        /* 
         * To make sure we don't deal with a basic block twice, we save the ID
         * of the basic blocks we have been to into a list.
         */
        /// <summary>
        /// Holds the ID's of the basic blocks we have already dealt with.
        /// </summary>
        private static List<string> done_ids = new List<string>();

        /// <summary>
        /// It tells us how many times has the algortihm run so far.
        /// </summary>
        private static int counter = 0;

        /// <summary>
        /// Fills the DeadVariables list of the function's instructions with proper information.
        /// </summary>
        /// <param name="func">Actual Function</param>
        public static void DeadVarsAlgortihm(Function func)
        {
            /*
             * Before the algorithm starts we ensure that all instructions have a
             * list of dead variables which is filled with all the variabes present in
             * the function. During the algorithm we will remove the variables from these
             * lists that are not really dead at the given point.
             * 
             * If it isn't the first run of the algorithm, then we shouldn't fill it with
             * all the variables again, rather with the ones not present in the list already.
             * So we won't spoil the information we have of them already.
             */
            if (counter == 0)
                SetAllVariablesAsDead(func, Variable.State.Free);
            else
                RefreshVariables(func, Variable.State.Free);

            /*
             * We start from the point called "fake exit block", the one and only
             * ultimate endpoint of all functions ever created. Thank you Kohlmann!
             *
             * GetLastBasicBlock() - it is supposed to give the function's "fake exit block"
             */
            BasicBlock lastblock = func.GetLastBasicBlock();

            /*
             * We go through all the instructions and deal with all their
             * referenced variables. At the and of this we will have the list of the dead
             * variables for all instructions.
             */
            recursive(lastblock);

            /* Now that the algorithm has ended, we step the counter. */
            counter++;
        }

        /// <summary>
        /// Recursive function to get to all the instructions of the Function.
        /// </summary>
        /// <param name="actual">Actual BasicBlock</param>
        private static void recursive(BasicBlock actual)
        {
            /*
             * We deal with every original(1) instruction in the basic block,
             * and in every instruction we deal with every referenced variable.
             * The referenced variables should be removed from the dead variables list
             * in all the instructions accesible from here.
             * 
             * (1): Only the original instructions determine whether a variable is dead
             *      or not, because the fake instructions work with dead variables only.
             *
             * deal_with_var() - it will be described in detail in the forthcoming parts
             */
            foreach (Instruction ins in actual.Instructions)
            {
                if (ins.isFake == false) 
                {
                    foreach (Variable var in ins.RefVariables)
                        deal_with_var(var, ins);
                }
            }

            /*
             * We have finished the task with this basic block, and we should not
             * come back here anymore, so we save its ID into the done_ids list.
             */
            done_ids.Add(actual.ID);

            /*
             * Now that this basic block is finished we should do the same things
             * with its predecessors recursively.
             * We only should deal with the basic blocks not marked as done.
             */
            foreach (BasicBlock block in actual.getPredecessors)
            {
                if ( !done_ids.Contains(block.ID) )
                    recursive(block);
            }
        }

        /// <summary>
        /// Recursive function to set a Variable as alive in the proper places.
        /// </summary>
        /// <param name="var">the Variable we are dealing with</param>
        /// <param name="ins">Actual Instruction</param>
        private static void deal_with_var(Variable var, Instruction ins)
        {
            /* 
             * This variable is used somewhere after this instruction, so it is alive here.
             * 
             * DeadVariables - it contains the dead variables at the point of the given instruction
             */
            ins.DeadVariables.Remove(var);

            /*
             * GetPrecedingInstructions() 
             *      - gives a list of the instructions followed by the actual instruction
             *      - if we are in the middle of the basic block, then it consists of only one instruction
             *      - if we are at the beginning of the basic block then it is the list of all
             *        the predecessing basic block's last instruction
             *      - if we are at the beginning of the first basic block (the one with no predecessors)
             *        then it is an empty list, meaning that we have nothing left to do here
             */
            List<Instruction> previous = ins.GetPrecedingInstructions();

            /*
             * Now we have that list of instructions, we should do the same thing we have done
             * to this instruction, assuming that it had not been done already.
             */
            foreach (Instruction i in previous)
            {
                /*
                 * If the variable is not in the instruction's dead variables list, then it indicates
                 * that we have dealt with this instruction.
                 */
                if ( i.DeadVariables.ContainsKey(var) )
                    deal_with_var(var, i);
            }
        }

        /// <summary>
        /// Used once by the Data Analysis Algorithm and fills all DeadVariables lists with all variables defined in the function 
        /// </summary>
        /// <param name="func">Actual Function</param>
        private static void SetAllVariablesAsDead(Function func, Variable.State state)
        {
            foreach (BasicBlock bb in func.BasicBlocks)
                foreach (Instruction inst in bb.Instructions)
                {
                    inst.DeadVariables.Clear();
                    foreach (Variable var in func.LocalVariables)
                        inst.DeadVariables.Add(var, state);
                }
        }

        /// <summary>
        /// Fills the DeadVariables lists with the variables that aren't already in them.
        /// </summary>
        /// <param name="func">Actual Function</param>
        private static void RefreshVariables(Function func, Variable.State state)
        {
            foreach (BasicBlock bb in func.BasicBlocks)
                foreach (Instruction inst in bb.Instructions)
                {
                    foreach (Variable var in func.LocalVariables)
                    {
                        if ( !inst.DeadVariables.ContainsKey(var) )
                            inst.DeadVariables.Add(var, state);
                    }
                }
        }

        /* -------------- isLoopBody algorithm starts ---------------- */

        /*
         * We decided that this function should be not in the BasicBlock, rather
         * outside it. In the future it can be placed to a more proper place,
         * now it's here just to make debugging possible...
         */

        /// <summary>
        /// List to hold the id's of the basic blocks we already reached.
        /// Used by the isLoopBody function.
        /// </summary>
        private static List<string> found_ids = new List<string>();

        /// <summary>
        /// Function to find out whether a basic block is in a loop body, or not.
        /// </summary>
        /// <param name="actual">The questioned basic block.</param>
        /// <returns>True if the basic block is in a loop, False if not.</returns>
        public static bool isLoopBody (BasicBlock bb)
        {
            /*
             * We clear the former found_ids list, because we don't want the previous run of
             * the algorithm to influence the present one.
             */
            found_ids.Clear();

            foreach (BasicBlock item in bb.getSuccessors)
                reachable_from(item);

            /*
             * If and only if we have got to this basic block during the algorithm,
             * then it is inside a loop.
             */
            if (found_ids.Contains(bb.ID))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Adds this basic block to the found_ids list, and calls itself for all the BB's successors.
        /// </summary>
        /// <param name="actual">Actual BasicBlock</param>
        private static void reachable_from(BasicBlock actual)
        {
            /*
             * First we check whether the actual basic block has been already
             * found. If yes, we have nothing to do left.
             */
            if (!found_ids.Contains(actual.ID))
            {
                /* We add the actual basic block's ID to the found_ids list. */
                found_ids.Add(actual.ID);

                /* Then we continue with all its successors. */
                foreach (BasicBlock item in actual.getSuccessors)
                    reachable_from(item);
            }      
        }

        /* ---------------- isLoopBody algorithm ends ------------------ */
    }
}
