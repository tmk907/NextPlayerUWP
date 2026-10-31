# Changelog: UWP to WinUI 3 migration

Work on the `winui3` branch, in `NextPlayerWinUI` and `NextPlayerWinUIDataLayer`.

## Template10 replaced with native components

The assessment found that the app used only a small part of Template10's 106 source files, and that its runtime
could not work on WinUI 3: `BootStrapper.Current` was null and `WindowWrapper` had no way to be created.

| Template10 | Replacement |
| --- | --- |
| `NavigationService`, `INavigable`, `ViewModelBase` | `Navigation/` in the app. Parameters are passed directly instead of as JSON. |
| `HamburgerMenu` | `NavigationView` in `Shell`, with back button and Alt+Left |
| `PageHeader` | `Controls/PageHeader`, a styled `CommandBar` |
| `ModalDialog`, `Busy`, `Splash` | Removed, nothing used them |
| `StringFormatConverter` | Moved to the app |

The `Template10WinUI` project is deleted. Its folder still holds ignored build output and a `.user` file, which are
safe to delete by hand. The UWP `Template10` project stays, because `NextPlayerUWP` still uses it.

## Startup

- Fixed the app never showing a page. Navigation failed on an empty page map and the exception was swallowed, so the
  player was never initialized either.
- Added `Program.cs` with a custom entry point. The app is single-instance, as on UWP.
- Restored the start page setting, file activation and secondary tile launch.

## Restored functionality

- All assets (7 of 113 had been carried over) and the manifest: file associations, capabilities and tiles.
- `TranslationHelper`, which returned an empty string for every text set from code.
- Keyboard shortcuts, now handled through `PreviewKeyDown`.
- Dialogs, file pickers and the share UI, which now use `XamlRoot` or the window handle.
- Now Playing background: album art with a tinted `AcrylicBrush`.
- Live and secondary tiles, CuteRadio incremental loading, licences `Expander`, cover fade-in and popup animation.
- Dark title bar on Windows 10 and 11, through DWM.
- Keep screen on, using `SetThreadExecutionState` because `DisplayRequest` is not supported.
- Scrobbling, through `ScrobbleSender`. It runs at startup and every 30 minutes, but only while the app is running.

## Bugs fixed

- Library scan stalled forever on files with cover art, with no error. Art now comes from tags first, then the folder,
  then a thumbnail. Each file has a limit of 4 running at once and a 20 second timeout. A 430 song scan took 15 seconds.
- Duration stayed at 0:00 and the slider did not move, because the value was read after a fixed 400 ms delay. It now
  follows `NaturalDurationChanged`.
- Volume was never saved, because it was only saved in a UWP background event.
- After a restart the player bar showed "Unknown Title", because the view model was created before the queue loaded.
- The current song changed on every restart and the queue was re-sorted. Creating the right panel triggered a sort, and
  during the first second after startup each re-added song counted as a drag and moved the saved index.
- Lyrics panel showed the first song of a restored queue instead of the current one.
- The app crashed at startup when the process that opened a file had already exited. It now starts normally.
- Crashes from `async void` methods left no log. They are now logged, then the app exits as before.
- `DispatcherWrapper.Dispatch` ran actions twice when called on the UI thread.
- Navigation pane was white in dark theme, which hid its icons.
- Right panel width in the narrow state was saved to the wrong field.
- Thumbnail streams were never disposed.

## Project changes

- Target framework raised to `net8.0-windows10.0.22621.0`, because the `CommunityToolkit.WinUI.*` packages need it to
  resolve their WinUI 3 assets. The minimum OS is still 10.0.17763.
- Added `CommunityToolkit.Mvvm`, `CommunityToolkit.WinUI.Collections`, `Microsoft.Toolkit.Uwp.Notifications` and
  `Microsoft.Windows.SDK.BuildTools.WinUIAnalyzer`.
- Removed `Microsoft.Services.Store.Engagement`.
- `PublishTrimmed` is off, because Newtonsoft.Json and the navigation service use reflection.
- `Nullable` changed from `enable` to `annotations`.
- Removed the mobile-only status bar setting.

## Removed features

- **Tag editing is not migrated, by decision.** The `TagsEditor` screen, its view model, `TagsManager` and the "Edit"
  item in all song menus are gone from the WinUI app. The "edit album" dialog stays, because it only changes album data
  in the app's database and does not write tags to files.

## Known issues

- **Cloud sign-in does not work (Dropbox, pCloud, OneDrive):** `WebAuthenticationBroker` is UWP-only. A replacement
  needs redirect URI changes in the provider developer settings.
- **Lyrics and now-playing extensions are disabled:** `UWPMusicPlayerExtensions` ships only `uap10.0` binaries. Its
  settings page is hidden and the host declaration is removed from the manifest.
- **Scrobbles are sent only while the app is running**, because the background task was not ported.
- **Not restored:** touch swipe on list items (`SlidableListItem`), album hover shadow, cover crossfade, Store feedback
  button, ads.
- `dotnet build` fails with MSB4062 on Windows App SDK 1.7. Use Visual Studio or its MSBuild.
- .NET 8 reaches end of support in November 2026.

## Testing status

Tested by driving the running x64 Debug build:

- All menu pages, settings pages, licences and a new playlist dialog
- Library scan of the local Music folder only
- Album page, playback, pause, next and mute persistence
- Back navigation, Alt+number and Ctrl+L, Now Playing background, dark theme
- Single-instance hand-off and opening a file, both on a running app and on a cold start

Not tested yet:

- Share, tile pinning, file pickers
- Scrobbling, and radio stations loading from the network
- A full scan including OneDrive folders
- Release, x86 runtime and ARM64 builds (x86 Debug was only compiled)
