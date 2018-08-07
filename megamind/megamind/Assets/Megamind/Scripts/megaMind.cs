using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// megaMind
//
// this script is attached to the GameManager GameObject in the megamind scene
// and controls the game behaviour

public class megaMind : MonoBehaviour
{
    public class EventRecord
    {

        public System.DateTime? EventTime { get; set; }
        public string ParticipantId { get; set; }
        public string EventName { get; set; }
        public string DataKey { get; set; }
        public string DataValue { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public string Color { get; set; }
        public int GameNumber { get; set; }
        /// <summary>
        /// CSV FORMATTED RECORD
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string dv = DataValue != null && DataValue.Contains(",") ? "\"" + DataValue + "\"" : DataValue;
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", EventTime.Value.ToString("yyyyMMddhhmmssff"), ParticipantId, EventName, DataKey, dv , Row, Col, Color, GameNumber);
        }
        public string GetHeader()
        {
            return "EventTime,ParticipantId,EventName,DataKey,DataValue,Row,Col,Color,GameNumber";
        }
    }

    public static class GameProgress 
    {
        
        static string baseDirectory = @"c:\temp\Mastermind";
        public static string OutputFilename = System.IO.Path.Combine(baseDirectory, string.Format("Megamind_{0}", System.DateTime.Today.ToString("yyyyMMdd")));
        public static void Write(string eventName)
        {

            Write(new EventRecord() { EventName = eventName });
        }
        public static void Write(EventRecord Record)
        {
            if (!System.IO.Directory.Exists(baseDirectory))
                System.IO.Directory.CreateDirectory(baseDirectory);

            string OutputFilename = System.IO.Path.Combine(baseDirectory, string.Format("Megamind_{0}", System.DateTime.Today.ToString("yyyyMMdd")));
            System.IO.FileInfo fi = new System.IO.FileInfo(OutputFilename);
            bool AddHeader = !fi.Exists;
            using (var sw = fi.AppendText())
            {
                if (AddHeader)
                    sw.WriteLine(Record.GetHeader());

                if (!Record.EventTime.HasValue)
                    Record.EventTime = System.DateTime.Now;

                sw.WriteLine(Record.ToString());
                sw.Flush();
                sw.Close();
            }

        }

    }

    // first of all some references to ui objects, 
    // which get modified in the gameplay

    public Image moveMarker;
    public Image secret;

    // list of images
    public Sprite[] pinImages;
    public Sprite[] swPinImages;

    public Button[] pinColors;


    // dialog windows
    public RectTransform panelGameOver;
    public RectTransform panelConfirm;
    public RectTransform panelGameWon;
    public RectTransform panelMenu;
    public RectTransform panelNewGame;

    // Gametime
    public Text gameTime;

    // which move are we making
    private int currentMove;
    int remainder;
    private int CurrentCol { get { System.Math.DivRem(currentMove, 4, out remainder); return remainder; } }
    private int CurrentRow { get { return 1 + (currentMove/ 4); } }
    // is the game over
    bool gameOver;

    // the clock increments every second
    float timerInterval = 1f;
    float timeElapsed;

    // user friendly time display in the xx:yy format
    float secondsElapsed;
    float minutesElapsed;

    // Use this for initialization
    void Start()
    {
        // initialize the game
        GameOfMatch = 1;
        initGame();
       
        // now get the number of allowed colors
        // and disable the other on the UI
        GameObject pinColumn = GameObject.Find("pinColumn");
        int colorCount = 1;
        foreach (Transform child in pinColumn.transform)
        {
            if (colorCount <= PlayerPrefs.GetInt("colorCount"))
            {
                Debug.Log(child.name);
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }

            colorCount++;
        }
    }



    // Update is called once per frame
    void Update()
    {
        // if the game is not over
        //if (!gameOver)
        //{
        //    // increment the clock
        //    timeElapsed += Time.deltaTime;
        //    if (timeElapsed > timerInterval)
        //    {
        //        timeElapsed -= timerInterval;
        //        secondsElapsed++;
        //        // convert the seconds to user friendly format
        //        if (secondsElapsed > 59)
        //        {
        //            secondsElapsed -= 60;
        //            minutesElapsed++;
        //        }
        //    }
        //}
        // write text to the screen clock
        //gameTime.text = minutesElapsed.ToString("00") + ":" + secondsElapsed.ToString("00");
        gameTime.text = string.Format("{0} of {1}", GameOfMatch, GamesPerMatch);
        // the escape key is equivalent to android's back button
        if (Input.GetKey(KeyCode.Escape))
        {
            GameProgress.Write(new EventRecord() { EventName = "Escape pressed" , GameNumber=GameOfMatch , Row =CurrentRow,Col =CurrentCol  });
            showMenu();
        }

    }


    //
    // public void initGame()
    //
    // This method resets the game
    //

    public void initGame()
    {
      
        // disable the game over dialog
        panelGameOver.gameObject.SetActive(false);
        // reset the ">" marker position
        Vector2 position = moveMarker.GetComponent<RectTransform>().anchoredPosition;
        position.y = -571;
        moveMarker.GetComponent<RectTransform>().anchoredPosition = position;
        // reset the current move
        currentMove = 1;
        // clear the game board
        clearButtonPictures();
        // activate the current button row
        activateButtons(currentMove);
        // generate a code to guess
        setCode(false);
        // reset the time
        timeElapsed = 0f;
        secondsElapsed = 0;
        minutesElapsed = 0;
        // the game is not over obviously
        gameOver = false;
    }



    //
    // public void setCode()
    //
    // This method calculates a code the player has to guess
    //
    private int GamesPerMatch = 5;
    private int GameOfMatch = 0;



    /// <summary>
    /// The game is setup to run in a 5 game sequence
    /// these are the initial colors for each of those games
    /// </summary>
    static int[][] ColorsForMatches =
    {
        new int[] {1,1,1,1 },
        new int[] {2,2,2,2 },
        new int[] {3,3,3,3 },
        new int[] {4,4,4,4 },
        new int[] {5,5,5,5 }
     };
    


    public void setCode(bool Randomize = false)
    {

        // find the gameobject

        GameObject code = GameObject.Find("Code");

        // build a list for the code colors
        // allowed number of colors are stored in playerprefs
        List<int> colors = new List<int>();
        for (int i = 1; i <= PlayerPrefs.GetInt("colorCount"); i++)
        {
            colors.Add(i);
        }

        // get the single code colors
        for (int i = 0; i < 4; i++)
        {
            // get current image
            Image codePin = code.transform.GetChild(i).GetComponent<Image>();
            
            int col;
            if (Randomize)
            {   // choose a color randomly
                col = colors[Random.Range(0, colors.Count)];
            }
            else
            {  
                //get the value from the preset color               
                col = colors[ColorsForMatches[GameOfMatch-1][i]];
                //turn off the same color restriction
                PlayerPrefs.SetInt("sameColor", 1);
            }

            // set the sprite
            codePin.sprite = pinImages[col];

            // multiple colors are allowed, if sameColor = 1
            // if not, the used color has to be removed
            if (PlayerPrefs.GetInt("sameColor") != 1)
                colors.Remove(col);

            string SecretColorSet = string.Join(",", colors.Select(c=> c.ToString()).ToArray<string >());
           
            GameProgress.Write(new EventRecord() { EventName = "SetSecretColors", GameNumber = GameOfMatch, DataKey ="SecretColors", DataValue = SecretColorSet });            
        }
    }



    //
    // public void clearButtonPictures()
    //
    // This method clears the game board
    //

    public void clearButtonPictures()
    {
        // iterate the ten rows
        for (int i = 1; i <= 10; i++)
        {
            // clear the color pins
            GameObject row = GameObject.Find("row-" + i);
            foreach (Transform child in row.transform)
            {
                Image picture = child.GetComponent<Image>();
                picture.sprite = pinImages[0];
            }
            // clear the black white pins
            GameObject pins = GameObject.Find("pins-" + i);
            foreach (Transform child in pins.transform)
            {
                Image picture = child.GetComponent<Image>();
                picture.sprite = swPinImages[0];
            }
        }
    }



    //
    // public void activateButtons(int activeRow)
    //
    // this method activates the current row
    // so the player can only click four buttons at a time
    //

    public void activateButtons(int activeRow)
    {
        for (int i = 1; i <= 10; i++)
        {
            GameObject row = GameObject.Find("row-" + i);
            foreach (Transform child in row.transform)
            {
                if (i == activeRow)
                {
                    // interactable dithers the button.
                    // enabled = true/false toggles the button script
                    // child.GetComponent<Button>().interactable = true;
                    child.GetComponent<Button>().enabled = true;
                }
                else
                {
                    child.GetComponent<Button>().enabled = false;
                }
            }
        }
    }



    //
    // public bool isMoveValid(int activeRow)
    //
    // This method checks the move for validity.
    // a move is valid, if the player set four pins
    //

    public bool isMoveValid(int activeRow)
    {
        GameObject row = GameObject.Find("row-" + activeRow);
        foreach (Transform child in row.transform)
        {
            Image picture = child.GetComponent<Image>();
            if (picture.sprite == pinImages[0])
                return false;
        }
        return true;
    }



    //
    // public void checkMove() 
    //
    // This method gets called when the player presses the CHECK MOVE Button
    // it checks the current move and the game over condition
    //

    public void checkMove()
    {

        // did player make a valid move?
        // (four pins set)
        if (!isMoveValid(currentMove))
        {
            GameProgress.Write(new EventRecord() { EventName = "CheckMove", GameNumber = GameOfMatch, DataKey = "Result", DataValue = "Invalid",Row =CurrentRow ,Col = CurrentCol });

            panelConfirm.gameObject.SetActive(true);
            return;
        }

        // compare pins to secret code
        // check black pins
        int blackPins = 0;
        // these array are needed to check player code against computer code
        bool[] codeCheck1 = new bool[4] { false, false, false, false };
        bool[] codeCheck2 = new bool[4] { false, false, false, false };
        // get the current move's row and the code row
        GameObject row = GameObject.Find("row-" + currentMove);
        GameObject code = GameObject.Find("Code");

        int[] PlayerPicks = new int[4];
        ///what did the user pick?
        for (int i = 0; i < 4; i++)
        {
            Sprite playerPin = row.transform.GetChild(i).GetComponent<Image>().sprite;
            for (int pc = 0; pc < 6; pc++)
            {
                if (playerPin == pinImages[pc])
                {
                    PlayerPicks[i] = pc;
                }
            }
        }
        

        // first for the simple things:
        // are pins with the correct color at the correct position
        // means: black Pins
        for (int i = 0; i < 4; i++)
        {
            Sprite playerPin = row.transform.GetChild(i).GetComponent<Image>().sprite;
            Sprite codePin = code.transform.GetChild(i).GetComponent<Image>().sprite;
            if (playerPin == codePin)
            {
                // alter arrays - these position don't have to be checked further
                codeCheck1[i] = true;
                codeCheck2[i] = true;
                blackPins++;
            }
        }

        // now for the fun part:
        // check for the white pins
        // means: player set a pin with the correct color, but at the wrong position
        int whitePins = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // this if-condition does the magic...
                if (i != j && !codeCheck1[i] && !codeCheck2[j])
                {
                    Sprite playerPin = row.transform.GetChild(j).GetComponent<Image>().sprite;
                    Sprite codePin = code.transform.GetChild(i).GetComponent<Image>().sprite;
                    if (playerPin == codePin)
                    {
                        whitePins++;
                        codeCheck1[i] = true;
                        codeCheck2[j] = true;
                        continue;
                    }
                }
            }
        }

        // set the black and white pins
        int black, white;
        black = 0;
        white = 0;
        GameObject pins = GameObject.Find("pins-" + currentMove);
        foreach (Transform child in pins.transform)
        {
            Image picture = child.GetComponent<Image>();
            // set the black pins
            if (black < blackPins)
            {
                black++;
                picture.sprite = swPinImages[1];
            }
            // set the white pins
            else if (white < whitePins)
            {
                white++;
                picture.sprite = swPinImages[2];
            }
            // player did not guess correctly - so he gets no pins
            else
                picture.sprite = swPinImages[0];
        }


        // player got four black pins - and wins.
        if (blackPins == 4)
        {
            secret.gameObject.SetActive(false);
            panelGameWon.Find("Info").GetComponent<Text>().text = "You guessed the combination in " + currentMove + " tries";
            panelGameWon.gameObject.SetActive(true);
            gameOver = true;

            GameProgress.Write(new EventRecord() { EventName = "CheckMove:WIN", GameNumber = GameOfMatch, Row = CurrentRow , Col = CurrentCol   });

        }

        // ok. no win, so increment the current move
        currentMove++;
        activateButtons(currentMove);

        // is player allowed to make another move? No?
        if (currentMove == 11)
        {
            panelGameOver.gameObject.SetActive(true);
            secret.gameObject.SetActive(false);
            gameOver = true;
            GameProgress.Write(new EventRecord() { EventName = "CheckMove:LOSEGAME", GameNumber = GameOfMatch, Row = CurrentRow, Col = CurrentCol });

        }
        // player may do another move
        else
        {
            GameProgress.Write(new EventRecord() { EventName = "CheckMove:NOWIN", GameNumber = GameOfMatch, Row = CurrentRow, Col = CurrentCol, DataKey = "UserSelection" ,DataValue = string.Join(",",PlayerPicks.Select(p=>p.ToString()).ToArray<string>()) });
            GameProgress.Write(new EventRecord() { EventName = "CheckMove:NOWIN", GameNumber = GameOfMatch, Row = CurrentRow, Col = CurrentCol, DataKey = "BlackPins", DataValue = blackPins.ToString()  });
            GameProgress.Write(new EventRecord() { EventName = "CheckMove:NOWIN", GameNumber = GameOfMatch, Row = CurrentRow, Col = CurrentCol, DataKey = "WhitePins", DataValue = whitePins.ToString() });

            Vector2 position = moveMarker.GetComponent<RectTransform>().anchoredPosition;
            position.y += 130;
            moveMarker.GetComponent<RectTransform>().anchoredPosition = position;
        }
    }



    // Here begins the UI Eventhandling

    #region UI Eventhandling


    //
    // public void gameOverOK()
    //
    // Player clicked OK on the game over dialog
    //

    public void gameOverOK()
    {
        panelGameOver.gameObject.SetActive(false);
        panelGameWon.gameObject.SetActive(false);
    }



    //
    // public void newGame()
    //
    // Player chose to start a new game
    //

    public void newGame()
    {
        panelGameOver.gameObject.SetActive(false);
        panelGameWon.gameObject.SetActive(false);
        secret.gameObject.SetActive(true);

        GameOfMatch++;
        if (GameOfMatch > GamesPerMatch)
        {
            GameProgress.Write(new EventRecord() { EventName = "MatchComplete", GameNumber = GameOfMatch });

            showMenu();
        }
        else
        {
            GameProgress.Write(new EventRecord() { EventName = "NewGame", GameNumber = GameOfMatch });

            initGame();
        }

    }



    //
    // public void confirmDialog()
    //
    // Player clicked the single OK button in a dialog
    //

    public void confirmDialog()
    {
        panelConfirm.gameObject.SetActive(false);
    }



    //
    // public void setPin()
    //
    // Player clicked on the pin sidebar
    // This method finds the first empty Pin slot and sets the color
    //

    public void setPin(int pinColor)
    {
        // find the actual row
        GameObject row = GameObject.Find("row-" + currentMove);
        // find the first empty pin
        foreach (Transform child in row.transform)
        {
            Image picture = child.GetComponent<Image>();
            if (picture.sprite == pinImages[0])
            {
                GameProgress.Write(new EventRecord() { EventName = "PickColor", GameNumber = GameOfMatch, Color = pinColor.ToString() });

                picture.sprite = pinImages[pinColor];
                return;
            }
        }
    }



    //
    // public void clearPin(Button button)
    //
    // Player clicked on a previously set pin
    // This pin is cleared again
    //

    public void clearPin(Button button)
    {
        Image picture = button.GetComponent<Image>();
        picture.sprite = pinImages[0];
    }



    //
    // public void showMenu()
    //
    // Player clicked on the menu button
    // menu dialog is displayed
    //

    public void showMenu()
    {
        panelMenu.gameObject.SetActive(true);
    }



    //
    // public void menuCancel()
    //
    // Player clicked on the cancel menu button
    // menu dialog is closed, player stays in the game
    //

    public void menuCancel()
    {
        panelMenu.gameObject.SetActive(false);
    }



    //
    // public void menuOK()
    //
    // Player clicked on the OK menu button
    // menu dialog is closed, player leaves the game
    // and returns to the menu level
    //

    public void menuOK()
    {
        panelMenu.gameObject.SetActive(false);
        Application.LoadLevel("menu");
    }



    //
    // public void showNewGame()
    //
    // Player clicked on the new game button
    // new game dialog is displayed
    //

    public void showNewGame()
    {
        panelNewGame.gameObject.SetActive(true);
    }



    //
    // public void newGameCancel()
    //
    // Player clicked on the cancel new game button
    // new game dialog is closed, player stays in the game
    //

    public void newGameCancel()
    {
        panelNewGame.gameObject.SetActive(false);
    }



    //
    // public void newGameOK()
    //
    // Player clicked on the OK new game button
    // new game dialog is closed, a new game is started
    //

    public void newGameOK()
    {
        panelNewGame.gameObject.SetActive(false);
        newGame();
    }

    #endregion UI Eventhandling

}
