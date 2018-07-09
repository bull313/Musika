/*
  Global Variables
*/
const DEFINITION_TAG = "dd".toUpperCase();  /* JS uses tag names in all caps */
const SPAN_TAG = "span";
const TERM_TAG = "dt".toUpperCase(); /* JS uses tag names in all caps */

/*
  Adds the pipe character (|) next to each every dictionary definition that is not next to a term.
  This is done to signify that a nonterminal (dictionary term) has multiple forms and the pipe represents OR.
*/
function addPipes() {
  /* Local Variables */
  const PIPE = "| ";
  var nonFirstDefinitions = [];
  var cachedText;
  var defList;
  var newNode;
  var i;

  /* Get all dictionary definitions that are not next to terms */
  defList = document.getElementsByTagName(DEFINITION_TAG);
  for (i = 0; i < defList.length; ++i)
    if (defList[i].previousSibling.previousSibling.tagName == DEFINITION_TAG ) /* Go back two siblings since the first one */
      nonFirstDefinitions.push(defList[i]);                                    /* is always a text node                    */

  for (i = 0; i < nonFirstDefinitions.length; ++i) {
    /* Store the content and delete the text node in the definition */
    cachedText = nonFirstDefinitions[i].textContent;
    while (nonFirstDefinitions[i].firstChild)
      nonFirstDefinitions[i].removeChild(nonFirstDefinitions[i].firstChild);

    /* Create a new text node that contains the a pipe followed by a space and then the original text */
    newNode = document.createTextNode(PIPE + cachedText);

    /* Append new text node to definition */
    nonFirstDefinitions[i].appendChild(newNode);
  }
}

/*
  Appends the arrow symbol ( -> ) next to dictionary terms to signify that the term derives into the definition.
*/
function addArrows() {
  /* Local Variables */
  const MARK_CLASSNAME = "mark";
  const ARROW = " -> ";
  var arrowElement;
  var arrowNode;
  var termList;
  var i;

  /* Get all dictionary terms */
  termList = document.getElementsByTagName(TERM_TAG);

  /* Create an arrow span node and append it next to a term for each term */
  for (i = 0; i < termList.length; ++i) {
    arrowNode = document.createTextNode(ARROW);
    arrowElement = document.createElement(SPAN_TAG);
    arrowElement.classList.add(MARK_CLASSNAME);
    arrowElement.appendChild(arrowNode);

    termList[i].parentElement.insertBefore(arrowElement, termList[i].nextSibling);
  }
}

/*
  Adds line break elements to each ending dictionary definition (a definition right before a term)
  so that they may remain inline-block and still be separated by line.
*/
function addLineBreaks() {
  /* Local Variables */
  const BREAK_ELEMENT = "br";
  var lastDefinitions = [];
  var nextElement;
  var defList;
  var i;

  /* Get all dictionary definitions that come right before a new term (last definition element in a rule) */
  defList = document.getElementsByTagName(DEFINITION_TAG);
  for (i = 0; i < defList.length; ++i) {
    var nextElement = defList[i].nextSibling.nextSibling;
    if (nextElement != null && nextElement.tagName == TERM_TAG) /* Go forward two siblings since the first one */
      lastDefinitions.push(defList[i]);                         /* is always a text node  */
  }

  for (i = 0; i < lastDefinitions.length; ++i)
    lastDefinitions[i].parentElement.insertBefore(document.createElement(BREAK_ELEMENT), lastDefinitions[i].nextSibling); /* Append line break tag */
}

/*
  Surrounds every token name (name in all caps) in an element that can be styled.
*/
function markTokens() {
  /* Local Variables */
  const TOKEN_CLASSNAME = "token";
  const LETTERS_ONLY = /^[A-Za-z_]{2,}$/; /* Made of only letters and has a length of at least 2 characters */
  const WORD_DELIM = " ";
  var wordNode;
  var spanNode;
  var cachedDef;
  var wordList;
  var defList;
  var i, j;

  /* Get a list of all definitions (only place a token can be) */
  defList = document.getElementsByTagName(DEFINITION_TAG);

  for (i = 0; i < defList.length; ++i) {
    /* Cache definition text into variable and remove text from element */
    cachedDef = defList[i].textContent;
    while (defList[i].firstChild)
      defList[i].removeChild(defList[i].firstChild);

    /* Break definition into a list of words */
    wordList = cachedDef.split(WORD_DELIM);

    for (j = 0; j < wordList.length; ++j) {
      wordNode = document.createTextNode(wordList[j] + WORD_DELIM);

      /* If word is a token, surround with element, else create text node. Then append to definition */
      if (wordList[j] == wordList[j].toUpperCase() && LETTERS_ONLY.exec(wordList[j])) { /* Check if word is all caps      */
        spanNode = document.createElement(SPAN_TAG);                                    /* and is only made of letters of */
        spanNode.classList.add(TOKEN_CLASSNAME);                                        /* at least two caracters         */
        spanNode.appendChild(wordNode);
        defList[i].appendChild(spanNode);
      } else {
        defList[i].appendChild(wordNode);
      }
    }
  }
}

/*
  Main function
*/
window.onload = function() {
  addPipes();
  addArrows();
  addLineBreaks();
  markTokens();
}
