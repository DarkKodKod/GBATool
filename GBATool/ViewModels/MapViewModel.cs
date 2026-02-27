using GBATool.Models;

namespace GBATool.ViewModels;

public class MapViewModel : ItemViewModel
{
    public MapModel? GetModel()
    {
        return ProjectItem?.FileHandler?.FileModel is MapModel model ? model : null;
    }

    #region Commands
    #endregion

    #region get/set
    #endregion

    public override void OnActivate()
    {
        base.OnActivate();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
    }
}
