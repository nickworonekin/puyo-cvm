# PuyoCvm
PuyoCvm allows you to create CVM files for use with Puyo Puyo! 15th Anniversary and Puyo Puyo 7. Supports all versions for PlayStation 2, PlayStation Portable, and Wii.

## Requirements
* [.NET 6.0 Runtime](https://dotnet.microsoft.com/download) or higher

## Download
The latest release can be downloaded from the [Releases](https://github.com/nickworonekin/puyo-cvm/releases) page.

## Usage
```
PuyoCvm <executable> <input> [options]
```

### Arguments
`executable`

The file path to the game's executable. The file name of the executable will generally be one of the following:

* SLPM_627.54 (for Puyo Puyo! 15th Anniversary on PS2)
* eboot.bin (for PlayStation Portable)
* main.dol (for Wii)

When using with PlayStation Portable executables, the executable must be decrypted prior to use. The executable can be decrypted with apps such as [DecEboot](https://www.romhacking.net/utilities/1225/).

`input`

The path to the directory containing the files and subdirectories to be added to the CVM. By default, only files and subdirectories in this directory defined in the executable's directory listing will be added; this behavior can be changed with the `--all-entries` option.

### Options
`-o, --output <path>`

The file path to the CVM to create. If not set, defaults to ROFS.CVM.

`--all-entries`

Add all files and subdirectories from the directory specified in the `input` argument to the CVM, not just those defined in the executable's directory listing.

## License
PuyoCvm is licensed under the [MIT license](LICENSE.md).