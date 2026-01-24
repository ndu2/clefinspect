# ClefInspect

A lightweight Windows (.NET, WPF) desktop app for viewing [CLEF](https://clef-json.org/) logfiles (JSON logfiles by Serilog).

![ClefInspect Screenshot](screenshot.png)

Currently runs on Windows with .NET 8.0 only. Distributed as a zip archive.

## Features

 * Displays CLEF events.
 * Show columns based on the properties available in your logfiles.
 * Filter events (text and properties) and pin (always display) individual events.
 * Auto Pin based on Pin Presets (see [Configuration](#configuration)).
 * Scroll to events by timestamp, switch between local and UTC timestamps.
 * Show the time difference between events.
 * Can follow (tail) log files while other processes write to it.
 * Tabs
 * Copy selected events to text or JSON.
 * Limitation: Structured data in the logfile gets "stringified".

## Why ClefInspect?

There are amazing full featured log servers, analysers, and viewers for [CLEF](https://clef-json.org/).

My work with CLEF logfiles is mostly bug-hunting and timing analysis of extensive logfiles. The main task is to collect the revealing events and put them into a context. As none of the tools I found could fit my workflow perfectly, I created ClefInspect.


With ClefInspect, you can crawl the logfiles and keep track of important events very efficiently by pining them. A typical workflow is *filter* -> *pin* -> *filter*.

## Usage

Use *File* -> *Open*, *open with* or *drag & drop* to open logfiles with ClefInspect.

The text filter searches in the message only. It allows for multiple keywords (e.g. `users,login,logoff`) and quotes (`"a comma , here","   do not trim the spaces   "`).

Events must match all filters to be displayed (pinned events are always displayed).

Use timestamp field to browse to a event for a given time (enter a timestamp + `Enter`). This does not act as a filter, but just navigates the view to a given time.

### Shortcuts

 - `Ctrl+C`: Copy selected lines to clipboard (formatted, with the displayed columns)
 - `Ctrl+Shift+C`: Copy selected lines to clipboard as CLEF Events
 - `Ctrl+P`: Pin selected lines
 - `Ctrl+U`: Unpin selected lines
 - `Ctrl+A` (when in the Event list): Select all displayed events
 - `Ctrl+O`: Open a logfile
 - `Ctrl+W`: Close the active tab
 - `Ctrl+Tab`: Switch the active tab

## Installation

ClefInspect is distributed with a zip. Just extract the zip to a folder of your choice (e.g. ClefInspect in your users directory).

### Configuration (optional)

You may want to put a file `ClefInspect.defaults.json` in the folder of `ClefInspect.exe`:

	{
	"clefFeatures": {
		"WriteableConfig": true
	},
	"viewSettings": {
		"LocalTime": true,
		"OneLineOnly": true,
		"DefaultFilterVisibility": [
		"Level",
		"SourceContext"
		],
		"DefaultColumnVisibility": [
			"ThreadId"
		],
		"DetailViewFraction": 0.33,
		"DetailView": false
	},
	"session": {
		"Files": [
		"C:\\temp\\a.json",
		"C:\\temp\\b.json"
		]
	},
	"pinPresets": [
		{
			"Name": "Pin Preset 1",
			"Color": "#4DA84D",
			"Enabled": true,
			"SearchText": ["booting","connecting"]
		},
		{
			"Name": "Pin Preset 2",
			"Color": "#B83579",
			"SearchText": ["some log message"]
		}
	]
	}


## Future

I use ClefInspect 1.3 actively, thus i will enhance and fix every now and then. Feel free to report bugs or wishes on github.

Commercial support is available via my employer (see my github profile).
