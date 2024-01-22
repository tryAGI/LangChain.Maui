using LangChain.Samples.Maui.ViewModels;

namespace LangChain.Samples.Maui.Views;

public partial class PlayRecordPage
{
	public PlayRecordPage(PlayRecordViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
	}
}

