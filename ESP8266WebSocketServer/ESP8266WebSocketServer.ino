/*
 Name:		ESP8266WebSocketServer.ino
 Created:	3/24/2021 9:26:26 PM
 Author:	Thomas Stephenson
 Adapted From: https://RandomNerdTutorials.com/esp8266-nodemcu-websocket-server-arduino/
 Special Thanks: Rui Santos
*/

#define USE_EEPROM

// Define the baud rate if we need to
#ifndef SERIAL_BAUD
#define SERIAL_BAUD 115200
#endif
// Define the HTTP port if we need to
#ifndef HTTP_PORT
#define HTTP_PORT 80
#endif

#include "dashlink.h"
#include "packets.h"

#ifdef USE_EEPROM
#include <EEPROM.h>
#endif
#include <ESP8266WiFi.h>
#include <ESPAsyncTCP.h>
#include <ESPAsyncWebServer.h>

//const char* ssid = "ENTER SSID HERE";
//const char* password = "ENTER PASSWORD HERE";

// Allow us to set the network credentials at compile time if we want to
#ifdef AUTOCONNECT
// Safely remove this
#undef AUTOCONNECT
#endif
// Check if these are set during compile
#ifdef NETSSID
#ifdef NETPASS

#define AUTOCONNECT 1
String ssid = NETSSID;
String password = NETPASS;

#endif
#endif

// Else disable autoconnect
#ifndef AUTOCONNECT

#define AUTOCONNECT 0
String ssid;
String password;

#endif

// Declare device information
const char *whoAmI = "ESPDashLinkv1.0:" __TIMESTAMP__;
const char *createTime = __TIME__;
constexpr uint8_t capabilities = PROXY;

// Create the AsyncWebServer and AsyncWebSocket
AsyncWebServer server(HTTP_PORT);
AsyncWebSocket ws("/ws");

const char root_html[] PROGMEM = R"rawliteral(
<!DOCTYPE HTML>
<html>
<head>
  <title>DashLink WS</title>
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="icon" href="data:,">
  <style>
  html {
    font-family: Arial, Helvetica, sans-serif;
    text-align: center;
  }
  </style>
</head>
<body>
  <div>
    <h1>DashLink WS Online</h1>
  </div>
</body>
</html>
)rawliteral";

int nextByte = -1;
bool checkArduinoEEPROM = false;
bool updateEEPROM = false;
bool errored = false;
bool wsSetup = false;

bool tryConnect() {
	// Send "trying to connect" message
	Serial.write(N_CONNECTING);

	WiFi.begin(ssid, password);
	wl_status_t wifistatus = WiFi.status();

	while (wifistatus != WL_CONNECTED) {
		if (wifistatus == WL_CONNECT_FAILED) {
			return false;
		}
		delay(1000);
		wifistatus = WiFi.status();
	}

	// Send "connected" message
	Serial.write(N_CONNECTED);
	Serial.write(N_ADDR);
	Serial.print(WiFi.localIP().toString());
	setEEPROMData();
	return true;
}

void requestConnect(bool getNewCreds) {
	// Send the "request network info" message
	Serial.write(getNewCreds ? N_REQUESTNEWINFO : N_REQUESTINFO);

	while (true) {
		if (Serial.available() > 0) {
			nextByte = Serial.read();

			if (nextByte == N_REQUESTINFO) {

				ssid = Serial.readString();
				password = Serial.readString();

				if (!tryConnect()) {
					errored = true;
				}
				break;

			}
			else {
				errored = true;
				break;
			}
		}
		delay(10);
	}
}

void post(bool requestCreds = false) {
	if (WiFi.isConnected()) {
		// Let the remote know the server is "restarting"
		// Assume the websocket is already open too
		ws.closeAll(1012);
		delay(500);
		WiFi.disconnect();
	}

	if (AUTOCONNECT && !requestCreds) {
		if (!tryConnect()) {
			updateEEPROM = true;
			requestConnect(true);
		}
	}
	else {
		updateEEPROM = true;
		requestConnect(false);
	}

	if (!errored && !wsSetup) {
		initWebSocket();

		// Set basic route for "/"
		server.on("/", HTTP_GET, [](AsyncWebServerRequest *request) {
			request->send_P(200, "text/html", root_html);
		});
		wsSetup = true;
	}
}

void sendByteClient(AsyncWebSocketClient *client, uint8_t data) {
	uint8_t *c = &data;
	client->binary(c, 1);
}

void sendPacketClient(AsyncWebSocketClient *client, uint8_t id, uint8_t *data, size_t len) {
	sendPacketClient(client, id, (const char *)data, len);
}

void sendPacketClient(AsyncWebSocketClient *client, uint8_t id, const char *data) {
	sendPacketClient(client, id, data, sizeof(char) * strlen(data));
}

void sendPacketClient(AsyncWebSocketClient *client, uint8_t id, const char *data, size_t len) {
	char *newData = (char *)malloc(len + 2);
	memcpy(newData + 1, data, len);
	newData[0] = id;
	newData[len + 1] = 0;
	client->binary(newData, len + 1);

	free(newData);
}

void onWebSocketMessage(AsyncWebSocketClient *client, void *arg, uint8_t *data, size_t len) {
	AwsFrameInfo *info = (AwsFrameInfo *)arg;

	if (info->final && info->index == 0 && info->len == len && info->opcode == WS_BINARY) {

		uint8_t size;

		switch (data[0]) {
		case S_WHOAMI:
			sendPacketClient(client, S_IAM, whoAmI);

			break;
		case S_CAPABILITIES:
			sendPacketClient(client, S_MYCAPABILITIES, (uint8_t *)capabilities, 1);

			break;
		case N_FORWARD:
			size = data[1];
			if (size > 0 && size + 2 <= len) {
				uint8_t *smallData = (uint8_t *) malloc(size + 1);
				memcpy(smallData, data + 2, size);
				smallData[size] = 0;
				Serial.write(smallData, size);
				free(smallData);
			}
			else {
				sendByteClient(client, N_FORWARDBAD);
			}

			break;
		default:
			sendByteClient(client, S_UNKNOWN);
			break;
		}
	}
}

void onEvent(AsyncWebSocket *server, AsyncWebSocketClient *client, AwsEventType type, void *arg, uint8_t *data, size_t len) {
	switch (type) {
	case WS_EVT_CONNECT:
		// WebSocket created
		break;
	case WS_EVT_DISCONNECT:
		// WebSocket closed
		break;
	case WS_EVT_DATA:
		// WebSocket data
		onWebSocketMessage(client, arg, data, len);
		break;
	case WS_EVT_PONG:
		// WebSocket pong
		break;
	case WS_EVT_ERROR:
		// WebSocket error (unhandled)
		break;
	default:
		break;
	}
}

void initWebSocket() {
	ws.onEvent(onEvent);
	server.addHandler(&ws);
}

void setEEPROMData() {
	if (!updateEEPROM) return;
	updateEEPROM = false;
	if (checkArduinoEEPROM) {
		// If Arduino EEPROM saving enabled, notify it
		Serial.write(N_SAVE);
	}
	else {
#ifdef USE_EEPROM
		strcpy(*eepromData.setTime, createTime);
		// Add 1 for \0 char
		eepromData.wifiCompLen[0] = ssid.length() + 1;
		eepromData.wifiCompLen[1] = password.length() + 1;

		size_t dataSize = sizeof(eepromData.wifiComp);
		memset(*eepromData.wifiComp, 0, dataSize);

		// Will this work???
		ssid.toCharArray(*eepromData.wifiComp, dataSize);
		password.toCharArray(*eepromData.wifiComp, dataSize, 1 + sizeof(char) * strlen(*eepromData.wifiComp));

		EEPROM.put(0, eepromData);
		EEPROM.commit();
#endif
	}
}

void setup() {
#ifdef USE_EEPROM

	int reqSize = sizeof(eepromData);
	if (reqSize > EEPROM.length()) {
		checkArduinoEEPROM = true;
	}
	else {
		EEPROM.begin(reqSize);
		EEPROM.get(0, eepromData);

		if (!strcmp(*eepromData.setTime, createTime) == 0) {
			updateEEPROM = true;
		}
		else {
			ssid = String(*eepromData.wifiComp);
			password = ssid.substring(eepromData.wifiCompLen[0], eepromData.wifiCompLen[0] + eepromData.wifiCompLen[1]);
			ssid = ssid.substring(0, eepromData.wifiCompLen[0]);
		}
	}
#else
	checkArduinoEEPROM = true;
#endif

	// Start the serial connection with the Arduino
	Serial.begin(SERIAL_BAUD);
	post();
}

void loop() {
	// Remain in an erroneous state until something happens...
	if (errored) {

		if (Serial.available() > 0) {
			nextByte = Serial.read();
			if (nextByte == N_RESET) {
				errored = false;
				post();
				return;
			}
		}

		// Notifies the Arduino that the WiFi module is now in an erroneous state
		Serial.write(N_ERROR);
		delay(1000);

		return;
	}

	// Force clean up of old WS connections
	ws.cleanupClients();

	if (Serial.available() > 0) {
		nextByte = Serial.read();

		switch (nextByte) {
		case S_WHOAMI:
			Serial.write(S_IAM);
			Serial.write(whoAmI);
			break;
		case S_CAPABILITIES:
			Serial.write(S_MYCAPABILITIES);
			Serial.write(capabilities);
			break;
		//case N_FORWARD:
			//uint8_t size = Serial.read();
			//char *data = (char *)malloc(size);
			//Serial.readBytes(data, size);
			//ws.binaryAll(data);
			//free(data);
			//break;
		case N_RESET:
			post();
			break;
		case N_RESETASKME:
			post(true);
			break;
    default:
      Serial.write(S_UNKNOWN);
      break;
		}
	}

	delay(10);
}
