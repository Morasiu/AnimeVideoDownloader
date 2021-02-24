# AnimeVideoDownloader

## WPF

Can be found at `AnimeVideoDownloader/DesktopDownloader`

Supported sites:
* OgladajAnime.pl
* Wbijam.pl
* Shinden.pl

Supported video providers:
* CDA
* ~~Mp4Up~~ Require passwords

History version can be found [here](CHANGELOG.md)

### Usage

1. Download from release page.
2. Run by clicking on `DesktopDownloader.exe`

![Window1](Docs/Window1.png)
![Window2](Docs/Window2.png)

## Console

Use:

```bash
dotnet run url download_directory
```

For example:

```bash
dotnet run https://drstone.wbijam.pl/pierwsza_seria.html D:/Dr.Stone/
```
