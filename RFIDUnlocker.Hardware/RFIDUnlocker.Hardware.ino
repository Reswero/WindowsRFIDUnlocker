#include <MFRC522.h>
#include <Keyboard.h>

#define RFID_SS_Pin 10
#define RFID_RST_Pin 2

#define RED_LED_PIN 9
#define GREEN_LED_PIN 6
#define BLUE_LED_PIN 5

#define MAX_LED_BRIGHTNESS 30

MFRC522 RFID(RFID_SS_Pin, RFID_RST_Pin);

void setup() {
  Serial.begin(9600);

  SPI.begin();
  RFID.PCD_Init();
}

void loop() {
  if (IsCardDetected()) {
    String uid = "";
    
    for (int i = 0; i < RFID.uid.size; i++) {
      uid += String(RFID.uid.uidByte[i], HEX);
    }

    uid.toUpperCase();
    Serial.println(uid);

    if (IsUidValid(uid)) {
      GrantAccess();
    }
    else {
      DenyAccess();
    }

    delay(2000);
  }
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
  // TODO: проверка uid в базе
  
  if (uid == "") {
    return true;
  }

  return false;
}

void GrantAccess() {
  BlinkLed(GREEN_LED_PIN);
  EnterPassword();
}

void DenyAccess() {
  BlinkLed(RED_LED_PIN);
}

void BlinkLed(int ledPin) {
  ResetAllLeds();

  for (int i = 1; i < 3; i++) {
    analogWrite(ledPin, MAX_LED_BRIGHTNESS);
    delay(100);
    analogWrite(ledPin, 0);
    delay(100);
  }
}

void ResetAllLeds() {
  analogWrite(RED_LED_PIN, 0);
  analogWrite(GREEN_LED_PIN, 0);
  analogWrite(BLUE_LED_PIN, 0);
}

void EnterPassword() {
  Keyboard.press(KEY_RETURN);
  Keyboard.releaseAll();
  delay(2000);

  Keyboard.print("");
  delay(100);

  Keyboard.press(KEY_RETURN);
  Keyboard.releaseAll();
}
