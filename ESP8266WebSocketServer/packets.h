/*
 Name:    packets.h
 Created: 3/24/2021 9:26:43 PM
 Author:  Thomas Stephenson
*/

#pragma once

// Arduino
constexpr uint8_t A_BUTTONS = 0x10;
constexpr uint8_t A_BUTTONPRESS = 0x11;
constexpr uint8_t A_BUTTONRELEASE = 0x12;
constexpr uint8_t A_ROTARY = 0x13;

constexpr uint8_t A_LCDLINE = 0x20;
constexpr uint8_t A_LCD0 = 0x21;
constexpr uint8_t A_LCD1 = 0x22;

// Net
constexpr uint8_t N_CONNECTING = 0xA0;
constexpr uint8_t N_CONNECTED = 0xA1;
constexpr uint8_t N_REQUESTINFO = 0xA2;
constexpr uint8_t N_REQUESTNEWINFO = 0xA3;
constexpr uint8_t N_ADDR = 0xA4;
constexpr uint8_t N_SAVE = 0xA5;
constexpr uint8_t N_RESET = 0xAD;
constexpr uint8_t N_RESETASKME = 0xAE;
constexpr uint8_t N_ERROR = 0xAF;
constexpr uint8_t N_FORWARD = 0xB0;
constexpr uint8_t N_FORWARDBAD = 0xB1;

// Shared
constexpr uint8_t S_WHOAMI = 0xF0;
constexpr uint8_t S_IAM = 0xF1;
constexpr uint8_t S_CAPABILITIES = 0xF2;
constexpr uint8_t S_MYCAPABILITIES = 0xF3;
constexpr uint8_t S_UNKNOWN = 0xFF;
