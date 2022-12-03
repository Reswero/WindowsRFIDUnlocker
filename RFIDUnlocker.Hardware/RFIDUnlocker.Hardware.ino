#include <MFRC522.h>
#include <Keyboard.h>

#define READ_TIMEOUT_MS 5000

#define RFID_SS_Pin 10
#define RFID_RST_Pin 2

#define RED_LED_PIN 9
#define GREEN_LED_PIN 6
#define BLUE_LED_PIN 5

#define MAX_LED_BRIGHTNESS 30

#define BUZZER_PIN 3
#define MAX_BUZZER_FREQUENCY 100

MFRC522 RFID(RFID_SS_Pin, RFID_RST_Pin);

long lastTime = 0;

void setup() {
  Serial.begin(9600);

  SPI.begin();
  RFID.PCD_Init();
}

void loop() {
  if (Serial.available() > 0) {
    String message = Serial.readString();

    if (message.startsWith("<?")) {
      handleRequest(message);
    }

    delay(2000);
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

void handleRequest(String request) {
  String command = request.substring(2, request.length() - 2);
  
  if (command == "add") {
    addCard();
  }
}

void addCard() {
  analogWrite(RED_LED_PIN, MAX_LED_BRIGHTNESS * 0.8);
  analogWrite(GREEN_LED_PIN, MAX_LED_BRIGHTNESS * 0.6);

  String response;
  long startTime = millis();

  while (!IsCardDetected() && (millis() - startTime <= READ_TIMEOUT_MS)) {}

  if (millis() - startTime < READ_TIMEOUT_MS) {
    String uid = "";
    for (int i = 0; i < RFID.uid.size; i++) {
      uid += String(RFID.uid.uidByte[i], HEX);
    }
    uid.toUpperCase();

    response = "<!add10|{\"uid\":\"" + uid + "\">";
  }
  else {
    response = "<!add11>";
  }
  
  Serial.println(response);
  resetAllLeds();
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
  
  if (uid == "") {
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
