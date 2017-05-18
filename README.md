Invisible Character Visualizer Visual Studio Extension
======================================================
[![CodeFactor](https://www.codefactor.io/repository/github/shaneray/shanespace.visualstudio.invisiblecharactervisualizer/badge)](https://www.codefactor.io/repository/github/shaneray/shanespace.visualstudio.invisiblecharactervisualizer)

Visual Studio Gallery
----------------------
https://marketplace.visualstudio.com/items?itemName=ShaneRay.InvisibleCharacterVisualizer

Motivation
----------
I ran into an issue where some code was not running as expected.  I hopped on StackOverflow and posted my question.

https://stackoverflow.com/questions/42423320/getmethod-returning-null

The question was quickly answered.  The issue ended up being an "Invisible" character hidden away in my string.  I had ran into this problem before and did not want to run into it again, so I created this extension.

With this extension it is very easy to spot "Invisible" characters that may be unwanted.

Screenshot
-------------------
![Capture](src/ShaneSpace.VisualStudio.InvisibleCharacterVisualizer/assets/capture.png)

Try It Out
-------------------
Copy the text below into the visual studio editor, then install the extension and see the difference.

- \xAD [­]
- \uFEFF [﻿]
- \uFEFF [﻿]
- \uFFF9 [￹]
- \uFFFA [￺]

\0-\x08
- 0001 []
- 0002 []
- 0003 []
- 0004 []
- 0005 []
- 0006 []
- 0007 []

\u000E-\u001F
- 000E []
- 000F []
- 0010 []
- 0011 []
- 0012 []
- 0013 []
- 0014 []
- 0015 []
- 0016 []
- 0017 []
- 0018 []
- 0019 []
- 001A []
- 001B []
- 001C []
- 001D []
- 001E []

\x7F-\u0084
- 007F []
- 0080 []
- 0081 []
- 0082 []
- 0083 []

\u0086-\x9F
- 0086 []
- 0087 []
- 0088 []
- 0089 []
- 008A []
- 008B []
- 008C []
- 008D []
- 008E []
- 008F []
- 0090 []
- 0091 []
- 0092 []
- 0093 []
- 0094 []
- 0095 []
- 0096 []
- 0097 []
- 0098 []
- 0099 []
- 009A []
- 009B []
- 009C []
- 009D []
- 009E []

\u200B-\u200F
- 200B [​]
- 200C [‌]
- 200D [‍]
- 200E [‎]

\u202A-\u202E
- 202A [‪]
- 202B [‫]
- 202C [‬]
- 202D [‭]

\u2060-\u2064
- 2060 [⁠]
- 2061 [⁡]
- 2062 [⁢]
- 2063 [⁣]

\u206A-\u206E
- 206A [⁪]
- 206B [⁫]
- 206C [⁬]
- 206D [⁭]