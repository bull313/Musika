<h1>Musika - The Music Programming Language</h1>
<h2>Musika Features</h2>
<ul>
	<li>Import 0 or more Musika files to utilize their patterns/music in a file</li>
	<li>
		Add the following metadata to a song:
		<ul>
			<li>song title</li>
			<li>song author(s)</li>
			<li>key signature</li>
			<li>time signature</li>
			<li>base tempo</li>
			<li>base octave</li>
		</ul>
	</li>
	<li>Define patterns/riffs that can be used in songs (like functions in a program)</li>
	<li>Define chords that can modularize complex macro-notes (chords)</li>
	<li>Take a segment of music and have it repeat a number of times</li>
	<li>Layer music segments on top of each other</li>
	<li>Change the metadata for a song in the middle of it to change aspects such as key, time, tempo, and octave</li>
	<li>Write notes in the specified key, give them a length, and keep the octave at the base or shift it one higher or lower</li>
	<li>Write comments in the code to make developer/composer notes</li>
</ul>
<h2>How to program in Musika</h2>
<h3>Basic Musika Anatomy</h3>
<p>Every Musika file is divided into 4 sections:</p>
<ul>
	<li>Accompaniment Section</li>
	<li>Info Section</li>
	<li>Pattern Section</li>
	<li>Music Section</li>
</ul>
<p>
	Each of these sections are separated by three dashes (---) on a new line.
	The accompaniment section is optional, but the other three are required.
</p>
<h3>The Accompaniment Section</h3>
<p>The following is an example of an accompaniment (another Musika file):</p>
<pre>
	<strong>accompany</strong> [musika_file] <strong>name</strong> new_accompaniment
</pre>
<p>You specify a Musika file within your directory (or as a part of the Musika library) solely by the name (NO FILE EXTENSION)</p>
<p>
	This statement will import the specified Musika file and it can be referenced later in the file using the name specified after the
	<strong>name</strong> keyword.
</p>
<p>
	Each accompaniment specification must be specified on its own line. After that, place three dashes (---) at the bottom line to
	enter the info section:
</p>
<pre>
	<strong>accompany</strong> [file1] <strong>name</strong> one
	<strong>accompany</strong> [file2] <strong>name</strong> two
	---
	<strong>title</strong>: "Song Title"
	<strong>author</strong>: "Song Author"
	<strong>coauthors</strong>: "Song coauthors"
	<strong>key</strong>: Cmaj
	<strong>time</strong>: 4 / 4
	<strong>tempo</strong>: 4 = 60
	<strong>octave</strong>: 4
	---
</pre>
<p>Note the bottom three dashes used to enter the patterns section afterwards</p>

<h3>Sample Programs</h3>
<ul>
	<li>
		<h4>example1.ka</h4>
		<pre>
			accompany [example2] name rhythm
			accompany [example3] name pattern_lib
			---
			title: "High Noon Chorus Lead"
			author: pattern_lib
			coauthors: pattern_lib
			key: pattern_lib
			time: pattern_lib
			tempo: pattern_lib
			octave: 5
			=>
			Any notes on the piece can be written here in a multi-line comment
			<=
			---
			& this is a single-line comment
			pattern [chorus1]:
			E....A..G..F.G.D,..A..G.F. & this is the lead pattern for high noon
			& this is another comment
			---
			layer(rhythm)
			repeat(2) {
				chorus1
				pattern_lib>chorus2
				pattern_lib>chorus_end
			}
			---
		</pre>
	</li>
	<li>
		<h4>example2.ka</h4>
		<pre>
			accompany [example3] name lib
			accompany [chords] name c
			---
			title: "High Noon Chorus Rhythm"
			author: lib
			coauthors: lib
			key: lib
			time: lib
			tempo: lib
			octave: 2
			=>
				This is the rhythm part of High Noon
			<=
			---
			pattern [chorus]:
			c>E5...c>B5..c>B5.c>B5.c>B5.
			---
			chorus*2*3
			c>E8.*4 c>B8.*4 c>C8.*4 c>D8.*4
			---
		</pre>
	</li>
	<li>
		<h4>exapmle3.ka</h4>
		<pre>
			title: "High Noon Chorus Patterns"
			author: "Benjamin Ladick"
			coauthors: "Masking The Fallen"
			key: Em
			time: common
			tempo: 4 = 144
			octave: 4
			---
			pattern [chorus2]:
			E.B,.D,.E.A..G..F.G.D..B.A.G.F.
			pattern [chorus_end]:
			G..F..F.G.D,.F.E.
			! octave: -1 !
			B.D.B.D.B.D.E'.
			---
			---
		</pre>
	</li>
</ul>




















