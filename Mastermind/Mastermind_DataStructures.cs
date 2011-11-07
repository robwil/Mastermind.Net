using System;
using System.Collections.Generic;
using System.Text;

namespace Mastermind
{
    // data structure used to package black and white hits
    public struct Match
    {
        public int blackHits, whiteHits;

        public Match(int blackHits, int whiteHits)
        {
            this.blackHits = blackHits;
            this.whiteHits = whiteHits;
        }
    }

    // data structure used to package sequence with its possibilities
    // used extensively by AI methods to pick next move
    public struct Possibility
    {
        public int worstCaseSetCount; // represents the worst-case count that the set of possibilities would have if this.sequence was guessed
        public ColorSequence sequence;
        public int w, b; // represents the amount of white and black matches which produces the worst-case count above

        public Possibility(int worstCaseSetCount, ColorSequence sequence, int w, int b)
        {
            this.worstCaseSetCount = worstCaseSetCount;
            this.sequence = sequence;
            this.w = w;
            this.b = b;
        }

        public override String ToString()
        {
            return sequence.ToString() + " => " + worstCaseSetCount + " (w = " + w + ", b = " + b + ")";
        }
    }

    // data structure representing a color sequence (i.e. code) in only 5 bytes
    // each byte repesents a hole which can contain one of the 8 colors
    // the color is specified using a 1 bit in the correct spot
    // e.g. if (colors[0] & (1 << 0)) then colors[0] is color #0
    // e.g. if (colors[0] & (1 << n)) then colors[0] is color #n where n = [0,7]
    public class ColorSequence : ICloneable
    {
        private byte[] colors;
        public ColorSequence()
        {
            colors = new byte[5];
        }
        public ColorSequence(byte[] colors)
        {
            this.colors = colors;
        }
        public byte this[int index]
        {
            get
            {
                return colors[index];
            }
            set
            {
                colors[index] = value;                
            }
        }
        public object Clone()
        {
            return new ColorSequence((byte[])colors.Clone());
        }
        public byte[] getColorsArray()
        {
            return colors;
        }
        public override String ToString()
        {
            String s = "[";
            for (int i = 0; i < colors.Length; i++)
            {
                s += colors[i];
                if (i != colors.Length - 1)
                    s += ",";
            }
            s += "]";
            return s;
        }
    }

    public class ColorSequenceComparer : EqualityComparer<ColorSequence>
    {
        public override bool Equals(ColorSequence x, ColorSequence y)
        {
            //Console.WriteLine("collision");
            for (int i = 0; i < 5; i++)
                if (x[i] != y[i])
                    return false;
            return true;
        }

        public override int GetHashCode(ColorSequence obj)
        {

            return (obj.getColorsArray()).GetHashCode();
        }
    }
}
