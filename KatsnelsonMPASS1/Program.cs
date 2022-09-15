//Author: Mike Katsnelson
//File Name: program.cs
//Project Name: PASS1
//Creation Date: Feb. 16, 2022
//Modified Date: Feb. 17, 2022
//Description: This program is an interactive word game that aims to mimic Wordle.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KatsnelsonMPASS1
{
    class Program
    {
        //Store the color based on the condition
        const ConsoleColor INCORRECT = ConsoleColor.DarkGray;
        const ConsoleColor WRONG_LOC = ConsoleColor.DarkYellow;
        const ConsoleColor CORRECT_LOC = ConsoleColor.DarkGreen;
        const ConsoleColor EMPTY_SPACE = ConsoleColor.Black;

        //Store the word length and number of attempts given to the user
        const int WORD_LENGTH = 5;
        const int NUM_ATTEMPTS = 6;

        //Store the StreadReader and StreamWriter variables
        static StreamReader inFile;
        static StreamWriter outFile;

        static Random rng = new Random();

        //Store a 2D array of the game board colors that work in conjunction
        static string[,] gameBoard = new string[NUM_ATTEMPTS, WORD_LENGTH];
        static ConsoleColor[,] colors = new ConsoleColor[NUM_ATTEMPTS, WORD_LENGTH];

        //Store the answer
        static string answer;

        //Store the colors and letters of the virtual keyboard
        static ConsoleColor[] keyboardColor = new ConsoleColor[26];
        static char [] keyboardKeys = {'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A','S','D','F','G','H','J','K','L','Z','X','C','V','B','N','M'};

        //Store the stats - 0-5: win distribution; 6: games played; 7: games won; 8: current win streak; 9: max win streak
        static List<int> stats = new List<int>();

        //Store value that contains whether game is won
        static bool winCondition = false;

        static void Main(string[] args)
        {
            //Store a list of possible answers and words available to the user
            List <string> answersDict = new List<string>();
            List <string> wordsDict = new List<string>();

            //Store all the possible values inside of a valid guess
            string possibleValues = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            //Store the user's current guess and all prior guesses
            string userGuess = "";
            List<string> guesses = new List<string>();

            //Store errors associated with an invalid guess
            string error = "";

            //Store a boolean value, used to ensure program continues to run until user exits
            bool runProgram = true;

            //Change the height and width of the console
            Console.WindowHeight = 32;
            Console.WindowWidth = 80;

            //Store end-game input
            string endInput;

            //Extract the words from the "WordleAnswers.txt" file into a list.
            answersDict = ReadFile("WordleAnswers.txt");

            //Extract the words from the "WordleAnswers.txt" and "WordleExtras.txt" files into a list.
            wordsDict = ReadFile("WordleAnswers.txt");
            wordsDict.AddRange(ReadFile("WordleExtras.txt"));

            //Extract the user's stats
            stats = InterpretStats();

            //Display Rules
            DisplayRules();
            Console.ReadLine();
            Console.Clear();

            //Loop used to ensure program will run until user exits
            while (runProgram == true)
            {
                //Reset gameBoard elements and answer
                ResetGame(answersDict);

                //Reset error message
                error = "";

                //Retrieve the user's guess
                error = GetUserInput(userGuess, possibleValues, wordsDict, error, guesses);

                //Clear console and display board
                Console.Clear();
                DrawBoard(error);

                //Increase number of games played by 1
                stats[6]++;

                //Check if current win streak is greater than max win streak
                if (stats[8] > stats[9])
                {
                    //Increase max win streak by 1
                    stats[9]++;
                }

                //Write stats to file
                WriteStats(stats);

                error = "Word: " + answer;
                Console.Clear();
                DrawBoard(error);

                //Display end-game screen once game is over
                Console.ReadLine();
                Console.Clear();
                DisplayEndGame();
                
                //Retrieve user feedback
                endInput = Console.ReadLine();
 
                //Execute the appropriate action based on user feedback
                if (endInput.Equals("R") || endInput.Equals("r"))
                {
                    //Reset stats
                    ResetStats();
                    DisplayEndGame();

                    //Collect user feedback
                    endInput = Console.ReadLine();
                    if (endInput.Equals("E") || endInput.Equals("e"))
                    {
                        //Exit program
                        Environment.Exit(0);
                    }

                }
                else if (endInput.Equals("E") || endInput.Equals("e"))
                {
                    //Exit progra,
                    Environment.Exit(0);
                }

                //Write stats to file
                WriteStats(stats);

                //Reset the list of user guesses
                for (int i = 0; i < guesses.Count; i++)
                {
                    guesses[i] = "";
                }
            }
        }
        
        //Pre: userGuess is a valid string depicting the user's guess, possibleValues is a valid string depicting all possible values, wordsDict is a valid list of strings 
        //containing all possible words, guesswa is a valid list of of strings containing all guesses made by the user
        //Post: None
        //Description: Retrieve the user's guess and perform tests to ensure validity
        private static string GetUserInput(string userGuess, string possibleValues, List<string> wordsDict, string error, List<string> guesses)
        {
            //Store booleans used to ensure the validity of the text
            bool validGuess = false;
            bool specialCharacters = false;

            //Loop through each row of the game board array
            for (int row = 0; row < gameBoard.GetLength(0); row++)
            {
                //Check that user's guess is not equal to the answer
                if (!userGuess.Equals(answer))
                { 
                    //Set variable to false. This will run the loop
                    validGuess = false;
                }

                //Continue asking for a guess until user enters a valid guess
                while (validGuess == false)
                {
                    //Clear console and draw board
                    Console.Clear();
                    DrawBoard(error);

                    //Prompt for and retrieve the user's guess
                    Console.WriteLine();
                    Console.Write("Word: ".PadLeft((int)(((Console.WindowWidth) + 16.0) / 6.0), ' '));
                    userGuess = Console.ReadLine();

                    //Loop through each letter in user's guess
                    foreach (char letter in userGuess)
                    {
                        //Check to ensure that the letter is valid
                        if (!possibleValues.Contains(letter))
                        {
                            //Display error message
                            error = "Please use appropriate letters!";

                            //Set boolean to true
                            specialCharacters = true;
                            break;
                        }
                    }
                    //If the string contains a special character, go back to the top
                    if (specialCharacters == true)
                    {
                        specialCharacters = false;
                        continue;
                    }

                    //Perform checks to ensure the validity of the user's guess
                    if (guesses.Contains(userGuess.ToUpper()))
                    {
                        //If word is already used, display error message
                        error = "Word already used!";
                    }
                    else if (userGuess.Length != WORD_LENGTH)
                    {
                        //If word is not 5 letters, display error message
                        error = "Word must be 5 letters!";
                    }
                    else if (!wordsDict.Contains(userGuess.ToLower()))
                    {
                        //If word is not contained in the dictionary, display error message
                        error = "Word is not in the dictionary!";
                    }
                    else
                    {
                        //If word passes all checks, capitlize it and add it to the list of all user guesses
                        userGuess = userGuess.ToUpper();
                        guesses.Add(userGuess);
                        error = "";

                        //Loop through each column of the gameboard array
                        for (int column = 0; column < gameBoard.GetLength(1); column++)
                        {
                            //Add the word to the gameboard and assign appropriate colors
                            gameBoard[row, column] = Convert.ToString(userGuess[column]);
                            colors[row, column] = AssignColor(gameBoard[row, column], column);
                        }

                        //Set boolean to true to end current iteration of the loop
                        validGuess = true;

                        ////Check if row is equal to 6
                        //if (row == gameBoard.GetLength(0));
                        //{
                        //    //Set error to the answer. This will display the answer
                        //    error = "Word: " + answer;
                        //}
                    }

                    //Check if user guessed the answer
                    if (userGuess.Equals(answer))
                    {
                        //Set error to the answer. This will display the answer
                        error = "Word: " + answer;

                        //Set win condition to true
                        winCondition = true;

                        //Increase relevant stats by 1
                        stats[row]++;
                        stats[7]++;
                        stats[8]++;

                        //Write stats to file
                        WriteStats(stats);
                    }
                }
            }

            //Return an error
            return error;
        }

        //Pre: letter is a valid string containing a letter from the user's guess, index is a valid string that represents the column number of the game board
        //Post: Return a color as type ConsoleColor
        //Description: Assign a color to each letter of a user's guess
        private static ConsoleColor AssignColor(string letter, int index)
        { 
            //Assign the correct color based on the letter's proximity to the answer
            if (letter.Equals(Convert.ToString((answer[index]))))
            {
                //If the letter matches the answer, output a green color
                AssignKeyboardColor(Convert.ToChar(letter), CORRECT_LOC);
                return CORRECT_LOC;
            }
            else if (answer.Contains(letter))
            {
                //If the answer word contains the letter, but does not match it, output a yellow color
                AssignKeyboardColor(Convert.ToChar(letter), WRONG_LOC);
                return WRONG_LOC;
            }
            else
            {
                //If the answer word does not contain the letter, output a gray color
                AssignKeyboardColor(Convert.ToChar(letter), INCORRECT);
                return INCORRECT;
            }
        }

        //Pre: letter is a valid char taken from the user's guess, color is a valid ConsoleColor
        //Post: None
        //Description: Assign a color to a key on the virtual keyboard
        private static void AssignKeyboardColor(char letter, ConsoleColor color)
        {
            //Loop through each key in the virtual keyboard
            for (int i = 0; i < keyboardKeys.Length; i++)
            {
                //Check whether the letter matches the letter on the virtual keyboard, and ensure that it is not already green 
                if (letter == keyboardKeys[i] && keyboardColor[i] != CORRECT_LOC)
                {  
                    //Assign a color to the key
                    keyboardColor[i] = color;
                }
            }
        }

        //Pre: error is a valid string that depicts an error associated with the user's guess
        //Post: None
        //Description: Draw the game board
        private static void DrawBoard(string error)
        {
            //Display the title
            DisplayTitle();

            //Loop through each row of the game board
            for (int row = 0; row < gameBoard.GetLength(0); row++)
            {
                //Loop through each column of the game board
                for (int column = 0; column < gameBoard.GetLength(1); column++)
                {
                    //Check whether columm is equal to zero / whether it is the begginning of a new line
                    if (column == 0)
                    {
                        //Move the gameboard left
                        Console.Write(" ".PadLeft((int)((Console.WindowWidth - 20.0) / 6.0)));
                    }
                    //Display an element of the gameBoard
                    Console.BackgroundColor = EMPTY_SPACE;
                    Console.Write(" " + gameBoard[row, column] + "  ");
                }
                
                //Create a new line
                Console.WriteLine("");

                //Loop through each column of the game board
                for (int column = 0; column < gameBoard.GetLength(1); column++)
                {
                    //Check whether columm is equal to zero / whether it is the begginning of a new line
                    if (column == 0)
                    {
                        //Move the gameboard left
                        Console.Write(" ".PadLeft((int)((Console.WindowWidth - 20.0) / 6.0)));
                    }

                    //Display the color underneath each letter
                    Console.BackgroundColor = colors[row, column];
                    Console.Write("   ");
                    Console.BackgroundColor = EMPTY_SPACE;
                    Console.Write(" ");
                }

                //Create a new line
                Console.WriteLine();
    
                //Display the virtual keyboard
                DisplayVirtualKeyboard(row, error);
            }
        }

        //Pre: path is a valid string containing the path of the file
        //Post: Return the list (of strings)
        //Description: Read data from a file
        private static List<String> ReadFile(string path)
        {
            //Create variable to store the data extracted from the file
            List<String> data = new List<String>();
            
            //Attempt to read from the file
            try
            {
                //Open the file
                inFile = File.OpenText(path);

                //Loop through the file
                while (!inFile.EndOfStream)
                {
                    //Retrieve the data from the file and add it to list
                    data.Add(inFile.ReadLine());
                }
                
                //Close the file
                inFile.Close();
            }
            catch (FileNotFoundException fnf)
            {
                //If exception is thrown, catch it and display message
                Console.WriteLine("Error: " + fnf.Message);
            }
            catch (FormatException fe)
            {
                //If exception is thrown, catch it and display message
                Console.WriteLine("Error: " + fe.Message);
            }
            catch (Exception e)
            {
                //If exception is thrown, catch it and display message
                Console.WriteLine("Error: " + e.Message);
            }
            finally
            {
                //Check that inFile is not equal to null
                if (inFile != null)
                {
                    //Close the file
                    inFile.Close();
                }
            }

            //Return the list
            return data;
        }
        
        //Pre: None
        //Post: Returns a list of integers the represent the stats
        //Description: Reads through a list of strings but returns a list of integers
        private static List<int> InterpretStats()
        {
            //Retrieve data from stats file
            List<String> data = ReadFile("Stats.txt");

            //Create a new list to store the data as an integer
            List<int> intData = new List<int>();

            //Loop through list of strings
            foreach (string d in data)
            {
                //Convert string to integer and add to new list
                intData.Add(Convert.ToInt32(d));
            }

            //Return the list of integers
            return intData;
        }

        //Pre: stats is a valid list of integers containing the usewr's stats
        //Post: None
        //Description: Write the stats onto a file
        private static void WriteStats(List<int> stats)
        {
            //Attempt to write to stats file
            try
            {
                //Open or create text file
                outFile = File.CreateText("Stats.txt");

                //Loop through file
                foreach (int i in stats)
                {
                    //Display the stat onto the file
                    outFile.WriteLine(i);
                }
                //Close the file
                outFile.Close();
            }
            catch (Exception e)
            {
                ///If exception is thrown, catch it and display message
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        //Pre: None
        //Post: None
        //Description: Reset user's stats
        private static void ResetStats()
        {
            //Loop through stats
            for (int i = 0; i < stats.Count; i++)
            {
                //Set each stat to 0
                stats[i] = 0;
            }

            //Write the new values into the file
            WriteStats(stats);
        }
        

        //Pre: None
        //Post: answersDict is a list of strings with all the possible answers
        //Description: Reset the game
        private static void ResetGame(List<string> answersDict)
        {
            //Loop through each index of the game board array
            for (int row = 0; row < gameBoard.GetLength(0); row++)
            {
                for (int column = 0; column < gameBoard.GetLength(1); column++)
                {
                    //Assign an initial value to each index of the arrays representing the game board and colors
                    gameBoard[row, column] = " ";
                    colors[row, column] = INCORRECT;
                }
            }

            //Loop through each index of the array of keyboard colors
            for (int index = 0; index < keyboardColor.Length; index++)
            {
                //Assign an initial color (black) to each element
                keyboardColor[index] = EMPTY_SPACE;
            }

            //Randomly generate an answer that is part of the list of possible answers
            answer = answersDict[rng.Next(0, answersDict.Count)].ToUpper();
        }

        private static void DisplayTitle()
        {
            //Display the title
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine(",--.   ,--. ,-----. ,------. ,------.  ,--.   ,------.".PadLeft(Console.WindowWidth - 13));
            Console.WriteLine("|  |   |  |'  .-.  '|  .--. '|  .-.  \\ |  |   |  .---'".PadLeft(Console.WindowWidth - 13));
            Console.WriteLine("|  |.'.|  ||  | |  ||  '--'.'|  |  \\  :|  |   |  `--, ".PadLeft(Console.WindowWidth - 13));
            Console.WriteLine("|   ,'.   |'  '-'  '|  |\\  \\ |  '--'  /|  '--.|  `---.".PadLeft(Console.WindowWidth - 13));
            Console.WriteLine("'--'   '--' `-----' `--' '--'`-------' `-----'`------'".PadLeft(Console.WindowWidth - 13));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("_________________________________________________________".PadLeft(Console.WindowWidth - 11));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n");
        }

        private static void DisplayVirtualKeyboard(int row, string error)
        {
            //Display the appropriate information based on the row number
            switch (row)
            {
                case 0:
                    //Display a line (for aesthetics)
                    Console.WriteLine("_________________________________________".PadLeft(Console.WindowWidth - 3));
                    break;

                case 1:
                    //Move the virtual keyboard left
                    Console.Write(("".PadLeft(Console.WindowWidth - 44)));

                    //Loop to display 10 letters of the virtual keyboard
                    for (int i = 0; i < 10; i++)
                    {
                        //Display a key from the keyboard with the appropriate color
                        Console.BackgroundColor = keyboardColor[i];
                        Console.Write(" " + keyboardKeys[i] + " ");
                        Console.BackgroundColor = EMPTY_SPACE;
                        Console.Write(" ");

                        //Check whether the end of the line was reached
                        if (i == (10 - 1))
                        {
                            //Display a new line
                            Console.WriteLine();
                        }
                    }
                    break;

                case 2:
                    //Move the virtual keyboard left
                    Console.Write("".PadLeft(Console.WindowWidth - 42));

                    //Loop to display 8 letters of the virtual keyboard
                    for (int i = 10; i < 19; i++)
                    {
                        //Display a key from the keyboard with the appropriate color
                        Console.BackgroundColor = keyboardColor[i];
                        Console.Write(" " + keyboardKeys[i] + " ");
                        Console.BackgroundColor = EMPTY_SPACE;
                        Console.Write(" ");

                        //Check whether the end of the line was reached
                        if (i == (19 - 1))
                        {
                            //Display a new line
                            Console.WriteLine();
                        }
                    }
                    break;

                case 3:
                    //Move the virtual keyboard left
                    Console.Write("".PadLeft(Console.WindowWidth - 39));

                    //Loop to display 8 letters of the virtual keyboard
                    for (int i = 19; i < 26; i++)
                    {
                        //Display a key from the keyboard with the appropriate color
                        Console.BackgroundColor = keyboardColor[i];
                        Console.Write(" " + keyboardKeys[i] + " ");
                        Console.BackgroundColor = EMPTY_SPACE;
                        Console.Write(" ");

                        //Check whether the end of the line was reached
                        if (i == (26 - 1))
                        {
                            //Display a new line
                            Console.WriteLine();
                        }
                    }
                    break;

                case 4:
                    //Display a line (for aesthetics)
                    Console.WriteLine("________________________________________".PadLeft(Console.WindowWidth - 3));
                    break;

                case 5:
                    //Display the error message
                    Console.WriteLine("".PadRight(39) + error);
                    break;

                default:
                    //Create a new line
                    Console.WriteLine();
                    break;
            }
        }

        //Pre: None
        //Post: None
        //Description: Display rules screen
        private static void DisplayRules()
        {
            //Display title
            DisplayTitle();

            //Display game breakdown
            Console.WriteLine( "".PadRight(4) + "- The program will automatically generate a 5-letter word.\n");
            Console.WriteLine("".PadRight(4) + "- Your goal is to guess that word! You will have a maximum of 6 tries.\n");
            Console.WriteLine("".PadRight(4) + "- Don't worry! After every attempt, the program will inform you\n");
            Console.WriteLine("".PadRight(6) + "of crucial information: \n\n");

            //Display "gray" color information
            Console.BackgroundColor = INCORRECT;
            Console.WriteLine("   ");
            Console.Write("   ");
            Console.BackgroundColor = EMPTY_SPACE;
            Console.WriteLine("  Gray symbolizes letters that are not present in the word.");
            Console.BackgroundColor = INCORRECT;
            Console.WriteLine("   \n");

            //Display "yellow" color information
            Console.BackgroundColor = WRONG_LOC;
            Console.WriteLine("   ");
            Console.Write("   ");
            Console.BackgroundColor = EMPTY_SPACE;
            Console.WriteLine("  Yellow symbolizes letters that appear in the word, but");
            Console.BackgroundColor = WRONG_LOC;
            Console.Write("   ");
            Console.BackgroundColor = EMPTY_SPACE;
            Console.WriteLine("  at a different location.");
            Console.BackgroundColor = WRONG_LOC;
            Console.WriteLine("   \n");
            
            //Display "green" color information
            Console.BackgroundColor = CORRECT_LOC;
            Console.WriteLine("   ");
            Console.Write("   ");
            Console.BackgroundColor = EMPTY_SPACE;
            Console.WriteLine("  Green symbolizes letters that appear in the word at the EXACT location.");
            Console.BackgroundColor = CORRECT_LOC;
            Console.Write("   ");

            Console.BackgroundColor = EMPTY_SPACE;
        }

        //Pre: userGuess is a valid string containing user's guess
        //Post: None
        //Desc: Display end-game screen
        static void DisplayEndGame()
        {
            //Display title
            DisplayTitle();

            //Check if game is won
            if (winCondition == true)
            {
                //Congratulate user
                Console.WriteLine("".PadRight(Console.WindowWidth - 48) + "You guessed it!\n\n");
            }
            else
            {
                //Display encouraging message
                Console.WriteLine("".PadRight(Console.WindowWidth - 54) + "You will get it next time!\n\n");
            }

            //Display win distribution
            Console.Write("".PadRight(Console.WindowWidth - 62) + "First Attempt: " + stats[0]);
            Console.WriteLine("".PadRight(8) + " Second Attempt: " + stats[1] + "\n");
            Console.Write("".PadRight(Console.WindowWidth - 62) + "Third Attempt: " + stats[2]);
            Console.WriteLine("".PadRight(8) + " Fourth Attempt: " + stats[3] + "\n");
            Console.Write("".PadRight(Console.WindowWidth - 62) + "Fifth Attempt: " + stats[4]);
            Console.WriteLine("".PadRight(8) + " Sixth Attempt: " + stats[5] + "\n\n");

            //Display main stats
            Console.Write("".PadRight(Console.WindowWidth - 62) + "Games Played: " + stats[6]);
            Console.WriteLine("".PadRight(9) + " Games Won: " + stats[7] + "\n");
            Console.Write("".PadRight(Console.WindowWidth - 62) + "Win Streak: " + stats[8]);
            Console.WriteLine("".PadRight(11) + " Max. Win Streak: " + stats[9] + "\n\n\n");

            //Display commands
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("".PadRight(Console.WindowWidth - 62) + "Press R and ENTER to RESET stats" + "\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("".PadRight(Console.WindowWidth - 62) + "Press E and ENTER to EXIT\n");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("".PadRight(Console.WindowWidth - 62) + "Press any other button and ENTER to play again" + "\n");
        }
    }
}
