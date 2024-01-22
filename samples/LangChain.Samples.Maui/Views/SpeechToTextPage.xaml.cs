using LangChain.Samples.Maui.ViewModels;

namespace LangChain.Samples.Maui.Views;

public partial class SpeechToTextPage
{
	public SpeechToTextPage(SpeechToTextViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
	}
}

