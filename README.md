# ClefInspect

A lightweight Windows (.NET, WPF) desktop app for viewing [CLEF](https://clef-json.org/) logfiles (JSON logfiles by Serilog).

![ClefInspect Screenshot](screenshot.png)

Runs on Windows only ([see future](#future)), distributed as a zip archive (Requires .NET6)


## Features

 * Display CLEF events on a desktop UI in the order they appear in the logfile.
 * Dynamic columns based on the properties available in the logfiles.
 * Filter events (text and properties) and pin (always display) individual events.
 * Scroll to events by timestamp, switch between local and UTC timestamps.
 * Show the time difference between events.
 * Can follow (tail) log files while other processes write to it.
 * Tabs
 * Copy selected events to text or JSON.

## Why?

There are amazing full featured log servers, analysers, and viewers for [CLEF](https://clef-json.org/).

My work on CLEF logfiles is mostly bug-hunting and timing analysis of extensive logfiles. The main task here is to collect the revealing events and put them into a context. As none of the tools I found could fit my workflow perfectly, I ended up using notepad++. But viewing JSON all day long is no fun. So, I created ClefInspect out of necessity (and to fill a weekend of bad weather).


You can crawl CLEF logs with ClefInspect very efficiently by using the workflow filter -> pin -> filter -> pin.


## Future

I use ClefInspect 1.1 actively, thus i will enhance and fix every now and then. Feel free to report bugs or wishes on github.

Commercial support is available via my employer, just drop me a note.

There are ideas to develop ClefInspect 2.0 for cross-platform by rewriting the WPF UI using Avalonia UI. Please contact my if you are interested to collaborate.
