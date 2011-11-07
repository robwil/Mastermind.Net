using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

namespace Mastermind
{
    class MastermindAI
    {
        /* NOTE:
         * the AI handles things almost completely different from the GUI game, so it has been separated into its own file to reflect this fact
         */

        private const int DEPTH = 5; // depth is how many colors in sequence, this is also hard-coded in ColorSequence.
        private const int COLORS = 8;
        // Note: Changing amount of colors in sequence would not be a pain for the GUI anyway, so refactoring this to make it parameterized is not a high priority.
        private HashSet<ColorSequence> current_set; // holds current set of all possible codes        
        // reference to main game object, where GUI elements are stored
        // this is so AI can display its guesses on screen
        private Mastermind mastermind;

        // convenience declarations
        private ColorSequenceComparer csc; // so we only have to make one of these
        private Stopwatch stopWatch;

        public MastermindAI(Mastermind mastermind)
        {          
            this.mastermind = mastermind;
            csc = new ColorSequenceComparer();
            current_set = new HashSet<ColorSequence>();
            generatePossibilities(new ColorSequence(), 0);
            
            stopWatch = new Stopwatch();
            stopWatch.Start();            
            play();            
            mastermind.revealAnswer();
            //checker2();
        }

        // used for sanity checking, debugging
        private void checker()
        {
            int i, j = 0;
            foreach (ColorSequence possibility in current_set)
            {                
                for (i = 0; i < DEPTH; i++)
                    if (possibility[i] == 1)
                        break;
                if (i == DEPTH)
                    j++;
            }
            Console.WriteLine(j);
        }

        private void checker2()
        {
            byte[][] seqs = { new byte[] { 1, 1, 1, 1, 1 }, new byte[] { 1, 1, 1, 1, 2 }, new byte[] { 4, 4, 8, 8, 8 }, new byte[] { 1,1,1,2,4 }, new byte[] { 1,1,2,2,4 },
                              new byte[] {1,1,2,4,8}, new byte[] {1,16,2,4,8}};

            foreach(byte[] seq in seqs)
            {
                Possibility profile = profilePossibility(new ColorSequence(seq));
                Console.WriteLine(profile);
            }
        }

        // recursive approach is easiest way to populate possibilities (colors ^ slots = 8 ^ 5 = 32768)
        private void generatePossibilities(ColorSequence c, int n)
        {
            if (n == DEPTH) {
                current_set.Add(c);
                return;
            }           
            for(int i = 0; i < COLORS; i++)
            {
                ColorSequence c2 = (ColorSequence)c.Clone();
                c2[n] = (byte)(1 << i);
                generatePossibilities(c2, n + 1);           
            }            
        }

        /**
         * 
         * calculateMatch()
         * Calculates the amount of black and white hits when comparing guess with c.
         * Returns a Match struct.
         * 
         **/
        private Match calculateMatch(ColorSequence guess, ColorSequence c)
        {
            bool[] blackVisited = new bool[DEPTH]; // store if we recorded a black hit here
            bool[] whiteVisited = new bool[DEPTH]; // store if we recorded a white hit here
            int blackHits = 0;
            int whiteHits = 0;            

            // compare for black hits
            for (int i = 0; i < DEPTH; i++)
            {                
                // compare the selected color with the code in same location to see if black hit
                if (guess[i] == c[i]) {
                    blackHits++;
                    blackVisited[i] = true;
                    continue;
                }
            }

            // compare for white hits
            for (int i = 0; i < DEPTH; i++)
            {
                if (blackVisited[i]) continue; // if guess generated black hit, no need to see if it will be a white hit
                // compare with all other colors in code to see if white hit
                for (int j = 0; j < DEPTH; j++)
                {
                    if (j == i) continue; // already checked these when looking for black hits above
                    if (!blackVisited[j] && !whiteVisited[j] && guess[j] == c[i])
                    {
                        whiteHits++;
                        whiteVisited[j] = true;
                        break;
                    }
                }
            }
            return new Match(blackHits, whiteHits);
        }

        /**
         * restricts current_set to only those sequences which would hold true if the guess "c"
         * returns w white hits and b black hits.
         * Returns a new set containing these, which is promptly discarded if it doesn't minimize possibilities.
         */
        private HashSet<ColorSequence> restrictSet(ColorSequence c, int w, int b)
        {
            HashSet<ColorSequence> new_set = new HashSet<ColorSequence>(csc);
            foreach(ColorSequence possibility in current_set)
            {
                Match m = calculateMatch(possibility, c);
                if (m.blackHits == b && m.whiteHits == w)
                    new_set.Add(possibility);
            }
            return new_set;
        }

        /**
         * profilePossibility
         * This takes a ColorSequence and profiles the possibility.
         * This means that it calculates the worst-case scenario, where guessing this possibility leads to the largest number of remaining possible sequences.
         */
        private Possibility profilePossibility(ColorSequence possibility)
        {            
            int maxPossibilities = 0;
            ColorSequence maxSequence = null;
            int maxW = 0;
            int maxB = 0;
            for (int w = 0; w <= 5; w++)
            {
                for (int b = 0; b <= 5; b++)
                {
                    if (w + b > 5) continue;
                    HashSet<ColorSequence> new_set = restrictSet(possibility, w, b);
                    if (new_set.Count > maxPossibilities)
                    {
                        maxPossibilities = new_set.Count;
                        maxSequence = possibility;
                        maxW = w;
                        maxB = b;
                    }
                }
            }
            return new Possibility(maxPossibilities, possibility, maxW, maxB);
        }

        // play the game!
        private void play()
        {
            // convert GUI representation of code to AI representation of code
            byte[] seq = new byte[5];
            for(int i = 0; i < DEPTH; i++)
            {
                seq[i] = (byte)(1 << mastermind.code[i]);
            }
            ColorSequence code = new ColorSequence(seq);

            // first guess is always the same because it is proven to be best in worst-case
            byte[] firstGuess = {1,1,2,4,8};
            ColorSequence guess = new ColorSequence(firstGuess);
            Match m = calculateMatch(code, guess);
            if (showOnGUI(guess, m))
                return;
            current_set = restrictSet(guess, m.whiteHits, m.blackHits);

            for (int i = 1; i <= 8; i++)
            {
                mastermind.lblPossibilities.Text = "Possibilities: " + current_set.Count;
                mastermind.progressBar.Maximum = current_set.Count;
                mastermind.progressBar.Value = 0;

                Possibility bestPossibility = new Possibility(int.MaxValue, null, 0, 0);
                foreach (ColorSequence possibility in current_set)
                {
                    Possibility profile = profilePossibility(possibility);
                    if (profile.worstCaseSetCount < bestPossibility.worstCaseSetCount) // found a better possibility
                    {
                        bestPossibility = profile;
                    }
                    //Console.WriteLine(bestPossibility.ToString());
                    mastermind.progressBar.Value += 1;
                    Application.DoEvents();
                }
                //Console.WriteLine(bestPossibility);
                m = calculateMatch(code, bestPossibility.sequence);
                if (showOnGUI(bestPossibility.sequence, m))
                    return;
                current_set = restrictSet(bestPossibility.sequence, m.whiteHits, m.blackHits);
            }
        }

        /**
         * Shows the AI's guess on the screen, with the appropriate pegs for the match it received.
         * The bool return value signifies whether game is over or not, which is handled by play(), this function's caller.
         */
        private bool showOnGUI(ColorSequence guess, Match m)
        {
            int x;
            // show the guess on the screen
            for (x = 0; x < DEPTH; x++)
            {
                mastermind.holes[x, mastermind.activeRow].Image = mastermind.images[(int)log2(guess[x])];
            }
            // show appropriate pins for the match
            x = 0;
            for (int i = 0; i < m.blackHits; i++)
                mastermind.pegs[x++, mastermind.activeRow - 1].Image = mastermind.blackPegImage;
            for (int i = 0; i < m.whiteHits; i++)
                mastermind.pegs[x++, mastermind.activeRow - 1].Image = mastermind.whitePegImage;
            for (; x < DEPTH; x++)
                mastermind.pegs[x, mastermind.activeRow - 1].Image = mastermind.emptyPegImage;
            for (x = 0; x < DEPTH; x++)
                mastermind.pegs[x, mastermind.activeRow - 1].Visible = true;

            mastermind.Refresh();

            // check for game end conditions
            if (m.blackHits == DEPTH)
            {
                cleanUp();
                TimeSpan ts = stopWatch.Elapsed;
                MessageBox.Show("The AI cracked the code in " + ( 9 - mastermind.activeRow) + " attempts! Processing took " + ts.Minutes + ":" + ts.Seconds + "." + ts.Milliseconds, "Computer wins!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            else
            {
                mastermind.progressBar.Location = new Point(mastermind.progressBar.Location.X, mastermind.progressBar.Location.Y - 55);
                mastermind.lblPossibilities.Location = new Point(mastermind.lblPossibilities.Location.X, mastermind.lblPossibilities.Location.Y - 55);
                if (--mastermind.activeRow == 0) // ran out of attempts, they lose
                {
                    cleanUp();
                    TimeSpan ts = stopWatch.Elapsed;
                    MessageBox.Show("The AI failed to crack the code in 8 attempts. Bad luck!Processing took " + ts.Minutes + ":" + ts.Seconds + "." + ts.Milliseconds, "Computer loses!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }                
            }
            return false;
        }

        private void cleanUp()
        {
            mastermind.progressBar.Visible = false;
            mastermind.lblPossibilities.Visible = false;
            stopWatch.Stop();
        }

        private int log2(byte p)
        {
            int i;
            for(i = 0; (p & 1) != 1; i++)            
                p >>= 1;                           
            return i;
        }
    }
}
