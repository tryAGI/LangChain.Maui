using LangChain.Samples.Maui.ViewModels;

namespace LangChain.Samples.Maui.Views;

public partial class RetrievalAugmentedGenerationPage
{
	public RetrievalAugmentedGenerationPage(RetrievalAugmentedGenerationViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
	}
}

