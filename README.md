# Musika - The Music Programming Language

The purpose of the Musika programming language is to provide a quick and easy way of writing music ideas and listening to them. Musika takes a file containing Musika code in the form of text (".ka" file extension) and uses a 2-step conversion process to turn code into an audible sound file (".wav"), playable in any audio player that accepts ".wav".

## Table of Contents
* Musika Features
* Compiler Binary Use
* Musika Programming Guide
    * Comments
    * Basic Musika Anatomy
    * The Accompaniment Section
        * **name** Convention
    * The Info Section
        * title:
        * author: 
        * coauthors: 
        * key: 
        * time: 
        * tempo: 
        * octave: 
    * The Pattern Section
        * Chord Definition
    * The Music Section
        * Music Elements
            * Functions
                * repeat
                * layer
            * Riffs
                * Note
                * Callback
                * Caret
            * Change In Music Info
                * Key
                * Time
                * Tempo
                * Octave
    * Musika Standard Library
        * scale
        * doublestops
        * powerchords
        * x_chords
    * Closing
    * Sample Programs
        * example_1.ka
        * example_2.ka
        * example_3.ka
        * example_4.ka
* Musika IDE Guide
    * Musika IDE as an Editor
    * Syntax Highlighting
    * Musika Compilation
    * Building a Musika File
    * From Binary to Audible WAV
    * Bypassing the "Build Note Sheet" Option

## Musika Features

The following indicate the key features of the Musika programming language:

*   Add metadata to a song:
    *   song title
    *   song author(s)
    *   key signature
    *   time signature
    *   tempo
    *   base octave level
*   Write notes in the specified key, give them a length, and adjust the octave level relative to the base
*   Define patterns/riffs that can package important phrases and be used as many times as desired in songs (like functions in a program)
*   Define chords that can modularize complex macro-notes (chords)
*   Change the metadata for a song in the middle of it to change aspects such as key, time, tempo, and octave
*   Take a note or segment of music and have it repeat a specified number of times
*   Layer music segments on top of each other to create harmonies
*   Write comments in the code to make developer/composer notes
*   Import other Musika files to utilize their patterns/music in a file (custom or from standard library)

## Compiler Binary Use
Use "compiler.exe" in a command line in order to use the compiler by itself. The binary receives 1 or 2 arguments
* Name of file to compile
* -d flag for double compilation (optional)

Be sure to specify the file extension in the file argument! This will be the file to compile. If the file is of extension ".ka", the binary will convert the text in that file into a note sheet binary. This binary will be stored in the same directory with the same file name and a ".mkc" file extension. All files specified as arguments are assumed to be in the same directory.

If a note sheet is passed in as an argument (".mkc" file extension), the compiler binary will convert that note sheet into an audible ".wav" file. It will store this file in the same directory with the same name and an extension of ".wav". This wav file can be played in any ".wav" audio player.

The "-d" flag can be added before or after the file argument. This can only be used for ".ka" files. If this flag is set, the specified file will first be converted into a ".mkc" note sheet binary, and that binary will then immediately be converted into an audible ".wav" file (both files will be available after the operation). This essentially combines the two compilation steps into one.

## Musika Programming Guide

### Comments
Single-line comments are specified with an _&_ and multi-line comments begin with a _=>_ and end with _<=_

<pre>
<i>& This is a single-line comment!</i>
<i>
& ... Code before multi-line comment ...

=>
 Everything between the
 arrows is part of the
 multi-line comment!
<=

& ... Code after multi-line comment ...
</i>
</pre>

These comments are ignored by the compiler, so you can add them for specification/clarification within your Musika code anywhere you like!

### Basic Musika Anatomy

Every Musika file is divided into 4 sections:

*   Accompaniment Section
*   Info Section
*   Pattern Section
*   Music Section

Each of these sections are separated by three dashes (---) on a new line. The accompaniment section is optional, but the other three are required.

### The Accompaniment Section

The following is an example of an accompaniment (another Musika file):

<pre>
<b>accompany</b> [musika_file] <b>name</b> new_accompaniment
</pre>

You specify a Musika file within your directory (or as a part of the Musika library) solely by the name and/or relative path (NO FILE EXTENSION).

To accompany any created Musika files, specify the filename/relative path in between brackets (\[\]). Note that relative paths can only go forward, not backward (e.g. \[dir1/file\] is okay, but \[../file\] is NOT accepted).

To accompany a file from Musika's standard library, specify the standard library name in between brackets AND underscores (e.g. \[\_filename\_\]). This means that any custom Musika files which begin AND end in underscores cannot be accompanied in other files, as they will be mistaken for standard library files.

This statement will import the specified Musika file and it can be referenced later in the file using the name specified after the **name** keyword.

#### name Convention
* The name must be a minimum of 1 character in length and has no set maximum
* The first character in the name must be an alphabetic letter (a-z or A-Z), an underscore (_), or a dollar sign ($).
* All following characters must also be alphabetic letters, underscores, dollar signs, or any numeric digits (0-9).

Each accompaniment specification must be specified on its own line. After that, place three dashes (---) at the bottom line to enter the info section:

<pre>
<b>accompany</b> [file1] <b>name</b> one
<b>accompany</b> [file2] <b>name</b> two
---
<b>title</b>: "Song Title"
<b>author</b>: "Song Author"
<b>coauthors</b>: "Song coauthors"
<b>key</b>: Cmaj                       <i>& The info section is here</i>
<b>time</b>: 4 / 4
<b>tempo</b>: 4 = 60
<b>octave</b>: 4
---
</pre>

Note the bottom three dashes used to enter the patterns section afterwards.

### The Info Section
The following are specified in the info section:

#### title:
The title of the song/piece.
This is specified by either a literal string:
<pre>
<b>title</b>: "Song Title"
</pre>
Or by a reference from an accompaniment:
<pre>
<b>title</b>: accompaniment_ref_Name
</pre>
The title has no effect on the output of a song.

#### author: 
Name of the author of the song.
This is specified by either a literal string:
<pre>
<b>author</b>: "Song Author"
</pre>
Or by a reference from an accompaniment:
<pre>
<b>author</b>: accompaniment_ref_Name
</pre>
The author has no effect on the output of a song.

#### coauthors: 
This is an _optional_ section that specifies any extra authors of the piece.
This is specified by either a literal string:
<pre>
<b>coauthors</b>: "Author 1, Author 2, Author 3"
</pre>
Or by a reference from an accompaniment:
<pre>
<b>coauthors</b>: accompaniment_ref_Name
</pre>
Note that each coauthor is separated by a space and a comma (", "). If the coauthors are not listed this way, the names of each coauthor may be split or conjoined in an undesired fashion.
The coauthors have no effect on the output of a song.

#### key: 
Key signature of the song/piece.
This is specified by either a key signature keyword:
<pre>
<b>key</b>: <i>Cmaj</i>
</pre>
Or by a reference from an accompaniment:
<pre>
<b>key</b>: accompaniment_ref_Name
</pre>
Supported key signatures:
* Cmaj - all notes are natural by default
* Gmaj - F is F#
* Dmaj - F is F#, C is C#
* Amaj - F is F#, C is C#, G is G#
* Emaj - F is F#, C is C#, G is G#, D is D#
* Bmaj - F is F#, C is C#, G is G#, D is D#, A is A#
* F#maj - F is F#, C is C#, G is G#, D is D#, A is A#, E is E#
* C#maj - F is F#, C is C#, G is G#, D is D#, A is A#, E is E#, B is B#
* Fmaj - B is Bb
* Bbmaj - B is Bb, E is Eb
* Ebmaj - B is Bb, E is Eb, A is Ab
* Abmaj - B is Bb, E is Eb, A is Ab, D is Db
* Am - Equivalent to Cmaj. Using a minor key is used to communicate feel to other users, but otherwise has the same effect as major keys on the output of the song
* Em - Equivalent to Gmaj
* Bm - Equivalent to Dmaj
* F#m - Equivalent to Amaj
* C#m - Equivalent to Emaj
* Dm - Equivalent to Fmaj
* Gm - Equivalent to Bbmaj
* Cm - Equivalent to Ebmaj
* Fm - Equivalent to Abmaj
* Bbm - Equivalent to C#maj
* Ebm - Equivalent to F#maj
* Abm - Equivalent to Bmaj

#### time:
The time signature of the piece.
This is specified literally:
<pre>
<b>time</b>: 6 / 8
</pre>
Using _common_ (4/4) or _cut_ (2/2) as a shorthand:
<pre>
<b>time</b>: <i>common</i>
</pre>
Or by a reference from an accompaniment:
<pre>
<b>time</b>: accompaniment_ref_Name
</pre>
If specified literally, the first number specifies the beats per measure and the second number is the base beat. So 6 / 8 means that there are 6 8th notes per measure and 4 / 4 specifies that there are 4 quarter notes per measure.
Because Musika does not specify time signatures, the top number of the time signature has no effect on the output of the song. It should be used solely to inform other users of the number of beats per measure. The bottom number does have an effect on the output of the song, as it changes the _base beat_ of the song, affecting the type of note each "dot" represents. More on that later.

#### tempo:
The tempo of the piece.
This is specified literally:
<pre>
<b>tempo</b>: 4 = 144
</pre>
Or by a reference from an accompaniment:
<pre>
<b>tempo</b>: accompaniment_ref_Name
</pre>
If specified literally, the first number specifies the base beat and the second number specifies the base beats per minute. So 4 = 144 means that there are 144 quarter notes per minute, and 2 = 60 means that there are 60 half notes per minute.

#### octave:
The base octave
This is specified literally:
<pre>
<b>octave</b>: 4
</pre>
Or by a reference from an accompaniment:
<pre>
<b>octave</b>: accompaniment_ref_Name
</pre>
If specified literally, this is the default octave for each note specified in patterns and in the main music sheet.
Knowing this, be careful with the octaves of each written note. _C_ is the lowest note in all octaves; therefore, all notes specified in the piece will
be higher in pitch than the _C_ of the same octave by default.

This means that an A minor scale
<pre>
<i>& octave: 4</i>
A. B. C. D. E. F. G. A.
</pre>
Will not continuously increase in pitch because the _C_ will be lower than the _B_ and _A_ before it. The octave must be set to 1 higher than the base from the _C_ on:
<pre>
<i>& octave: 4</i>
A. B.
! octave: 5 !
C. D. E. F. G. A.
</pre>
or
<pre>
<i>& octave: 4</i>
A. B. C'. D'. E'. F'. G'. A'.
</pre>
Octave values are supported from 0 to 8 inclusive.

_Details on note specification later_

### The Pattern Section
The pattern section comes directly after the info section. It allows you to write named repeatable patterns that can be referenced in the music section.
Patterns are analogous to functions in normal programming (however, there are no parameters in patterns).

The pattern section also allows you to define chords, which are groups of notes played at the same time. The chords you define in the pattern section (or that are defined 
in an accompanied file) can be used in the music section.

This is the basic anatomy of a pattern definition:

<pre>
<b>pattern</b> [pattern_name]:
    <i>...Music code...</i>
</pre>

The code that goes into a pattern is written exactly the same way as the _music_ section, which is discussed later.

The pattern naming convention is the same as the accompaniment naming convention.

You can define as many patterns as you like. However, in order for one pattern to reference another, it must be defined first.
For example:

<pre>
<i>...Accompaniment Section...</i>
---
<i>...Info Section...</i>
---
<b>pattern</b> [p2]:
    <i>...Music Code...</i>
    p1

<b>pattern</b> [p1]:
    <i>...Music Code...</i>
---
<i>...Music Section...</i>
---

</pre>

This code will result in a contextual error because the _p1_ pattern is called before it is defined. Musika reads and compiles patterns one at a time in the order they were specified. Therefore, in order to reference a pattern, it must have been fully defined first.

Switching the definition order will result in the code successfully compiling.

<pre>
<i>...Accompaniment Section...</i>
---
<i>...Info Section...</i>
---
<b>pattern</b> [p1]:
    <i>...Music Code...</i>

<b>pattern</b> [p2]:
    <i>...Music Code...</i>
    p1
---
<i>...Music Section...</i>
---

</pre>

Because accompaniments are always defined before patterns, any pattern stored in an accompanied file can be referenced from any pattern in the current file.

#### Chord Definition

A chord is defined in the pattern section in the following way:

<pre>
<b>chord</b> chord_name <b>is</b> A;B'';C,
</pre>

Note that the chord body consists of individual notes followed by an optional set of  _,_ or _'_.

Each note by default is set to the octave specified in the info section. However, you can change each note's octave using _,_ and _'_.

Every _,_ represents 1 octave lower than the base octave.

Every _'_ represents 1 octave higher than the base octave.

The note _F'''_ will be 3 octaves higher than the base.

The note _G,,_ will be 2 octaves lower than the base.

The note _C','',',,,'_ will be the same octave as the base. This is because there is one _'_ for each _,_. These all cancel each other out. If there were one more _'_, then the note would be 1 octave higher than the base.

Each individual note in the chord is separated by a semicolon (;).

### The Music Section
The music section is the heart of a Musika program, and is also the most complex.

For each accompanied file, the music section can be considered its own pattern that can be referenced.

For the main file, the notes generated will come from the music section of that file.

The contents of a __pattern__ definition follows the same anatomy as the music section.

The simplest music section is a blank line:

<pre>
<i>...Accompaniment Section...</i>
---
<i>...Info Section...</i>
---
<i>...Pattern Section...</i>
---
<i>& There is nothing here but a comment (no comment required)</i>
---

</pre>

All accompaniments, info, patterns, and chords will be saved, but no notes will be generated to play. This can be done for files that are made solely to contain a library of patterns and chords (files that are always meant to be accompaniments). However, files with no music in the music section cannot be converted into ".wav" files.

The music section is just a set of _music elements_. These music elements make up the actual notes of the song.

#### Music Elements
There are 2 types of music elements:
* functions
* riffs

##### Functions
_functions_ are built-in Musika commands that accept a piece of music as input and manipulate the music as output. Sometimes literal music can be passed in and sometimes it must be in the form of a pattern.

Currently, there are 2 Musika functions:
* __repeat__
* __layer__

###### Repeat
__repeat__ can be used in the following form:
<pre>
<b>repeat</b>(4) {
    pattern_in_repeat
}
</pre>

Everything inside of the braces ({}) is another section of music (much like a pattern).

The number within the parenthesis can be any integer. It represents the number of times you would like to repeat the section of music within the braces.

In the example above, the music contained in the _pattern\_in\_repeat_ section will be repeated back-to-back 4 times.

###### Layer
__layer__ can be used in the following form:
<pre><b>layer</b>(pattern_in_layer)</pre>

The __layer__ function requires its argument to be in pattern form (__not__ a chord). It can also be the main music of an accompanied section.

The pattern passed in __layer__ function will be played _over_ the music that comes after it. This means that it will be played at the same time.

A useful way to think about this is that if you wanted a harmony to be played (for example, a G major scale and an E minor scale played together in harmony), you could define the E minor scale in a pattern and then call the pattern:

<pre>
<i>...Info Section...</i>
---
<b>pattern</b> [e_minor]:
    <i>...Music Code...</i>
---
part_1
<b>layer</b>(e_minor)
<i>...G Major Scale Code...</i>
---

</pre>

This code will play the harmony.

Remember, position is important. If the __layer__ reference were called _after_ the _G Major Scale Code_, then the E minor pattern will not be played until after the _G Major Scale Code_ is completed. In this case, you would get one after the other instead of the harmony played together.

In the below example:

<pre>
<b>accompany</b> [file] <b>name</b> acc
---
<i>...Info Section...</i>
---
<b>pattern</b> [part_1]:
    <i>...Music Code...</i>
---
part_1
<b>layer</b>(acc)
<i>...Rest of Music Section...</i>
---

</pre>

The main music of the _acc_ file will play on top of the rest of the music in the music section immediately after all of the music in _part\_1_ is played.
If the __layer__ were written before the _part\_1_ call, it would be played over _part\_1_ music.

##### Riffs

Much like music is made up of music elements, a riff is made up of riff elements.

A riff element can be any of the following:
* A note played for a set number of beats
* A "callback" (pattern reference, chord reference, or accompaniment pattern/chord reference)
* A caret (^), which is a shorthand repeat symbol
* A change in music info

###### Note
A note represents a frequency value. [This table was used to determine the frequencies of each note](https://pages.mtu.edu/~suits/notefreqs.html). A note is any of the following letters: A, B, C, D, E, F, G.

A note can contain an accidental: A# (A sharp), Bb (B flat), C* (C double sharp), Dbb (D double flat), E$ (E natural).

A note can also be assumed to be sharp or flat by default based on the current key signature. For example, if the key signature is _Emaj_, then both _D_ and _D#_ will play the same note. However, in _Em_, _D_ will play a D natural while a _D#_ will play a D sharp.

The accidentals add "rigidity" to your notes (i.e. make them independent of key signature). For example, to get a D natural in a key with a D sharp, an accidental must be set (D$ will _always_ play a D natural regardless of the key signature, whereas a D will give you a D, D#, or Db depending on the key).

A note's octave can be changed much like in a chord (see chord definition) where the _,_ lowers the note from the base octave by 1 and the _'_ raises the note from the base octave by 1. For example, _G''',',,,,_ is 1 octave lower than the base. With no _,_ or _'_, the note will play at the base octave by default.

A note must also be followed by a series of dots (.). The dots represent the number of _base beats_ to play the note. In 4 / 4 time, each dot represents a quarter note; in 2 / 2 time, each dot represents a half note; and in 6 / 8 time, each dot represents an eight note.

If the __time__ is set to _common_, (4/4) time, then _A..._ will be played for 3 quarter notes long. If the __time__ is set to 8 / 8, then that same note specification will be played for 3 eighth notes instead of quarter notes. Each dot represents one _base beat_ of the song. You can change the base beat of the song as much as desired to get the proper subdivision whenever necessary; more on that later.

###### Callback
A callback is a reference to a local pattern or chord (within the file), a pattern or chord from an accompaniment file, or the main music from an accompaniment file.

A callback to a local pattern, local chord, or accompaniment main music is simply the name of the pattern, chord, or accompaniment name respectively. To access a specific pattern or chord from an accompanied file, use the _>_ operator. Examples are shown here:

<pre>
<b>accompany</b> [song] <b>name</b> song
<i>...more accompaniments if desired...</i>
---
<i>...Info Section...</i>
---
<b>chord</b> chord_name <b>is</b> A;B'';C,

<b>pattern</b> [p1]:
    <i>...Music Code...</i>
---
p1
chord_name....
song
song>riff
song>song_chord....
---

</pre>

In this example, the main music section is first calling back the _p1_ pattern. Therefore, the music section of the _p1_ pattern will be played.

Next, the _chord\_name_ chord is called back, so it will be played next. Unlike a pattern or accompaniment main music section, a chord does not have a set length. Therefore, just like a note, a set of dots must follow the chord callback to specify the number of main beats the chord should be played for.

After that, the "song" accompaniment was referenced directly. Therefore, the main music specified in the "song.ka" file will be played.
Then the song's "riff" pattern is referenced using the _>_ operator. This indicates that you are playing the "riff" pattern from the "song" accompaniment.
Finally, the song's "song_chord" chord is referenced using the _>_ operator. This indicates that you are playing the "song_chord" chord coming from the "song" accompaniment. Just like local chords, you must specify the length of the chord with dots.

In the above example, if the __time__ is set to _cut_, then the _chord\_name_ chord will be played for 4 half notes.

###### Caret

The caret (^) is a shorthand repeat symbol. It _cannot_ be the first riff element of _any_ music section (including patterns).
The caret is followed by an integer, and it repeats the most recently played note that number of times.

For example:
<pre>
<i>& the time was set to common</i>
A. ^ 4
E....
d_minor_chord.. ^ 2
pattern_call ^ 3
</pre>

This code will play the A note 1 quarter note long 4 times. After that, it will play an E note 4 quarter notes long. Then it will play the _d\_minor\_chord_ chord for 2 quarter notes long 2 times. Finally, it will play the last note in the _pattern\_call_ pattern however long it played last 2 additional times (3 times total).

You can see why the caret cannot begin a section. It must have a note or chord to repeat, which is set to the last note to play before the caret appears.

Also note that the spacing is optional, all examples in this document represent the conventions. However, the following section...
<pre>
<i>& the time was set to common</i>
A. 
    ^             4
E....
d_minor_chord..^2
</pre>

... or any other spacing format is valid.

##### Change In Music Info
Within a riff, you can change the base music information that was specified in the __info__ section.

To do this, begin with a bang symbol (!). Then enter the name of the info to change followed by a colon (:):
* key
* time
* tempo
* octave

_Note that the title, author, and coauthors cannot be changed in the middle of the music section like this._

And then make your change

###### Key
Changing the key signature works the same way as setting it in the first place:
<pre><b>!</b> <b>key:</b> <i>Gm</i> <b>!</b></pre>

This will change the key signature from its current key to G minor for all music code below the change. If the key is the same, this statement will do nothing.

###### Time
Changing the time signature can be done just like setting it in the first place:
<pre><b>!</b> <b>time:</b> <i>common</i> <b>!</b></pre>

This will change the time signature from the current one to common time. If the base beat does not change, this statement will do nothing.

On top of this, the base beat can be changed directly inputting a single positive number:
<pre><b>!</b> <b>time:</b> <i>8</i> <b>!</b></pre>

If the original time signature was _common_, then the above statement will set the base beat to the eighth note while automatically adjusting the beats per measure to keep the same relative time signature. In this case, it will convert 4 / 4 to 8 / 8. This means that all of the "dots" above this change will still be quarter notes, because the original time signature was _common_. After _! time: 8 !_, each dot will now represent an eight note. This is how subdivisions can change as necessary.

Keep in mind that the _base beat_ value is a subdivision of a whole note. In other words, a base beat value of 1 represents 1 / 1 whole notes (i.e. a whole note); a base beat value of 4 represents 1 / 4 of a whole note (i.e. a quarter note); a base beat value of 3 represents 1 / 3 of a whole note (i.e. a whole note triplet). In order to determine the proper base beat value, think of the number of beats that go into a whole note. For example, to get quarter note triplets, know that a quarter note triplet is one third of a quarter note, and there are 4 quarter notes in a whole note. Therefore, the number of quarter note triplets in a whole note is 3 triplets per quarter note * 4 quarter notes per whole note = 12 quarter note triplets per whole note. This means that setting _time: 12_ will yield quarter note triplets.

###### Tempo
Changing the tempo can be done just like setting it in the first place:
<pre><b>!</b> <b>tempo:</b> <i>2 = 66</i> <b>!</b></pre>

This will change the tempo from the current one to the tempo of half note equals 66 beats per measure. If the tempo is the same, this statement will do nothing.

On top of this, the tempo can be changed relatively to the original tempo by inputting a single integer:
<pre><b>!</b> <b>tempo:</b> <i>3</i> <b>!</b></pre>

This statement will make the current tempo 3 times slower than the original tempo. If the number were negative (e.g. -3), then the statement will make the current tempo 3 times faster than the original tempo.

###### Octave
Changing the octave can be done just like setting it in the first place:
<pre><b>!</b> <b>octave:</b> <i>4</i> <b>!</b></pre>

This will change the octave from the current one to 4.

On top of this, the octave can be changed relative to the original octave by inputting a single integer with a preceding sign:
<pre><b>!</b> <b>octave:</b> <i>-1</i> <b>!</b></pre>

This statement will decrement the current octave by 1. Setting it to +1 will increase the current octave by 1.

### Musika Standard Library
As stated previously, accompaniments can be custom files, or standard library imports. To make a standard library import, the accompaniment name must be in between \_underscores\_. The following files are supported for the Musika standard library:

#### scale
<pre>
<b>accompany</b> [_scale_] <b>name</b> scale
---
<b>title</b>: scale             <i>& You can put your own title, author, and other metadata in here instead of referencing the library for that information,</i>
<b>author</b>: scale            <i>& but keep in mind that the core metadata that plays the notes (key signature, time, tempo, and octave) will not affect the output of the actual pattern.</i>
<b>coauthors</b>: scale
<b>key</b>: scale
<b>time</b>: scale
<b>tempo</b>: scale
<b>octave</b>: scale
---
</pre>
_note that "scale" does not have to the be the *name* of the accompaniment; this is an example of a name_

This library only contains the C major scale. Use it as a pattern if you want to!

#### doublestops
<pre>
<b>accompany</b> [_doublestops_] <b>name</b> ds
</pre>
_note that "ds" does not have to the be the *name* of the accompaniment; this is an example of a name_

This file contains typical double stops formed as Musika chords. It supports the following base notes from the Circle of Fifths:
* C
* G
* D
* A
* E
* B
* Fs (F#)
* Cs (C#)
* Ab/Gs (Ab/G#)
* Eb
* Bb
* F

The following types of double stops are supported for each of these notes:
* m2 - minor 2nd
* M2 - major 2nd
* m3 - minor 3rd
* M3 - major 3rd
* 4 - perfect 4th 
* T - triad
* 5 - perfect 5th
* m6 - minor 6th
* M6 - major 6th
* m7 - minor 7th
* M7 - major 7th
* 8 - octave 

To specify an octave, use an underscore and an octave value (supported 0 - 8). Keep in mind that 8 is the highest octave supported in the language, so if a doublestop requires an octave value higher than 8, it is not supported. For example, C major 8 at octave 8 is NOT supported because it requires C9, which is not a supported frequency.

Examples:
* Dm2_4 - D minor 2nd at octave 4 (D4, Eb4)
* EbT_6 - Eb triad at octave 6 (Eb6, A6)
* Cs8_1 - C# octave double stop at octave 1 (C#1 and C#2)

_Note: Ab and G# are paired together. Ab double stops are supported for major intervals and G# is supported for minor intervals. For example, AbM6_5 is supported but Abm6_5 is not supported. Conversely, Gsm7_4 is supported by GsM7_4 is not supported. Perfect, octave, and triad intervals are supported for both notes._

#### powerchords
<pre>
<b>accompany</b> [_powerchords_] <b>name</b> pc
</pre>
_note that "pc" does not have to the be the *name* of the accompaniment; this is an example of a name_

This file contains "power chords" in the form of Musika chords. "Power chords" for the sake of this library is defined as the root note, perfect 5th, and octave (e.g. C 4 power chord is C4, G4, C5). It supports the following base notes from the Circle of Fifths:
* C
* G
* D
* A
* E
* B
* Fs (F#)
* Cs (C#)
* Ab
* Gs (G#)
* Eb
* Bb
* F

Examples:
* CP_3 - C3 power chord (C3, G3, C4)
* EbP_6 - Eb6 power chord (Eb6, Bb6, Eb7)
* GsP_1 - G#1 power chord (G#1, D#2, G#2)

### x_chords
<pre>
<b>accompany</b> [_c_chords_] <b>name</b> c
<b>accompany</b> [_gsab_chords_] <b>name</b> gsab
<b>accompany</b> [_bb_chords_] <b>name</b> bb
<b>accompany</b> [_a_chords_] <b>name</b> a
</pre>
_note that each of the names do not have to the be the *name*s of the accompaniments; these are examples of names_

These library files are used to add more complex chords to the song.

Supported files:
* \_c_chords\_ - C chords
* \_cs_chords\_ - C# chords
* \_d_chords\_ - D chords
* \_eb_chords\_ - Eb chords
* \_e_chords\_ - E chords
* \_f_chords\_ - F chords
* \_fs_chords\_ - F# chords
* \_g_chords\_ - G chords
* \_gsab_chords\_ - G# and Ab chords
* \_a_chords\_ - A chords
* \_bb_chords\_ - Bb chords
* \_b_chords\_ - B chords

The following chord types are supported for each of these files:
* M - major (root, major 3rd, perfect 5th)
* M6 - major 6th (root, major 3rd, perfect 5th, major 6th)
* M7 - major 7th (root, major 3rd, perfect 5th, major 7th)
* M8 - major with octave (root, major 3rd, perfect 5th, octave)
* M9 - major 9th (root, major 3rd, perfect 5th, major 7th, major 9th)
* M11 - major 11th (root, major 3rd, perfect 5th, major 7th, major 9th, perfect 11th)
* M13 - major 13th (root, major 3rd, perfect 5th, major 7th, major 9th, perfect 11th, major 13th)
* m - minor (root, minor 3rd, perfect 5th)
* m6 - minor 6th (root, minor 3rd, perfect 5th, major 6th)
* m7 - minor 7th (root, minor 3rd, perfect 5th, minor 7th)
* m8 - minor with octave (root, minor 3rd, perfect 5th, octave)
* m9 - minor 9th (root, minor 3rd, perfect 5th, minor 7th, major 9th)
* m11 - minor 11th (root, minor 3rd, perfect 5th, minor 7th, major 9th, perfect 11th)
* m13 - minor 13th (root, minor 3rd, perfect 5th, minor 7th, major 9th, perfect 11th, minor 13th)
* aug - augmented (root, major 3rd, sharp 5th)
* dim - diminished (root, minor 3rd, flat 5th)
* dom7 - dominant 7th (root, major 3rd, perfect 5th, minor 7th)
* dim7 - diminished 7th (root, minor 3rd, flat 5th, flat minor 7th)
* hdim7 - half diminished 7th (root, minor 3rd, flat 5th, minor 7th)
* sus2 - suspended 2nd (root, major 2nd, perfect 5th)
* sus2_8 - suspended 2nd (root, major 2nd, perfect 5th, octave)
* sus4 - suspended 4th (root, perfect 4th, perfect 5th)
* sus4_8 - suspended 4th (root, perfect 4th, perfect 5th, octave)

_Note that G# and Ab chords are in the same file._

Add octave modifiers (this is _required_) to specify the base octave of each chord with an underscore (\_) and an octave number.

The following examples comply with this accompaniment section:
<pre>
<b>accompany</b> [_c_chords_] <b>name</b> c
<b>accompany</b> [_cs_chords_] <b>name</b> cs
<b>accompany</b> [_d_chords_] <b>name</b> d
<b>accompany</b> [_eb_chords_] <b>name</b> eb
<b>accompany</b> [_e_chords_] <b>name</b> e
<b>accompany</b> [_f_chords_] <b>name</b> f
<b>accompany</b> [_fs_chords_] <b>name</b> fs
<b>accompany</b> [_g_chords_] <b>name</b> g
<b>accompany</b> [_gsab_chords_] <b>name</b> gsab
<b>accompany</b> [_a_chords_] <b>name</b> a
<b>accompany</b> [_bb_chords_] <b>name</b> bb
<b>accompany</b> [_b_chords_] <b>name</b> b
</pre>

Examples:
* c>M_7 - C major (C7, E7, G7)
* cs>M6_2 - C# major 6th (C#2, E#2, G#2, A#2)
* d>M7_3 - D major 7th (D3, F#3, A3, C#4)
* eb>M8_4 - Eb major with octave (Eb4, G4, Bb4, Eb5)
* e>M9_5 - E major 9th (E5, G#5, B5, D#6, F#6)
* f>M11_6 - F major 11th (F6, A6, C7, E7, G7)
* fs>M13_5 - F# major 13th (F#5, A#5, C#6, E#6, G#6, B6)
* g>m_7 - G minor (G7, Bb7, D8)
* gsab>m6_0 - G# minor 6th (G#0, B0, D#1, F1)
* a>m7_1 - A minor 7th (A1, C2, E2, G2)
* bb>m8_2 - Bb minor with octave (Bb2, Db3, F3, Bb3)
* b>m9_3 - B minor 9th (B3, D4, F#4, A4, C#5)
* c>m11_4 - C minor 11th (C4, Eb4, G4, Bb4, D5, F5)
* cs>m13_5 - C# minor 13th (C#5, E5, G#5, B4, D6, F#6, A6)
* d>aug_6 - D augmented (D6, F#6, A#6)
* eb>dim_6 - Eb diminished (Eb6, Gb6, Bbb6)
* e>dom7_7 - E dominant 7th (E7, G#7, B7, D8)
* f>dim7_1 - F diminished 7th (F1, Ab1, Cb1, Ebb2)
* fs>hdim7_2 - F# half diminished 7th (F#2, A2, C2, E2)
* g>sus2_3 - G suspended 2nd (G3, A3, D4)
* gsab>sus2_8_4 - Ab suspended 2nd with octave (Ab4, Bb4, Eb5, Ab5)
* a>sus4_5 - A suspended 4th (A5, D6, E6)
* bb>sus4_8_6 - Bb suspended 4th with octave (Bb6, Eb7, F7, Bb7)

### Closing
_ALL Musika files must end in a newline character!_

There you have it! A Musika program is simply a collection of these components. Below are some sample programs so you can see collections of the components put together.

### Sample Programs

#### example1.ka
<pre>
<b>title</b>: "Example 1"
<b>author</b>: "Musika"
<b>key</b>: Cmaj
<b>time</b>: common
<b>tempo</b>: 4 = 144
<b>octave</b>: 4
<b>---</b>
<i>& Simple song: no patterns needed!</i>
<b>---</b>
! <b>time</b>: 8 !
<b>repeat</b>(2) {
 C. E. G. C'. G. E. C. G,.
}

! <b>octave</b>: -1 !
repeat(2) {
 A. C'. E'. A'. E'. C'. A. E.
}

! <b>octave</b>: +1 !
! <b>key</b>: Gmaj !
G. A. B. ! octave: +1 ! C. D. E. F. G. B. G. D. G. B,. D. B,. G,.

! <b>key</b>: Fmaj !
! <b>octave</b>: -1 !
F. A. C'. F'. C'. A. F. D.

! <b>key</b>: Emaj !
E. G. B. E'. B. G. E. B,.

! <b>key</b>: Am !
! <b>time</b>: 1 !
! <b>octave</b>: -1 !
A.
<b>---</b>
</pre>

#### example2.ka

<pre>
<b>accompany</b> [_powerchords_] <b>name</b> pc
<b>---</b>
<b>title</b>: "Example 2"
<b>author</b>: "Musika"
<b>coauthors</b>: "Coauth1, coauth2, coauth3"
<b>key</b>: Cmaj
<b>time</b>: common
<b>tempo</b>: 4 = 120
<b>octave</b>: 4
<i>
=>
	This is a collection of patterns. No main music here, so it is used for accompaniments!
<=
</i>
<b>---</b>
<b>pattern</b> [scale_chords]:
! <b>time</b>: 1 !
pc>CP_3.
pc>GP_3.
pc>CP_4.
pc>GP_4.
pc>CP_5.
pc>GP_4.
pc>CP_4.
pc>GP_3.
pc>CP_3.

<b>pattern</b> [scale_harmony]:
E.. F. G. A. B. C'. D'.

! <b>octave</b>: +1 !
E.. F. G. A. B. C'. D'. E'..
D'. C'. B. A. G. F. E.. D. C.

! <b>octave</b>: -1 !
B. A. G. F. E..
<b>---</b>

<b>---</b>
</pre>

#### example3.ka

<pre>
<b>accompany</b> [_scale_] <b>name</b> scale <i>& This references the standard library file "scale.ka"</i>
<b>accompany</b> [example2] <b>name</b> ex2 <i>& This references a file in the same directory "example2.ka"</i>
<b>---</b>
<b>title</b>: "Example 3"
<b>author</b>: "Musika"
<b>key</b>: scale
<b>time</b>: scale
<b>tempo</b>: scale
<b>octave</b>: scale
<b>---</b>

<b>---</b>
scale <i>& This is the main music from the "scale" standard library file! Let's first play it by itself</i>

layer(ex2>scale_chords) <i>& Play example 2's "scale_chords" pattern on top of the scale</i>
scale

layer(ex2>scale_harmony) <i>& Now let's play the scale again, but give it a harmony!</i>
scale
<b>---</b>
</pre>

#### example4.ka

<pre>
<b>title</b>: "Example 4"
<b>author</b>: "Musika"
<b>key</b>: Cmaj
<b>time</b>: common
<b>tempo</b>: 4 = 90
<b>octave</b>: 4
<b>---</b>
<b>chord</b> Cmajguitar <b>is</b> C,;E,;G,;C;E
<b>chord</b> Dmajguitar <b>is</b> D,;A,;D;F#
<b>chord</b> Gmajguitar <b>is</b> G,,;B,,;D,;G,;B,;G

<b>pattern</b> [rhythm]:
! <b>time</b>: 8 !
Cmajguitar. ^ 8

! <b>time</b>: 8 !
Gmajguitar. ^ 8

! <b>time</b>: 8 !
Dmajguitar. ^ 8

! <b>time</b>: 1 !
Cmajguitar.

pattern [lead]:
! <b>time</b>: 16 !
! <b>octave</b>: +1 !
C. D. E. F. G. C'. E. C'. E. C'. D. B. A. B. C'. F'.

! <b>key</b>: Gmaj !
G'. B. D'. B. A. B. E. F. G. F. G. D,. E. F. G. F.

! <b>key</b>: Dmaj !
D. A,. D. A,. F. A,. E. A,. D. C,. D. F. D. A. F,. A. 

! <b>key</b>: Cmaj !
C. E. G. E. C. E. G. C'. E'. G'. E'. B. D'. C'. A. B.

! <b>time</b>: 1 !
C.

<b>pattern</b> [main]:
<b>layer</b>(rhythm)
lead

<b>---</b>
main main
<b>---</b>
</pre>

### Musika IDE Guide

The following shows the basic features of the Musika IDE and how it can be used to create and manage Musika files.

#### Musika IDE as an Editor
At its core, the Musika IDE behaves as a basic editor specific for Musika files. The "File" menu item provides basic features including:
* New - Clear the editor and start an unsaved session
* Open - Open an existing Musika file
* Save - Save the current Musika file as it is
* Save As - Save a copy the current Musika file with a new name
* Exit - Quit the IDE

#### Syntax Highlighting
By default, the Musika IDE does not do any kind of styling to Musika code. However, syntax highlighting can be toggled on and off using _View -> Toggle Syntax Highlighting_. It should be noted, however, that syntax highlighting takes a significant performance drain on the editor, and it is recommended to avoid use of this feature for any file that is not small.

#### Musika Code Compilation
Musika code undergoes a 2-step compilation process:
1. Musika text is saved in a *.ka file
2. The contents of a *.ka file is built into a "note sheet" binary representation of the code. This is stored in a *.mkc (compiled Musika) file.
3. The binary data in a *.mkc file is converted into a *.wav file. This is the final form of the program and it can be played on any media player that supports *.wav

In order to turn Musika code into a playable *.wav file, it must first be saved and stored somewhere in a *.ka file. You can use the IDE to create, edit, and save a .ka file or you can use a separate text editor.

#### Building a Note Sheet Binary
To build a Musika file, it must first be opened in the Musika IDE. The _Build -> Build Note Sheet_ menu item will compile the file into a note sheet binary. A file with an identical name and an extension of *.mkc will be created and stored in the same directory as your *.ka file. This will only happen if the code is free of any syntax or contextual errors. If there are any present, you must fix them before a note sheet can be generated.

#### From Binary to Audible WAV
To turn a note sheet binary into a playable *.wav file, first make sure you have the corresponding *.ka file open in the Musika IDE if it is not already open. Then the _Build -> Build WAV File_ menu item will convert that note sheet binary into an audible *.wav file. This file can be opened in any media player that supports *.wav. However, you can also play the song directly in the Musika IDE using the _Play -> Play Song_ menu item. This will play the *.wav representation of the *.ka file you are currently viewing in the editor, if there is one. You can stop playing a song that is playing using the _Play -> Stop Song_ menu item; and _Play Song_ will play the song back from the beginning.

#### Bypassing the "Build Note Sheet" Option
Note that if you select _Build -> Build WAV File_ before _Build -> Build Note Sheet_, the build wave file operation will build the note sheet binary first, and then build the *.wav file; but it will cancel the process if there are any syntax or contextual errors in the Musika code. Be careful bypassing the _Build Note Sheet_ option though; because if a *.mkc file corresponding to your *.ka file is present, the _Build WAV File_ will create a *.wav file based on that binary without rebuilding it, potentially using an out-of-date binary (i.e. it only builds a note sheet binary if there is not one already present). If you have a note sheet binary present, but you've updated your Musika code, be sure to either delete the existing binary or use the _Build -> Build Note Sheet_ option first before building the *.wav file.

### Happy Hacking!!!
