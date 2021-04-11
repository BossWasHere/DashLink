/*
 Name:    ArduinoDashLink.ino
 Created: 2/22/2021 3:45:32 PM
 Author:  Thomas Stephenson
*/

#include <limits.h>
#include "configuration.h";

#ifdef NET_SERIAL
#include <SoftwareSerial.h>
// Define the baud rate if we need to
#ifndef SERIAL_BAUD
#define SERIAL_BAUD 115200
#endif
#ifdef USE_EEPROM
#include <EEPROM.h>
#endif
#endif

char whoAmI[32];
int bS[BUTTON_COUNT];
#ifdef NET_SERIAL
SoftwareSerial net(0, 1);
#endif

int nextByte = -1;
int nextLength = -1;

char serialBuffer[512];

int buttonState = 0;
unsigned long encLastButtonPress = 0;
unsigned long encLastLastButtonPress = 0;
unsigned long encTimerTrigger = ULONG_MAX;
bool encPressWait = false;
long encLastPos = 0;

void recvLcdSerial(bool top) {
    nextLength = Serial.read();
    if (nextLength < 0)
    {
        return;
    }

    char line[LCD_LINE + 1];
    memset(line, ' ', LCD_LINE);
    line[LCD_LINE] = '\0';
    if (nextLength > LCD_LINE) {
        Serial.readBytes(line, LCD_LINE);
        Serial.readBytes((uint8_t*)nullptr, nextLength - LCD_LINE);
    }
    else
    {
        Serial.readBytes(line, nextLength);
    }
    lcd.setCursor(0, top ? 0 : 1);
    lcd.print(line);
}

void setup() {
    sprintf(whoAmI, "DashLinkv1.0:%02X%02X%02X%02X%02X%02X%02X%02X", UniqueID8[0], UniqueID8[1], UniqueID8[2], UniqueID8[3], UniqueID8[4], UniqueID8[5], UniqueID8[6], UniqueID8[7]);

    for (int i = 0; i < 8; i++)
    {
        pinMode(bP[i], INPUT);
    }

    pinMode(SW, INPUT_PULLUP);

    lcd.begin(LCD_LINE, 2);
    lcd.print(splashTop);
    lcd.setCursor(0, 1);
    lcd.print(splashBottom);

    Serial.begin(9600);

#ifdef NET_SERIAL
    net.begin(SERIAL_BAUD);
#endif
}

void loop() {

    if (Serial.available() > 0) {
        nextByte = Serial.read();

        switch (nextByte) {
        case S_WHOAMI:
            Serial.write(S_IAM);
            Serial.write(UINT8STRLEN(whoAmI));
            Serial.write(whoAmI);
            Serial.write(S_WHOAMI);
            break;

        case S_IAM:
            lcd.setCursor(0, 0);
            lcd.print(connectedTo);
            recvLcdSerial(false);
            break;

        case S_CAPABILITIES:
            Serial.write(S_MYCAPABILITIES);
            Serial.write(capabilities);
            break;

        case A_BUTTONS:
            Serial.write(A_BUTTONS);
            Serial.write(BUTTON_COUNT);
            break;
        
        case A_LCDLINE:
            Serial.write(A_LCDLINE);
            Serial.write(LCD_LINE);
            break;
        case A_LCD0:
            recvLcdSerial(true);
            break;
        case A_LCD1:
            recvLcdSerial(false);
            break;
        default:
            Serial.write(S_UNKNOWN);
            break;
        }
    }

    for (uint8_t i = 0; i < BUTTON_COUNT; i++)
    {
        buttonState = digitalRead(bP[i]);
        if (buttonState != bS[i])
        {
            Serial.write(buttonState == LOW ? A_BUTTONRELEASE : A_BUTTONPRESS);
            Serial.write(i);
            bS[i] = buttonState;
        }
    }

    long encNewPos = rotEnc.read();
    if (encNewPos != encLastPos)
    {
      
#ifdef ROTARY_STEP
    #ifdef ROTARY_READ_OFFSET
        if ((encNewPos + ROTARY_READ_OFFSET) % ROTARY_STEP == 0) {
    #else
        if (encNewPos % ROTARY_STEP == 0) {
    #endif
#endif

        if (encLastPos < encNewPos)
        {
            Serial.write(A_ROTARY);
            Serial.write(1);
        }
        else
        {
            Serial.write(A_ROTARY);
            Serial.write(3);
        }
        encLastPos = encNewPos;

#ifdef ROTARY_STEP
        }
#endif
    }

    // Read the button state
    int btnState = digitalRead(SW);

    // If LOW, the button is pressed
    if (btnState == LOW)
    {
        if (!encPressWait) {
            encLastLastButtonPress = encLastButtonPress;
            encLastButtonPress = millis();

            encPressWait = true;
        }
    }
    // Only trigger once on button release
    else if (encPressWait)
    {
        encPressWait = false;

        unsigned long mdiff = millis() - encLastButtonPress;

        if (mdiff > ROTARY_PRESS_CONFIG) {
            // TODO: implement config menu
        }
        else
        {
            mdiff = encLastButtonPress - encLastLastButtonPress;
            // The difference between the last two presses was long/single press
            if (mdiff > MIN_ROTARY_PRESS_TIME) {
                encTimerTrigger = millis() + MIN_ROTARY_PRESS_TIME;
            }
            // The difference between the last two presses was short/double press
            else
            {
                Serial.write(A_ROTARY);
                Serial.write(12);
                // Reset the single press timer
                encTimerTrigger = ULONG_MAX;
            }
        }
    }

    // We wait for the trigger timer period before we send a single press
    if (encTimerTrigger < millis()) {
        encTimerTrigger = ULONG_MAX;

        Serial.write(A_ROTARY);
        Serial.write(4);
    }
}
