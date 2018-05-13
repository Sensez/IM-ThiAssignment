using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private Tts _t;
        private Calculator _calc;
        private String expression;

        public MainWindow()
        {
            InitializeComponent();
            _t = new Tts();
            _calc = new Calculator();
            _t.Speak(chooseRandomSpeech("greeting"));
            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            getExpression(json);
            _calc.resetValues();
        }
        public string chooseRandomSpeech(string type)
        {
            Random rnd = new Random();
            int random = rnd.Next(0, 3);

            String[] greeting = { "Olá, eu sou a Cheila a calculadora falante, em que lhe posso ser util ?",
                "Bem vindo ! Em que lhe posso ser util hoje ?",
                "Ora aqui estou eu, Cheila, a magnifica calculadora ! Que contas vamos fazer hoje ?" };

            String[] help = { "Pode contar comigo para efetuar operações com dois números de até 4 digitos." +
                    " As operações disponíveis são as de soma, multiplicação, divisão, subtração, raiz quadrada, " +
                    "e elevar a um numero. Um exemplo de utilização seria: Cheila, soma 5 e 5",
                "Tenho todo o prazer em o ajudar, tente por exemplo algo como, quanto é um numero mais outro, ou melhor ainda, " +
                "quanto é a soma de um numero com outro. Pode dividir, somar, subtrair, multiplicar e ainda aplicar raizes e elevar numeros. Todas as operações são válidas" +
                "para numeros com até 4 digitos.",
                "Pode contar comigo para o auxiliar a efetuar operações tais como as de soma, multiplicação, divisão e subtração a numeros com até 4 digitos." +
                "Porque não tenta efetuar desde já uns calculos teste, como por exemplo, somar um numero com outro" };

            String[] goodbye = {
                 "Ate amanhã, nem que seja para me dizer olá, porque eu merêço! Tenha um resto de um bom dia",
                "Sempre às ordens ! Tenha um resto de um bom dia.",
                "Espero ter ajudado, que tenha o resto de um bom dia." };

            switch (type)
            {
                case "greeting": return greeting[random];
                case "help": return help[random];
                case "goodbye": return goodbye[random];
                default: return "";
            }
        }
        public void getExpression(dynamic json)
        {
            expression = (string)json.recognized[0].ToString();
            Console.WriteLine(expression);
        }
    }
}