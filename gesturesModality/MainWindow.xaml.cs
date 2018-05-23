namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;
    using Microsoft.Kinect.Wpf.Controls;
    using mmisharp;

    public partial class MainWindow
    {

        private string expression;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        private VisualGestureBuilderDatabase gestureDatabase;
        private VisualGestureBuilderFrameSource gestureFrameSource;
        private VisualGestureBuilderFrameReader gestureFrameReader;

        private MultiSourceFrameReader multiFrameReader;
        private Body[] bodies;
        private Calculator _calc;
        private Boolean gotResult;
        private double lastResult;

        public MainWindow()
        {
            this.lastResult = 0;
            this.InitializeComponent();
            this.expression = "";
            this._calc = new Calculator();
            this.gotResult = false;

            this.lce = new LifeCycleEvents("GESTURES", "FUSION", "gm-1", "gestures", "command");
            this.mmic = new MmiCommunication("localhost", 8000, "User1", "GESTURES");
            mmic.Send(lce.NewContextRequest());

            KinectRegion.SetKinectRegion(this, kinectRegion);
            App app = ((App)Application.Current);
            app.KinectRegion = kinectRegion;

            // Use the default sensor
            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();
            this.kinectRegion.KinectSensor.Open();

            bodies = new Body[this.kinectRegion.KinectSensor.BodyFrameSource.BodyCount];

            gestureDatabase = new VisualGestureBuilderDatabase(@"gestures.gbd");
            gestureFrameSource = new VisualGestureBuilderFrameSource(this.kinectRegion.KinectSensor, 0);

            foreach (var gesture in gestureDatabase.AvailableGestures)
            {
                this.gestureFrameSource.AddGesture(gesture);
            }

            gestureFrameSource.TrackingId = 1;
            gestureFrameReader = gestureFrameSource.OpenReader();
            gestureFrameReader.IsPaused = true;
            gestureFrameReader.FrameArrived += gestureFrameReader_FrameArrived;
            
            multiFrameReader = this.kinectRegion.KinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);
            multiFrameReader.MultiSourceFrameArrived += multiFrameReader_MultiSourceFrameArrived;
            
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            sendMessage("goodbye");
            Environment.Exit(-1);
        }

        private void Number(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if(gotResult == true)
            {
                expression = "";
                gotResult = false;
                lastResult = 0;
                TextRegion.Text = expression;
            }
            switch (button.Name)
            {
                case "Zero": changeExpression("0");  break;
                case "One": changeExpression("1"); break;
                case "Two": changeExpression("2");  break;
                case "Three": changeExpression("3"); break;
                case "Four": changeExpression("4"); break;
                case "Five": changeExpression("5"); break;
                case "Six": changeExpression("6"); break;
                case "Seven": changeExpression("7"); break;
                case "Eight": changeExpression("8"); break;
                case "Nine": changeExpression("9"); break;
            }
        }

        private void ResultRequest(object sender, RoutedEventArgs e)
        {
            sendMessage(expression);
        }

        private void EraseLastChar(object sender, RoutedEventArgs e)
        {
            if (!expression.Equals(""))
            {
               if(char.IsDigit(expression[expression.Length - 1]))
                {
                    expression = expression.Remove(expression.Length-1);
                    changeExpression("");
                }
                else
                {
                    expression = expression.Remove(expression.Length - 3);
                    changeExpression("");
                }
            }
        }

        private void Help(object sender, RoutedEventArgs e)
        {
            sendMessage("help");
        }

        public void sendMessage(String message)
        {
            string json = "{ \"recognized\": [";
            json += "\"" + message + "\", ";
            json = json.Substring(0, json.Length - 2);
            json += "] }";

            lastResult = _calc.makeCalculation(expression);
            TextRegion.Text = lastResult.ToString();
            expression = lastResult.ToString();
            _calc.resetValues();
            gotResult = true;

            if (TextRegion.Text.Contains(","))
            {
                gotResult = false;
                lastResult = 0;
                expression = "";
            }

            var exNot = lce.ExtensionNotification("", "", 1, json);
            mmic.Send(exNot);
        }

        public void AddOperator(String op) {

            if(gotResult == true)
                gotResult = false;

            switch (op)
            {
                case "+": changeExpression(",+,"); break;
                case "-": changeExpression(",-,"); break;
                case "/": changeExpression(",/,"); break;
                case "*": changeExpression(",*,"); break;
            }
        }

        public void changeExpression(String ext)
        {
            expression += ext;
            TextRegion.Text = expression.Replace("," , "");
        }

        private Boolean waitingForOp()
        {
            if (!expression.Equals(""))
                return char.IsDigit(expression[expression.Length - 1]);
            else
                return false;
        }

        private void gestureFrameReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null && waitingForOp())
                {
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                    if (discreteResults != null)
                    {
                        foreach (Gesture gesture in this.gestureFrameSource.Gestures)
                        {
                            DiscreteGestureResult result = null;
                            discreteResults.TryGetValue(gesture, out result);

                            if(result != null && result.Detected)
                            {
                                switch (gesture.Name)
                                {
                                    case "Plus": if (result.Confidence > 0.8) AddOperator("+"); break;
                                    case "Minus": if (result.Confidence > 0.95) AddOperator("-"); break;
                                    case "Times": if (result.Confidence > 0.90) AddOperator("*"); break;
                                    case "divide": if (result.Confidence > 0.90) AddOperator("/"); break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void multiFrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame multiFrame = e.FrameReference.AcquireFrame();
            
            if (!gestureFrameSource.IsTrackingIdValid)
            {
                using (BodyFrame bodyFrame = multiFrame.BodyFrameReference.AcquireFrame())
                {
                    if (bodyFrame != null)
                    {
                        bodyFrame.GetAndRefreshBodyData(bodies);
                        foreach (var body in bodies)
                        {
                            if (body != null && body.IsTracked)
                            {
                                gestureFrameSource.TrackingId = body.TrackingId;
                                gestureFrameReader.IsPaused = false;
                            }
                        }
                    }
                }
            }
        }

    }
}
