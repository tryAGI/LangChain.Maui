# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

.NET MAUI integration for the LangChain .NET ecosystem. Provides a MAUI-specific provider library (`LangChain.Providers.Maui`) and a sample application demonstrating various AI scenarios on mobile and desktop platforms (Android, iOS, macOS Catalyst, Windows). The sample app showcases RAG with PDF loading, speech-to-text, and audio playback.

## Build and Test Commands

```bash
# Build the entire solution
dotnet build LangChain.Maui.slnx

# Build the provider library only
dotnet build src/libs/LangChain.Providers.Maui/LangChain.Providers.Maui.csproj

# Build the sample app (platform-specific)
dotnet build samples/LangChain.Samples.Maui/LangChain.Samples.Maui.csproj

# Run the sample app (macOS Catalyst)
dotnet build samples/LangChain.Samples.Maui/LangChain.Samples.Maui.csproj -t:Run -f net8.0-maccatalyst
```

Note: Building requires the .NET MAUI workload (`dotnet workload install maui`). Windows targets require Windows OS.

## Architecture

### Project Structure

```
src/
├── Directory.Build.props              # Common build properties (C# preview, nullable)
└── libs/
    └── LangChain.Providers.Maui/      # MAUI provider library
        └── LangChain.Providers.Maui.csproj
samples/
└── LangChain.Samples.Maui/           # Sample MAUI application
    ├── MauiProgram.cs                 # App builder with DI registration
    ├── App.xaml / AppShell.xaml        # App shell and navigation
    ├── ViewModels/
    │   ├── RetrievalAugmentedGenerationViewModel.cs  # RAG demo with PDF
    │   ├── SpeechToTextViewModel.cs                  # STT demo
    │   └── PlayRecordViewModel.cs                    # Audio playback demo
    ├── Views/
    │   ├── StartPage.xaml             # Landing page
    │   ├── RetrievalAugmentedGenerationPage.xaml     # RAG UI
    │   ├── SpeechToTextPage.xaml      # STT UI
    │   └── PlayRecordPage.xaml        # Audio UI
    ├── Platforms/                     # Platform-specific bootstrapping
    └── Resources/                     # Icons, fonts, images, styles
```

### Provider Library

`LangChain.Providers.Maui` targets multiple MAUI platforms:
- `net8.0` (shared), `net8.0-android`, `net8.0-ios`, `net8.0-maccatalyst`, `net8.0-windows10.0.19041.0`
- Uses `UseMaui` SDK property
- Defines XAML namespace `https://www.langchain.com/` mapped to `LangChain.Maui`
- Dependencies: `CommunityToolkit.Maui`, `LangChain.Core`

### Sample App

The sample app demonstrates three scenarios:

1. **RAG (Retrieval-Augmented Generation)** — Load a PDF (from file picker or URL), create embeddings with OpenAI `TextEmbeddingV3SmallModel`, store in `InMemoryVectorDatabase`, then ask questions answered by `Gpt4OmniModel` using similar document retrieval.

2. **Speech-to-Text** — Uses `CommunityToolkit.Maui` `SpeechToText` for on-device transcription.

3. **Audio Playback** — Uses `Plugin.Maui.Audio` for audio recording and playback.

Dependencies:
- `LangChain` meta-package (includes providers)
- `LangChain.DocumentLoaders.Pdf` (PDF parsing)
- `CommunityToolkit.Maui` / `CommunityToolkit.Mvvm` (MVVM, UI helpers)
- `Plugin.Maui.Audio` (audio recording/playback)

### MVVM Pattern

The sample app uses CommunityToolkit.Mvvm:
- ViewModels extend `ObservableObject`
- Commands use `[RelayCommand]` source generator attribute
- Properties use `[ObservableProperty]` source generator attribute
- DI via `MauiProgram.cs` using `builder.Services`

## Key Conventions

- **Target framework:** `net8.0` with MAUI platform extensions
- **Language:** C# preview, nullable reference types enabled, implicit usings
- **MVVM:** CommunityToolkit.Mvvm with source generators
- **API keys:** Entered at runtime in the sample app UI (not from environment variables)
