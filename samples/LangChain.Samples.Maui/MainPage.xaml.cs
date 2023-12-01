using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;

namespace LangChain.Samples.Maui;

[ObservableObject]
public partial class MainPage
{
	private readonly IAudioManager _audioManager;
	private readonly IAudioRecorder _audioRecorder;
	
	private static string[] DefaultLanguages { get; } = {
		"en-US",
	};
	
	[ObservableProperty]
	private ObservableCollection<string> _languages = new(DefaultLanguages);
	
	[ObservableProperty]
	private string _selectedLanguage = DefaultLanguages.First();
	
	[ObservableProperty]
	private string _recognitionText = string.Empty;
	
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(StartRecordingCommand))]
	[NotifyCanExecuteChangedFor(nameof(StopRecordingCommand))]
	private bool _isRecording;
	
	public MainPage(IAudioManager audioManager)
	{
		_audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
		_audioRecorder = audioManager.CreateRecorder();
		
		InitializeComponent();
		BindingContext = this;
	}
	
	[RelayCommand]
	private async Task PlaySomeText(CancellationToken cancellationToken)
	{
		try
		{
			using var client = new HttpClient();
			await using var stream = await client.GetStreamAsync("https://onlinetestcase.com/wp-content/uploads/2023/06/100-KB-MP3.mp3", cancellationToken);
			using var audioPlayer = _audioManager.CreateAsyncPlayer(stream);

			await audioPlayer.PlayAsync(cancellationToken);
		}
		catch (Exception e)
		{
			await Toast.Make(e.Message).Show(CancellationToken.None);
		}
	}
	
	private bool CanStartRecording() => !IsRecording;
	
	[RelayCommand(CanExecute = nameof(CanStartRecording))]
	private async Task StartRecording(CancellationToken cancellationToken)
	{
		try
		{
			IsRecording = true;
			await _audioRecorder.StartAsync();
		}
		catch (Exception e)
		{
			await Toast.Make(e.Message).Show(CancellationToken.None);
		}
	}
	
	private bool CanStopRecording() => IsRecording;
	
	[RelayCommand(CanExecute = nameof(CanStopRecording))]
	private async Task StopRecording(CancellationToken cancellationToken)
	{
		try
		{
			IsRecording = false;
			var audioSource = await _audioRecorder.StopAsync();
			await using var stream = audioSource.GetAudioStream();
			using var audioPlayer = _audioManager.CreateAsyncPlayer(stream);

			await audioPlayer.PlayAsync(cancellationToken);
		}
		catch (Exception e)
		{
			await Toast.Make(e.Message).Show(CancellationToken.None);
		}
	}
	
	[RelayCommand]
	private async Task Listen(CancellationToken cancellationToken)
	{
		try
		{
			var isGranted = await SpeechToText.Default.RequestPermissions(cancellationToken);
			if (!isGranted)
			{
				await Toast.Make("Permission not granted").Show(CancellationToken.None);
				return;
			}

			var recognition = await SpeechToText.ListenAsync(
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

