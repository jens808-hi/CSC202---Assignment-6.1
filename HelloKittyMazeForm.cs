// Using built-in libraries for Windows Forms and the tools within the app, including labels, buttons, images, list boxes, message boxes, sound effects and external links
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

// Name of my project where all my forms and code is stored
namespace WindowsFormsApp2
{
    // Represents the GUI's Main/starting window of my project
    public partial class HelloKittyMazeForm : Form
    {
        // Array that stores recipes names and thier correspoinding list of required ingredients 
        Recipe[] recipeArray;
        // Stores the name of the currently generated recipe
        Recipe currentRecipe;
        // Array that stores the instructions for the recipe
        String[][] instructionsArray;
        // Tracks correct guesses for score logs
        int correctGuesses = 0;
        // Tracks wrong guesses for score logs
        int wrongGuesses = 0;
        // Tracks how many tries the user has made, using a 3-try game loop counter 
        int tries = 0;  
        //  Stores the folder path inside the user's Documents folder
        string mazeFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SanrioApp");
        // Stores the file path for my receipt history text file where it will be saved and read from
        string mazeScoreFilePath;

        // Stores the correct list of ingredients for the current recipe 
        List<string> correctIngredients;
        // Random number generator used to randomly select recipe from the dictionary 
        private Random rand = new Random();
        // Variable to track the notepad process 
        Process notepadProcess;


        // Creates a SoundPlayer object named bgMusic that will load and play the .wav for background music  
        private SoundPlayer bgMusic = new SoundPlayer("opening-cartooon-sound.wav");
        // Plays mouse clikcing sound 
        private SoundPlayer player1 = new SoundPlayer("mouse-clicking-sound.wav");
        // Plays alert sound 
        private SoundPlayer player2 = new SoundPlayer("alert-sound.wav");

        // Runs first when the form is initially created 
        public HelloKittyMazeForm()
        {
            // Sets up all the buttons, labels, textboxes, and controls
            InitializeComponent();

            // Disables submit button until a recipe is generated, prevents user from submittin before starting a round
            btnSubmit.Enabled = false;


            // Runs the array and populates all available recipes, each recipe matches to a a list of correct ingredients
            recipeArray = new Recipe[]
            {
                new Recipe("Strawberry Shortcake", new string[] { "strawberry", "eggs", "milk", "cake" }, Properties.Resources.strawberry_shortcake),
                new Recipe("Matcha Cream Cake", new string[] { "eggs", "milk", "cake", "green tea" }, Properties.Resources.matcha_cream_cake),
                new Recipe("Matcha Green Tea Shortbread", new string[] { "eggs", "milk", "cookies", "green tea" }, Properties.Resources.matcha_green_tea_shortbread),
                new Recipe("Strawberry Matcha Ice Cream", new string[] { "ice cream", "milk", "green tea", "strawberry" }, Properties.Resources.strawberry_matcha_ice_cream),
                new Recipe("Strawberry Tres Leches cake", new string[] { "eggs", "milk", "cake", "strawberry" }, Properties.Resources.strawberry_tres_leches_cake),
                new Recipe("Watermelon Boba Milk Tea", new string[] { "milk", "tea", "boba", "watermelon" }, Properties.Resources.watermelon_boba_milk_tea),
                new Recipe("Strawberry Watermelon Ice Cream", new string[] { "ice cream", "strawberry", "watermelon" }, Properties.Resources.strawberry_watermelon_ice_cream),
                new Recipe("Strawberry Papaya Milk Tea", new string[] { "milk", "papaya", "strawberry", "tea" }, Properties.Resources.strawberry_papaya_milk_tea),
                new Recipe("Strawberry Boba Milk Shake", new string[] { "ice cream", "milk", "strawberry", "boba" }, Properties.Resources.watermelon_boba_milk_tea),
            };

            // Runs the array for all the cooking/baking instructions that matches to each recipe
            instructionsArray = new string[][]
            {
                new string[]
                {
                    "Gather all ingredients.",
                    "Mix ingredients together.",
                    "Bake and enjoy!"
                },
                new string[]
                {
                    "Prepare matcha mixture.",
                    "Blend with cake batter.",
                    "Bake and serve."
                },
                  new string[]
                {
                    "Prepare matcha powder mixture.",
                    "Blend eggs with milk and cookie batter.",
                    "Bake and serve."
                },
                    new string[]
                {
                    "Mix icecream and matcha.",
                    "Cook strawberries.",
                    "Add strawberry puree to a cup of milk.",
                    "Gently fold strawberry milk into matcha icecream.",
                    "Freeze for 3 hours and serve."
                },
                    new string[]
                {
                    "Gather all ingredients.",
                    "Blend cake batter with eggs.",
                    "Bake the cake.",
                    "Prepare the strawberry milk.",
                    "Soak the cake.",
                    "Serve chilled"
                },
                new string[]
                {
                    "Brew the tea.",
                    "Add milk and watermelon.",
                    "Mix in boba pearls.",
                },
                    new string[]
                {
                    "Cook strawberries.",
                    "Chop watermelon into chunks.",
                    "Add strawberry puree to icecream",
                    "Gently fold watermelon into strawberry icecream.",
                    "Freeze for 3 hours and serve."
                },
                 new string[]
                {
                    "Brew the tea.",
                    "Add milk and papaya.",
                    "Mix in boba pearls.",
                },
                  new string[]
                {
                    "Brew the tea.",
                    "Add milk and strawberry.",
                    "Mix in boba pearls.",
                    "Add icecream to mixture and mix well",
                    "Serve chilled and enjoy.",
                }
            };
        }

        // Resets the game after a win or after 3 failed attemps 
        private void ResetGame()
        {
            // Resets the counter back to zero
            tries = 0;
            // Clears the current recipe so user has to generate a new one
            currentRecipe = null;
            // Removes all selected items from the ingredient list box
            lstIngredients.ClearSelected();
            // Disables submit button until a new recipe is generated 
            btnSubmit.Enabled = false;
            // Re-enables the generate button so user can start a new round
            btnGenerate.Enabled = true;
        }
        // Runs when the form first appears on the screen 
        private void HelloKittyMazeForm_Load_1(object sender, EventArgs e)
        {
            // Runs the string array that stores all ingredient options, which also represent the "shelf" location in the grocery maze
            string[] allIngredients =
            {
                "ice cream",
                "eggs",
                "milk",
                "tea",
                "cake",
                "strawberry",
                "papaya",
                "green tea",
                "boba",
                "cookies",
                "watermelon" };

            // Adds every string stored in the allIngredients array to the Items collection of lstIngredients control all at once, with each ingredient appearing as an individual selectable entry
            lstIngredients.Items.AddRange(allIngredients);

            // Checks to see if the folder already exists
            if (!Directory.Exists(mazeFolderPath))
            {
                // If the folder does not exist, it creates the folder at the path inside the variable
                Directory.CreateDirectory(mazeFolderPath);
            }

            // Combines the folder path with the file name and sets the full file path
            mazeScoreFilePath = Path.Combine(mazeFolderPath, "MazeScoreHistory.txt");
        }

        // Button that generates the recipe and starts a new round
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Resets attempt counter at the start of a new round
            tries = 0;
            wrongGuesses = 0;

            // Randomly selects a recipe from the list
            int index = rand.Next(recipeArray.Length);
            // Stores the selected recipe name
            currentRecipe = recipeArray[index];
            // Stores the correct ingredient list 
            //correctIngredients = selectedRecipe.Ingredients;

            // Displays the recipe name to user
            lblRecipe.Text = "Recipe: " + currentRecipe.Name;
            // Tells the user to start selecting ingredients
            lblStatus.Text = "Select ingredients!";
            // Changes the status text color to blue violet
            lblStatus.ForeColor = Color.BlueViolet;

            // Clears the previous selections
            lstIngredients.ClearSelected();

            // Disables generate button during an active round
            btnGenerate.Enabled = false;

            // Enables submit buttom after recipe is generated
            btnSubmit.Enabled = true;
        }

        // Validates the user's ingredient selection, compares them to the correct recipe, and implements the 3 try game loop logic using an incremented counter
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            // Tries to run the code safely but if an error happens, catch block will handle it
            try
            {
                // Ensures recipe has been generated before enabling submit button 
                if (currentRecipe == null)
                    // Throws an exception if the user tries to submit before generating a recipe, triggers the catch block for to handle the error
                    throw new Exception("Generate a recipe first!");

                // Ensures the user has selected at least one ingredient
                if (lstIngredients.SelectedItems.Count == 0)
                    // Throws an exception if the users clicks submit without selectin any ingredients, preventing empty submissions and triggers the error handling 
                    throw new Exception("You must select ingredients!");

                // Stores the ingredients the user selected and compares it to the correct recipe regardless of order and prevents duplicates
                HashSet<string> userSelection = new HashSet<string>();

                // Adds all selected items to Hashset
                foreach (var item in lstIngredients.SelectedItems)
                {
                    // Converts the selected listbox item to a string and stores it in the user's ingredient collection for comparison
                    userSelection.Add(item.ToString());
                }
                // Checks if user selection matches exactly to recipe ingredients 
                if (userSelection.SetEquals(currentRecipe.Ingredients))
                {
                    // Increase correct score
                    correctGuesses++;
                    // Displays a message to user when the correct ingredients are selected
                    lblStatus.Text = "That's Correct! Hello Kitty reached the flag!";
                    // Changes the status text color to green to indicate the answer is correct 
                    lblStatus.ForeColor = Color.Green;

                    // Save results before resetting
                    SaveMazeResult("WIN", wrongGuesses);

                    // Tries to run the code safely but if an error happens, catch block will handle it
                    try
                    {
                        // Creates a new Recipe form (Cookbook) using the current recipe object
                        frmRecipe recipeForm = new frmRecipe(currentRecipe);
                        // Opens the Recipe Form
                        recipeForm.ShowDialog();
                    }
                    // Runs if any error occurs inside the try block and stores it in ex to show details of the error
                    catch (Exception ex)
                    {
                        // If something goes wrong, display error message
                        MessageBox.Show("Error: Error opening recipe: \n" + ex.Message);
                    }

                    // Ends the loop after 3 tries
                    ResetGame();
                    // Exits the round
                    return;
                }
                else
                {
                    // Increment value by 1 for wrong guesses and total number of attempts
                    wrongGuesses++;
                    tries++;
                }
                
                // Checks if the user has used less than 3 attempts, if so the condition becomes true and the game continues 
                if (tries < 3)
                {
                    // Updates the status label to tell the user their guess is wrong,
                    // then calculates how many attempts they have left using the $ symbol to insert it directly inside the string
                    lblStatus.Text = $"Wrong! {3 - tries} tries left";
                    // Changes the text color to dark orange to indicate the answer is wrong but the game isn't over just yet 
                    lblStatus.ForeColor = Color.DarkOrange;
                    // Clears selections for the next attempt
                    lstIngredients.ClearSelected();
                }
                else
                {
                    // Updates the label after the 3rd failed attempt to tell the user that the game is over and to generate new recipe
                    lblStatus.Text = "Game Over! Generate a new recipe.";
                    // Changes the status text color to red to indicate the answer is wrong 
                    lblStatus.ForeColor = Color.Red;

                    // Save only when the round ends
                    SaveMazeResult("Game Over", wrongGuesses);

                    // Ends the loop after 3 tries
                    ResetGame();
                }
            }
            // Runs if any error occurs inside the try block and stores it in ex to show details of the error
            catch (Exception ex)
            {
                // Displays error message if validation fails
                lblStatus.Text = "Error!: " + ex.Message;
                // Changes the label text color to red to indicate an error warning
                lblStatus.ForeColor = Color.Red;

                // Clears the selection after an error
                lstIngredients.ClearSelected();
            }

        }

        // Method that saves the results and wrong guesses of the maze game to text file
        private void SaveMazeResult(string resultType, int wrongAttemptsThisRound)
        {
            // Tries to run the code safely but if an error happens, catch block will handle it
            try
            {
                // Creates a string called result that stores all the saved information to append to later
                string result = 
                    // Title for result log 
                    "Hello Kitty Maze Results\n" +
                    // Shows the result passed into the method
                    "Result: " + resultType +
                    // Shows the number of wrong attempts 
                    "\nWrong Guesses: " + wrongAttemptsThisRound +
                    // Shows the current date and time in this general ("g") format MM/DD/YYYY and HH:MM AM/PM
                    "\nDate: " + DateTime.Now.ToString("g") +
                    // Shows separator line
                    "\n---------------------------------------------\n";

                // Opens the file at mazeScoreFilePath and adds scoring text to the end of the file without deleting any existing scores
                File.AppendAllText(mazeScoreFilePath, result);
            }
            // Runs if any error occurs inside the try block and stores it in ex to show details of the error
            catch (Exception ex)
            {
                // If something goes wrong, display error message
                MessageBox.Show("Error saving maze score: " + ex.Message);
            }
        }

        // Runs when the "View Score" button is clicked
        private void btnViewMazeScores_Click(object sender, EventArgs e)
        {
            // Tries to run the code safely but if an error happens, catch block will handle it
            try
            {
                // Checks to see if the folder already exists
                if (File.Exists(mazeScoreFilePath))
                {
                    // Start Notepad and store the process
                    notepadProcess = Process.Start(new ProcessStartInfo()
                    {
                        // Name of the file to open in Notepad 
                        FileName = mazeScoreFilePath,
                        // Tells windows to use the default program
                        UseShellExecute = true,
                    });
                }
                else
                {   // Shows a message box with the error message to provide user with feedback
                    MessageBox.Show("No maze score history found yet.",
                        // Title of the message box
                        "Maze Scores",
                        // Displays an Ok button
                        MessageBoxButtons.OK,
                        // Displays an info icon (blue circle with lowercase "i")
                        MessageBoxIcon.Information);
                }
            }
            // Runs if any error occurs inside the try block and stores it in ex to show details of the error
            catch (Exception ex)
            {
                // If something goes wrong, display error message
                MessageBox.Show("Error opening maze file: " + ex.Message);
            }
        }
        // Runs when the "Clear Score" button is clicked
        private void btnClearMazeScores_Click(object sender, EventArgs e)
        {
            // Creates a message box to display message to the user
            DialogResult result = MessageBox.Show(
                "Are you sure you want to clear maze score history?",
                // Title of the message box
                "Confirm",
                // Displays two buttons on the message box, Yes and No
                MessageBoxButtons.YesNo,
                // Displays a question mark icon
                MessageBoxIcon.Question);

            // If the user selects yes
            if (result == DialogResult.Yes)
            {
                // Tries to run the code safely but if an error happens, catch block will handle it
                try
                {
                    // Opens the file at mazeScoreFilePath, deletes previous score history and writes an empty string;  completely overwrites the file 
                    File.WriteAllText(mazeScoreFilePath, string.Empty);

                    // Creates a message box to display message to the user
                    MessageBox.Show("Maze score history has been cleared!",
                        // Title of the message box
                        "Confirmation",
                        // Displays an Ok button
                        MessageBoxButtons.OK,
                        // Displays an info icon (blue circle with lowercase "i")
                        MessageBoxIcon.Information);
                }
                // If the file is currently open another program (like notepad), IOException will occur
                catch (IOException)
                {
                    // Creates a message box to display message to the user
                    MessageBox.Show("Please close the MazeScoreHistory.txt file in Notepad first before clearing.",
                        // Title of the message box
                        "File in Use",
                        // Displays an Ok button
                        MessageBoxButtons.OK,
                        // Displays an warning icon (exclamation inside triangle)
                        MessageBoxIcon.Warning);
                }
                // Runs if any error occurs inside the try block and stores it in ex to show details of the error
                catch (Exception ex)
                {
                    // If something goes wrong, display error message
                    MessageBox.Show("Error clearing maze score file: " + ex.Message);
                }
            }
        }

        // Runs when the "View Cookbook" button is clicked
        private void btnCookbook_Click(object sender, EventArgs e)
        {
            // Tries to run the code safely but if an error happens, catch block will handle it
            try
            {
                // Checks if the recipe Array is empty or exists. If true, then there are no recipes to display
                if (recipeArray == null || recipeArray.Length == 0)
                    // Prevents cookbook form from opening empty, forcing the program to stop and sends error message to the catch block
                    throw new Exception("No recipes available.");

                // Creates a new Cookbook form window while sending the reicpeArray and instructionsArray into the new form to display
                CookbookForm cookbook = new CookbookForm(recipeArray, instructionsArray);
                // Shows the cookbook as a pop-up 
                cookbook.ShowDialog();
            }
            // Runs if any error occurs inside the try block and stores it in ex to show details of the error
            catch (Exception ex)
            {
                // If something goes wrong, display error message
                MessageBox.Show("Error opening cookbook: \n" + ex.Message);
            }
        }

        // Runs when the user clicks "Return to Sanrio Menu" in the toolbar
        private void returnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Stops looping background music 
            bgMusic.Stop();

            //Returns the user to the NewSanrioGUI form 
            NewSanrioGUI sanrioGUI = new NewSanrioGUI();

            // Hides the HelloKittyMaze Form while playing
            this.Hide();
            // Shows the NewSanrioGUI form
            sanrioGUI.ShowDialog();
            // Closes the Maze form
            this.Show();
        }

        // Runs when the user clicks the "Exit" menu option in the toolbar
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Creates a a message box asking the user for confirmation before exiting the application 
            DialogResult result = MessageBox.Show(
               "Are you sure you want to exit the application?",
               // Title of the message box
               "Quit",
               // Displays two buttons on the message box, Yes and No
               MessageBoxButtons.YesNo,
               // Displays a question mark icon
               MessageBoxIcon.Question
            );

            // Plays mouse clicking sound
            player1.Play();

            // Asks the user to make a decision 
            if (result == DialogResult.Yes)
            {
                // Plays alert sound 
                player2.Play();
                // Closes all forms and shuts down the entire application 
                Application.Exit();
            }
        }

        // Runs when the user clicks the "Sanrio Quiz" menu option in the toolbar
        private void QuizToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Returns the user to the new Sanrio Main form 
            SanrioMain sanrioMain = new SanrioMain();

            // Hides the menu while playing 
            this.Hide();
            // Shows the Sanrio Main form
            sanrioMain.ShowDialog();
            // Shows the menu again when done taking quiz
            this.Show();
        }

        // Runs when the user clicks the "Grocery Shopping" menu option in the toolbar
        private void groceryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Stops looping background music 
            bgMusic.Stop();

            //Returns the user to the new Sanrio Grocery form 
            SanrioGroceryForm sanrioGrocery = new SanrioGroceryForm();

            this.Hide();
            // Shows the Sanrio Grocery form 
            sanrioGrocery.ShowDialog();
            // Closes the Sanrio Main form
            this.Show();
        }

        // Runs when the user clicks the "YouTube Link" menu option in the toolbar
        private void youTubeLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Stops looping background music 
            bgMusic.Stop();

            // Stores the URL of the YouTube/Bing video link 
            string url = "https://www.bing.com/videos/riverview/relatedvideo?q=dear%20daniel%20hello%20kitty&mid=429998C9548B77833FBF429998C9548B77833FBF&ajaxhist=0";

            // Tries to open the URL in the default web browser safely but if an error happens, catch block will handle it 
            try
            {
                // Creates a ProcessStartInfo object to open the link in the default browser
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    // The property that holds the URL to open 
                    FileName = url,
                    // Tells Windows to open the url using the default browser  
                    UseShellExecute = true,
                };
                // Starts the process of opening the web browser using the specified URL
                Process.Start(processStartInfo);
            }
            // Runs if any error occurs inside the try block and stores it in ex to show details of the error
            catch (Exception ex)
            {
                // Displays the error message from the exception
                MessageBox.Show("Unable to open YouTube link.\n" + ex.Message);
            }
        }

        // Runs when the user clicks the "Sanrio Store" menu option in the toolbar
        private void storeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // Stops looping background music 
            bgMusic.Stop();

            // Stores the URL of the stores link 
            string url = "https://www.sanrio.com/";

            // Tries to open the URL in the default web browser safely but if an error happens, catch block will handle it
            try
            {
                // Creates a ProcessStartInfo object to open the link in the default browser
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    // The property that holds the URL to open
                    FileName = url,
                    // Tells Windows to open the url using the default browser 
                    UseShellExecute = true,
                };
                // Starts the process of opening the web browser using the specified URL
                Process.Start(processStartInfo);
            }
            // Runs if any error occurs inside the try block and stores it in ex to show details of the error 
            catch (Exception ex)
            {
                // Displays the error message from the exception
                MessageBox.Show("Unable to open YouTube link.\n" + ex.Message);
            }
        }
    }
}

   


