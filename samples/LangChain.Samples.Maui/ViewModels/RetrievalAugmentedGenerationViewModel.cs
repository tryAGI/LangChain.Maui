using LangChain.Databases.InMemory;
using LangChain.Providers.OpenAI.Predefined;
using LangChain.DocumentLoaders;
using LangChain.Databases;
using LangChain.Extensions;
using LangChain.Providers;

namespace LangChain.Samples.Maui.ViewModels;

public partial class RetrievalAugmentedGenerationViewModel(
	IFilePicker filePicker)
	: ObservableObject
{
	private IVectorDatabase? _database;
	private IVectorCollection? _collection;
	private IEmbeddingModel? _embeddings;
	
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
			_embeddings ??= new TextEmbeddingV3SmallModel(ApiKey);
			_database ??= new InMemoryVectorDatabase();
			
			var result = await filePicker.PickAsync();
			if (result == null)
			{
				Status = "No file selected";
				return;
			}
			
			Status = $"Selected file: {result.FileName}. Creating embeddings...";
			
			await using var stream = await result.OpenReadAsync();
			
			_collection = await _database.AddDocumentsFromAsync<PdfPigPdfLoader>(
				_embeddings, // Used to convert text to embeddings
				dimensions: 1536, // Should be 1536 for TextEmbeddingV3SmallModel
				dataSource: DataSource.FromStream(stream),
				collectionName: result.FileName,
				cancellationToken: cancellationToken);
			
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
			_embeddings ??= new TextEmbeddingV3SmallModel(ApiKey);
			_database ??= new InMemoryVectorDatabase();
			
			Status = "Creating embeddings...";
			
			_collection = await _database.AddDocumentsFromAsync<PdfPigPdfLoader>(
				_embeddings, // Used to convert text to embeddings
				dimensions: 1536, // Should be 1536 for TextEmbeddingV3SmallModel
				dataSource: DataSource.FromUrl(PdfUrl),
				collectionName: PdfUrl,
				cancellationToken: cancellationToken);
			
			Status = "Embeddings created. You can ask question now.";
		}
		catch (Exception e)
		{
			Status = $"Embeddings creation failed: {e}";
		}
	}
	
	private bool CanAskQuestion()
	{
		return _collection != null &&
		       !string.IsNullOrWhiteSpace(Question);
	}

	[RelayCommand(CanExecute = nameof(CanAskQuestion))]
	private async Task AskQuestion(CancellationToken cancellationToken)
	{
		try
		{
			_embeddings ??= new TextEmbeddingV3SmallModel(ApiKey);
			if (_collection == null)
			{
				return;
			}
			
			var llm = new Gpt4OmniModel(ApiKey);
			
			Status = "Finding similar documents...";
			
			var similarDocuments = await _collection.GetSimilarDocuments(
				_embeddings, Question, amount: 5);

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
			
			ChatResponse? response = null;
			await foreach (var update in llm.GenerateAsync(
				Prompt,
				settings: (ChatSettings?)null,
				cancellationToken: cancellationToken).ConfigureAwait(false))
			{
				response = update;
			}

			Answer = response?.LastMessageContent ?? string.Empty;
			Status = "Answer generated";
		}
		catch (Exception e)
		{
			Status = $"Answer generation failed. {e}";
		}
	}
}
