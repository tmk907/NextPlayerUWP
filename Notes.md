
NextPlayerUWP started as UWP application for desktop and mobile.

Originally it has following UWP and netstandard projects:
- NextPlayerUWP
- NextPlayerUWPDataLayer
- NextPlayerUWPDataLayer.Core
- Template10 (Library)
- ScrobblerBG

I started migration to WinUI3 and created following projects:
- NextPlayerWinUI
- NextPlayerWinUIDataLayer
- NextPlayerWinUIDataLayer.Core
- Template10WinUI

Current status:
- projects build
- NextPlayerWinUI can be launched
- I don't remember if I removed or commented some code from WinUI projects to make app build and launch. Compare NextPlayerUWP with NextPlayerWinUI and Template10 (Library) with Template10WinUI.
- Template10 (Library) should be migrated to WinUI3 or replaced by native components. Start with assessment what can be done. Not all functions from Template10 are used. Code that is not needed can be removed.
- NextPlayerUWP targeted desktop and mobile. WinUI version can target only desktop. If it make it easier to migrate code, mobile versions of screens can be removed.

Helpful docs:
[Migrate a UWP app to WinUI 3](https://learn.microsoft.com/en-us/windows/apps/develop/ai-assisted/migrate/uwp-to-winui)