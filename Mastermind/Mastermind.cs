using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace Mastermind
{
    public partial class Mastermind : Form
    {        
        // game-specific constants
        private const int COLS = 5;
        private const int ROWS = 9;
        private const int COLORS = 8;
        private Random random = new Random();

        // game UI-specific data
        protected internal PictureBox[,] holes = new PictureBox[COLS, ROWS]; // x values refer to column, y values to row
        protected internal PictureBox[,] pegs = new PictureBox[COLS, ROWS - 1]; // rows-1 because there is no need to have pegs for answer row
        private PictureBox[] colorImages = new PictureBox[COLORS];
        protected internal Image[] images = new Image[COLORS + 6]; // extra 3 are for inactive, active, and answer holes + white, black, empty pegs

        // data needed during gameplay
        private bool gameStarted = false;
        private Image selectedColor = null;
        protected internal int activeRow = ROWS - 1;
        protected internal int[] code = new int[COLS]; // the actual code that is the goal of the game

        // game-specific convenience data
        private Image inactiveHoleImage, activeHoleImage, answerHoleImage;
        protected internal Image emptyPegImage, whitePegImage, blackPegImage;

        // AI object
        MastermindAI ai;

        public Mastermind()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen; // center form on screen
            //printResources();
            // game-specific code follows
            loadImages();
            createControls();
            setupGame();
        }            

        /**
         * 
         * printResources()
         * For internal use only to get the resource names used by .NET.
         * This is how the paths to the image resource streams in loadImages() were determined.
         * 
         **/
        private void printResources()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string[] names = currentAssembly.GetManifestResourceNames();
            foreach (string name in names)
            {
                Console.WriteLine(name);
            }
        }

        /**
         * 
         * loadImages()
         * Loads the PNG files from Images/ resource folder into C# Image objects (i.e. into memory).
         * These are what get plugged into the PictureBoxes and appear on the screen.
         * 
         **/
        private void loadImages()
        {
            // use Reflection to get current assembly and get the image files from Resources
            Assembly currentAssembly = Assembly.GetExecutingAssembly();    
            Stream imageStream;

            // load colored circle images
            for (int i = 0; i < COLORS; i++)
            {
                imageStream = currentAssembly.GetManifestResourceStream("Mastermind.Images.shadow_ball_" + i + ".png");
                images[i] = Image.FromStream(imageStream);
            }

            // load hole and peg images
            String[] types = {"inactive", "active", "answer", "empty_peg", "white_peg", "black_peg"};
            int offset = COLORS;
            for (int i = offset; i < offset + 6; i++)
            {
                imageStream = currentAssembly.GetManifestResourceStream("Mastermind.Images." + types[i - offset] + ".png");
                images[i] = Image.FromStream(imageStream);
            }
            // map commonly used hole and peg images to convenience variables to make later code more readable
            inactiveHoleImage = images[offset];
            activeHoleImage = images[offset + 1];
            answerHoleImage = images[offset + 2];
            emptyPegImage = images[offset + 3];
            whitePegImage = images[offset + 4];
            blackPegImage = images[offset + 5];
        }

        /**
         * 
         * createControls()
         * Dynamically create the PictureBox controls that represent the game board.
         * Fill them with the default images for a starting game.
         * Also places the controls on the screen. This method is the one place where all game UI elements are.
         * 
         **/
        private void createControls()
        {            
            // create controls for holes
            int baseX = 20;
            int baseY = 20;
            int xSpacing = 5;
            int ySpacing = 5;
            Size imageSize = new Size(50, 50);
            for (int y = 0; y < ROWS; y++)
            {
                for (int x = 0; x < COLS; x++)
                {
                    holes[x, y] = new PictureBox();
                    holes[x, y].Size = imageSize;
                    holes[x, y].Image = inactiveHoleImage;
                    holes[x, y].Location = new Point(baseX + x * (imageSize.Width + xSpacing), baseY + y * (imageSize.Height + ySpacing));
                    holes[x, y].Click += new EventHandler(Hole_Click);
                    holes[x, y].Name = "hole" + x + "," + y;
                    this.Controls.Add(holes[x, y]);
                }
            }

            // create controls for colors (so user can place them)
            baseX = 20;
            baseY = 515;
            for (int i = 0; i < COLORS; i++)
            {
                colorImages[i] = new PictureBox();
                colorImages[i].Size = imageSize;
                colorImages[i].Image = images[i];
                colorImages[i].Location = new Point(baseX + i * (imageSize.Width + xSpacing), baseY);
                colorImages[i].Click += new EventHandler(Color_Click);
                colorImages[i].Name = "color" + i;
                this.Controls.Add(colorImages[i]);
            }
            
            // create controls for pegs
            baseX = 295;
            baseY = 75;
            xSpacing = 0;
            ySpacing = 5;
            imageSize = new Size(30, 50);
            for (int y = 0; y < ROWS-1; y++)
            {
                for (int x = 0; x < COLS; x++)
                {
                    pegs[x, y] = new PictureBox();
                    pegs[x, y].Size = imageSize;
                    pegs[x, y].Image = emptyPegImage;
                    pegs[x, y].Location = new Point(baseX + x * (imageSize.Width + xSpacing), baseY + y * (imageSize.Height + ySpacing));
                    pegs[x, y].Visible = false;
                    this.Controls.Add(pegs[x, y]);
                }
            }           
        }        

        void Color_Click(object sender, EventArgs e)
        {
            if (!gameStarted)
                return;

            PictureBox colorImage = (PictureBox)sender;
            String name = colorImage.Name;

            // color images are named like "color0", "color1", etc.
            // so the following line extracts the number by skipping first 5 characters and converting to int
            int i = Convert.ToInt32(name.Substring(5));

            // change global game state to reflect selected color
            selectedColor = images[i];
            // and show it to the user by enabling a border on only the selected color
            for (int j = 0; j < COLORS; j++)
            {
                if (j == i)
                    colorImages[j].BorderStyle = BorderStyle.FixedSingle;
                else
                    colorImages[j].BorderStyle = BorderStyle.None;
            }
        }

        void Hole_Click(object sender, EventArgs e)
        {
            if (!gameStarted)
                return;

            PictureBox hole = (PictureBox)sender;
            String name = hole.Name;

            // holes are named like "hole0,0", "hole1,5", etc.
            // so the following extracts the x and y
            int x = Convert.ToInt32(name.Substring(4, 1));
            int y = Convert.ToInt32(name.Substring(6, 1));

            if (selectedColor != null && y == activeRow)
            {
                hole.Image = selectedColor;
            }
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            startGame();
        }

        private void btnSubmitRow_Click(object sender, EventArgs e)
        {
            int x;
            // validate that all holes in active row are filled
            for (x = 0; x < COLS; x++)
            {
                if (holes[x, activeRow].Image == activeHoleImage)
                    break;
            }
            if (x != COLS) // means that the loop broke, so one of holes is not filled
            {
                MessageBox.Show("You must fill in all the holes for the active row before submitting.", "Some Holes Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // calculate match and show appropriate pins
            Match m = calculateUserMatch();
            Console.WriteLine("White hits: " + m.whiteHits + " / blackHits: " + m.blackHits);
            x = 0;
            for (int i = 0; i < m.blackHits; i++)
                pegs[x++, activeRow - 1].Image = blackPegImage;
            for (int i = 0; i < m.whiteHits; i++)
                pegs[x++, activeRow - 1].Image = whitePegImage;
            for (; x < COLS; x++)
                pegs[x, activeRow - 1].Image = emptyPegImage;
            for (x = 0; x < COLS; x++)
                pegs[x, activeRow - 1].Visible = true;

            if (m.blackHits == COLS)
            {
                MessageBox.Show("Good job! You cracked the code in " + (ROWS - activeRow) + " attempts!", "Congratulations, you win!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                endGame();
            }
            else
            {

                btnSubmitRow.Location = new Point(btnSubmitRow.Location.X, btnSubmitRow.Location.Y - 55);
                if (--activeRow == 0) // ran out of attempts, they lose
                {
                    MessageBox.Show("Sorry, you didn't crack the code in the eight attempts.", "You Lost!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    endGame();
                }
                else
                { // highlight new row as active
                    for (x = 0; x < COLS; x++)
                    {
                        holes[x, activeRow].Image = activeHoleImage;
                    }
                }
            }
        }

        private void endGame()
        {
            gameStarted = false;
            btnSubmitRow.Visible = false;
            // reveal the answer row
            revealAnswer();
        }

        private void setupGame()
        {
            gameStarted = false;
            btnSubmitRow.Visible = false;
            progressBar.Visible = false;
            lblPossibilities.Visible = false;
        }  

        private void startGame()
        {            
            activeRow = ROWS - 1;
            selectedColor = null;
            // reset holes, pegs, and colors to initial state
            for (int y = 0; y < ROWS; y++)
            {
                for (int x = 0; x < COLS; x++)
                {    
                    if (y < ROWS - 1) // because pegs don't exist for answer row, so there is one less
                        pegs[x, y].Visible = false;
                    if (y == 0) // top row is the answer holes
                        holes[x, y].Image = answerHoleImage;
                    else if (y == activeRow) // bottom row is active to start game
                        holes[x, y].Image = activeHoleImage;
                    else // all else start inactive
                        holes[x, y].Image = inactiveHoleImage;
                }                
            }
            for (int i = 0; i < COLORS; i++)
            {
                colorImages[i].BorderStyle = BorderStyle.None;
            }
            // reset submit row button
            btnSubmitRow.Visible = true;
            btnSubmitRow.Location = new Point(330, 470);
            // randomly generate code
            for (int i = 0; i < COLS; i++)
            {
                code[i] = random.Next(0, COLORS - 1);
               // Console.WriteLine("Code " + i + ": " + code[i]);
            }

            gameStarted = true; // enables interactions            
        }

        /**
         * 
         * calculateUserMatch()
         * Calculates the amount of black and white hits for the activeRow.
         * Returns a Match struct.
         * 
         **/
        private Match calculateUserMatch()
        {
            int[] guesses = new int[COLS]; // record user guesses
            bool[] blackVisited = new bool[COLS]; // store if we recorded a black hit here
            bool[] whiteVisited = new bool[COLS]; // store if we recorded a white hit here
            int blackHits = 0;
            int whiteHits = 0;
            // convert images into ints and store in "guesses"
            for (int i = 0; i < COLS; i++)
            {
                // determine which color was picked by comparing images
                int guessedColor = -1;
                for (int j = 0; j < COLORS; j++)
                {
                    if (holes[i, activeRow].Image == images[j])
                    {
                        guessedColor = j;
                        break;
                    }
                }
                // error checking
                if (guessedColor == -1) {
                    MessageBox.Show("Something went very wrong with the match calculation!", "Serious Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("Failed Match Calculation!");
                }
                guesses[i] = guessedColor;
            }

            // compare for black hits
            for (int i = 0; i < COLS; i++)
            {                
                // compare the selected color with the code in same location to see if black hit
                if (code[i] == guesses[i]) {
                    blackHits++;
                    blackVisited[i] = true;
                    continue;
                }
            }

            // compare for white hits
            for (int i = 0; i < COLS; i++)
            {
                if (blackVisited[i]) continue; // if guess generated black hit, no need to see if it will be a white hit
                // compare with all other colors in code to see if white hit
                for (int j = 0; j < COLS; j++)
                {
                    if (j == i) continue; // already checked these when looking for black hits above
                    if (!blackVisited[j] && !whiteVisited[j] && code[j] == guesses[i])
                    {
                        whiteHits++;
                        whiteVisited[j] = true;
                        break;
                    }
                }
            }
            return new Match(blackHits, whiteHits);
        }

        private void btnAIStart_Click(object sender, EventArgs e)
        {
            // reset anything that may have been changed by someone playing a game
            startGame();
            endGame();
            // show progress bar and label for possibilities; make sure they are at their default locations
            progressBar.Location = new Point(303, 470);
            progressBar.Visible = true;
            lblPossibilities.Location = new Point(301, 492);
            lblPossibilities.Visible = true;
            // disable buttons so they can't interrupt computer
            btnStartGame.Enabled = false;
            btnAIStart.Enabled = false;
            // launch AI
            ai = new MastermindAI(this);            
            // after AI is finished, re-enable buttons
            btnStartGame.Enabled = true;
            btnAIStart.Enabled = true;
        }



        protected internal void revealAnswer()
        {
            // reveal the answer row
            for (int x = 0; x < COLS; x++)
            {
                holes[x, 0].Image = images[code[x]];
            }
        }
    }
}
