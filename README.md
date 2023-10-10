# WinchModTemplate
A template Dredge mod for use with the Winch mod loader

### Prerequisites
- An IDE that supports .NET, like Visual Studio ([community version available here](http://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community))
- Dredge Mod Manager (get it from the [Dredge Mods](https://dredgemods.com) website)
- Winch installed in the mod manager
- A GitHub account (required for using the template and releasing your mod to the public)

### Creating your mod
1. [Generate your repository from this template](https://github.com/new?template_name=WinchModTemplate&template_owner=DREDGE-Mods)
2. Clone your new repository to your machine
3. Open all files in a text editor and do a find replace on `WinchModTemplate` to change it to the name of your mod without spaces. For example, `CosmicHorrorFishingBuddies`.
4. Rename all files with `WinchModTemplate` in the name to instead be the name of your mod without spaces.
5. Edit `WinchModTemplate/mod_meta.json`
6. Edit `WinchModTemplate/WinchModTemplate.csproj.user`
7. Open `WinchModTemplate.sln` in Visual studio
8. Start writing your code in `WinchModTemplate.cs`
9. Build and release the mod

### Editing the mod_meta file

The `mod_meta.json` file looks something like this:
```
{
	"Name": "Winch Mod Template",
	"ModGUID": "YourName.WinchModTemplate",
	"Version": "0.0.1",
	"ModAssembly": "WinchModTemplate.dll",
	"MinWinchVersion": "0.2.3",
	"Entrypoint": "WinchModTemplate.Loader/Initialize"
}
```
You will want to change most of these fields

- `Name`: This is the human readable name of your mod. They will see this name in the mod manager and on the website.
- `ModGUID`: This is the unique identifier for your mod. To prevent overlaps, you should make this `[YourName].[ModName]`. For example, `xen.CosmicHorrorFishingBuddies`.
- `ModAssembly`: The main `.dll` file for your mod. This should be changed along with all other references to `WinchModTemplate` in a find/replace on all files.
- `MinWinchVersion`: Set this to be the current Winch version you're using.
- `EntryPoint`: This is the class and method that is initially called to start your mod. Normally you can just leave this alone.

### Editing the csproj.user file
- In the `DredgePath` line, change this to be the actual path of your Dredge install.
- In the `OutputPath` line, change `YourName.WinchModTemplate` to match the `ModGUID` you wrote in `mod_meta.json`.

### Releasing your mod
Can either be done manually or with the GitHub action.

#### Manually
First build the mod using your IDE. Find it in the mods folder under your Dredge install. Zip the contents of the folder with your mod's `ModGUID`. For the mod to work properly with the manager, the name of the zip must not change between versions. Ideally, keep the zip file name as your `ModGUID`.

Create a new release on GitHub. Have the version tag exactly match the `Version` property in the `mod_meta.json` file. When you update the mod, be sure that you incremented this version number. You should follow [semantic versioning](https://semver.org/). When making a new release for an update, you leave the previous release alone and create a new one with the new version tag.

#### Action
1. Go to the settings for your repository and make sure that GitHub actions have permissions to create releases.
2. Go to the actions tab on your repository and manually trigger the Create Release action.
3. Go to the releases tab and verify that the release was properly made, then publish it (it will be a draft).

#### Adding to the database
Open an [issue](https://github.com/DREDGE-Mods/DredgeModDatabase/issues/new?assignees=&labels=add-new-mod&projects=&template=add-new-mod.yml&title=%5BYour+mod+name+here%5D) on the mod [database](https://github.com/DREDGE-Mods/DredgeModDatabase) to have your mod featured on [dredgemods.com](https://dredgemods.com/) and make it available in the [Dredge Mod Manager](https://github.com/DREDGE-Mods/DredgeModManager).
