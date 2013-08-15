﻿//#define PSEUDOCODE

#if !PSEUDOCODE

/*
 * So this is the algorithm for meshing the control flow transition blocks.
 * This algorithm can be divided into two main parts:
 *  - Meshing the unconditional jumps
 *  - Meshing the conditional jumps
 * 
 * The complexity of the final CFG is based on the CFT Ratio, or Control Flow Transition
 * Ratio, which is the basic parameter of this algorithm of the obfuscaion. Depending on
 * this, the run time of the obfuscated part obviously slow down, so further considerations
 * must be done to set the default value of the CFT Ratio.
 * 
 * First of all, the algorithm needs a set of jumps -- conditional and unconditional also
 * -- on which the meshing will be terminated. The setting of this set is the first step,
 * because if we mesh up the CFG based on the unconditional jumps, and we try to determine
 * the conditional jumps which will locate the place of the meshing by the conditional
 * jumps, we might face the situation that for instance we only mesh up the conditional
 * jumps which are generated by us erstwhile, the original conditional jumps will remain
 * unmeshed... this explanation is getting to be too verbosed, so the point is: the first
 * step is to define the several jumps which the mesh will be based on.
 */

// Dmitriy: Agree on that, but we can easily solve it. Let's discuss it on Friday!


/*
 * This is the main function, it meshes up the single conditional and
 * unconditional jumps.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;

namespace Obfuscator
{
    public static class Meshing
    {

        /// <summary>
        /// The test version of the meshing algorithm, which will mesh the CFT-s
        /// </summary>
        /// <param name="funct">Thse function which has the control flow transitions to be meshed up</param>
        public static void MeshingAlgorithm( Function funct )
        {
            // Now it meshes up all of the unconditional jumps, and only them.
            List<BasicBlock> basicblocks = funct.GetUnconditionalJumps();
            foreach (BasicBlock bb in basicblocks)
            {
                // Only the fake lane gets injected.
                // Not to mention, that it is also in a test phase.
                InsertFakeLane(bb);
                InsertDeadLane(bb);
                //TODO: Upgrading Fake lane inserting more (polyrequ copy...), adding dead lane inserting
            }
        }

        /// <summary>
        /// Inserts the fake lane into the CFT
        /// Still in test phase, now it only has a not so smart condition in the conditional jump
        /// </summary>
        /// <param name="bb">The actual basic block with the unconditional jump</param>
        private static void InsertFakeLane(BasicBlock bb)
        {
            //bb.InsertAfter(bb.getSuccessors[0]);

            // Saving the original target of the jump
            BasicBlock originaltarget = bb.getSuccessors.First();

            // Creating a new basic block after "bb"
            BasicBlock fake1 = bb.SplitAfterInstruction(bb.Instructions.Last());

            // Creating the second fake block, after fake1
            BasicBlock fake2 = fake1.SplitAfterInstruction(fake1.Instructions.Last());

            // And now setting the edges
            fake1.LinkTo(originaltarget, true);

            // And then converting its nop instruction into a ConditionalJump, and by that we create a new block
            fake1.Instructions.First().MakeConditionalJump(fake1.parent.LocalVariables[ Randomizer.GetSingleNumber( 0, fake1.parent.LocalVariables.Count-1)], Randomizer.GetSingleNumber(0, 100), (Instruction.RelationalOperationType) Randomizer.GetSingleNumber(0,5), fake2);

            // It creates the fake lane, but the condition is not a smart one yet, it is a ~random~ condition.
            // TODO: Creating smart conditions
            //       Create a PolyRequied copy of the originaltarget, and link the conditional jump to it instead of the originaltarget itself
            
        }

        /// <summary>
        /// Inserts the dead lane into the CFT and also changing the original unconditional jump into a conditional
        /// In thest phase -> condition not always true
        /// </summary>
        /// <param name="bb">The actual basic block with the unconditional jump</param>
        private static void InsertDeadLane(BasicBlock bb)
        {

            // First, creating a basic block pointing to a random block of the function
            BasicBlock dead1 = new BasicBlock(bb.parent);
            dead1.LinkTo(bb.parent.BasicBlocks[Randomizer.GetSingleNumber(0, bb.parent.BasicBlocks.Count - 1)], true);

            // And making it dead
            dead1.dead = true;

            // Now including another one, with the basicblock splitter
            BasicBlock dead2 = dead1.SplitAfterInstruction(dead1.Instructions.First());

            // And making it dead too
            dead2.dead = true;

            // Now including another one, with the basicblock splitter
            BasicBlock dead3 = dead1.SplitAfterInstruction(dead1.Instructions.First());

            // And making it dead too
            dead3.dead = true;

            // Now creating the conditional jump
            dead1.Instructions.First().MakeConditionalJump(bb.parent.LocalVariables[Randomizer.GetSingleNumber(0, bb.parent.LocalVariables.Count - 1)], Randomizer.GetSingleNumber(0, 100), Instruction.RelationalOperationType.Less, dead3);

            // Here comes the tricky part: changing the bb's unconditional jump to a conditional, which is always true
            bb.Instructions.Last().ConvertToConditionalJump(bb.parent.LocalVariables[Randomizer.GetSingleNumber(0, bb.parent.LocalVariables.Count - 1)], Randomizer.GetSingleNumber(0, 100), Instruction.RelationalOperationType.Less, dead1);

            // And finally we link the remaining block
            dead2.LinkTo(bb.parent.BasicBlocks[Randomizer.GetSingleNumber(0, bb.parent.BasicBlocks.Count - 1)], true);

        }

    }
}


#endif
#if PSEUDOCODE 

void MeshFunction( Function actualfunction )
{
    /*
     * The getBBCunJumps returns a list of the basic blocks that has one
     * successor, which also has a sucessor. Not all of them, but some,
     * depending on the CFTRatio.
     */
    
    list<BasicBlock> basicblocks = Function.getBBUncJumps( actualfunction );
    // TODO: Ebből az alhalmazt, Ratio alapján
    /*
     * Now we go through the list, and mesh the jumps.
     */
    
    foreach ( BasicBlock bb in basicblocks )
        MeshUnconditional( bb );
    
    /*
     * Now we need the list of the conditional jumps.
     */
    
    basicblocks = Function.getBBCondJumps( actualfunction );
    
    /*
     * And go through the list of conditional jumps as well.
     */
    
    foreach ( BasicBlock bb in basicblocks )
        MeshConditional( bb );
}

/*
 * The MeshUncoditional function gets a BasicBlock, and creates
 * the Control Flow Transition.
 */

void MeshUnconditional( BasicBlock actualbb )
{
    ///* First we insert an entry point. */
    
    //BasicBlock ep = InsertEntryPoint( actualbb );
    
    ///* Next, we insert the fake flow, with one fake block, and
    // * we create a copy of the successor of the actual basic block
    // * (if needed). */
    
    //InsertFakeFlow( actualbb );
    
    ///* Finally comes the dead flow, with 3 blocks. They are all dead. */
    
    //InsertDeadFlow( ep );


    
    

    return;
    

}

/*
 * The InsertEntryPoint is the function that inserts the fake basic block
 * which will serve as the entry point of the CFT.
 */
void InsertEntryPoint( BasicBlock bb )
{
    /* We need a new BB. */

    BasicBlock ep = new BasicBlock();

    /* And we need to ad it to the function. */

    bb.parent.BasicBlocks.Add( ep );
    //parent...
    /* A function now creates an instruction with a fake conditional jump,
     * and it is fake because it always continues in the true way. */

    Instruction i = CreateFakeCondJump( bb.Instructions[0].getID() );

    /* Appending the instruction to the block we just created. */
    
    ep.Append( i );

    /* And setting its successor to the actual block. */

    ep.getSuccessors.Add( bb );

    /* Now we use the ChangeGotos function, and it changes all goto-s in the
     * function with one ID, to have an other ID */

    bb.parent.ChangeGotos( bb.Instructions[0].getID(), ep.Instructions[0].getID() );

    /* And finally we set the bacisblocks' predecessors and successors, so
     * the edges in the CFG. */
    
    foreach ( BasicBlock pred in pp.getPredecessors )
    {
        pred.getSuccessors.Delete( bb );
        pred.getSuccessors.Add( ep );
        bb.getPredecessors.Delete( pred );
    }
    bb.Predecessors.Add( ep );   
}

/*
 * Now we can proceed to the second step, wich means inserting the fake side of the
 * meshed control flow. The fake path looks like this:
 * 
 *           ------------------
 *          | Fake entry block |
 *           ------------------
 *        (True) /
 *          -----------
 *         | Actual BB |
 *          -----------
 *    (False) /   \ (True)
 *    ------------ \
 *   | Fake Block | |
 *    ------------ /
 *   |  Actual    |
 *   | Successor  |
 *    ------------
 *    
 * Since we have the injected entry point, this part of the algorythm includes changing
 * the unconditional jump of the actual basicblock into a conditional jump, with the
 * following ends: The true lane must go to the actual successor and the false must go
 * to the fake block. (Because of the future code generation, it cannot be solved that
 * the true lane would be followed immediatelly by the false...)
 */

void InsertFakeFlow( BasicBlock bb )
{
    // Using:   AppendTo( BasicBlock, Instruction );
    //          ChangeToConditional( BasicBlock, BasicBlock, Instruction, TrueEnum TrueOnly, ... );

    BasicBlock fake1 = AppendTo( actualbb );
    BasicBlock fake2 = AppendTo( actualbb, fake1.Instructions[0] );
    
    ChangeToConditional( fake2, actualbb, fake1.Instructions[0], random );

    FillWithFake( fake1 );
    FllWithFake( fake2 );
}

/*
 * Todo: somehow change an unconditional jump to a conditional:
 * Notes:
 * Generating the condition is the key.
 * Not sure how the dead variables are represented...
 * We might generate a random reloperand, and choose two random
 * dead variables...
 */
ChangeToConditional( Instruction );

/*
 * Now here comes the dead path generation.
 * It needs the entry point for parameter.
 */

/*
 * Now here comes the dead part of the control flow transition,
 * wich looks like this:
 * 
 *            ------------------
 *          | Fake entry block |
 *           ------------------
 *                           \ (False)
 *                           ---------
 *                          |   bb1   | 
 *                           ---------
 *                      (True) /    \ (False)
 *                         ------  ------
 *                        | bb2  || bb3  |
 *                         ------  ------
 *                            |       |
 *                        (Random) (Random)
 * 
 * 
 */

void InsertDeadFlow( BasicBlock pre )
{
    // Using    ChangeToConditional( BasicBlock, BasicBlock, Instruction, TrueEnum TrueOnly, ... );
    //          AppendTo( BasicBlock, Instruction );
    //          RandomJumperBlock( Function );    

    BasicBlock dead1 = RandomJumperBlock( pre.parent );
    BasicBlock dead2 = RandomJumperBlock( pre.parent );

    ChangeToConditional( pre, dead1, dead1.Instructions[0], trueonly );

    BasicBlock dead3 = AppendTo( dead1, dead1.Instructions[0] );
    ChangeToConditional( dead3, dead1.Instructions[0], random );
    
}

/*
 * So now, we have the full CFT of the unconditional jump:
 * 
 *           ------------------
 *          | Fake entry block |
 *           ------------------
 *      (True) /             \ (False)
 *        -----------       ----------
 *       | Actual BB |      |   bb1   | 
 *        -----------       ----------
 *  (False) /   \ (True) (True) /    \ (False)
 *  ------------ \          ------  ------
 * | Fake Block | |         | bb2  || bb3  |
 *  ------------ /          ------  ------
 * |  Actual    |              |       |
 * | Successor  |          (Random) (Random)
 *  ------------
 *  
 */

/*
 * Now the next step is the conditional jump, which will be a tough one,
 * if I guess right. I start with analyzing the sheet wich describes the
 * method of meshing the conditional jumps, and later on I will try to create
 * the algorithm itself.
 */

enum Relop
	{
	    equ,
        notequ,
        great,
        greatequ,
        less,
        lessequ
	}

struct K
    {
		int i;
        bool usable_relops[6];
        bool used;
        K(int i) : i(i), used(false)
        {
            for (int i=0; i<6; i++)
                usable_relops[i] = true;
        }
	}

void MeshConditional( BasicBlock bb )
{
    int C = ReadConst( bb.Instructions.Last() );
    Relop r = ReadRelop( bb.Instructions.Last() );
    // Select m;
    m = 5; // At first we choose a constant number.
    
    // Generating the constants based on C
    list<K> KList;
    GenerateList( KList, C);
    
    // Iterating K[i] 
    while ( !AllUsed( KList ) )
    {
        K selected = Select_i(KList);
        Relop act_r = SelectUsableRelop( selected );
    }
}

/* Still to do:
 * 
 * void InsertFakeFlow(...); <- Done, details needed
 * void InsertDeadFlow(...); <- Done, same situation
 * 
 * These details include the discussion of the way helper functions
 * will work, and so on.
 * 
 * void MeshConditional(); <- Started brainstorming
 */

#endif