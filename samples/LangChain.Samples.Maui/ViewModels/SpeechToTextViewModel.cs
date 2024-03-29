using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;

namespace LangChain.Samples.Maui.ViewModels;

public partial class SpeechToTextViewModel(
	ISpeechToText speechToText)
	: ObservableObject
{
	private static string[] DefaultLanguages { get; } =
	[
		"en-US"
	];
	
	[ObservableProperty]
	private ObservableCollection<string> _languages = new(DefaultLanguages);
	
	[ObservableProperty]
	private string _selectedLanguage = DefaultLanguages.First();
	
	[ObservableProperty]
	private string _recognitionText = string.Empty;
	
	[RelayCommand]
	private async Task Listen(CancellationToken cancellationToken)
	{
		try
		{
			var isGranted = await speechToText.RequestPermissions(cancellationToken);
			if (!isGranted)
			{
				await Toast.Make("Permission not granted").Show(CancellationToken.None);
				return;
			}

			var recognition = await speechToText.ListenAsync(
				CultureInfo.GetCultureInfo(SelectedLanguage),
				new Progress<string>(partialText =>
				{
					RecognitionText += partialText + " ";
				}), cancellationToken);

			if (recognition.IsSuccessful)
			{
				RecognitionText = recognition.Text;
			}
			else
			{
				await Toast.Make(recognition.Exception?.Message ?? "Unable to recognize speech").Show(CancellationToken.None);
			}
		}
		catch (Exception e)
		{
			await Toast.Make(e.Message).Show(CancellationToken.None);
		}
	}
}