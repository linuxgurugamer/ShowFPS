ShowFPS Plugin
==============
Elián Hanisch <lambdae2@gmail.com>
v1.1, May 2016


Requirements
------------

KSP version 1.1 or later.


Installation
------------

Just copy into your KSP's GameData directory.

Features
--------

Simple movable framerate counter on your screen. For those that 
don't have access to the usual Windows utilities for display it, like Linux users.

Graph window to show FPS over time. Graph shows current FPS, a moving average of the FPS, and the Simulation rate (ie: how much is it lagging, related to yellow/red clocking). It can also show a line of a normal symrate (no lagging).
Along the left of the graph will be a scale for the FPS. Along the right will be a scale for the symrate.

Usage
-----

Just press F8 for toggle the FPS counter on and off. Clicking on the toolbar will show the Graph.

You can click and drag the counter anywhere you like. You can also change the default key F8 to anything you like in the settings.cfg file.


Graph Window Controls

	Buttons
		Refresh					Redraw the graph
		Clear					Clear all data from graph 
		Rescale					Rescale the graph to fix all of the data.

	Toggles
		Show Max Symrate		Show a grey line of what the normal (no losses) symrate should be
		Periodic auto-rescale	Will automatically rescale the graph once a minute, if necessary

	Sliders
		Transparency			Lets the background of the graph be transparent. Only takes effect on newly drawn lines or by resizing the graph
		Frequency				How often to plot a datapoint

	Resizer
		A resizer control is at the lower right of the screen. Click and drag it to resize the window

The toggle and slider settings are saved between game sessions
