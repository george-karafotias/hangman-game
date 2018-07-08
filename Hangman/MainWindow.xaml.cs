using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Hangman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int wins;
        private int defeats;
        private int currentWordIndex;
        private int lives;
        private List<string> allWords;
        private List<string> wordsPlayed;
        private List<string> allLetters;
        private List<string> existingLetters;
        private List<string> notExistingLetters;

        public MainWindow()
        {
            InitializeComponent();
            Reset();
        }

        private void Reset()
        {
            ReadWords();

            wordsPlayed = new List<string>();
            allLetters = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            existingLetters = new List<string>();
            notExistingLetters = new List<string>();
            wins = 0;
            defeats = 0;
            currentWordIndex = 0;
            lives = 5;

            ResetGui();
        }

        private void ReadWords()
        {
            allWords = new List<string>();
            StreamReader reader = new StreamReader("words.txt");
            string currentLine;
            while ((currentLine = reader.ReadLine()) != null)
            {
                allWords.Add(currentLine.ToUpper());
            }
        }

        private void ResetGui()
        {
            Wins.Text = "0";
            Defeats.Text = "0";
            TotalWords.Text = allWords.Count.ToString();
            WordsPlayed.Text = "0";
            NewRoundButton.Visibility = System.Windows.Visibility.Hidden;
            FinishedGameMessage.Visibility = System.Windows.Visibility.Hidden;
            
            AlphabetList.Items.Clear();
            foreach (string letter in allLetters)
            {
                ListBoxItem alphabetListItem = new ListBoxItem();
                alphabetListItem.Content = letter;
                AlphabetList.Items.Add(alphabetListItem);
            }
            NotExistingLettersList.Items.Clear();

            ShowNextWord();
        }

        private void ShowNextWord()
        {
            CurrentWordList.Items.Clear();
            string currentWord = allWords[currentWordIndex];
            for (int i = 0; i < currentWord.Length; i++)
            {
                ListBoxItem wordLetterItem = new ListBoxItem();
                wordLetterItem.Content = "_";
                wordLetterItem.Focusable = false;
                CurrentWordList.Items.Add(wordLetterItem);
            }
        }

        private void AlphabetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlphabetList.SelectedItem == null) return;

            ListBoxItem selectedLetterItem = AlphabetList.SelectedItem as ListBoxItem;
            string selectedLetter = selectedLetterItem.Content.ToString();
            string currentWord = allWords[currentWordIndex];

            int letterIndex = CultureInfo.CurrentCulture.CompareInfo.IndexOf(currentWord, selectedLetter, CompareOptions.IgnoreCase);
            
            if (letterIndex >= 0)
            {
                existingLetters.Add(selectedLetter);

                for (int i = 0; i < currentWord.Length; i++)
                {
                    string currentLetter = currentWord[i].ToString();
                    if (string.Equals(currentLetter, selectedLetter, StringComparison.OrdinalIgnoreCase))
                    {
                        ListBoxItem currentLetterItem = CurrentWordList.Items[i] as ListBoxItem;
                        currentLetterItem.Content = selectedLetter;
                    }
                }
            }
            else
            {
                notExistingLetters.Add(selectedLetter);
                
                ListBoxItem notExistingLetterItem = new ListBoxItem();
                notExistingLetterItem.Content = selectedLetter;
                notExistingLetterItem.Focusable = false;
                NotExistingLettersList.Items.Add(notExistingLetterItem);
            }

            selectedLetterItem.Content = " ";
            selectedLetterItem.Focusable = false;

            if (WordFound())
            {
                wordsPlayed.Add(currentWord);
                wins++;
                ProcessRoundFinished(true);
                return;
            }

            if (GameOver())
            {
                wordsPlayed.Add(currentWord);
                defeats++;
                ProcessRoundFinished(false);
                return;
            }
        }

        private bool WordFound()
        {
            string currentWord = allWords[currentWordIndex];
            string nowWord = NowWord();
            if (string.Equals(currentWord, nowWord, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private bool GameOver()
        {
            if (notExistingLetters.Count >= lives && !WordFound())
                return true;
            return false;
        }

        private string NowWord()
        {
            StringBuilder nowWord = new StringBuilder();
            foreach (ListBoxItem letterItem in CurrentWordList.Items)
            {
                nowWord.Append(letterItem.Content.ToString());
            }
            return nowWord.ToString();
        }

        private void ProcessRoundFinished(bool win)
        {
            if (wordsPlayed.Count == allWords.Count)
            {
                string sMessageBoxText = "Do you want to start a new game?";
                string sCaption = "Hangman";
                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                if (rsltMessageBox == MessageBoxResult.Yes)
                {
                    Reset();
                }
                else if (rsltMessageBox == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }

                return;
            }

            string msg = "";
            if (win)
            {
                msg = "YOU WON THIS ROUND!";
                FinishedGameMessage.Background = Brushes.Green;
            }
            else
            {
                msg = "GAME OVER!";
                FinishedGameMessage.Background = Brushes.Red;
            }
                
            FinishedGameMessage.Text = msg;
            FinishedGameMessage.Visibility = System.Windows.Visibility.Visible;
            NewRoundButton.Visibility = System.Windows.Visibility.Visible;
            Wins.Text = wins.ToString();
            Defeats.Text = defeats.ToString();
            WordsPlayed.Text = wordsPlayed.Count.ToString();
        }

        private void NewRoundButton_Click(object sender, RoutedEventArgs e)
        {
            existingLetters = new List<string>();
            notExistingLetters = new List<string>();
            currentWordIndex++;
            if (currentWordIndex >= allWords.Count)
                currentWordIndex = 0;

            AlphabetList.Items.Clear();
            foreach (string letter in allLetters)
            {
                ListBoxItem alphabetListItem = new ListBoxItem();
                alphabetListItem.Content = letter;
                AlphabetList.Items.Add(alphabetListItem);
            }
            NotExistingLettersList.Items.Clear();

            NewRoundButton.Visibility = System.Windows.Visibility.Hidden;
            FinishedGameMessage.Visibility = System.Windows.Visibility.Hidden;

            ShowNextWord();
        }

    }
}
