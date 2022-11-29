# Hearthstone Access Localization
This repository contains all the text used by Hearthstone Access.

The en-US locale represents American English and is the only officially supported language. However, people are encouraged to contribute translations for their native languages so they can be included in Hearthstone Access releases.

## Contributing
All text for a given language lives in a file called `ACCESSIBILITY.txt` under the folder for that locale. Using American English as an example, all text would live under `en-US/ACCESSIBILITY.txt`.

In order to localize Hearthstone Access into a new language, the easiest way is to start with the `en-US` file and simply translate the text.

In order to test the translations themselves, you can copy the edited file into your Strings folder in your Hearthstone installation directory. Under a normal American English Hearthstone installation, this would mean copying the file to `C://Program Files (x86)/Hearthstone/Strings/enUS/ACCESSIBILITY.txt`.

### File format

Hearthstone Access uses the already existing Hearthstone's translation system. Under this system, each piece of text consists of:

Key	Value	Comment.

Note that there's a tab between the Key, Value and Comment. Each entry should also end with a newline character.

### Text format

#### Static text

For static text, all that's needed is to provide the exact value for the key, with an optional comment. This would look something like:

`ACCESSIBILITY_WELCOME_TO_HEARTHSTONE	Welcome to Hearthstone	Optional comment`

Note the tab spaces in between the Key, Value and comment.

#### Dynamic text

For dynamic text, users should write the static text around placeholder elements. Placeholder elements are numbers inside curly brackets such as {0} and {1}. As an example:

`ACCESSIBILITY_PRESS_KEY_TO_START	Press {0} to start	0=key e.g. Press Enter to start`

In this example, {0} will be replaced by the game with the correct key. Note that the en-US file specifies what every number represents in the number, often providing examples in more complex phrases.

#### Plurals

In some cases and languages, a given text may require different variations depending on whether it refers to one or multiple entities. Using an example:

`ACCESSIBILITY_GAMEPLAY_YOU_START_WITH_N_CARDS	You start with {0} |4(card,cards)	0=number of cards`

In this example, the word `card` is used if {0} is 1. In all other cases, `cards` is used instead. This is accomplished by prefixing two comma-separated words inside parentheses with the special character `|4` - a vertical bar followed by the number 4. 

Note that in some cases, the distinction between singular and plural is not dependent on the number. For example, the phrase `was silenced` has a `were silenced` counterpart, and both need to exist. In these cases, you will find two different keys such as `ACCESSIBILITY_GAMEPLAY_DIFF_ENTITY_BECAME_SILENCED` and `ACCESSIBILITY_GAMEPLAY_DIFF_MULTIPLE_ENTITIES_BECAME_SILENCED`.

As with all text, contributors are advised to read the American English file first in order to get familiar with the system.

#### Gender nouns

Some languages make use of gender nouns. However, due to the huge amount of cards that exist today, there are no plans to support gender nouns anytime soon. If your language requires gender nouns in dynamic text, please try to work around it using phrases that don't require them as much.

Using ordinal numbers as an example, you can simply configure them so they read as normal digits and configure any text that uses them as if it were refering to `Raid Leader number 1` as opposed to `First Raid Leader`.

#### Special keys

The file includes a few special keys which are used to format various text strings. These entries begin with `ACCESSIBILITY_FORMATTING_` and the provided comment in the en-US file should provide clarity on what they do.

#### Optional keyboard key overrides

The file includes a special key which can be used to override how certain keyboard commands are read. These keys begin with `ACCESSIBILITY_INPUT_KEY_OVERRIDE_`. For example, in the en-US file, you can find:

`ACCESSIBILITY_INPUT_KEY_OVERRIDE_A	eh	Optional. Overrides the A key to be read as eh by TTS`

Note that this may not be needed depending on the language or speech synthesiser you happen to be using.

### Getting help

If you're not familiar with git or have any other questions, you can find someone able to help in the #localization Discord channel on the Hearthstone Access Discord server.
