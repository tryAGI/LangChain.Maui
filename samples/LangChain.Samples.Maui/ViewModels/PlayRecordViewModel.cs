using CommunityToolkit.Maui.Alerts;
using Plugin.Maui.Audio;

namespace LangChain.Samples.Maui.ViewModels;

public partial class PlayRecordViewModel(
	IAudioManager audioManager)
	: ObservableObject
{
	private readonly IAudioRecorder _audioRecorder = audioManager.CreateRecorder();
	
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(StartRecordingCommand))]
	[NotifyCanExecuteChangedFor(nameof(StopRecordingAndPlayRecordedDataCommand))]
	private bool _isRecording;

	[RelayCommand]
	private async Task PlaySomeMp3(CancellationToken cancellationToken)
	{
		try
		{
			using var client = new HttpClient();
			await using var stream = await client.GetStreamAsync("https://onlinetestcase.com/wp-content/uploads/2023/06/100-KB-MP3.mp3", cancellationToken);
			using var audioPlayer = audioManager.CreateAsyncPlayer(stream);

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
	private async Task StopRecordingAndPlayRecordedData(CancellationToken cancellationToken)
	{
		try
		{
			IsRecording = false;
			var audioSource = await _audioRecorder.StopAsync();
			await using var stream = audioSource.GetAudioStream();
			using var audioPlayer = audioManager.CreateAsyncPlayer(stream);

			await audioPlayer.PlayAsync(cancellationToken);
		}
		catch (Exception e)
		{
			await Toast.Make(e.Message).Show(CancellationToken.None);
		}
	}
}