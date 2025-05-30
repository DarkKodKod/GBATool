using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows;

namespace GBATool.Commands.Banks;

public class MoveSpriteToBankCommand : Command
{
    public override bool CanExecute(object? parameter)
    {
        if (parameter == null)
        {
            return false;
        }

        object[] values = (object[])parameter;
        BankModel? model = (BankModel?)values[0];
        SpriteVO spriteVO = (SpriteVO)values[1];

        if (string.IsNullOrEmpty(spriteVO.SpriteID))
        {
            return false;
        }

        return model != null && !model.IsFull;
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;

        BankModel model = (BankModel)values[0];
        SpriteVO spriteVO = (SpriteVO)values[1];

        if (spriteVO.TileSetID == null)
        {
            return;
        }

        TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteVO.TileSetID);

        if (tileSetModel == null)
        {
            return;
        }

        SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteVO.SpriteID);

        if (string.IsNullOrEmpty(sprite?.ID))
        {
            return;
        }

        (bool, string) ret = model.RegisterSprite(sprite);

        if (ret.Item1 == true)
        {
            SignalManager.Get<BankImageUpdatedSignal>().Dispatch();
        }
        else
        {
            MessageBox.Show(ret.Item2, "Error", MessageBoxButton.OK);
        }
    }
}
