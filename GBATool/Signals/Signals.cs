using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Signals;

// Generics
public class BrowseFolderSuccessSignal : Signal<Control, string> { }
public class BrowseFileSuccessSignal : Signal<string, bool> { }
public class SetUpWindowPropertiesSignal : Signal<WindowVO> { }
public class FinishedLoadingProjectSignal : Signal { }
public class ProjectItemLoadedSignal : Signal<string> { }
public class ProjectConfigurationSavedSignal : Signal { }
public class ShowLoadingDialogSignal : Signal { }
public class CloseDialogSignal : Signal { }
public class WriteBuildOutputSignal : Signal<string, OutputMessageType, string> { }
public class MouseWheelSignal : Signal<MouseWheelVO> { }
public class MouseImageSelectedSignal : Signal<Image, Point> { }
public class OptionOnionSkinSignal : Signal<bool> { }
public class ProjectBuildCompleteSignal : Signal { }

// MainWindowViewModel
public class SizeChangedSignal : Signal<SizeChangedEventArgs, bool> { }
public class LoadConfigSuccessSignal : Signal { }
public class ExitSuccessSignal : Signal { }
public class UpdateAdornersSignal : Signal<TreeViewItem, DragEventArgs> { }
public class MouseLeftButtonDownSignal : Signal<Point> { }
public class MouseLeftButtonUpSignal : Signal { }
public class MouseMoveSignal : Signal<MouseMoveVO> { }
public class MouseLeaveSignal : Signal<MouseLeaveVO> { }
public class ProjectItemSelectedSignal : Signal<ProjectItem> { }
public class ProjectItemUnselectedSignal : Signal<ProjectItem> { }
public class CloseProjectSuccessSignal : Signal { }
public class OpenProjectSuccessSignal : Signal<ProjectOpenVO> { }
public class UpdateRecentProjectsSignal : Signal<string[]> { }
public class DropElementSignal : Signal<ProjectItem, ProjectItem> { }
public class LoadProjectItemSignal : Signal<ProjectItem> { }
public class UpdateFolderSignal : Signal<ProjectItem> { }
public class CreateProjectSuccessSignal : Signal<string> { }
public class InitializeAdornersSignal : Signal<TreeViewItem, DragEventArgs> { }
public class DetachAdornersSignal : Signal { }
public class GotoProjectItemSignal : Signal<string> { }

// File system
public class RegisterFileHandlerSignal : Signal<ProjectItem, string?> { }
public class RenameFileSignal : Signal<ProjectItem> { }

// Edit
public class PasteElementSignal : Signal<ProjectItem, ProjectItem> { }
public class MoveElementSignal : Signal<ProjectItem, ProjectItem> { }
public class DeleteElementSignal : Signal<ProjectItem> { }
public class FindAndCreateElementSignal : Signal<ProjectItem> { }
public class CreateNewElementSignal : Signal<ProjectItem> { }

// TileSet
public class UpdateTileSetImageSignal : Signal { }
public class SpriteSelectCursorSignal : Signal { }
public class SpriteSize16x16Signal : Signal { }
public class SpriteSize16x32Signal : Signal { }
public class SpriteSize16x8Signal : Signal { }
public class SpriteSize32x16Signal : Signal { }
public class SpriteSize32x32Signal : Signal { }
public class SpriteSize32x64Signal : Signal { }
public class SpriteSize32x8Signal : Signal { }
public class SpriteSize64x32Signal : Signal { }
public class SpriteSize64x64Signal : Signal { }
public class SpriteSize8x16Signal : Signal { }
public class SpriteSize8x32Signal : Signal { }
public class SpriteSize8x8Signal : Signal { }
public class ConfirmSpriteDeletionSignal : Signal<SpriteVO> { }
public class SelectSpriteSignal : Signal<SpriteVO> { }

// SpriteList
public class UpdateSpriteListSignal : Signal { }
public class CleanUpSpriteListSignal : Signal { }
public class AddSpriteSignal : Signal<SpriteVO> { }
public class DeletingSpriteSignal : Signal<SpriteVO> { }

// Character
public class ColorPaletteControlSelectedSignal : Signal<int[]> { }
public class CleanColorPaletteControlSelectedSignal : Signal { }
public class AnimationTabNewSignal : Signal { }
public class RenamedAnimationTabSignal : Signal<string> { }
public class AnimationTabDeletedSignal : Signal<ActionTabItem> { }
public class SwitchCharacterFrameViewSignal : Signal<string, string, int> { }
public class UpdateCharacterImageSignal : Signal { }
public class NewAnimationFrameSignal : Signal<string, string, int> { }
public class DeleteAnimationFrameSignal : Signal<string, int> { }
public class SelectFrameSpritesSignal : Signal<SpriteControlVO[]> { }
public class ResetFrameSpritesSelectionAreaSignal : Signal<Point> { }
public class PreviousFrameCharacterAnimationSignal : Signal<string> { }
public class PauseCharacterAnimationSignal : Signal<string> { }
public class UpdateCollisionViewSignal : Signal { }
public class NextFrameCharacterAnimationSignal : Signal<string> { }
public class UpdateVerticalAxisSignal : Signal<int> { }
public class NewCollisionIntoSpriteSignal : Signal { }
public class CollisionColorSelectedSignal : Signal<string, Color> { }
public class DeleteCollisionSignal : Signal<SpriteCollisionVO> { }
public class UpdateSpriteBaseSignal : Signal<int> { }
public class UpdateOriginPositionSignal : Signal<int, int> { }
public class StopCharacterAnimationSignal : Signal<string> { }
public class PlayCharacterAnimationSignal : Signal<string> { }
public class AddOrUpdateSpriteIntoCharacterFrameSignal : Signal<CharacterSprite, string> { }
public class LoadWithSpriteControlsSignal : Signal<List<SpriteControlVO>, string> { }
public class FillWithPreviousFrameSpriteControlsSignal : Signal<List<SpriteControlVO>, string> { }
public class DeleteSpritesFromCharacterFrameSignal : Signal<string[]> { }
public class SpriteFrameHideSelectionSignal : Signal { }
public class SpriteFrameShowSelectionSignal : Signal<CharacterDragObjectVO[]> { }
public class InformationToCorrectlyDisplayTheMetaSpriteCenteredSignal : Signal<double, double, double, double> { }
public class UpdateSpriteVisualPropertiesSignal : Signal<string, bool, bool> { }

// Banks
public class FileModelVOSelectionChangedSignal : Signal<FileModelVO> { }
public class AddNewTileSetLinkSignal : Signal<BankLinkVO> { }
public class CleanupTileSetLinksSignal : Signal { }
public class CleanupBankSpritesSignal : Signal { }
public class AdjustCanvasBankSizeSignal : Signal<bool> { }
public class ReloadBankViewImageSignal : Signal { }
public class SelectTileSetSignal : Signal<string> { }
public class BankImageUpdatedSignal : Signal { }
public class BankSpriteDeletedSignal : Signal<SpriteModel> { }
public class ObtainTransparentColorSignal : Signal<SpriteModel> { }
public class GeneratePaletteFromBankSignal : Signal<string, IEnumerable<SpriteModel>, Color, BitsPerPixel> { }
public class SetBankModelToBankViewerSignal : Signal<BankModel?> { }
public class CharacterFrameEditorViewLoadedSignal : Signal { }
public class UpdateBankViewerParentWithImageMetadataSignal : Signal<BankImageMetaData?> { }
public class ReloadBankImageSignal : Signal { }
public class MoveDownSelectedSpriteElementSignal : Signal<int> { }
public class RemoveSpriteSelectionFromBank : Signal { }
public class MoveUpSelectedSpriteElementSignal : Signal<int> { }
public class SetColorFromColorPickerSignal : Signal<Control, Color> { }
public class ReturnTransparentColorFromBankSignal : Signal<Color> { }
public class TryCreatePaletteElementSignal : Signal<string, List<Color>> { }

// Palettes
public class ColorPaletteSelectedSignal : Signal<Color, int, int> { }
public class PaletteColorArrayChangeSignal : Signal<int[]> { }
