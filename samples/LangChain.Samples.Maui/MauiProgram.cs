using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace LangChain.Samples.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services
			.AddSingleton(AudioManager.Current)
			.AddSingleton(SpeechToText.Default)
			
			.AddTransient<ViewModels.SpeechToTextViewModel>()
			.AddTransient<ViewModels.PlayRecordViewModel>()
			
			.AddTransient<Views.StartPage>()
			.AddTransient<Views.SpeechToTextPage>()
			.AddTransient<Views.PlayRecordPage>()
			;
		
		return builder.Build();
	}
}
