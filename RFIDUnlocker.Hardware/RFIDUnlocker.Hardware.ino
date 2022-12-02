#include <MFRC522.h>
#include <Keyboard.h>
#include <SD.h>

#define RFID_SS_Pin 10
#define RFID_RST_Pin 2

#define SD_SS_Pin 3

#define RED_LED_PIN 9
#define GREEN_LED_PIN 6
#define BLUE_LED_PIN 5

#define MAX_LED_BRIGHTNESS 30

#define BUZZER_PIN 11
#define MAX_BUZZER_FREQUENCY 100

MFRC522 RFID(RFID_SS_Pin, RFID_RST_Pin);

long lastTime = 0;

void setup() {
  Serial.begin(9600);

  SPI.begin();
  RFID.PCD_Init();
  SD.begin(SD_SS_Pin);

  enableRFID();
}

void loop() {
  if (Serial.available() > 0) {
    String message = Serial.readString();

    if (message.startsWith("<?")) {
      handleMessage(message);
    }

    delay(2000);
  }

  if (millis() - lastTime >= 1000) {
    lastTime = millis();
    initRFID();
  }

  if (IsCardDetected()) {
    String uid = "";
    
    for (int i = 0; i < RFID.uid.size; i++) {
      uid += String(RFID.uid.uidByte[i], HEX);
    }

    uid.toUpperCase();
    Serial.println(uid);

    if (IsUidValid(uid)) {
      grantAccess();
    }
    else {
      denyAccess();
    }

    delay(2000);
  }
}

void handleMessage(String message) {
  String command = message.substring(2, message.length() - 2);
  
  if (command == "add") {
    addCard();
  }
}

void addCard() {
  analogWrite(RED_LED_PIN, MAX_LED_BRIGHTNESS * 0.8);
  analogWrite(GREEN_LED_PIN, MAX_LED_BRIGHTNESS * 0.6);

  while (!IsCardDetected()) {}

  String uid = "";
  for (int i = 0; i < RFID.uid.size; i++) {
    uid += String(RFID.uid.uidByte[i], HEX);
  }
  uid.toUpperCase();

  enableSD();
  String filePath = uid + ".crd";

  if (!SD.exists(filePath)) {
    Serial.println(filePath);
    File file = SD.open(filePath, FILE_WRITE);
    file.close();
  }
  
  resetAllLeds();
  enableRFID();
}

void initRFID() {
  digitalWrite(RFID_RST_Pin, 0);
  delay(50);
  digitalWrite(RFID_RST_Pin, 1);
  RFID.PCD_Init();
}

void enableRFID() {
  digitalWrite(RFID_SS_Pin, 1);
  digitalWrite(SD_SS_Pin, 0);
}

void enableSD() {
  digitalWrite(RFID_SS_Pin, 0);
  digitalWrite(SD_SS_Pin, 1);
}

bool IsCardDetected() {
  if (!RFID.PICC_IsNewCardPresent()) {
    return false;
  }

  if (!RFID.PICC_ReadCardSerial()) {
    return false;
  }
  
  return true;
}

bool IsUidValid(String uid) {
  
  enableSD();
  String filePath = uid + ".crd";
  bool exists = SD.exists(filePath);
  enableRFID();

  if (exists) {
    return true;
  }

  return false;
}

void grantAccess() {
  blinkLed(GREEN_LED_PIN);
  playAccessSound();
  enterPassword();
}

void denyAccess() {
  blinkLed(RED_LED_PIN);
  playDenySound();
}

void blinkLed(int ledPin) {
  resetAllLeds();

  for (int i = 1; i < 3; i++) {
    analogWrite(ledPin, MAX_LED_BRIGHTNESS);
    delay(100);
    analogWrite(ledPin, 0);
    delay(100);
  }
}

void resetAllLeds() {
  analogWrite(RED_LED_PIN, 0);
  analogWrite(GREEN_LED_PIN, 0);
  analogWrite(BLUE_LED_PIN, 0);
}

void enterPassword() {
  Keyboard.press(KEY_RETURN);
  Keyboard.releaseAll();
  delay(2000);

  Keyboard.print("");
  delay(100);

  Keyboard.press(KEY_RETURN);
  Keyboard.releaseAll();
}

void playAccessSound() {
  for (int i = 0; i < 3; i++) {
    delay(100);
    tone(BUZZER_PIN, MAX_BUZZER_FREQUENCY);
    delay(100);
    noTone(BUZZER_PIN);
  }
}

void playDenySound() {
  for (int i = 0; i < 2; i++) {
    delay(100);
    tone(BUZZER_PIN, MAX_BUZZER_FREQUENCY);
    delay(200);
    noTone(BUZZER_PIN);
  }
}
