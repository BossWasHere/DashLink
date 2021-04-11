/*
 Name:    dashlink.h
 Created: 3/24/2021 9:28:12 PM
 Author:  Thomas Stephenson
*/

#pragma once

#define NELEMS(x) (sizeof(x) / sizeof((x)[0]))
#define UINT8STRLEN(x) ((uint8_t)strlen(x))

enum cabilityflags {
	NONE = 0,
  // Device has buttons
	BUTTON_PRESS = 1,
  // Device has rotary encoder
	ROTARY_TURN = 2,
  // Rotary encoder has button
	ROTARY_BUTTON = 4,
  // Device has liquid crystal display
	LIQUID_CRYSTAL = 8,

  // Other
	HOST = 64,
	PROXY = 128
};

// Create our EEPROM data model for our WiFi credentials
#ifdef USE_EEPROM
struct {
	char *setTime[8];
	uint8_t wifiCompLen[2];
	// 30 + 63 characters + 2*(\0)
	char *wifiComp[95];
} eepromData;
#endif
