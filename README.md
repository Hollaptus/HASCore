<h1 align="center">HAS Core</h1>
<div align="center">
  <img height="360" alt="HAS Core" src="https://github.com/user-attachments/assets/c8d5bfc4-f92e-44aa-a2b9-ddfc0669dcb8" />
</div>

<b>HAS</b> <i>(Hollow Apparatus Soundboard)</i> <b>Core</b> - a program written in C# using the NAudio library that uses hotkeys to play sounds into a chosen sound device. It is similar to [EXP Soundboard](https://sourceforge.net/projects/expsoundboard/), except that HAS Core is not as cross-platform as EXP, but, there are more features in HASC than EXP.

This project is a continuation of a project ["JNSoundboard"](https://github.com/Nothing4You/JNSoundboard), using the same source code as base, but reworked and made on ".NET Core" instead of ".NET Framework".

**Binaries are on the [Releases](https://github.com/Hollaptus/HASCore/releases/) page.**

Features:
* Can play MP3, WAV, WMA, M4A, and AC3 audio files
* Play sounds through any sound device (speakers, virtual audio cable, etc.)
* Microphone loopback (loops microphone sound through playback device)
* Add, edit, remove, and clear hotkeys
* Can play a random sound out of multiple (just select multiple files when adding a hotkey)
* Restrict hotkey so that the hotkey is only played when a certain window is in the foreground
* Save (and load) hotkeys to XML file
* Hotkey that stops currently playing sound
* Hotkeys that load XML files containing hotkeys
* Auto press push-to-talk key when playing sound
* Text-to-speech

Requires: 
* .NET Core 10
* NAudio

How to play sound effects over microphone:
You can't really play it "over" the microphone, however you can route them both through a virtual audio cable.
To do that, first install a virtual audio cable (I recommend [VB-CABLE](http://vb-audio.pagesperso-orange.fr/Cable/index.htm)), set the playback device to the virtual audio cable, then set the loopback device to your microphone.
Lastly, in the application that is going to use the microphone, set the microphone device to "VB-Audio Virtual Cable".

Screenshots: 

<p><img width="740" height="600" alt="Main Window" src="https://github.com/user-attachments/assets/5e53a8ea-0dcb-4278-8161-30effa145068" /></p>

<p><img width="428" height="296" alt="Add Hotkey Window" src="https://github.com/user-attachments/assets/5aa899f4-1279-454c-889a-1e5c0dd4d7a1" /></p>

<p><img width="577" height="485" alt="Settings Window" src="https://github.com/user-attachments/assets/3475f44f-5c42-4637-95d6-5b9f27578ab7" /></p>

<p><img width="483" height="294" alt="Text-To-Speech Window" src="https://github.com/user-attachments/assets/fa2806a8-eeb6-4f9b-a71f-0185d68055b8" /></p>
