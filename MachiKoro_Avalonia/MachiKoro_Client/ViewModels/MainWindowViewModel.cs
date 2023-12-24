using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using MachiKoro_Client.Models;
using MachiKoro_Client.Views;
using ReactiveUI;

namespace MachiKoro_Client.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
   public JClient Player { get;}
   
   
   

   public MainWindowViewModel()
   {
      Player = new JClient();
      Cards = new ObservableCollection<Bitmap>();
      ConnectPlayerReactiveCommand = ReactiveCommand.Create( () => ConnectPlayer());
      ThrowDiceReactiveCommand = ReactiveCommand.Create(() => ThrowDice());
      EndTurnReactiveCommand = ReactiveCommand.Create((() => EndTurn()));
      
      BuyCardReactiveCommand = ReactiveCommand.Create<string>(BuyCard);

      ResetMyCardsReactiveCommand = ReactiveCommand.Create(() => ResetMyCards());

   }
   
    
   // --- Reactive Commands
    
   public ReactiveCommand<Unit, Unit> ConnectPlayerReactiveCommand { get; }
   public ReactiveCommand<Unit, Unit> ThrowDiceReactiveCommand { get; }
   public ReactiveCommand<Unit, Unit> EndTurnReactiveCommand { get; }
   public ReactiveCommand<string, Unit> BuyCardReactiveCommand { get; }
   public ReactiveCommand<Unit, Unit> ResetMyCardsReactiveCommand { get; }
    
   // --- Commands

   private void ConnectPlayer()
   {
      Task.Run(() => Player.StartSessionAsync());
      ResetMyCards();
   }

   private void ThrowDice()
   {
         Player.DiceThrowPacketCreate();
   }

   private void EndTurn()
   {
      Player.ChangeTurn();
   }

   public void BuyCard(string cardName)
   {
      Player.BuyCard(cardName);
      Console.WriteLine($"КУПИЛИ КАРТУ {cardName}");
      ResetMyCards();
   }

   public void ResetMyCards()
   {
      Cards.Clear();
      foreach (var variableCard in Player.MyCards)
      {
         Cards.Add(Helpers.ImageHelper.LoadFromResource("../Assets/CardImages/" + variableCard.Name + ".jpg"));
      }
   }

   public ObservableCollection<Bitmap> Cards { get; set; }
}