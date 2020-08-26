/*
  Constants
*/
const DEFINITION_TAG            = "dd".toUpperCase();       /* Dictionary definition tag - JS uses tag names in all caps        */
const SPAN_TAG                  = "span";                   /* Span tag to auto-generate                                        */
const TERM_TAG                  = "dt".toUpperCase();       /* Dictionary title tag                                             */
const PIPE                      = "| ";                     /* Insert a pipe between adjacent <dd> tags                         */
const MARK_CLASSNAME            = "mark";                   /* Name of CSS class styling arrows                                 */
const ARROW                     = " -> ";                   /* Arrow text                                                       */
const BREAK_ELEMENT             = "br";                     /* Line break element                                               */
const ASTERISK                  = '*';                      /* Asterisk character                                               */
const ASTERISK_CLASSNAME        = "star";                   /* Name of class to style asterisks*                                */
const TOKEN_CLASSNAME           = "token";                  /* Name of CSS class to color Musika tokens                         */
const LETTERS_OR_ASTERISK_ONLY  = /^[A-Za-z_]{2,}\*?$/;     /* Made of only letters and has a length of at least 2 characters   */
const WORD_DELIM                = ' ';                      /* Split words by space                                             */

/*
  Adds the pipe character (|) next to each every dictionary definition that is not next to a term.
  This is done to signify that a nonterminal (dictionary term) has multiple forms and the pipe represents OR.
*/
function addPipes()
{
  /* Local Variables */
  let nonFirstDefinitions = [];
  let cachedText;
  let defList;
  let newNode;
  let i;
  /* / Local Variables */

  /* Get all dictionary definitions that are not next to terms */
  defList = document.getElementsByTagName(DEFINITION_TAG);
  for (i = 0; i < defList.length; ++i)
  {
    if (defList[i].previousSibling.previousSibling.tagName == DEFINITION_TAG )  /* Go back two siblings since the first one */
    {                                                                           /* is always a text node                    */
      nonFirstDefinitions.push(defList[i]);                                    
    }
  }

  for (i = 0; i < nonFirstDefinitions.length; ++i)
  {
    /* Store the content and delete the text node in the definition */
    cachedText = nonFirstDefinitions[i].textContent;
    while (nonFirstDefinitions[i].firstChild)
    {
      nonFirstDefinitions[i].removeChild(nonFirstDefinitions[i].firstChild);
    }

    /* Create a new text node that contains the a pipe followed by a space and then the original text */
    newNode = document.createTextNode(PIPE + cachedText);

    /* Append new text node to definition */
    nonFirstDefinitions[i].appendChild(newNode);
  }
}

/*
  Appends the arrow symbol ( -> ) next to dictionary terms to signify that the term derives into the definition.
*/
function addArrows()
{
  /* Local Variables */
  let arrowElement;
  let arrowNode;
  let termList;
  let i;
  /* / Local Variables */

  /* Get all dictionary terms */
  termList = document.getElementsByTagName(TERM_TAG);

  /* Create an arrow span node and append it next to a term for each term */
  for (i = 0; i < termList.length; ++i)
  {
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
function addLineBreaks()
{
  /* Local Variables */
  let lastDefinitions = [];
  let nextElement;
  let defList;
  let i;
  /* / Local Variables */

  /* Get all dictionary definitions that come right before a new term (last definition element in a rule) */
  defList = document.getElementsByTagName(DEFINITION_TAG);
  for (i = 0; i < defList.length; ++i)
  {
    nextElement = defList[i].nextSibling.nextSibling;
    if (nextElement != null && nextElement.tagName == TERM_TAG) /* Go forward two siblings since the first one */
      lastDefinitions.push(defList[i]);                         /* is always a text node  */
  }

  for (i = 0; i < lastDefinitions.length; ++i)
  {
    lastDefinitions[i].parentElement.insertBefore(document.createElement(BREAK_ELEMENT), lastDefinitions[i].nextSibling); /* Append line break tag */
  }
}

/*
  Surrounds every token name (name in all caps) in an element that can be styled.
*/
function markTokens()
{
  /* Local Variables */
  let wordNode;
  let asteriskSpanNode;
  let spanNode;
  let cachedDef;
  let word;
  let wordList;
  let defList;
  let i;
  let j;
  /* / Local Variables */

  /* Get a list of all definitions (only place a token can be) */
  defList = document.getElementsByTagName(DEFINITION_TAG);

  for (i = 0; i < defList.length; ++i)
  {
    /* Cache definition text into variable and remove text from element */
    cachedDef = defList[i].textContent;
    while (defList[i].firstChild)
    {
      defList[i].removeChild(defList[i].firstChild);
    }

    /* Break definition into a list of words */
    wordList = cachedDef.split(WORD_DELIM);

    for (j = 0; j < wordList.length; ++j)
    {
      word = wordList[j] + WORD_DELIM;
      wordNode = document.createTextNode(word);

      /* If word is a token, surround with element, else create text node. Then append to definition */
      if (wordList[j] == wordList[j].toUpperCase() && LETTERS_OR_ASTERISK_ONLY.exec(wordList[j]))
      {                                                                                             /* Check if word is all caps            */
        spanNode = document.createElement(SPAN_TAG);                                                /* and is only made of letters/asterisk */
        spanNode.classList.add(TOKEN_CLASSNAME);                                                    /* of at least two characters           */

        /* Surround an asterisk with its own span */
        if (word.indexOf(ASTERISK) > -1)
        {
            asteriskSpanNode = document.createElement(SPAN_TAG);
            asteriskSpanNode.classList.add(ASTERISK_CLASSNAME);
            asteriskSpanNode.appendChild(document.createTextNode(ASTERISK));

            spanNode.appendChild(document.createTextNode(word.substring(0, word.indexOf(ASTERISK))));
            spanNode.appendChild(asteriskSpanNode);
            spanNode.appendChild(document.createTextNode(word.substring(word.indexOf(ASTERISK) + 1)));
        }
        else
        {
            spanNode.appendChild(wordNode);
        }

        defList[i].appendChild(spanNode);
      }
      else
      {
        defList[i].appendChild(wordNode);
      }
    }
  }
}

/*
  Main function
*/
window.onload = function()
{
  addPipes();
  addArrows();
  addLineBreaks();
  markTokens();
}
