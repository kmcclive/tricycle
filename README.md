# Tricycle
Tricycle is an open-source video transcoder for macOS and Windows.  It takes the guesswork out of converting videos by using layman's terms and providing a reasonable default configuration.  Tricycle is powered by other open-source projects such as [FFmpeg](https://ffmpeg.org), [x264](https://www.videolan.org/developers/x264.html), and [x265](http://www.x265.org/).

![Tricycle Screenshot](/images/screenshot.png)

## Download
### Latest Version (2.2.0)
* [macOS](https://github.com/kmcclive/tricycle/releases/download/release%2F2-2-0/Tricycle-macOS.dmg)
* [Windows](https://github.com/kmcclive/tricycle/releases/download/release%2F2-2-0/Tricycle-Windows.msi)
### [All Versions](https://github.com/kmcclive/tricycle/releases)

## Features
* Reads/decodes most video and audio formats
* Writes/encodes to the following formats
  * Container formats:
    * MP4
    * MKV
  * Video formats:
    * AVC (H.264)
    * HEVC (H.265)
  * Audio formats:
    * AAC
    * Dolby Digital (AC-3)
* Supports 4K resolution and HDR (HDR10)
* Tonemaps HDR to SDR
* Scales video to standard resolutions
* Detects and crops black bars
* Crops to a selected aspect ratio
* Reduces noise in video
* Overlays subtitles (all or forced only)
* Supports mutliple audio tracks in mono, stereo, or 5.1 surround

## System Requirements
### macOS
* macOS High Sierra (10.13) or later
### Windows
* Windows 7 or later (64-bit)
* .NET Framework 4.6.1 or later

## License
Tricycle is licensed under the [GNU General Public License (GPL) Version 2](COPYING.txt).  Please see the [LICENSE file](LICENSE.txt) for more information.

## Help
If you uncover a bug or have a feature request, please create an issue.

## Contributing
Most contributions are welcome, but those not meeting the project's goals or standards may be rejected.

To begin, create a branch from `master` for the issue you are working on.  Please use the following naming convention.
> \<feature|bugfix\>/\<issue-key\>-\<short-description\>

If an issue does not exist for the improvement you would like to make, please create one.  Once work is complete, create a pull request to have the branch merged into `master`.