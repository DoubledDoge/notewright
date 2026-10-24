using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using AvaloniaEdit.Rendering;
using Notewright.UI.ViewModels;

namespace Notewright.UI.Views.Player;

[SuppressMessage(
	"Performance",
	"CA1812:Avoid uninstantiated internal classes",
	Justification = "Instantiated by Avalonia's XAML loader at runtime."
)]
internal sealed partial class SheetView : UserControl
{
	private readonly DiagnosticRenderer _renderer = new();
	private bool _syncing;
	private SheetViewModel? _vm;

	public SheetView()
	{
		InitializeComponent();
		Editor.TextArea.TextView.BackgroundRenderers.Add(_renderer);
		Editor.TextChanged += (_, _) =>
		{
			if (_vm is null || _syncing)
			{
				return;
			}

			_syncing = true;
			_vm.SheetText = Editor.Text;
			_syncing = false;
		};
		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object? sender, EventArgs e)
	{
		_vm?.PropertyChanged -= OnVmPropertyChanged;

		_vm = DataContext as SheetViewModel;
		if (_vm is null)
		{
			return;
		}

		_vm.PropertyChanged += OnVmPropertyChanged;
		PushTextToEditor();
	}

	private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SheetViewModel.SheetText))
		{
			PushTextToEditor(); // import replaces the text from outside the editor
		}
		else if (e.PropertyName == nameof(SheetViewModel.Diagnostics))
		{
			_renderer.Diagnostics = _vm!.Diagnostics;
			Editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
		}
	}

	private void PushTextToEditor()
	{
		if (_vm is null || _syncing || Editor.Text == _vm.SheetText)
		{
			return;
		}

		_syncing = true;
		Editor.Text = _vm.SheetText;
		_syncing = false;
	}

	private void OnDiagnosticSelected(object? sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count == 0 || e.AddedItems[0] is not DiagnosticItem d)
		{
			return;
		}

		Editor.Select(d.Start, Math.Max(1, d.Length));
		Editor.TextArea.Caret.BringCaretToView();
		Editor.Focus();
	}
}
