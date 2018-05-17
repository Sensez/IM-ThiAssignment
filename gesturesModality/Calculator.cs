using System;
using System.Text.RegularExpressions;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    class Calculator
    {

        private string operation;
        private double result;
        private Regex regex;
        private bool beggining;


        public Calculator()
        {
            this.operation = "";
            this.result = 0;
            this.regex = new Regex(@"[0-9]+");
            this.beggining = true;
        }

        public double calculate(String operation, double result, String part)
        {
            switch (operation)
            {
                case "+": result = result + double.Parse(part); break;
                case "-": result = result - double.Parse(part); break;
                case "*": result = result * double.Parse(part); break;
                case "/": result = result / double.Parse(part); break;
                case "^": result = Math.Pow(result, double.Parse(part)); break;
                case "raiz": result = Math.Sqrt(result); break;
                default: Console.WriteLine("Uknown"); break;
            }
            return result;
        }

        public void resetValues()
        {
            this.operation = "";
            this.result = 0;
            this.beggining = true;
        }

        public double makeCalculation(string expression)
        {
            string[] parts = expression.Split(',');
            foreach (string part in parts)
            {
                if (regex.IsMatch(part))
                {
                    if (beggining && operation.Equals("raiz"))
                    {
                        beggining = false;
                        result = double.Parse(part);
                        result = calculate(operation, result, part);
                    }
                    else if (beggining)
                    {
                        result = double.Parse(part);
                        beggining = false;
                    }
                    else
                        result = calculate(operation, result, part);
                }
                else
                {
                    operation = part;
                    if (operation.Equals("raiz") && beggining == false)
                    {
                        result = calculate(operation, result, part);
                    }
                }
            }
            return Math.Round(result, 3);
        }
    }
}