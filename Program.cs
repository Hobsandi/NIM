using System;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Text.Json;
using Microsoft.VisualBasic;


// IDEER
// instantwin drag


namespace NyttC_projekt
{
    public class Program
    {
        public enum MenuChoice {
            PlayVsFriend,
            PlayVsComputer,
            ShowRules,
            ShowLeaderboard,
            Exit,

            ResetLeaderboard = 66



        }
        public static void Main(string[] args)
        {
            UtilityMethods u = new UtilityMethods();
            Leaderboard Leaderboard = new Leaderboard();
            
             
            //Variabler
            bool exit = false;

            //Konstanter
            const int PILEAMOUNT = 4;

            Console.Clear();
            //Välkomna till spelet
            Welcome();
            Console.ReadLine();

            //Huvudloop
            while(!exit) {
                Console.Clear();
                //Visa meny
                ShowMenu();
                //Ta in användarval
                int pickedMenuOption = u.HandleStringUserInput("Välj ett alternativ: ", 1, 5);
                //Behandla användarval
                switch (pickedMenuOption-1) {
                    case (int)MenuChoice.PlayVsFriend: 
                        //Spela mot en vän
                        //Ta in spelarnamn
                        int playAgain = 1;
                        Console.Clear();
                        Console.Write("Skriv namnet på första spelaren: ");
                        string player1Name = Console.ReadLine();
                        Console.Write("Skriv namnet på andra spelaren: ");
                        string player2Name = Console.ReadLine();
                        
                        //Spelloop
                        while (playAgain == 1) {
                            Player player1 = new Player(player1Name);
                            Player player2 = new Player(player2Name);
                            PileStructure playField = new PileStructure(PILEAMOUNT);
                            //Instansierar objekt
                            Game ActiveGame = new Game(player1, player2, playField, Leaderboard);
                            ActiveGame.Play();
                            Console.ReadLine();

                            Console.Clear();
                            playAgain = u.HandleStringUserInput("Vill ni spela igen? Skriv 1 för ja och 0 för nej: ",0,1);

                        }
                        break;
                    case (int)MenuChoice.PlayVsComputer: 
                        //Spela mot datorn
                        Console.Clear();

                        //Ta in spelarnamn och definiera variabler
                        Console.Write("Skriv ditt namn: ");
                        string playerName = Console.ReadLine();
                        playAgain = 1;
                        //Spelloop
                        while (playAgain == 1) {
                            Player singplayer = new Player(playerName);
                            Player computer = new Player("Datorn",true);
                            PileStructure playField = new PileStructure(PILEAMOUNT);
                            Game ActiveGame = new Game(singplayer, computer, playField, Leaderboard);
                            ActiveGame.Play();
                            Console.ReadLine();

                            Console.Clear();
                            playAgain = u.HandleStringUserInput("Vill du spela igen? Skriv 1 för ja och 0 för nej: ",0,1);

                        }
                        break;
                    case (int)MenuChoice.ShowRules:
                        //Visa regler
                        Console.Clear();
                        ShowRules();
                        break;
                    case (int)MenuChoice.ShowLeaderboard:
                        //Visa leaderboard
                        Console.Clear();
                        Leaderboard.Show();
                        if (Console.ReadLine() == "66") {
                            Leaderboard.Reset();
                            Console.Clear();
                            Console.WriteLine("Leaderboarden har nollställts.");
                            Console.ReadLine();
                        }
                        break;
                    case (int)MenuChoice.Exit:
                        //Avsluta
                        Console.Clear();
                        int choice = u.HandleStringUserInput("Är du säker? Skriv 0 för nej och 1 för ja.", 0, 1);
                        if (choice == 1) {
                            exit = true;
                        }
                        break;
                }         
            }
        }

        static void Welcome() {
            string welcomeString = "Välkommen till NIM!";
            for (int i = 0; i<100; i++) {
                Console.WriteLine(welcomeString);
                Console.WriteLine($"Laddar... {i}%");
                Thread.Sleep(1);
                Console.Clear();
            }
            Console.WriteLine(welcomeString);
            Console.WriteLine("Klar. Tryck Enter för att börja.");
        }
        static void ShowMenu() {
            Console.WriteLine("----MENY----");
            Console.WriteLine("1. Spela mot en vän");
            Console.WriteLine("2. Spela mot datorn");
            Console.WriteLine("3. Spelregler");
            Console.WriteLine("4. Visa leaderboard");
            Console.WriteLine("5. Avsluta");
        }
        static void ShowRules() {
            Console.WriteLine("---- SPELREGLER ----");
            Console.WriteLine("Nim är ett klassiskt matematiskt spel som spelas av två spelare."); 
            Console.WriteLine("I spelet har man arrangerat ett antal stickor i ett antal högar.");
            Console.WriteLine("De två spelarna turas om att plocka ett valfritt antal stickor från en valfri hög.");
            Console.WriteLine("Den spelare som plockar den sista stickan har förlorat.");
            PileStructure Example = new PileStructure(4);
            Example.Show();
            Console.ReadLine();
        }

    }
}

public class Game 
{
    public Player player1;
    public Player player2;
    public Player winner;
    public Player loser;
    public PileStructure playField;
    private UtilityMethods u = new UtilityMethods();
    public Leaderboard leaderboard;
    //Movelist??

    public Game(Player player1, Player player2, PileStructure playField, Leaderboard leaderboard) {
        this.player1 = player1;
        this.player2 = player2;
        this.playField = playField;
        this.leaderboard = leaderboard;
    }
    public void Play() {
        int index = 0;
        playField.Show();
        while (playField.SticksLeft() != 0) {
            if (index == 0) {
                Move(player1);
                index = 1;
            } else {
                Move(player2);
                index = 0;
            }
            Console.Clear();
            playField.Show();
        }
        if (index == 0) {
            this.winner = player1;
            this.loser = player2;
        } else {
            this.winner = player2;
            this.loser = player1;
        }
        FinishGame();
    }
    private void saveResults() {
        leaderboard.UpdatePlayer(player1);
        leaderboard.UpdatePlayer(player2);
        leaderboard.Put();
        
    }
    private void Move(Player player) {
        if (!player.isComputer) {
            int[] allowedIndices = this.playField.GetNonEmptyPiles();
            int pileChoice = u.HandlePileChoiceInput(player.name + "'s tur.\nVilken hög vill du ta ifrån?", 1, this.playField.height, allowedIndices);
            
            Console.Clear();
            playField.HighlightSticks(pileChoice-1);
            playField.Show();
            playField.ResetHighlight();

            int sticksLeft = this.playField.SticksLeft(pileChoice-1);
            int stickAmount = u.HandleStringUserInput(player.name + "'s tur. \nHur många pinnar vill du plocka?",1,sticksLeft);
            playField.RemoveSticks(pileChoice, stickAmount);
        }
        else {
            Random rand = new Random();
            int[] allowedIndices = this.playField.GetNonEmptyPiles();
            int pileChoice = allowedIndices[rand.Next(allowedIndices.Length-1)];
            
            int sticksLeft = this.playField.SticksLeft(pileChoice);
            int stickAmount = rand.Next(1,sticksLeft);

            Console.Clear();
            playField.HighlightSticks(pileChoice, stickAmount);
            playField.Show();
            playField.ResetHighlight();

            playField.RemoveSticks(pileChoice+1, stickAmount);
            Console.WriteLine(player.name + " kommer plocka " + stickAmount + " stickor från hög " + (pileChoice+1));
            Console.ReadLine();
        }
    }

    private void FinishGame() {
        Console.WriteLine(winner.name + " vann!\nBättre lycka nästa gång " + loser.name +".\nTack för att du spelade.");
        this.player1.totalPlayed++;
        this.player2.totalPlayed++;
        this.winner.totalWins++;
        saveResults();
    }
}


public class Leaderboard
{
    private Player[] players;
    private UtilityMethods u = new UtilityMethods();
    private const string filePath = "leaderboard.csv";
    
    public Leaderboard() {
        players = new Player[GetPlayerAmount()];
        Get();
    }

    public void Reset() {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Skriv rubrikerna igen
            writer.WriteLine("name,totalPlayed,totalWins");
        }
        players = new Player[GetPlayerAmount()];
    }

    private int GetPlayerAmount() {
        string[] lines = File.ReadAllLines(filePath);
        int amountOfPlayers = lines.Length-1;
        return amountOfPlayers;
    }
    public void Put() {
        u.SavePlayersToCsv(filePath,players);
    }
    public void UpdatePlayer(Player playerToCheck) {
        bool playerExists = false;
        foreach(Player player in players) {
            if (player.name == playerToCheck.name) {
                player.totalWins += playerToCheck.totalWins;
                player.totalPlayed += playerToCheck.totalPlayed;
                playerExists = true;
            }
        }
        if (!playerExists) {
            AddPlayer(playerToCheck);
        }
    }
    public void AddPlayer(Player playerToAdd) {
        Player[] newLeaderboard = new Player[players.Length + 1];
        for (int i = 0; i < players.Length; i++) {
            newLeaderboard[i] = players[i];
        }
        newLeaderboard[newLeaderboard.Length-1] = playerToAdd;
        this.players = newLeaderboard;
    } 
    public void Get() {
        using (StreamReader reader = new StreamReader(filePath)) {
            //Hoppa över rubrik
            reader.ReadLine();
            int index = 0;

            //läs varje rad
            string line;
            while ((line = reader.ReadLine()) != null) {
                string[] values = line.Split(',');
                string name = values[0];
                int totalWins = Int32.Parse(values[1]);
                int totalPlayed = Int32.Parse(values[2]);

                Player player = new Player(name);
                player.totalPlayed = totalPlayed;
                player.totalWins = totalWins;
                players[index] = player;
                index++;
            }

        }

    }
    public void Show() {
        Console.WriteLine("           ----LEADERBOARD----");
        Console.WriteLine("{0,-15} {1,-10} {2,-5}","SPELARE","VINSTER","TOTALT SPELADE");
        
        foreach(Player player in players) {
            Console.WriteLine("{0,-15} {1,-10} {2,-5}",player.name,player.totalWins,player.totalPlayed);
        }
    }

}
public class Player 
{
    public string name {get; set; }
    public int totalPlayed {get; set; }
    public int totalWins {get; set; }
    public bool isComputer {get; set; }

    public Player(string name, bool isComputer = false) {
        this.name = name;
        this.isComputer = isComputer;
    }
}

public class PileStructure 
{
    private UtilityMethods u = new UtilityMethods();
    public int height;
    private int[][] piles;
    private int[] highlighted = [-1,-1];

    public PileStructure(int height) {
        this.height = height;
        this.piles = this.CreatePiles(this.height);
    }

    private int[][] CreatePiles(int height) {
        //INTE HÅRDKODA FYRAN : FIXA
        int[][] piles  = new int[height][]; 
        for (int i = 0; i < piles.Length; i++) { 
            piles[i] = new int[2*i+1]; 
            for (int j = 0; j < piles[i].Length; j++) { 
                piles[i][j] = 1; 
            }   
        } 
        return(piles); 
    }

    public void RemoveSticks(int pileChoice, int stickAmount) {
        int pileIndex = pileChoice - 1;
        int removedSticks = 0;
        for (int i = piles[pileIndex].Length - 1; i >= 0; i--) {
            if (removedSticks < stickAmount && piles[pileIndex][i] == 1) {
                piles[pileIndex][i] = 0;
                removedSticks++;
            }
        }
    }

    public int[] GetNonEmptyPiles() {
        int[] nonEmptyPiles = new int[this.height];
        int index = 0;
        for (int i = 0; i < this.height; i++) {
            if (this.SticksLeft(i) > 0) {
                nonEmptyPiles[index] = i;
                index++;
            }
        }
        int[] cleanedArray = new int[index];
        for (int i = 0; i < cleanedArray.Length; i++) {
            cleanedArray[i] = nonEmptyPiles[i];
        }
        return cleanedArray;
    }
    public int SticksLeft(int pileChoice = -1) {
        int sticksLeft = 0;

        if (pileChoice == -1) {
            for (int i = 0; i < this.piles.Length; i++) {
                for (int j = 0; j < this.piles[i].Length; j++) {
                        sticksLeft += piles[i][j];
                    }
                }
        }
        else {
            foreach (int stick in piles[pileChoice]) {
                sticksLeft += stick;
            }
        }
        return sticksLeft;
    }
    
    public void HighlightSticks(int pileChoice, int stickAmount = -1) {
        this.highlighted[0] = pileChoice;
        this.highlighted[1] = stickAmount;
    }
    public void ResetHighlight() {
        this.highlighted = [-1,-1];
    }
    
    public void Show() {
        string pileOutput = "";
        for (int i = 0; i<this.piles.Length; i++) {
            pileOutput += i+1;
            pileOutput += "     ";
            int[] pile = this.piles[i]; 
            string mellanslag = String.Concat(Enumerable.Repeat(" ", piles.Length-i)); 
            pileOutput += mellanslag;
            int skipStick = this.SticksLeft(this.highlighted[0]) - this.highlighted[1];
            int hlCount = 0;
            for (int j = 0; j < pile.Length; j++) { 
                if (!(this.highlighted[0] == -1 && this.highlighted[1] == -1) && i == this.highlighted[0] && j >= skipStick && hlCount < this.highlighted[1]) {
                    // Console.WriteLine("MARKERA INDIVIDUELLT!" + hlCount);
                    pileOutput += u.StylizeString(pile[j].ToString(),3); 
                    hlCount++;               
                }
                else if (this.highlighted[0] != -1 && this.highlighted[1] == -1 && this.highlighted[0] == i) {
                    pileOutput += u.StylizeString(pile[j].ToString(),3);
                }
                else {
                    pileOutput += pile[j];
                }
            }
            Console.WriteLine(pileOutput);
            pileOutput = "";
        } 
    }
}

public class UtilityMethods 
{
    public UtilityMethods() {}
    public int HandleStringUserInput(string prompt, int floor, int ceiling) {
        string input; 
        int output = -1;
        bool correctInput = false;

        while (!correctInput) {
            Console.Write(prompt + $"({floor}-{ceiling}): ");
            input = Console.ReadLine();
            try {
                output = Int32.Parse(input);  
                correctInput = true;              
            }
            catch (FormatException) {
                //AJA BAJA BARA SIFFROR
                correctInput = false;
                Console.WriteLine("Aja baja bara siffror!");
                continue;
            }
            if (output < floor || output > ceiling) {
                //bajskorv
                correctInput = false;
                Console.WriteLine("Håll dig inom ramarna!");
            }
        }
        return output;
    }
    public int HandlePileChoiceInput(string prompt, int floor, int ceiling, int[] allowedIndices) {
        string input; 
        int output = -1;
        bool correctInput = false;
        bool isInIndices = false;
        string allowedChoices = "";
        if (allowedIndices.Length > 1) {
            for (int i = 0; i < allowedIndices.Length-2; i++ ) {
                allowedChoices += (allowedIndices[i]+1) + ", ";
            }
            allowedChoices += allowedIndices[allowedIndices.Length-2]+1 + " eller " + (allowedIndices[allowedIndices.Length-1]+1);
        }
        else {
            allowedChoices = (allowedIndices[0]+1).ToString();
        }

        while (!correctInput) {
            Console.Write(prompt + $" ({allowedChoices}): ");
            input = Console.ReadLine();
            try {
                output = Int32.Parse(input);  
                correctInput = true;              
            }
            catch (FormatException) {
                //AJA BAJA BARA SIFFROR
                correctInput = false;
                Console.WriteLine("Aja baja bara siffror!");
                continue;
            }
            if (output < floor || output > ceiling) {
                //bajskorv
                correctInput = false;
                Console.WriteLine("Håll dig inom ramarna!");
            } 
            foreach (int index in allowedIndices) {
                if (output-1 == index) {
                    //correctInput = true;
                    isInIndices = true;
                    break;
                } else {
                    isInIndices = false;
                }
            }
            if (isInIndices) {
                correctInput = true;
            } else {
                correctInput = false;
                Console.WriteLine("Högen är tom!");
            }
        }
        return output;
    }
    public string StylizeString(string input, int styleChoice) {
        string output = "";
        string reset = "\u001B[0m";
        /*
        FÄRGER 0-8
        0 Svart, 
        1 röd, 
        2 grön, 
        3 ljusgul, 
        4 gul, 
        5 blå, 
        6 lila, 
        7 cyan, 
        8 vit

        ANNAT 9-13 
        9 fetstilt, 
        10 ej-fetstil, 
        11 understruken, 
        12 sluta understryk, 
        13 blinka */
        string[] styles = ["\u001B[30m", "\u001B[31m", "\u001B[32m", "\u001B[93m", "\u001B[33m", "\u001B[34m", "\u001B[35m", "\u001B[36m", "\u001B[37m", "\u001B[1m", "\u001B[21m", "\u001B[4m", "\u001B[24m", "\u001B[5m"];
        output += styles[styleChoice];
        output += input;
        output += reset;
        
        return output;

    }
    
    public void CenterText(string text) {
        int windowWidth = Console.WindowWidth;
        int textStartPos = (windowWidth - text.Length) / 2;

        textStartPos = textStartPos < 0 ? 0 : textStartPos;
        Console.WriteLine(new string(' ', textStartPos) + text);
    }

    public void SavePlayersToCsv(string filePath, Player[] players) {
        using (StreamWriter writer = new StreamWriter(filePath)) {

            writer.WriteLine("name,totalPlayed,totalWins");

            foreach (Player Player in players) {
                writer.WriteLine($"{Player.name},{Player.totalPlayed},{Player.totalWins}");
            }
        }
    }
}