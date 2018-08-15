# Musika - The Music Programming Language

## Musika Features

*   Add the following metadata to a song:
    *   song title
    *   song author(s)
    *   key signature
    *   time signature
    *   tempo
    *   octave
*   Change the metadata for a song in the middle of it to change aspects such as key, time, tempo, and octave
*   Define patterns/riffs that can be used in songs (like functions in a program)
*   Define chords that can modularize complex macro-notes (chords)
*   Take a segment of music and have it repeat a number of times
*   Layer music segments on top of each other
*   Write notes in the specified key, give them a length, and keep the octave at the base or shift it higher or lower
*   Write comments in the code to make developer/composer notes
*   Import 0 or more Musika files to utilize their patterns/music in a file

## How to program in Musika

### Basic Musika Anatomy

Every Musika file is divided into 4 sections:

*   Accompaniment Section
*   Info Section
*   Pattern Section
*   Music Section

Each of these sections are separated by three dashes (---) on a new line. The accompaniment section is optional, but the other three are required.

Also note that single-line comments are specified with an _&_ and multi-line comments begin with a _=>_ and end with _<=_

<pre>
<i>& This is a single-line comment!</i>
<i>
& ... Code before multi-line comment ...

=>
 Everything between the
 arrows is part of the
 multi-line comment!
<=

& ... Code before multi-line comment ...
</i>
</pre>

These comments are ignored by the compiler, so you can add them for specification/clarification within your Musika code.

### The Accompaniment Section

The following is an example of an accompaniment (another Musika file):

<pre>
<b>accompany</b> [musika_file] <b>name</b> new_accompaniment
</pre>

You specify a Musika file within your directory (or as a part of the Musika library) solely by the name and/or relative path (NO FILE EXTENSION)

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

#### author: 
Who wrote the song/piece.
This is specified by either a literal string:
<pre>
<b>author</b>: "Song Author"
</pre>
Or by a reference from an accompaniment:
<pre>
<b>author</b>: accompaniment_ref_Name
</pre>

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
Note that each coauthor is separated by a space and a comma (", "). This is very important when parsing the metadata for coauthors, so please use this convention!

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
Key signatures:
    - Cmaj
    - Gmaj
    - Dmaj
    - Amaj
    - Emaj
    - Bmaj
    - F#maj
    - C#maj
    - Fmaj
    - Bbmaj
    - Ebmaj
    - Abmaj
    - Am
    - Em
    - Bm
    - F#m
    - C#m
    - Dm
    - Gm
    - Cm
    - Fm
    - Bbm
    - Ebm
    - Abm

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

The code that goes into a pattern is written exactly the same way as the __music__ section, which is discussed below.

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

This code will result in a contextual error because the _p1_ pattern is called before it is defined. Musika stores patterns one-by-one. Therefore, in order to reference a pattern, it must have been fully defined first.

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

A chord is defined a little bit differently than a pattern:

<pre>
<b>chord</b> chord_name <b>is</b> A;B'';C,
</pre>

Note that the chord body consists of individual notes followed by an optional set of  _,_ or _'_.

Each note by default is set to the octave specified in the info section. However, you can change each note's octave using _,_ and _'_.

Every _,_ represents 1 octave lower than the base octave.

Every _'_ represents 1 octave higher than the base octave.

The note _F'''_ will be 3 octaves higher than the base.

The note _G,,_ will be 2 octaves lower than teh base.

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

All accompaniments, info, patterns, and chords will be saved, but no notes will be generated to play. This can be done for files that are made solely to contain a library of patterns and chords (files that are always meant to be accompaniments). However, no music will be played if you run the file directly.

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

The number within the parenthesis can be any integer. It represents the number of times you would like to repeate the section of music within the braces.

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
<b>layer</b>(part_1)
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
* A carrot (^), which is a shorthand repeat symbol
* A change in music info

###### Note
A note is exactly what it sounds like. A note is any of the following letters: A, B, C, D, E, F, G.

A note can contain an accidental: A# (A sharp), Bb (B flat), C* (C double-sharp), Dbb (D double flat), E$ (E natural).

A note can also be assumed to be sharp or flat by default based on the current key signature. For example, if the key signature is _Emaj_, then both _D_ and _D#_ will play the same note. However, in _Em_, _D_ will play a D natural while a _D#_ will play a D sharp.

To get a D natural in a key with a D sharp, an accidental must be set (D$ will _always_ play an D natural regardless of the key signature).

A note's octave can be changed much like in a chord (see chord definition) where the _,_ lowers the note from the base octave by 1 and the _'_ raises the note from the base octave by 1. For example, _G''',',,,,_ is 1 octave lower than the base. With no _,_ or _'_, the note will play at the base octave by default.

A note must also be followed by a series of dots (.). The dots represent the number of base beats to play the note.

For example, if the __time__ is set to _common_, (4/4) time, then _A..._ will be played for 3 quarter notes long. If the __time__ is set to 8 / 8, then that same note specification will be played for 3 eighth notes instead of quarter notes.

###### Callback
A callback is a reference to a local pattern or chord (within the file), a pattern or chord from an accompaniment file, or the main music from an accompaniment file.

A callback to a local pattern or chord is simply the name of the pattern or chord:

<pre>
<i>...Accompaniment Section...</i>
---
<i>...Info Section...</i>
---
<b>chord</b> chord_name <b>is</b> A;B'';C,

<b>pattern</b> [p1]:
    <i>...Music Code...</i>
---
p1
chord_name....
---

</pre>

In this example, the main music section is first calling back the _p1_ pattern. Therefore, the music section of the _p1_ pattern will be played.

Next, the _chord\_name_ chord is called back, so it will be played next. Unlike a pattern or accompaniment main music section, a chord does not have a set length. Therefore, just like a note, a set of dots must follow the chord callback to specify the number of main beats the chord should be played for.

In the above example, if the __time__ is set to _cut_, then the _chord\_name_ chord will be played for 4 half notes.

###### Carrot

The carrot (^) is a shorthand repeat symbol. It _cannot_ be the first riff element of _any_ music section (including patterns).
The carrot is followed by an integer, and it repeats the most recently played note that number of times.

For example:
<pre>
<i>& the time was set to common</i>
A. ^ 4
E....
d_minor_chord.. ^ 2
pattern_call ^ 3
</pre>

This code will play the A note 1 quarter note long 4 times. After that, it will play an E note 4 quarter notes long. Then it will play the _d\_minor\_chord_ chord for 2 quarter notes long 2 times. Finally, it will play the last note in the _pattern\_call_ pattern however long it played last 2 additional times (3 times total).

You can see why the carrot cannot begin a section. It must have a note or chord to repeat, which is set to the last note to play before the carrot appears.

Also note that the spacing is optional, all examples in this document represent the conventions. However, the following section...
<pre>
<i>& the time was set to common</i>
A. 
    ^             4
E....
d_minor_chord..^2
</pre>

... or any other spacing format is valid.

###### Change In Music Info
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

This will change the time signature from the current one to common time. If the time is the same, this statement will do nothing.

On top of this, the base beat can be changed without changing the actual time signature by inputting a single positive number:
<pre><b>!</b> <b>time:</b> <i>8</i> <b>!</b></pre>

If the original time signature was _common_, then the above statement will set the base beat to the eighth note while automatically adjusting the beats per measure to keep the same relative time signature. In this case, it will convert 4 / 4 to 8 / 8.

###### Tempo
Changing the tempo can be done just like setting it in the first place:
<pre><b>!</b> <b>tempo:</b> <i>2 = 66</i> <b>!</b></pre>

This will change the tempo from the current one to the tempo of half note equals 66 beats per measure. If the tempo is the same, this statement will do nothing.

On top of this, the tempo can be changed relatively to the original tempo by inputting a single integer:
<pre><b>!</b> <b>tempo:</b> <i>3</i> <b>!</b></pre>

This statement will make the current tempo 3 times faster than the original tempo. If the number were negative (e.g. -3), then the statement will make the current tempo 3 times slower than the original tempo.

###### Octave
Changing the octave can be done just like setting it in the first place:
<pre><b>!</b> <b>octave:</b> <i>4</i> <b>!</b></pre>

This will change the octave from the current one to 4.

On top of this, the octave can be changed relatively ot the original octave by inputting a single integer with a preceding sign:
<pre><b>!</b> <b>octave: </b> <i>-1</i> <b>!</b></pre>

This statement will decrement the current octave by 1. Setting it to +1 will increase the current octave by 1.

### Closing
_ALL Musika files must end in a newline character!_

There you have it! A Musika program is simply a collection of these components. Below are some sample programs so you can see collections of the components put together.

### Sample Programs

#### example1.ka

<pre>
<b>accompany</b> [example2] <b>name</b> rhythm
<b>accompany</b> [example3] <b>name</b> pattern_lib
<b>---</b>
<b>title</b>: "High Noon Chorus Lead"
<b>author</b>: pattern_lib
<b>coauthors</b>: pattern_lib
<b>key</b>: pattern_lib
<b>time</b>: pattern_lib
<b>tempo</b>: pattern_lib
<b>octave</b>: 5
<i>=&gt;</i>
<i>Any notes on the piece can be written here in a multi-line comment</i>
<i>&lt;=</i>
<b>---</b>
<i>& this is a single-line comment</i>
<b>pattern</b> [chorus1]:
E....A..G..F.G.D,..A..G.F. <i>& this is the lead pattern for high noon</i>
<i>& this is another comment</i>
<b>---</b>
<b>layer</b>(rhythm)
<b>repeat</b>(2) {
    chorus1
    pattern_lib>chorus2
    pattern_lib&gt;chorus_end
}
<b>---</b>

</pre>

#### example2.ka

<pre>
<b>accompany</b> [example3] <b>name</b> lib
<b>accompany</b> [chords] <b>name</b> c
<b>---</b>
<b>title</b>: "High Noon Chorus Rhythm"
<b>author</b>: lib
<b>coauthors</b>: lib
<b>key</b>: lib
<b>time</b>: lib
<b>tempo</b>: lib
<b>octave</b>: 2
<i>=&gt;</i>
    <i>This is the rhythm part of High Noon</i>
<i>&lt;=</i>
<b>---</b>
<b>pattern</b> [chorus]:
c>E5...c>B5..c>B5.c>B5.c>B5.
<b>---</b>
chorus^2^3
c>E8.^4 c>B8.^4 c>C8.^4 c>D8.^4
<b>---</b>

</pre>

#### exapmle3.ka

<pre>
<b>title</b>: "High Noon Chorus Patterns"
<b>author</b>: "Benjamin Ladick"
<b>coauthors</b>: "Masking The Fallen"
<b>key</b>: Em
<b>time</b>: common
<b>tempo</b>: 4 = 144
<b>octave</b>: 4
<b>---</b>
<b>pattern</b> [chorus2]:
E.B,.D,.E.A..G..F.G.D..B.A.G.F.
<b>pattern</b> [chorus_end]:
G..F..F.G.D,.F.E.
! octave: -1 !
B.D.B.D.B.D.E'.
<b>---</b>

<b>---</b>

</pre>

### Happy Hacking!!!
