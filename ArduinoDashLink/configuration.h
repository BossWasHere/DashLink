/*
 Name:    configuration.h
 Created: 2/23/2021 3:12:13 PM
 Author:  Thomas Stephenson
*/

#include "dashlink.h"
#include "packets.h"
#include <Encoder.h>
#include <LiquidCrystal.h>
#include <ArduinoUniqueID.h>

/*
 * This file holds the configuration information for ArduinoDashLink
 * To avoid the necessity for complicated build processes.
 * You should make edits here, but avoid committing any unnecessary changes to the git repo if you are a developer.
 */

// If you are using this in combination with the ESP8266 or other board, uncomment the next line.
//#define NET_SERIAL

// If you are storing the network data on this device's EEPROM (instead of the ESP board), uncomment the next line.
//#define USE_EEPROM

// Define rotary step for every n turns (i.e. use this to adjust the "click" turns).
#define ROTARY_STEP 4


#ifdef ROTARY_STEP

// Will take the value between steps. Use if input signal appears to be noisy.
#define ROTARY_READ_OFFSET 2
#endif

// The minimum time that must have elapsed for any press, double or single.
#define MIN_ROTARY_PRESS_TIME 50

// The minimum time that must have elapsed for a regular press.
#define ROTARY_PRESS_TIME 200

// The minimun time that must have elapsed for the config menu to appear.
#define ROTARY_PRESS_CONFIG 1000

// The length of each line on the LCD.
#define LCD_LINE 16

// Text to display on startup.
const char *splashTop = "DashLink";
const char *splashBottom = "Connecting...";
const char *connectedTo = "Connected to";

// Capabilities (See dashlink.h for more options).
constexpr uint8_t capabilities = BUTTON_PRESS | ROTARY_TURN | ROTARY_BUTTON | LIQUID_CRYSTAL;

// Declare button pins.
constexpr uint8_t BUTTON_COUNT = 8;
constexpr uint8_t bP[BUTTON_COUNT] = { A0, A1, A2, A3, A4, A5, 6, 7 };

// Declare rotary encoder pins. Use interrupt pins for CLK and DT (2&3 for Uno).
constexpr uint8_t CLK = 2;
constexpr uint8_t DT = 3;
constexpr uint8_t SW = 4;

// Set up liquid crystal object.
// Args: (rs, enable, d4, d5, d6, d7)
LiquidCrystal lcd(13, 12, 11, 10, 9, 8);

// Set up rotary encoder object.
Encoder rotEnc(CLK, DT);
