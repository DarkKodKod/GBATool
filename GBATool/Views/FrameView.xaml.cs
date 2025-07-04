﻿using ArchitectureLibrary.Signals;
using GBATool.Commands.Utils;
using GBATool.Signals;
using System.ComponentModel;
using System.Windows.Controls;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for FrameView.xaml
/// </summary>
public partial class FrameView : UserControl, INotifyPropertyChanged
{
    private int _verticalLineXPos = 0;
    private string _crossData = string.Empty;
    private string _originGuide = string.Empty;
    public event PropertyChangedEventHandler? PropertyChanged;

    #region Commands
    public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
    #endregion

    #region get/set
    public Canvas Canvas { get => canvas; }

    public string OriginGuide
    {
        get => _originGuide;
        set
        {
            _originGuide = value;

            OnPropertyChanged(nameof(OriginGuide));
        }
    }

    public string CrossData
    {
        get => _crossData;
        set
        {
            _crossData = value;

            OnPropertyChanged(nameof(CrossData));
        }
    }

    public int VerticalLineXPos
    {
        get => _verticalLineXPos;
        set
        {
            _verticalLineXPos = value;

            OnPropertyChanged(nameof(VerticalLineXPos));
        }
    }
    #endregion

    public FrameView()
    {
        InitializeComponent();
    }

    public void OnActivate()
    {
        #region Signals
        SignalManager.Get<UpdateVerticalAxisSignal>().Listener += OnUpdateVerticalAxis;
        SignalManager.Get<UpdateOriginPositionSignal>().Listener += OnUpdateOriginPosition;
        #endregion

        SetCrossPosition(0, 0);
    }

    public void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<UpdateVerticalAxisSignal>().Listener -= OnUpdateVerticalAxis;
        SignalManager.Get<UpdateOriginPositionSignal>().Listener -= OnUpdateOriginPosition;
        #endregion
    }

    private void SetCrossPosition(int centerPosX, int centerPosY)
    {
        const int lineTotalLength = 2;
        const int lineLength = lineTotalLength / 2;

        int firstInitLineX = centerPosX - lineLength;
        int firstInitLineY = centerPosY - lineLength;
        int firstEndLineX = centerPosX + lineLength;
        int firstEndLineY = centerPosY + lineLength;

        string firstLine = $"M{firstInitLineX},{firstInitLineY}L{firstEndLineX},{firstEndLineY}";
        string secondLine = $"M{firstEndLineX},{firstInitLineY}L{firstInitLineX},{firstEndLineY}";

        CrossData = string.Concat(firstLine, secondLine);

        OriginGuide = $"M{centerPosX},{centerPosY}L200,{centerPosY}M{centerPosX},{centerPosY}L{centerPosX},200";
    }

    private void OnUpdateVerticalAxis(int value)
    {
        VerticalLineXPos = value;
    }

    private void OnUpdateOriginPosition(int posX, int posY)
    {
        SetCrossPosition(posX, posY);
    }

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
