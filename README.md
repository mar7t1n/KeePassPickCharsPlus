# Pick-Chars-Plus

**Pick-Chars-Plus** is a lightweight [KeePass](http://keepass.info/) plugin that integrates into KeePass’s entry context menu and allows users to quickly view specific characters from an entry’s protected string fields. It supports the **Password** field as well as **Advanced String** fields.

The plugin provides functionality similar to the built-in [PICKCHARS placeholder](https://keepass.info/help/base/placeholders.html#pickchars), but instead of inserting characters into another field, it simply displays the character at the requested position for easy reference. This is especially useful when a website or phone-based security check asks for specific characters from your password (characters are also shown using phonetics).

If you’ve ever had to count along a password to work out the 5th and 9th characters, this plugin removes the guesswork.

---

## Installation

[Download the latest release](https://github.com/mar71n/KeePass-Pick-Chars-Plus/releases)

1. Ensure you are using **KeePass 2.x**
2. Copy `PickCharsPlus.dll` into the KeePass installation directory (where `KeePass.exe` is located), or into a subdirectory of it
3. Restart KeePass to load the plugin

---

## Usage

1. Right-click an entry to open its context menu  
2. Click **Pick Chars Plus**  
3. In the window that appears, select the secure string you want to use (Password or Advanced String field)  
4. Click the button(s) corresponding to the character positions you need  

   ![](./doc/Screenshot1.jpg)

5. When finished, close the window or press **ESC**
