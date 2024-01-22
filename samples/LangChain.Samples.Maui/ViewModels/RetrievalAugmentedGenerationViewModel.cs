using LangChain.Databases.InMemory;
using LangChain.Providers.OpenAI;
using LangChain.Sources;
using LangChain.VectorStores;

namespace LangChain.Samples.Maui.ViewModels;

public partial class RetrievalAugmentedGenerationViewModel(
	IFilePicker filePicker)
	: ObservableObject
{
	private VectorStore? _database;
	
	[ObservableProperty]
	private string _status = string.Empty;
	
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SelectPdfFromComputerCommand))]
	[NotifyCanExecuteChangedFor(nameof(SelectPdfFromUrlCommand))]
	private string _apiKey = string.Empty;
	
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SelectPdfFromUrlCommand))]
	private string _pdfUrl = "https://canonburyprimaryschool.co.uk/wp-content/uploads/2016/01/Joanne-K.-Rowling-Harry-Potter-Book-1-Harry-Potter-and-the-Philosophers-Stone-EnglishOnlineClub.com_.pdf";
	
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(AskQuestionCommand))]
	private string _question = string.Empty;
	
	[ObservableProperty]
	private string _prompt = string.Empty;
	
	[ObservableProperty]
	private string _answer = string.Empty;

	private bool CanSelectPdfFromComputer()
	{
		return !string.IsNullOrWhiteSpace(ApiKey);
	}

	[RelayCommand(CanExecute = nameof(CanSelectPdfFromComputer))]
	private async Task SelectPdfFromComputer(CancellationToken cancellationToken)
	{
		try
		{
			var result = await filePicker.PickAsync();
			if (result == null)
			{
				Status = "No file selected";
				return;
			}
			
			Status = $"Selected file: {result.FileName}. Creating embeddings...";
			
			await using var stream = await result.OpenReadAsync();
			var documents = await PdfPigPdfSource.FromStreamAsync(
				stream, cancellationToken);
			
			var embeddings = new Gpt35TurboModel(ApiKey);
			var database = await InMemoryVectorStore.CreateIndexFromDocuments(
				embeddings, documents);
			_database = database.Store;
			
			Status = "Embeddings created. You can ask question now.";
		}
		catch (Exception e)
		{
			Status = $"Embeddings creation failed: {e}";
		}
	}
	
	private bool CanSelectPdfFromUrl()
	{
		return !string.IsNullOrWhiteSpace(ApiKey) &&
		       !string.IsNullOrWhiteSpace(PdfUrl);
	}

	[RelayCommand(CanExecute = nameof(CanSelectPdfFromUrl))]
	private async Task SelectPdfFromUrl(CancellationToken cancellationToken)
	{
		try
		{
			Status = "Loading file...";
			
			var documents = await PdfPigPdfSource.FromUriAsync(
				new Uri(PdfUrl), cancellationToken);
			
			Status = "Creating embeddings...";
			
			var embeddings = new Gpt35TurboModel(ApiKey);
			var database = await InMemoryVectorStore.CreateIndexFromDocuments(
				embeddings, documents);
			_database = database.Store;
			
			Status = "Embeddings created. You can ask question now.";
		}
		catch (Exception e)
		{
			Status = $"Embeddings creation failed: {e}";
		}
	}
	
	private bool CanAskQuestion()
	{
		return _database != null &&
		       !string.IsNullOrWhiteSpace(Question);
	}

	[RelayCommand(CanExecute = nameof(CanAskQuestion))]
	private async Task AskQuestion(CancellationToken cancellationToken)
	{
		try
		{
			if (_database == null)
			{
				return;
			}
			
			var gpt35 = new Gpt35TurboModel(ApiKey);
			
			Status = "Finding similar documents...";
			
			var similarDocuments = await _database.GetSimilarDocuments(
				Question, amount: 5);

			Prompt =
				$"""
				 Use the following pieces of context to answer the question at the end.
				 If the answer is not in context then just say that you don't know, don't try to make up an answer.
				 Keep the answer as short as possible.

				 {similarDocuments.AsString()}

				 Question: {Question}
				 Helpful Answer:
				 """;
			Status = "Generating answer...";
			
			var response = await gpt35.GenerateAsync(
				Prompt, CancellationToken.None).ConfigureAwait(false);
        
			Answer = response.LastMessageContent;
			Status = "Answer generated";
		}
		catch (Exception e)
		{
			Status = $"Answer generation failed. {e}";
		}
	}
}