<p align="center">
  <img src="docs/Banner.png" alt="GreenKeeper" width="650" />
</p>

---

## About

Does this sound familiar? You find a pretty plant at the supermarket, buy it, and promise yourself you'll take good care of it. Then life gets busy, the days blur together, and before you know it, your plant has withered from neglect.

I wanted to make a change. With GreenKeeper, you get a desktop app that helps you keep track of your plants' care schedules, from watering and fertilizing to sunlight needs, so nothing falls through the cracks again.

## Download & Installation

You have two options to get GreenKeeper running:

- **[Download the prebuilt release](#download-the-prebuilt-release)**
- **[Clone and build from source](#clone-and-build-from-source)**

### Download the prebuilt release

#### Step 1: Download the latest release

Go to the [**Releases**](../../releases) page and download the ZIP file
for the latest version, e.g. `GreenKeeper-v1.0.0-win-x64.zip`.

> [!TIP]
> The asset is listed under **Assets** at the bottom of each release. Look
> for the file ending in `-win-x64.zip`.

#### Step 2: Extract the ZIP file

Right-click the downloaded ZIP file and select **Extract All...**, then
choose a folder of your choice (e.g. `C:\Programs\GreenKeeper`).

>[!NOTE]
>GreenKeeper doesn't need to be installed into `Program Files` - any
>folder works, as long as you keep the extracted files together in one place

#### Step 3: Run GreenKeeper

Open the extracted folder and double-click `GreenKeeper.exe`

>[!Warning]
>Since GreenKeeper isn't actually digitally signed, Windows SmartScreen may show a
>**"Windows protected your PC"** warning on first launch. This is expected
>for small, independently published applications - it does not mean the
>app to be unsafe.
>
>To proceed, click **More info**, then **Run anyway**.

That's it - no installer, no .NET Runtime to install separately. Everything
needed to run the app is already bundled in the download, so you are good to go!

#### System requirements

| Requirement | Details |
|---|---|
| Operating System | Windows 10 or later (64-bit) |
| Disk space | ~150 MB |
| .NET Runtime | Not required - bundled with the app |

#### Updating to a newer version

1. Download the new release ZIP as described above.
2. Extract it to a **new** folder (or overwrite the old files).
3. Delete the old version's files if you extracted to a new folder.

>[!NOTE]
>Your plant data is stored separately from the application files, in
>`%LocalAppData%\GreenKeeper`. Updating GreenKeeper - even to a different
>folder - never affects your existing data.

#### Uninstalling

Since GreenKeeper doesn't use a system installer, uninstalling is simple:

1. Delete the folder you extracted GreenKeeper into.
2. *(Optional)* To also remove your saved plant data, delete the folder `%LocalAppData%\GreenKeeper`.
