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

        public MainWindow()
        {
            this.InitializeComponent();
            this.expression = "";

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
            sendMessage("Shutdown");
            Environment.Exit(-1);
        }

        private void Number(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            switch (button.Name)
            {
                case "Zero": expression += "0";  break;
                case "One": expression += "1";  break;
                case "Two": expression += "2";  break;
                case "Three": expression += "3";  break;
                case "Four": expression += "4";  break;
                case "Five": expression += "5";  break;
                case "Six": expression += "6";  break;
                case "Seven": expression += "7";  break;
                case "Eight": expression += "8";  break;
                case "Nine": expression += "9";  break;
            }
            Console.WriteLine("expression: " + expression);
        }

        private void ResultRequest(object sender, RoutedEventArgs e)
        {
            sendMessage(expression);
        }

        public void sendMessage(String message)
        {
            string json = "{ \"recognized\": [";
            json += "\"" + message + "\", ";
            json = json.Substring(0, json.Length - 2);
            json += "] }";

            Console.WriteLine(message);
            var exNot = lce.ExtensionNotification("", "", 1, json);
            mmic.Send(exNot);
        }

        public void AddOperator(String op) {
            switch (op)
            {
                case "+": expression += ",+,"; break;
                case "-": expression += ",-,"; break;
                case "/": expression += ",/,"; break;
                case "*": expression += ",*,"; break;
            }
            Console.WriteLine("expression: " + expression);
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
                                    case "Minus": if (result.Confidence > 0.95) AddOperator("-"); break;
                                    case "Times": if (result.Confidence > 0.95) AddOperator("*"); break;
                                    case "Plus": if (result.Confidence > 0.95) AddOperator("+"); break;
                                    case "divide": if (result.Confidence > 0.95) AddOperator("/"); break;
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
