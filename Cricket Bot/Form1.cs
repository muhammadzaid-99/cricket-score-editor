using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cricket_Bot
{
    public partial class CricForm : Form
    {

        #region Deafult Constructor
        public CricForm()
        {
            InitializeComponent();
        }
        #endregion



        #region Bowler Class
        class Bowler
        {
            public string Name;
            public string Wickets;
            public string RunsConceded;
            public string Overs;
        }

        #endregion

        #region Bowler Array
        string[][] Bowlers = new string[31][];
        #endregion

        #region Update Button
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            ToUpperCase();

            WriteFiles();
        }
        #endregion


        #region Text Box Changes

        #region  Target Box Change 
        private void TargetTextBox_TextChanged(object sender, EventArgs e)
        {
            var Score = this.ScoreTextBox.Text;
            
            // checking if the target box is NOT empty
            if (!(TargetTextBox.Text == string.Empty)) {
                ToWin(Score);
            }
            else
            {
                // for empty target box, to win box is empty
                this.ToWinTextBox.Text = string.Empty;
            }

            RequiredRunRate();
        }
        #endregion

        #region Score Box Change
        private void ScoreTextBox_TextChanged(object sender, EventArgs e)
        {
            var Score = this.ScoreTextBox.Text;

            // changes To Win
            ToWin(Score);

            // calculate CRR
            CurrentRunRate(Score);

            RequiredRunRate();
        }

        #endregion

        #region Overs Box change
        private void OversTextBox_TextChanged(object sender, EventArgs e)
        {
            this.OversTextBox.Text = OversEdit(this.OversTextBox.Text);

            // calculate CRR
            CurrentRunRate(HyphenRemover(this.ScoreTextBox.Text));

            BallsLeft();

            RequiredRunRate();

            CheckOversDifference(0);
        }
        #endregion

        #region Total Overs Box Change
        private void TotalOversTextBox_TextChanged(object sender, EventArgs e)
        {
            // Calc RR
            RequiredRunRate();

            BallsLeft();

            CheckOversDifference(1);
        }
        #endregion

        #region Bowlers Boxes Changes
            // in bowlers area
        #endregion

        #endregion


        #region Helping Methods

        #region ToWin/Trail/Lead Calculate
        private string ToWin(string ScoreOnly)
        {
            ScoreOnly = HyphenRemover(ScoreOnly);

            // varibales (integers) for converting strings of target and score for calculating To Win
            int ScoreOnlyInt, TargetInt;

            // convert score (to int)
            int.TryParse(ScoreOnly, out ScoreOnlyInt);

            // convert target (to int)
            int.TryParse(TargetTextBox.Text, out TargetInt);

            // To Win Result
            var ResultOfToWin = TargetInt - ScoreOnlyInt;

            // if Result is NOT less thhan 1
            if (!(ResultOfToWin < 0))
            {
                // Sending result to To Win text Box, by converting to string
                this.ToWinTextBox.Text = ResultOfToWin.ToString();
            }
            // considered as lead
            else
            {
                ResultOfToWin = Math.Abs(ResultOfToWin);

                this.ToWinTextBox.Text = ResultOfToWin.ToString();

            }
            return this.ToWinTextBox.Text;
        }


        #endregion

        #region HyphenRemover from Score
        private string HyphenRemover(string ScoreGot)
        {
            ScoreGot = ScoreGot.Replace('/', '-');

            // score
            var ScoreVar = ScoreGot;


            // bool for "-" preceding wkts
            bool hasHyphen = true;

            // index of last hyphen
            int lastIndex;

            // checking for repeated hyphens and removing the last wickets part
            while (hasHyphen)
            {

                // if full score has "-" then then remove last part
                if (ScoreVar.Contains('-'))
                {
                    // finding " - " in full score 
                    lastIndex = ScoreVar.LastIndexOf('-');

                    // remove from last hyphen
                    ScoreVar = ScoreVar.Remove(lastIndex);
                }
                else
                {
                    // else exit loop, score is without wickets
                    hasHyphen = false;
                }

            }

            return ScoreVar;
        }

        #endregion

        #region CRR Calculate
        private void CurrentRunRate(string ScoreGot)
        {
            // remove hyphen, gets core without wkts
            ScoreGot = HyphenRemover(ScoreGot);

            // two fractional numbers for score and overs (as CRR is fractional)
            double ScoreFloat, OversFloat;

            // convert score to fraction
            double.TryParse(ScoreGot, out ScoreFloat);

            // convert overs to fraction
            double.TryParse(OversInBalls(this.OversTextBox.Text), out OversFloat);

            OversFloat = OversFloat / 6;

            // calculate CRR
            double CRR = ScoreFloat / OversFloat;

            // rounding to two decimal places
            CRR = Math.Round(CRR, 2);

            // writes CRR in CRR box by converting to string
            this.CRRTextBox.Text = CRR.ToString();
        }

        #endregion

        #region Overs In Balls Converter
        private string OversInBalls(string Overs)
        {

            // overs before the period
            var OversInitial = Overs;

            // last index of period will be saved here
            int lastIndex;

            // bool if overs have period
            bool hasPeriod = true;

            // while overs have period, do
            while (hasPeriod) {

                // if overs have period
                if (OversInitial.Contains('.')) {

                    // lastIndex is equal to the lst entry of period in OversInitial
                    lastIndex = OversInitial.LastIndexOf('.');

                    // remove period and successive text from overs Initial starting from that period
                    OversInitial = OversInitial.Remove(lastIndex);
                }
                else
                {
                    // when no period left, exit loop
                    hasPeriod = false;
                }
            }


            // new variable for overs text after the period, currently equal to overs
            var OversFinal = Overs;

            // if overs final has period
            if (OversFinal.Contains('.')) {

                // then find the index of first period and save in variable
                var PeriodIndex = OversFinal.IndexOf('.');

                // get the text after the period
                OversFinal = OversFinal.Substring(PeriodIndex + 1);
            }
            else
            {
                // if no period exists, no text after period exists, only initial counts
                OversFinal = String.Empty;
            }

            // strings to integer, initial and final of overs
            int OversInitialInt, OversFinalInt;

            // initial to integer
            int.TryParse(OversInitial, out OversInitialInt);

            // final to integer
            int.TryParse(OversFinal, out OversFinalInt);

            int OversBalls = OversInitialInt * 6 + OversFinalInt;

            return OversBalls.ToString();
        }

        #endregion

        #region RR Calculate

        private void RequiredRunRate()
        {
            var Score = this.ScoreTextBox.Text;
            var ForWin = ToWin(Score);

            double ForWinFloat;

            double.TryParse(ForWin, out ForWinFloat);

            double ReqRR = ForWinFloat / OversLeft();

            ReqRR = Math.Round(ReqRR, 2);

            if (!(ReqRR < 0))
            {
                this.RRTextBox.Text = ReqRR.ToString();
            }
            else
            {
                this.RRTextBox.Text = String.Empty;
            }
        }

        #endregion

        #region Overs Remaining

        private double OversLeft()
        {
            var Overs = this.OversTextBox.Text;

            var TotalOvers = this.TotalOversTextBox.Text;

            var OversBalls = OversInBalls(Overs);
            var TotalOversBalls = OversInBalls(TotalOvers);

            int OversBallsInt, TotalOversBallsInt;

            int.TryParse(OversBalls, out OversBallsInt);
            int.TryParse(TotalOversBalls, out TotalOversBallsInt);

            int RemainingBalls = TotalOversBallsInt - OversBallsInt;

            var RemainingBallsString = RemainingBalls.ToString();

            double RemainingBallsFloat;

            double.TryParse(RemainingBallsString, out RemainingBallsFloat);

            double RemainingOvers = RemainingBallsFloat / 6;

            return RemainingOvers;
        }


        #endregion

        #region File Writers
        private void WriteFiles()
        {
            #region Title
            // Writing Title
            using (StreamWriter TitleWriter = new StreamWriter("Title.txt"))
            {
                TitleWriter.WriteLine(this.TitleTextBox.Text);
            }
            #endregion

            #region Score
            // Writing Score
            using (StreamWriter ScoreWriter = new StreamWriter("Score.txt"))
            {
                ScoreWriter.WriteLine(this.ScoreTextBox.Text);
            }
            #endregion

            #region Description
            // Writing Description
            using (StreamWriter DescriptionWriter = new StreamWriter("Description.txt"))
            {
                DescriptionWriter.WriteLine(this.DescriptionTextBox.Text);
            }
            #endregion

            #region Target
            // Writing Target
            using (StreamWriter TargetWriter = new StreamWriter("Target.txt"))
            {
                TargetWriter.WriteLine(this.TargetTextBox.Text);
            }
            #endregion

            #region Overs
            // Writing Overs
            using (StreamWriter OversWriter = new StreamWriter("Overs.txt"))
            {
                OversWriter.WriteLine(this.OversTextBox.Text);
            }
            #endregion

            #region CRR
            // Writing CRR
            using (StreamWriter CRRWriter = new StreamWriter("CurrentRunRate.txt"))
            {
                CRRWriter.WriteLine(this.CRRTextBox.Text);
            }
            #endregion

            #region RR
            // Writing RR
            using (StreamWriter RRWriter = new StreamWriter("RequiredRunRate.txt"))
            {
                RRWriter.WriteLine(this.RRTextBox.Text);
            }
            #endregion

            #region To Win / Trail / Lead
            // Writing To Win
            using (StreamWriter ToWinWriter = new StreamWriter("ToWin_Trail_Lead.txt"))
            {
                ToWinWriter.WriteLine(this.ToWinTextBox.Text);
            }
            #endregion

            #region Balls Left

            // Writing Balls Left
            using (StreamWriter BallsLedtWriter = new StreamWriter("BallsLeft.txt"))
            {
                BallsLedtWriter.WriteLine(this.BallsLeftTextBox.Text);
            }

            #endregion

            #region Partnership
            // Writing Partnership
            using (StreamWriter PShipWriter = new StreamWriter("PartnerShip.txt"))
            {
                PShipWriter.WriteLine(this.PShipTextBox.Text);
            }
            #endregion

            #region Batting Team Name
            // Writing Batting Team Name (appears before score)
            using (StreamWriter BattingTeamWriter = new StreamWriter("BattingTeam.txt"))
            {
                BattingTeamWriter.WriteLine(this.BattingTeamTextBox.Text);
            }
            #endregion

            #region Toss
            // Writing Toss (team winning toss)
            using (StreamWriter TossWriter = new StreamWriter("Toss.txt"))
            {
                TossWriter.WriteLine(this.TossTextBox.Text);
            }
            #endregion

            #region Total Overs
            // Writing total Overs
            using (StreamWriter TotalOversWriter = new StreamWriter("TotalOvers.txt"))
            {
                TotalOversWriter.WriteLine(this.TotalOversTextBox.Text);
            }
            #endregion

            #region Match Status
            // Writing Match Status
            using (StreamWriter MatchStatusWriter = new StreamWriter("MatchStatus.txt"))
            {
                MatchStatusWriter.WriteLine(this.MatchStatusTextBox.Text);
            }
            #endregion

            #region Batsman 1 Name
            // Writing Batsman 1 Name
            using (StreamWriter Bat1Writer = new StreamWriter("Batsman1_Name.txt"))
            {
                Bat1Writer.WriteLine(this.B1NameTextBox.Text);
            }
            #endregion

            #region Batsman 2 Name
            // Writing Batsman 2 Name
            using (StreamWriter Bat2Writer = new StreamWriter("Batsman2_Name.txt"))
            {
                Bat2Writer.WriteLine(this.B2NameTextBox.Text);
            }
            #endregion

            #region Batsman 1 Score
            // Writing Batsman 1 Score
            using (StreamWriter Bat1Writer = new StreamWriter("Batsman1_Score.txt"))
            {
                Bat1Writer.WriteLine(this.B1ScoreTextBox.Text);
            }
            #endregion

            #region Batsman 2 Score
            // Writing Batsman 2 Score
            using (StreamWriter Bat2Writer = new StreamWriter("Batsman2_Score.txt"))
            {
                Bat2Writer.WriteLine(this.B2ScoreTextBox.Text);
            }
            #endregion

            #region Batsman 1 Balls
            // Writing Batsman 1 Balls
            using (StreamWriter Bat1Writer = new StreamWriter("Batsman1_BallsFaced.txt"))
            {
                Bat1Writer.WriteLine(this.B1BallsTextBox.Text);
            }
            #endregion

            #region Batsman 2 Balls
            // Writing Batsman 2 Balls
            using (StreamWriter Bat2Writer = new StreamWriter("Batsman2_BallsFaced.txt"))
            {
                Bat2Writer.WriteLine(this.B2BallsTextBox.Text);
            }
            #endregion

            #region Batsman 1 4s
            // Writing Batsman 1 4s
            using (StreamWriter Bat1Writer = new StreamWriter("Batsman1_4s.txt"))
            {
                Bat1Writer.WriteLine(this.B14sTextBox.Text);
            }
            #endregion

            #region Batsman 2 4s
            // Writing Batsman 2 4s
            using (StreamWriter Bat2Writer = new StreamWriter("Batsman2_4s.txt"))
            {
                Bat2Writer.WriteLine(this.B24sTextBox.Text);
            }
            #endregion

            #region Batsman 1 6s
            // Writing Batsman 1 6s
            using (StreamWriter Bat1Writer = new StreamWriter("Batsman1_6s.txt"))
            {
                Bat1Writer.WriteLine(this.B16sTextBox.Text);
            }
            #endregion   

            #region Batsman 2 6s
            // Writing Batsman 2 6s
            using (StreamWriter Bat2Writer = new StreamWriter("Batsman2_6s.txt"))
            {
                Bat2Writer.WriteLine(this.B26sTextBox.Text);
            }
            #endregion

            #region Batsman 1 Strike Rate
            // Writing Batsman 1 SR
            using (StreamWriter Bat1Writer = new StreamWriter("Batsman1_StrikeRate.txt"))
            {
                Bat1Writer.WriteLine(this.B1SRTextBox.Text);
            }
            #endregion

            #region Batsman 2 Strike Rate
            // Writing Batsman 2 SR
            using (StreamWriter Bat2Writer = new StreamWriter("Batsman2_StrikeRate.txt"))
            {
                Bat2Writer.WriteLine(this.B2SRTextBox.Text);
            }
            #endregion

            #region Last Bat

            // Writing Last Bat
            using (StreamWriter LastBatWriter = new StreamWriter("LastBat.txt"))
            {
                LastBatWriter.WriteLine(this.LastBatTextBox.Text);
            }

            #endregion

            #region Bowler Name
            using (StreamWriter NameWriter = new StreamWriter("BowlerName.txt"))
            {
                NameWriter.WriteLine(this.BowlerNameTextBox.Text);
            }
            #endregion

            #region Bowler Wickets
            using (StreamWriter WktsWriter = new StreamWriter("BowlerWickets.txt"))
            {
                WktsWriter.WriteLine(this.BowlerWicketsTextBox.Text);
            }
            #endregion

            #region Bowler Runs Conceded
            using (StreamWriter RunsWriter = new StreamWriter("BowlerRunsConceded.txt"))
            {
                RunsWriter.WriteLine(this.BowlerRunsConcededTextBox.Text);
            }
            #endregion

            #region Bowler Overs
            using (StreamWriter OversWriter = new StreamWriter("BowlerOvers.txt"))
            {
                OversWriter.WriteLine(this.BowlerOversTextBox.Text);
            }
            #endregion

            #region Bowler Economy
            using (StreamWriter EcoWriter = new StreamWriter("BowlerEconomy.txt"))
            {
                EcoWriter.WriteLine(this.BowlerEconomyTextBox.Text);
            }
            #endregion

        }

        #endregion

        #region File Readers
        private void ReadFiles()
        {
            #region Title
            if (File.Exists("Title.txt"))
            {
                // Reading Title
                using (StreamReader TitleReader = new StreamReader("Title.txt"))
                {
                    this.TitleTextBox.Text = TitleReader.ReadLine();
                }
            }
            #endregion

            #region Score
            if (File.Exists("Score.txt"))
            {
                // Reading Score
                using (StreamReader ScoreReader = new StreamReader("Score.txt"))
                {
                    this.ScoreTextBox.Text = ScoreReader.ReadLine();
                }
            }
            #endregion

            #region Description
            if (File.Exists("Description.txt"))
            {
                // Reading Description
                using (StreamReader DescriptionReader = new StreamReader("Description.txt"))
                {
                    this.DescriptionTextBox.Text = DescriptionReader.ReadLine();
                }
            }
            #endregion

            #region Target
            if (File.Exists("Target.txt"))
            {
                // Reading Target
                using (StreamReader TargetReader = new StreamReader("Target.txt"))
                {
                    this.TargetTextBox.Text = TargetReader.ReadLine();
                }
            }
            #endregion

            #region Overs
            if (File.Exists("Overs.txt"))
            {
                // Reading Overs
                using (StreamReader OversReader = new StreamReader("Overs.txt"))
                {
                    this.OversTextBox.Text = OversReader.ReadLine();
                }
            }
            #endregion

            #region Partnersip
            if (File.Exists("PartnerShip.txt"))
            {
                // Reading P'Ship
                using (StreamReader PShipReader = new StreamReader("PartnerShip.txt"))
                {
                    this.PShipTextBox.Text = PShipReader.ReadLine();
                }
            }
            #endregion

            #region Total Overs
            if (File.Exists("TotalOvers.txt"))
            {
                // Reading TotalOvers
                using (StreamReader TotalOversReader = new StreamReader("TotalOvers.txt"))
                {
                    this.TotalOversTextBox.Text = TotalOversReader.ReadLine();
                }
            }
            #endregion

            #region Toss
            if (File.Exists("Toss.txt"))
            {
                // Reading Toss
                using (StreamReader TossReader = new StreamReader("Toss.txt"))
                {
                    this.TossTextBox.Text = TossReader.ReadLine();
                }
            }
            #endregion

            #region Batting Team
            if (File.Exists("BattingTeam.txt"))
            {
                // Reading Batting Team
                using (StreamReader BattingTeamReader = new StreamReader("BattingTeam.txt"))
                {
                    this.BattingTeamTextBox.Text = BattingTeamReader.ReadLine();
                }
            }
            #endregion

            #region Match Status
            if (File.Exists("MatchStatus.txt"))
            {
                // Reading Batting Team
                using (StreamReader MatchStatusReader = new StreamReader("MatchStatus.txt"))
                {
                    this.MatchStatusTextBox.Text = MatchStatusReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 1 Name
            if (File.Exists("Batsman1_Name.txt"))
            {
                // Reading Batsman 1 Name
                using (StreamReader BatReader = new StreamReader("Batsman1_Name.txt"))
                {
                    this.B1NameTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 2 Name
            if (File.Exists("Batsman2_Name.txt"))
            {
                // Reading Batsman 2 Name
                using (StreamReader BatReader = new StreamReader("Batsman2_Name.txt"))
                {
                    this.B2NameTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 1 Score
            if (File.Exists("Batsman1_Score.txt"))
            {
                // Reading Batsman 1 Score
                using (StreamReader BatReader = new StreamReader("Batsman1_Score.txt"))
                {
                    this.B1ScoreTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 2 Score
            if (File.Exists("Batsman2_Score.txt"))
            {
                // Reading Batsman 2 Score
                using (StreamReader BatReader = new StreamReader("Batsman2_Score.txt"))
                {
                    this.B2ScoreTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 1 BallsFaced
            if (File.Exists("Batsman1_BallsFaced.txt"))
            {
                // Reading Batsman 1 BallsFaced
                using (StreamReader BatReader = new StreamReader("Batsman1_BallsFaced.txt"))
                {
                    this.B1BallsTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 2 BallsFaced
            if (File.Exists("Batsman2_BallsFaced.txt"))
            {
                // Reading Batsman 2 BallsFaced
                using (StreamReader BatReader = new StreamReader("Batsman2_BallsFaced.txt"))
                {
                    this.B2BallsTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 1 4s
            if (File.Exists("Batsman1_4s.txt"))
            {
                // Reading Batsman 1 4s
                using (StreamReader BatReader = new StreamReader("Batsman1_4s.txt"))
                {
                    this.B14sTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 2 4s
            if (File.Exists("Batsman2_4s.txt"))
            {
                // Reading Batsman 2 4s
                using (StreamReader BatReader = new StreamReader("Batsman2_4s.txt"))
                {
                    this.B24sTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 1 6s
            if (File.Exists("Batsman1_6s.txt"))
            {
                // Reading Batsman 1 6s
                using (StreamReader BatReader = new StreamReader("Batsman1_6s.txt"))
                {
                    this.B16sTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Batsman 2 6s
            if (File.Exists("Batsman2_6s.txt"))
            {
                // Reading Batsman 2 6s
                using (StreamReader BatReader = new StreamReader("Batsman2_6s.txt"))
                {
                    this.B26sTextBox.Text = BatReader.ReadLine();
                }
            }
            #endregion

            #region Last Bat
            if (File.Exists("LastBat.txt"))
            {
                // Reading last Bat
                using (StreamReader LBatReader = new StreamReader("LastBat.txt"))
                {
                    this.LastBatTextBox.Text = LBatReader.ReadLine();
                }
            }
            #endregion

            #region Bowler Name
            if (File.Exists("BowlerName.txt"))
            {
                // Reading Bowler Name
                using (StreamReader BlrNameReader = new StreamReader("BowlerName.txt"))
                {
                    this.BowlerNameTextBox.Text = BlrNameReader.ReadLine();

                    int BowlerIndex = this.BowlersListBox.SelectedIndex;
                    Bowlers[BowlerIndex][0] = this.BowlerNameTextBox.Text;
                    RenameBowler();
                }
            }
            #endregion

            #region Bowler Wickets
            if (File.Exists("BowlerWickets.txt"))
            {
                // Reading Bowler Wkts
                using (StreamReader BlrWktsReader = new StreamReader("BowlerWickets.txt"))
                {
                    this.BowlerWicketsTextBox.Text = BlrWktsReader.ReadLine();

                    int BowlerIndex = this.BowlersListBox.SelectedIndex;
                    Bowlers[BowlerIndex][1] = this.BowlerWicketsTextBox.Text;
                }
            }
            #endregion

            #region Bowler Runs Conceded
            if (File.Exists("BowlerRunsConceded.txt"))
            {
                // Reading Bowler Runs Cncd
                using (StreamReader BlrRunsReader = new StreamReader("BowlerRunsConceded.txt"))
                {
                    this.BowlerRunsConcededTextBox.Text = BlrRunsReader.ReadLine();

                    int BowlerIndex = this.BowlersListBox.SelectedIndex;
                    Bowlers[BowlerIndex][2] = this.BowlerRunsConcededTextBox.Text;
                }
            }
            #endregion

            #region Bowler Overs
            if (File.Exists("BowlerOvers.txt"))
            {
                // Reading Bowler Overs
                using (StreamReader BlrOversReader = new StreamReader("BowlerOvers.txt"))
                {
                    this.BowlerOversTextBox.Text = BlrOversReader.ReadLine();

                    int BowlerIndex = this.BowlersListBox.SelectedIndex;
                    Bowlers[BowlerIndex][3] = this.BowlerOversTextBox.Text;
                }
            }
            #endregion

        }
        #endregion

        #region Text Before Period (OVERS)
        private string TextBeforePeriod(string Overs)
        {

            // replacing period with hyphen and saving in new variable (to use hyphen remover)
            var OversInitial = Overs.Replace('.', '-');

            // using hyphen remover and getting text preceding period (hyphen after replace)
            OversInitial = HyphenRemover(OversInitial);

            return OversInitial;
        }
        #endregion

        #region Text After Period (OVERS)
        private string TextAfterPeriod(string Overs)
        {
            // get index of first period
            var PeriodIndex = Overs.IndexOf('.');

            // getting number after first period and saving in var
            var OversFinal = Overs.Substring(PeriodIndex + 1);

            // check repeatedly if number after period still contains another period
            while (OversFinal.Contains('.'))
            {
                // save number after period in new var, 
                var OversFinalPeriodIndex = OversFinal.IndexOf('.');

                // remove all successive text(e.g. 5.4 will be 5)
                OversFinal = OversFinal.Remove(OversFinalPeriodIndex);
            }

            return OversFinal;
        }
        #endregion

        #region Overs Increment Auto Update Allowance

        private void CheckOversIncrementAllowance()
        {
            if (this.OversIncCheckBox.Checked == true)
            {
                OversIncrement("1");
            }
        }

        #endregion

        #region Check Overs Difference
        private void CheckOversDifference(int value)
        {
            // value: 0 is for overs, 1 is for total overs, to check who is calling

            var TotalOvers = OversInBalls(this.TotalOversTextBox.Text);
            var Overs = OversInBalls(this.OversTextBox.Text);

            int TotalOversInt, OversInt;

            int.TryParse(TotalOvers, out TotalOversInt);
            int.TryParse(Overs, out OversInt);

            if (OversInt >= TotalOversInt)
            {
                if (!(TotalOversInt == 0))
                {
                    if (value == 0)
                    {
                        this.OversTextBox.Text = this.TotalOversTextBox.Text;
                    }
                    else if (value == 1)
                    {
                        this.TotalOversTextBox.Text = this.OversTextBox.Text;
                    }
                }
            }
        }
        #endregion

        #region Overs Increment
        private void OversIncrement(string value)
        {
            var Overs = this.OversTextBox.Text;

            int OversStart, OversEnd;

            int.TryParse(TextBeforePeriod(Overs), out OversStart);
            int.TryParse(TextAfterPeriod(Overs), out OversEnd);

            int amount;
            int.TryParse(value, out amount);

            

            OversEnd += amount;

            if (OversEnd > 5)
            {
                OversEnd = 0;
                OversStart++;

                Overs = OversStart.ToString() + "." + OversEnd.ToString();
            }
            else if (OversEnd < 0)
            {
                OversEnd = 5;
                OversStart--;

                ChangeBatsman();

                Overs = OversStart.ToString() + "." + OversEnd.ToString();
            }

            if (OversEnd == 0)
            {
                ChangeBatsman();
            }
            

            this.OversTextBox.Text = OversStart.ToString() + "." + OversEnd.ToString();
        }
        #endregion

        #region Score Increment
        private string ScoreIncrement(int value)
        {
            var Score = this.ScoreTextBox.Text;

            Score = HyphenRemover(Score);


            int ScoreInt;
            int.TryParse(Score, out ScoreInt);

            ScoreInt += value;

            return ScoreInt.ToString();
        }
        #endregion

        #region Wickets Increment
        private string WicketsIncrement(int value)
        {
            var Score = this.ScoreTextBox.Text;

            Score = Score.Replace('/', '-');
            Score = Score.Replace(" ", "");

            int firstIndex;

            firstIndex = Score.IndexOf('-');

            var Wickets = Score.Substring(firstIndex + 1);

            int WicketsInt;
            int.TryParse(Wickets, out WicketsInt);

            WicketsInt += value;

            if (WicketsInt > 10)
                return (WicketsInt - 1).ToString();

            return WicketsInt.ToString();
        }
        #endregion

        #region Join Score and Wickets
        private void JoinScoreWickets(int scoreValue, int wicketsValue)
        {
            var Score = ScoreIncrement(scoreValue);
            var Wickets = WicketsIncrement(wicketsValue);

            var Middle = ScoreFormat();

            this.ScoreTextBox.Text = Score + Middle + Wickets;

        }
        #endregion

        #region On Window Load
        private void CricForm_Load(object sender, EventArgs e)
        {
            CreateBowler();
            SelectDefaultBowler();
            ReadFiles();

            var Score = this.ScoreTextBox.Text;

            ToWin(Score);
            CurrentRunRate(Score);
            RequiredRunRate();      
        }

        #endregion

        #region UpperCasing
        private void ToUpperCase()
        {
            this.MatchStatusTextBox.Text = this.MatchStatusTextBox.Text.ToUpper();
            this.TossTextBox.Text = this.TossTextBox.Text.ToUpper();
            this.BattingTeamTextBox.Text = this.BattingTeamTextBox.Text.ToUpper();
        }
        #endregion

        #region Overs Box Edit/Change Inc/Dec
        private string OversEdit(string Overs)
        {
            // declaring thwo integers two store left and right side of period
            int OversStart, OversEnd;

            // convrting left and right side (got from Methods) into ints
            int.TryParse(TextBeforePeriod(Overs), out OversStart);

            int.TryParse(TextAfterPeriod(Overs), out OversEnd);

            // Note: TextAfterPeriod also check for additional periods and remove them with successive text

            // if number after period is greater than 5
            if (Overs.Contains(".") && OversEnd > 5)
            {
                // number before period + 1, number aster period  = 0
                Overs = (OversStart + 1).ToString() + '.' + '0';
            }

            else if (Overs.Contains(".") && OversEnd < 0)
            {
                Overs = (OversStart - 1).ToString() + '.' + '5';
            }

            return Overs;
        }
        #endregion

        #region Auto Update Check
        private void CheckAndUpdate()
        {
            if (this.RPUB_CheckBox.Checked == false)
            {
                ToUpperCase();
                WriteFiles();
            }
        }
        #endregion

        #region Batsman Stats Updating
        private void IncreaseBatsmanStats(int value)
        {
            if (this.B1RadioButton.Checked == true)
            {
                // Batsman 1

                // Score
                var Bat1_Score = this.B1ScoreTextBox.Text;

                int Bat1_ScoreInt;
                int.TryParse(Bat1_Score, out Bat1_ScoreInt);

                Bat1_ScoreInt += value;

                this.B1ScoreTextBox.Text = Bat1_ScoreInt.ToString();

                // Balls Faced
                var B1_Balls = this.B1BallsTextBox.Text;

                int B1_BallsInt;
                int.TryParse(B1_Balls, out B1_BallsInt);

                this.B1BallsTextBox.Text = (B1_BallsInt + 1).ToString();
            }

            else

            if (this.B2RadioButton.Checked == true)
            {
                // Batsman 2

                //Score
                var Bat2_Score = this.B2ScoreTextBox.Text;

                int Bat2_ScoreInt;
                int.TryParse(Bat2_Score, out Bat2_ScoreInt);

                Bat2_ScoreInt += value;

                this.B2ScoreTextBox.Text = Bat2_ScoreInt.ToString();

                // Balls faced

                // Balls Faced
                var B2_Balls = this.B2BallsTextBox.Text;

                int B2_BallsInt;
                int.TryParse(B2_Balls, out B2_BallsInt);

                this.B2BallsTextBox.Text = (B2_BallsInt + 1).ToString();
            }
        }
        #endregion

        #region Bowler Stats Updating

        private void ChangeBowlerStats(bool hasWicket, int Runs, bool incOvers)
        {
            // Wickets
            int Wickets; 
            int.TryParse(this.BowlerWicketsTextBox.Text, out Wickets);

            if (hasWicket)
            {
                this.BowlerWicketsTextBox.Text = (Wickets + 1).ToString();
            }

            // Runs
            int RunsConceded;
            int.TryParse(this.BowlerRunsConcededTextBox.Text, out RunsConceded);

            RunsConceded += Runs;

            this.BowlerRunsConcededTextBox.Text = RunsConceded.ToString();

            // Overs
            if (incOvers)
            {
                BowlerOversIncrement(1);
            }
        }
        #endregion

        #region Bowler Overs Increment
        private void BowlerOversIncrement(int value)
        {
            var Overs = this.BowlerOversTextBox.Text;

            int OversStart, OversEnd;

            int.TryParse(TextBeforePeriod(Overs), out OversStart);
            int.TryParse(TextAfterPeriod(Overs), out OversEnd);

            OversEnd += value;

            if (OversEnd > 5)
            {
                OversEnd = 0;
                OversStart++;


                Overs = OversStart.ToString() + "." + OversEnd.ToString();
            }
            else if (OversEnd < 0)
            {
                OversEnd = 5;
                OversStart--;

                Overs = OversStart.ToString() + "." + OversEnd.ToString();
            }

            this.BowlerOversTextBox.Text = OversStart.ToString() + "." + OversEnd.ToString();
        }

        #endregion

        #region Change Batsman / Over Crossing

        public void ChangeBatsman()
        {
            if (this.B1RadioButton.Checked == true)
            {
                this.B2RadioButton.Checked = true;
                this.B1NameTextBox.Text = this.B1NameTextBox.Text.Replace(" *", "");
                this.B2NameTextBox.Text = this.B2NameTextBox.Text.Replace(" *", "");
                this.B1NameTextBox.Text = this.B1NameTextBox.Text.Replace("*", "");
                this.B2NameTextBox.Text = this.B2NameTextBox.Text.Replace("*", "");

                this.B2NameTextBox.Text += " *";
            }
            else
            {
                this.B1RadioButton.Checked = true;
                this.B1NameTextBox.Text = this.B1NameTextBox.Text.Replace(" *", "");
                this.B2NameTextBox.Text = this.B2NameTextBox.Text.Replace(" *", "");
                this.B1NameTextBox.Text = this.B1NameTextBox.Text.Replace("*", "");
                this.B2NameTextBox.Text = this.B2NameTextBox.Text.Replace("*", "");

                this.B1NameTextBox.Text += " *";
            }

            // " 🏏 " (Emoji)
        }


        #endregion

        #region Player Overs/PShip Change ALLOWANCE

        private void PlayerOversChange()
        {
            if (this.PlayerOversChangeAllowanceCheckBox.Checked == true)
            {
                IncreaseBatsmanStats(0);

                ChangeBowlerStats(false, 0, true);

                CalculatePartnership(0, 1);
            }
        }

        #endregion

        #region Calculating Strike Rate
        private void BatsmanStatsChanged(object sender, EventArgs e)
        {
            var B1_Score = this.B1ScoreTextBox.Text;
            var B2_Score = this.B2ScoreTextBox.Text;

            var B1_Balls = this.B1BallsTextBox.Text;
            var B2_Balls = this.B2BallsTextBox.Text;

            int B1_BallsInt, B2_BallsInt;

            int.TryParse(B1_Balls, out B1_BallsInt);
            int.TryParse(B2_Balls, out B2_BallsInt);

            double B1_ScoreDouble, B2_ScoreDouble;

            double.TryParse(B1_Score, out B1_ScoreDouble);
            double.TryParse(B2_Score, out B2_ScoreDouble);

            if (B1_BallsInt < 1)
            {
                this.B1SRTextBox.Text = Math.Round((B1_ScoreDouble * 100), 2).ToString();
            }
            else
            {
                this.B1SRTextBox.Text = Math.Round(((B1_ScoreDouble / B1_BallsInt) * 100), 2).ToString();
            }

            if (B2_BallsInt < 1)
            {
                this.B2SRTextBox.Text = Math.Round((B2_ScoreDouble * 100), 2).ToString();
            }
            else
            {
                this.B2SRTextBox.Text = Math.Round(((B2_ScoreDouble / B2_BallsInt) * 100), 2).ToString();
            }
        }
        #endregion

        #region Calculate Partnership

        private void CalculatePartnership(int ScoreIncreaseValue, int BallsIncreaseValue)
        {
            var PShipForScore = this.PShipTextBox.Text;
            var PShipForBalls = this.PShipTextBox.Text;

            while (PShipForScore.Contains("("))
            {
                int BracketIndexForScore = PShipForScore.IndexOf('(');

                PShipForScore = PShipForScore.Remove(BracketIndexForScore);
            }

            int BracketIndexForBalls = PShipForBalls.IndexOf('(');

            PShipForBalls = PShipForBalls.Replace(")", "");

            PShipForBalls = PShipForBalls.Substring(BracketIndexForBalls + 1);

            int PShipScoreInt, PShipBallsInt;

            int.TryParse(PShipForScore, out PShipScoreInt);
            int.TryParse(PShipForBalls, out PShipBallsInt);

            PShipBallsInt += BallsIncreaseValue;
            PShipScoreInt += ScoreIncreaseValue;

            this.PShipTextBox.Text = PShipScoreInt.ToString() + " " + "(" + PShipBallsInt.ToString() + ")";
        }

        #endregion

        #region Boundary Hitting

        private void HitBoundary(int value)
        {
            if (this.B1RadioButton.Checked == true)
            {
                if (value == 4)
                {
                    var FoursB1 = this.B14sTextBox.Text;

                    int FoursB1Int;
                    int.TryParse(FoursB1, out FoursB1Int);

                    FoursB1Int++;

                    this.B14sTextBox.Text = FoursB1Int.ToString();
                } else
                if (value == 6)
                {
                    var SixsB1 = this.B16sTextBox.Text;

                    int SixsB1Int;
                    int.TryParse(SixsB1, out SixsB1Int);

                    SixsB1Int++;

                    this.B16sTextBox.Text = SixsB1Int.ToString();
                }
            }

            else

            if (this.B2RadioButton.Checked == true)
            {
                if (value == 4)
                {
                    var FoursB2 = this.B24sTextBox.Text;

                    int FoursB2Int;
                    int.TryParse(FoursB2, out FoursB2Int);

                    FoursB2Int++;

                    this.B24sTextBox.Text = FoursB2Int.ToString();
                }
                else
                if (value == 6)
                {
                    var SixsB2 = this.B26sTextBox.Text;

                    int SixsB2Int;
                    int.TryParse(SixsB2, out SixsB2Int);

                    SixsB2Int++;

                    this.B26sTextBox.Text = SixsB2Int.ToString();
                }
            }
        }

        #endregion

        #region Balls Left

        private void BallsLeft()
        {
            if (!(this.TargetTextBox.Text == String.Empty))
            {

                var Overs = this.OversTextBox.Text;

                var Balls = OversInBalls(Overs);

                var TotalOvers = this.TotalOversTextBox.Text;

                int BallsInt, TotalOversInt;

                int.TryParse(Balls, out BallsInt);
                int.TryParse(TotalOvers, out TotalOversInt);

                int TotalBallsInt = TotalOversInt * 6;

                int BallsLeft = TotalBallsInt - BallsInt;

                if (!(BallsLeft < 0))
                {
                    this.BallsLeftTextBox.Text = BallsLeft.ToString();
                } 
                else
                {
                    this.BallsLeftTextBox.Text = String.Empty;
                }
            }
        }

        #endregion

        #region Economy Calculation
        private void CalculateEconomy()
        {
            var Balls = OversInBalls(this.BowlerOversTextBox.Text);

            var Runs = this.BowlerRunsConcededTextBox.Text;

            double BallsFloat, RunsFloat;
            double.TryParse(Balls, out BallsFloat);
            double.TryParse(Runs, out RunsFloat);

            double OversFloat = BallsFloat / 6;

            double Economy = RunsFloat / OversFloat;

            Economy = Math.Round(Economy, 2);

            this.BowlerEconomyTextBox.Text = Economy.ToString();
        }

        #endregion

        #region Rename Bowler
        private void RenameBowler()
        {
            var NewName = this.BowlerNameTextBox.Text;

            int BowlerIndex = this.BowlersListBox.SelectedIndex;

            var NewCheckingName = NewName.Replace(" ", "");

            if ((!(NewCheckingName == String.Empty)) && (!(BowlerIndex < 0)))
            {
                this.BowlerNameTextBox.BackColor = Color.White;

                Bowlers[BowlerIndex][0] = NewName;
                this.BowlersListBox.Items.RemoveAt(BowlerIndex);
                this.BowlersListBox.Items.Insert(BowlerIndex, NewName);

                this.BowlersListBox.SelectedIndex = BowlerIndex;
            }
            else
            {
                this.BowlerNameTextBox.BackColor = Color.LightYellow;
            }
        }
        #endregion

        #region Select Default Bowler (Bowler 0)
        private void SelectDefaultBowler()
        {
            this.BowlersListBox.SelectedIndex = 0;
        }
        #endregion

    


        #endregion


        #region Score Buutons
        private void PlusOneButton_Click(object sender, EventArgs e)
        {
            JoinScoreWickets(1, 0);

            IncreaseBatsmanStats(1);

            ChangeBowlerStats(false, 1, true);

            CheckOversIncrementAllowance();

            ChangeBatsman();

            CalculatePartnership(1, 1);

            CheckAndUpdate();

        }

        private void PlusTwoButton_Click(object sender, EventArgs e)
        {
            JoinScoreWickets(2, 0);

            IncreaseBatsmanStats(2);

            ChangeBowlerStats(false, 2, true);

            CheckOversIncrementAllowance();

            CalculatePartnership(2, 1);

            CheckAndUpdate();
        }

        private void PlusThreeButton_Click(object sender, EventArgs e)
        {
            JoinScoreWickets(3, 0);

            IncreaseBatsmanStats(3);

            ChangeBowlerStats(false, 3, true);

            CheckOversIncrementAllowance();

            ChangeBatsman();

            CalculatePartnership(3, 1);

            CheckAndUpdate();
        }

        private void PlusFourButton_Click(object sender, EventArgs e)
        {
            JoinScoreWickets(4, 0);

            IncreaseBatsmanStats(4);

            ChangeBowlerStats(false, 4, true);

            HitBoundary(4);

            CheckOversIncrementAllowance();

            CalculatePartnership(4, 1);

            CheckAndUpdate();
        }

        private void PlusSixButton_Click(object sender, EventArgs e)
        {
            JoinScoreWickets(6, 0);

            IncreaseBatsmanStats(6);

            ChangeBowlerStats(false, 6, true);

            HitBoundary(6);

            CheckOversIncrementAllowance();

            CalculatePartnership(6, 1);

            CheckAndUpdate();
        }

        private void PlusWicketButton_Click(object sender, EventArgs e)
        {
            JoinScoreWickets(0, 1);

            IncreaseBatsmanStats(0);

            ChangeBowlerStats(true, 0, true);

            CheckOversIncrementAllowance();

            // wicket ball counts in PShip
            CalculatePartnership(0, 1);

            CheckAndUpdate();
        }


        #endregion

        #region Overs Buttons
        private void OversIncButton_Click(object sender, EventArgs e)
        {
            PlayerOversChange(); //includes bowler, batsman and partnership

            OversIncrement("1");

            CheckAndUpdate();
        }

        private void OversDecButton_Click(object sender, EventArgs e)
        {
            OversIncrement("-1");

            CheckAndUpdate();
        }


        #endregion

        #region Extras Buttons
        private void ExtraPlusOneButton_Click(object sender, EventArgs e)
        {
            JoinScoreWickets(1, 0);

            ChangeBowlerStats(false, 1, false);

            CalculatePartnership(1, 0);

            CheckAndUpdate();
        }

        private void MinusOneButton_Click(object sender, EventArgs e)
        {
            JoinScoreWickets(-1, 0);

            ChangeBowlerStats(false, -1, false);

            CalculatePartnership(-1, 0);

            CheckAndUpdate();
        }

        #endregion


        #region Score Format Settings

        #region Spaces CheckBox
        private void ScoreFormatSpacesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ScoreFormatSpacesCheckBox.Checked == true)
            {
                this.R1RadioButton.Text = "10 - 2";
                this.R2RadioButton.Text = "10 / 2";

                JoinScoreWickets(0, 0);

                CheckAndUpdate();
            }
            else
            {
                this.R1RadioButton.Text = "10-2";
                this.R2RadioButton.Text = "10/2";

                JoinScoreWickets(0, 0);

                CheckAndUpdate();
            }


        }
        #endregion

        #region Format Radio Buttons 
        private void R1RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            JoinScoreWickets(0, 0);

            CheckAndUpdate();
        }


        private void R2RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            JoinScoreWickets(0, 0);

            CheckAndUpdate();

        }

        #endregion

        #region Score Format
        private string ScoreFormat()
        {
            var Format = " - ";

            if (this.R1RadioButton.Checked == true)
            {
                Format = " - ";
            }
            else if (this.R2RadioButton.Checked == true)
            {
                Format = " / ";
            }

            if (!(ScoreFormatSpacesCheckBox.Checked == true))
            {
                Format = Format.Replace(" ", "");
            }
            return Format;
        }


        #endregion

        #endregion

        #region Quick Clear Batsman Stats
        private void MouseEnterClearLabel(object sender, EventArgs e)
        {
            var LabelAccessed = (Label)sender;

            LabelAccessed.ForeColor = Color.Red;
        }

        private void MouseLeaveClearLabel(object sender, EventArgs e)
        {
            var LabelAccessed = (Label)sender;

            LabelAccessed.ForeColor = Color.DarkGray;
        }

        private void B1ClearLabel_Click(object sender, EventArgs e)
        {
            this.B1NameTextBox.Text = this.B1NameTextBox.Text.Replace(" *", "");
            this.B2NameTextBox.Text = this.B2NameTextBox.Text.Replace(" *", "");
            this.B1NameTextBox.Text = this.B1NameTextBox.Text.Replace("*", "");
            this.B2NameTextBox.Text = this.B2NameTextBox.Text.Replace("*", "");

            this.LastBatTextBox.Text = this.B1NameTextBox.Text + " " + this.B1ScoreTextBox.Text + " (" + this.B1BallsTextBox.Text + ")";

            this.B1NameTextBox.Text = "- -";
            this.B1ScoreTextBox.Text = "0";
            this.B1BallsTextBox.Text = "0";
            this.B14sTextBox.Text = "0";
            this.B16sTextBox.Text = "0";
            this.B1SRTextBox.Text = "0";

            this.PShipTextBox.Text = "0 (0)";
        }

        private void B2ClearLabel_Click(object sender, EventArgs e)
        {
            this.B1NameTextBox.Text = this.B1NameTextBox.Text.Replace(" *", "");
            this.B2NameTextBox.Text = this.B2NameTextBox.Text.Replace(" *", "");
            this.B1NameTextBox.Text = this.B1NameTextBox.Text.Replace("*", "");
            this.B2NameTextBox.Text = this.B2NameTextBox.Text.Replace("*", "");

            this.LastBatTextBox.Text = this.B2NameTextBox.Text + " " + this.B2ScoreTextBox.Text + " (" + this.B2BallsTextBox.Text + ")";

            this.B2NameTextBox.Text = "- -";
            this.B2ScoreTextBox.Text = "0";
            this.B2BallsTextBox.Text = "0";
            this.B24sTextBox.Text = "0";
            this.B26sTextBox.Text = "0";
            this.B2SRTextBox.Text = "0";

            this.PShipTextBox.Text = "0 (0)";
        }





        #endregion


        #region Bowlers Area

        #region Bowlers Creation


        private void CreateBowler()
        {
            for (int i = 0; i < 31; i++) 
            {
                Bowler NewBowler = new Bowler();

                NewBowler.Name = $"Bowler {i}";
                NewBowler.Wickets = "0";
                NewBowler.RunsConceded = "0";
                NewBowler.Overs = "0";

                BowlerArray(i, NewBowler.Name, NewBowler.Wickets, NewBowler.RunsConceded, NewBowler.Overs);

                this.BowlersListBox.Items.Add(NewBowler.Name);
            }
        }
        
        #endregion

        #region Child Bowler Array
        public void BowlerArray(int BowlerCount, string Name, string Wickets, string RunsConceded, string Overs)
        {
            Bowlers[BowlerCount] = new string[4];
 
            Bowlers[BowlerCount][0] = Name;
            Bowlers[BowlerCount][1] = Wickets;
            Bowlers[BowlerCount][2] = RunsConceded;
            Bowlers[BowlerCount][3] = Overs;   
        }
        #endregion

        #region On SelectionChange
        private void BowlersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int BowlerIndex = this.BowlersListBox.SelectedIndex;

            if (!(BowlerIndex < 0))
            {
                this.BowlerNameTextBox.Text = Bowlers[BowlerIndex][0];
                this.BowlerWicketsTextBox.Text = Bowlers[BowlerIndex][1];
                this.BowlerRunsConcededTextBox.Text = Bowlers[BowlerIndex][2];
                this.BowlerOversTextBox.Text = Bowlers[BowlerIndex][3];
            }
        }


        #endregion
        
        #region Event Handlers

        private void BowlerWicketsTextBox_TextChanged(object sender, EventArgs e)
        {
            int BowlerIndex = this.BowlersListBox.SelectedIndex;

            var Wickets = this.BowlerWicketsTextBox.Text;

            Bowlers[BowlerIndex][1] = Wickets;
        }

        private void BowlerRunsConcededTextBox_TextChanged(object sender, EventArgs e)
        {
            CalculateEconomy();

            int BowlerIndex = this.BowlersListBox.SelectedIndex;
            var RunsConceded = this.BowlerRunsConcededTextBox.Text;

            Bowlers[BowlerIndex][2] = RunsConceded;

        }

        private void BowlerOversTextBox_TextChanged(object sender, EventArgs e)
        {
            this.BowlerOversTextBox.Text = OversEdit(this.BowlerOversTextBox.Text);
            CalculateEconomy();

            int BowlerIndex = this.BowlersListBox.SelectedIndex;

            var Overs = this.BowlerOversTextBox.Text;

            Bowlers[BowlerIndex][3] = Overs;
        }

        private void BowlerNameTextBox_TextChanged(object sender, EventArgs e)
        {
            RenameBowler();
        }


        #endregion

        #region Quick Rename / Key Press Event (F2)
        private void BowlersListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                this.BowlerNameTextBox.SelectAll();
                this.BowlerNameTextBox.Focus();
            }
        }


        #endregion

        #endregion
    }
}
