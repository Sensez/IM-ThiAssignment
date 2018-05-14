﻿using System;
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

            String help = "Pode contar comigo para efetuar uma sequência de operações com numeros inteiros. " +
                    "Poderá efetuar somas, subtrações, multiplicações e divisões." +
                    " Para tal, simplesmente tem que selecionar os números da interface gráfica e indicar segundo os gestos " +
                    "disponiveis quais as operações a efetuar. ";

            String[] goodbye = {
                 "Ate amanhã, nem que seja para me dizer olá, porque eu merêço! Tenha um resto de um bom dia",
                "Sempre às ordens ! Tenha um resto de um bom dia.",
                "Espero ter ajudado, que tenha o resto de um bom dia." };

            switch (type)
            {
                case "greeting": return greeting[random];
                case "help": return help;
                case "goodbye": _t.goodbye(); return goodbye[random];
                default: return "";
            }
        }
        public void getExpression(dynamic json)
        {
            expression = (string)json.recognized[0].ToString();
            switch (expression)
            {
                case "goodbye": _t.Speak(chooseRandomSpeech("goodbye")); break;
                case "help": _t.Speak(chooseRandomSpeech("help")); break;
                default: _t.Speak("O resultado da operação é " + _calc.makeCalculation(expression).ToString()); break;
            }
        }
    }
}